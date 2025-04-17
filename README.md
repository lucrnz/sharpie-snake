# SharpieSnake

A project for running Python inside C#, leveraging WebAssembly & WASI.

Inspired by: [Xe's python runner](https://github.com/Xe/x/tree/master/llm/codeinterpreter/python)

## Where to download python.wasm

Take a look at:
[webassembly-language-runtimes](https://github.com/webassemblylabs/webassembly-language-runtimes/tree/main)

## Where to put python.wasm

It goes in the exact directory as the built artifact (aka the .exe file, or binary).

It does not go in the "Current working directory".

I might optinally try to read an environment variable in the future.

## Disclaimer

This project is provided as a reference implementation and for educational purposes, before using it in a production environment, please consider:

- Thoroughly testing the implementation
- Security implications of running Python code via WebAssembly
- Performance requirements for your specific use case
- Maintenance and support considerations
- Alternative solutions that might better fit your needs
## License

This project is licensed under Creative Commons Zero v1.0 Universal (CC0-1.0). This means you can copy, modify, distribute and perform the work, even for commercial purposes, all without asking permission.

See [LICENSE](./LICENSE) for more information.
