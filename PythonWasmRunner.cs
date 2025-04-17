using System.Text;
using System.Reflection;
using Wasmtime;
using Module = Wasmtime.Module;
using WasmEngineConfig = Wasmtime.Config;

public class PythonWasmRunner : IDisposable
{
    private static bool initialized = false;
    private static Engine engine = null!;
    private static Module module = null!;
    private static readonly string tempFileNameSpace = "python-wasm-runner";

    private static void Initialize()
    {
        if (initialized)
            return;

        try
        {
            engine = new Engine(new WasmEngineConfig()
                .WithOptimizationLevel(OptimizationLevel.Speed));

            string wasmFilePath = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".",
                "python.wasm"
            );
            byte[] wasmBytes = File.ReadAllBytes(wasmFilePath);
            module = Module.FromBytes(engine, "Python", wasmBytes);

            initialized = true;
        }
        catch (Exception ex)
        {
            initialized = false;
            throw new InvalidOperationException("Failed to initialize PythonWasmRunner.", ex);
        }
    }

    public class Result
    {
        private string _stdout = string.Empty;
        public string Stdout
        {
            get => _stdout;
            set => _stdout = value.ReplaceLineEndings(Environment.NewLine).Trim();
        }

        private string _stderr = string.Empty;
        public string Stderr
        {
            get => _stderr;
            set => _stderr = value.ReplaceLineEndings(Environment.NewLine).Trim();
        }

        public string PlatformError { get; set; } = string.Empty;

        public string CombinedOutput => $"{Stdout}{Environment.NewLine}{Stderr}";
    }

    public static Result Run(string userCode)
    {
        Initialize(); // engine and module are ready

        string tmpDir = Path.Combine(Path.GetTempPath(), $"{tempFileNameSpace}-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tmpDir);

        string stdOutPath = Path.Combine(tmpDir, ".stdout.txt");
        string stdErrPath = Path.Combine(tmpDir, ".stderr.txt");
        string scriptPath = Path.Combine(tmpDir, "main.py");

        File.WriteAllText(scriptPath, userCode.ReplaceLineEndings("\n"), Encoding.UTF8);

        try
        {
            using (var store = new Store(engine))
            {
                var wasi = new WasiConfiguration()
                .WithStandardOutput(stdOutPath)
                .WithStandardError(stdErrPath)
                .WithArgs("python", "/main.py")
                .WithPreopenedDirectory(tmpDir, "/");

                store.SetWasiConfiguration(wasi);

                using var linker = new Linker(engine);
                linker.DefineWasi();

                var instance = linker.Instantiate(store, module);
                instance.GetFunction("_start")?.Invoke();
            }
            
            return new Result
            {
                Stdout = File.ReadAllText(stdOutPath, Encoding.UTF8),
                Stderr = File.ReadAllText(stdErrPath, Encoding.UTF8),
                PlatformError = string.Empty
            };
        }
        catch (Exception ex)
        {
            return new Result
            {
                Stdout = File.Exists(stdOutPath) ? File.ReadAllText(stdOutPath) : string.Empty,
                Stderr = File.Exists(stdErrPath) ? File.ReadAllText(stdErrPath) : string.Empty,
                PlatformError = $"{ex.Message}{Environment.NewLine}{ex.StackTrace ?? string.Empty}".Trim()
            };
        }
        finally
        {
            try
            {
                Directory.Delete(tmpDir, true);
            }
            catch (Exception cleanupEx)
            {
                Console.WriteLine($"Failed to delete temp directory: {cleanupEx.Message}");
            }
        }
    }

    public void Dispose()
    {
        module?.Dispose();
        engine?.Dispose();
    }
}
