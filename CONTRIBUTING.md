# Contributing to ITK.Optiflow

Thank you for your interest in **ITK.Optiflow**!  
This project is open source under the **Apache 2.0 License** and welcomes issues, bug fixes, new adapters/parsers/generators, and improvements to documentation and tests.

---

## 📜 Table of Contents
- [Code of Conduct](#code-of-conduct)
- [Architecture Overview](#architecture-overview)
- [Requirements](#requirements)
- [Local Setup on Windows](#local-setup-on-windows)
- [Build, Test & Debug](#build-test--debug)
- [How to Contribute](#how-to-contribute)
- [Branches, Commits & Pull Requests](#branches-commits--pull-requests)
- [Versioning & Releases](#versioning--releases)
- [Guidelines for New Modules](#guidelines-for-new-modules)
- [Security & Responsible Disclosure](#security--responsible-disclosure)
- [License Notice](#license-notice)

---

## 🤝 Code of Conduct
We expect all contributors to maintain a respectful and inclusive environment.  
By participating, you agree to act kindly, constructively, and professionally.  
Report any misconduct via issue or (for sensitive cases) directly to the maintainers by email.

---

## 🧩 Architecture Overview
**OptiFlow** is a middleware framework for ophthalmic optics workflows.  
Main components:

- **Adapters**: Handle input/output channels (e.g., FTP, web services, email).  
- **Parsers**: Convert incoming data → internal standardized format.  
- **Transformations**: Business rule and validation engine.  
- **Generators**: Convert internal format → target format (e.g., B2B order, plain text).  
- **Workflows**: Orchestrate Parse → Transform → Generate → Route.

> The repository root contains `Optiflow.sln`, `UnitTests/`, and module folders like `Adapter/`, `Parser/`, `Transformator/`, `Generator/`, `Workflows/`.

---

## ⚙️ Requirements

- **Windows 10 / 11**  
- **Visual Studio 2022** (or 2019 with .NET 4.8 support)  
- **.NET Framework 4.8 Developer Pack**  
- **Git**  
- *(optional)* **ReSharper** / **StyleCop.Analyzers** for code style checks  

> Verify installation:
> ```
> dotnet --list-runtimes
> ```
> Ensure that **.NET Framework 4.8** is available.

---

## 🪟 Local Setup on Windows

1. Clone the repository:
   ```bash
   git clone https://github.com/itkraus/ITK.Optiflow.git
   ```
2. Open `Optiflow.sln` in Visual Studio.  
3. Restore NuGet packages automatically or via  
   `Tools → NuGet Package Manager → Restore Packages`.  
4. Set build configuration to **Debug**.  
5. Run or test the project (see below).

---

## 🧪 Build, Test & Debug

### Build via Visual Studio
- Menu: **Build → Build Solution (Ctrl + Shift + B)**  
- Or using Developer Command Prompt:
  ```cmd
  msbuild Optiflow.sln /p:Configuration=Release
  ```

### Run Tests
- Menu: **Test → Run All Tests**  
- Or via CLI:
  ```cmd
  vstest.console.exe UnitTests\bin\Debug\UnitTests.dll
  ```

### Debugging
- Select startup project → **F5**  
- Set breakpoints and use the standard VS debugger.

---

## 🚀 How to Contribute

### 1. Open an Issue
- Describe the problem / feature request / enhancement clearly.  
- Include reproducible steps and expected behavior.  
- Tag appropriate labels (bug, feature, parser, adapter, etc.).

### 2. Fork & Branch
- Fork the repository.  
- Create a feature branch from `master`:
  ```
  feat/parser-vca-normalization
  fix/adapter-ftp-timeout
  chore/docs-contributing
  ```

### 3. Development
- Follow [Guidelines for New Modules](#guidelines-for-new-modules).  
- Add or update tests.  
- Update README or documentation if behavior or interfaces change.

### 4. Pull Request
- Summarize your changes (What / Why / How tested?).  
- Checklist before submitting:
  - [ ] Build & tests pass  
  - [ ] No sensitive data included  
  - [ ] Breaking changes documented  
  - [ ] Docs/examples updated  
  - [ ] License header in new files (if applicable)

---

## 🧭 Branches, Commits & Pull Requests

### Commit Messages (Conventional Commits)
Use **Conventional Commits** syntax:

```
feat(adapter): add SFTP keepalive option
fix(parser): correct sphere rounding for VCA
docs: improve local setup section
test(rule-engine): add edge cases for prism calc
chore(ci): add VS2019 build step
```

### PR Checks
- Successful build (Debug/Release)  
- All unit tests green  
- Optional: StyleCop / Analyzer without warnings  

---

## 🏷️ Versioning & Releases
We use **Semantic Versioning** (`MAJOR.MINOR.PATCH`):

- **MAJOR** – breaking changes  
- **MINOR** – new features (backward compatible)  
- **PATCH** – bug fixes or internal improvements  

> Example: `v1.3.2`

Maintain a changelog (new section per release).  
Releases are tagged and published via GitHub.

---

## 🧱 Guidelines for New Modules

### Adapters
- Implement clean error handling (timeouts, retries, logging).  
- Use configuration (App.config or settings classes).  
- Never hardcode credentials or file paths.

### Parsers
- Be robust against incomplete or malformed input.  
- Use consistent exception types (e.g., `ParserException`).  
- Include unit tests for edge cases.

### Transformations (Rule Engine)
- Implement deterministic, documented rules.  
- Develop business logic with test-driven approach.  
- Keep it easily extensible (e.g., `IRule` interface).

### Generators
- Follow target specifications exactly.  
- Validate output (schema/format).  
- Implement round-trip tests where possible.

### Workflows
- Maintain clear data flow: Input → Steps → Output.  
- Log every major operation.  
- Define error handling strategy (fail-fast / retry).

---

## 🔒 Security & Responsible Disclosure
- **Never commit secrets** (passwords, tokens, internal URLs).  
- Do **not disclose vulnerabilities publicly**.  
  Instead, send a private report to the maintainers including:
  - Description, impact, and reproduction steps  
  - (Optional) proof-of-concept and fix suggestion  
- We will evaluate, prioritize, and coordinate a fix responsibly.

---

## ⚖️ License Notice
Contributions are licensed under the **Apache 2.0 License**.  
By submitting a pull request, you agree to the **Developer Certificate of Origin (DCO)** by adding a `Signed-off-by` line in your commits:

```
Signed-off-by: Your Name <your.email@example.com>
```

Add it automatically using:
```bash
git commit -s -m "feat: add new VCA field mapping"
```

---

**Thank you for contributing to ITK.Optiflow!** ✨  
Your work helps make optical data processing more transparent, reliable, and modern.

