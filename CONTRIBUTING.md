# Contributing to SimpleRSS

Thank you for your interest in contributing.

## Getting started

1. Fork the repository and clone it locally.
2. Ensure you have the [.NET SDK](https://dotnet.microsoft.com/download) installed (8.0 or later).
3. Build and test:

```bash
dotnet build
dotnet test
```

## Pull requests

1. Create a branch from `main`.
2. Keep changes focused — one feature or fix per pull request.
3. Add or update tests for behavior changes.
4. Ensure `dotnet build` and `dotnet test` pass on all target frameworks.
5. Update `CHANGELOG.md` under the `Unreleased` section for user-facing changes.
6. Open a pull request with a clear description of what changed and why.

## Code style

- Match the existing code style and naming conventions.
- Enable nullable reference types and treat warnings as errors.
- Prefer simple, readable code over clever abstractions.
- XML doc comments on public APIs are appreciated.

## Reporting issues

- Search existing issues before opening a new one.
- Include the SimpleRSS version, target framework, and a minimal reproduction when reporting bugs.
- For security issues, see [SECURITY.md](SECURITY.md).

## License

By contributing, you agree that your contributions will be licensed under the [Apache License 2.0](LICENSE).
