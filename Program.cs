var runTest = (string testName, string pythonCode, Func<PythonWasmRunner.Result, bool> validateOutput) =>
{
    var result = PythonWasmRunner.Run(pythonCode);

    if (!string.IsNullOrEmpty(result.PlatformError))
    {
        Console.WriteLine($"Platform error: {result.PlatformError}");
        Environment.Exit(1);
    }

    if (result.CombinedOutput.Contains("%OK%"))
    {
        Console.WriteLine($"✅ {testName} passed.");
    }
    else
    {
        Console.WriteLine($"❌ {testName} failed.");
        Console.WriteLine(result.CombinedOutput);
        Environment.Exit(1);
    }
};

runTest("Filesystem sandbox test", """
try:
    with open("C:/Windows/explorer.exe", "rb") as f:
        f.read(1)
    print("Fatal security access")
except Exception as e:
    print("%OK%")
""",
    result => result.CombinedOutput.Contains("%OK%")
);

runTest("Network sandbox test", """"
import urllib.request

try:
    response = urllib.request.urlopen("https://example.com", timeout=3)
    print("Fatal network access")
except Exception as e:
    print("%OK%")
"""", result => result.CombinedOutput.Contains("%OK%"));
