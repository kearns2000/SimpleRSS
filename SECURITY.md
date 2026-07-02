# Security Policy

## Supported versions

| Version | Supported          |
| ------- | ------------------ |
| 2.1.x   | :white_check_mark: |
| 2.0.x   | :white_check_mark: |
| 1.x     | :x:                |

## Reporting a vulnerability

If you discover a security vulnerability in SimpleRSS, please report it responsibly.

**Do not open a public GitHub issue for security vulnerabilities.**

Instead, use one of these channels:

- Open a [GitHub private security advisory](https://github.com/kearns2000/SimpleRSS/security/advisories/new) if available
- Contact the maintainer via the [NuGet package owner profile](https://www.nuget.org/profiles/Paddy%20Kearns)

Please include:

- A description of the vulnerability
- Steps to reproduce
- Potential impact
- Suggested fix (if you have one)

You should receive a response within a reasonable timeframe. We will work with you to understand and address the issue before any public disclosure.

## Security considerations

SimpleRSS fetches and parses untrusted XML from remote URLs. When using the library:

- Feed XML is parsed with DTD processing disabled and XML size limits applied.
- HTTP responses are limited by `MaxResponseBytes` before parsing.
- Use HTTPS feed URLs where possible.
- Sanitize feed HTML content before rendering it in a UI.
