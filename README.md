# Smart Password Manager CLI (C#) <sup>v1.0.1</sup>

**Terminal-based smart password manager with deterministic password generation. Generate, manage, and retrieve passwords without storing them - all from your command line. Your secret phrase never leaves your device.**

---

[![GitHub top language](https://img.shields.io/github/languages/top/smartlegionlab/SmartPasswordManagerCsharpCli)](https://github.com/smartlegionlab/SmartPasswordManagerCsharpCli)
[![GitHub license](https://img.shields.io/github/license/smartlegionlab/SmartPasswordManagerCsharpCli)](https://github.com/smartlegionlab/SmartPasswordManagerCsharpCli/blob/master/LICENSE)
[![GitHub release](https://img.shields.io/github/v/release/smartlegionlab/SmartPasswordManagerCsharpCli)](https://github.com/smartlegionlab/SmartPasswordManagerCsharpCli/)
[![GitHub stars](https://img.shields.io/github/stars/smartlegionlab/SmartPasswordManagerCsharpCli?style=social)](https://github.com/smartlegionlab/SmartPasswordManagerCsharpCli/stargazers)
[![GitHub forks](https://img.shields.io/github/forks/smartlegionlab/SmartPasswordManagerCsharpCli?style=social)](https://github.com/smartlegionlab/SmartPasswordManagerCsharpCli/network/members)

---

## ⚠️ Disclaimer

**By using this software, you agree to the full disclaimer terms.**

**Summary:** Software provided "AS IS" without warranty. You assume all risks.

**Full legal disclaimer:** See [DISCLAIMER.md](https://github.com/smartlegionlab/SmartPasswordManagerCsharpCli/blob/master/DISCLAIMER.md)

---

## Core Principles

- **Zero-Password Storage**: No passwords are ever stored or transmitted
- **Deterministic Regeneration**: Passwords are recreated identically from your secret phrase
- **Metadata Management**: Store only descriptions and verification keys
- **Client-Side Generation**: All cryptographic operations happen on your device
- **On-Demand Discovery**: Passwords exist only when you generate them

## Key Features

- **Smart Password Generation**: Deterministic from secret phrase
- **Public/Private Key System**: 30 iterations for private key, 60 for public key
- **Secret Verification**: Verify secret without exposing it
- **Interactive Mode**: Menu-driven interface for easy use
- **Command-Line Mode**: Scriptable operations for automation
- **Export/Import**: Backup and restore your password metadata
- **Cross-Platform**: Windows, Linux support
- **Hidden Input**: Secret phrase entry with asterisks masking

## Security Model

- **Proof of Knowledge**: Public keys verify secrets without exposing them
- **Deterministic Security**: Same secret + length = same password, always
- **Metadata Separation**: Non-sensitive data stored in JSON file
- **Local Processing**: Secret and password never leave your device
- **No Recovery Backdoors**: Lost secret = permanently lost access (by design)

---

## Research Paradigms & Publications

- **[Pointer-Based Security Paradigm](https://doi.org/10.5281/zenodo.17204738)** - Architectural Shift from Data Protection to Data Non-Existence
- **[Local Data Regeneration Paradigm](https://doi.org/10.5281/zenodo.17264327)** - Ontological Shift from Data Transmission to Synchronous State Discovery

---

## Technical Foundation

Powered by **[smartpasslib-csharp](https://github.com/smartlegionlab/smartpasslib-csharp)** — C# implementation of deterministic password generation.

**Key derivation (same as Python/JS/Kotlin/Go versions):**

| Key Type    | Iterations | Purpose                                               |
|-------------|------------|-------------------------------------------------------|
| Private Key | 30         | Password generation (never stored, never transmitted) |
| Public Key  | 60         | Verification (stored on server)                       |

**Character Set:** `abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$&*-_`

## Download

### Pre-built Binaries (no .NET required)

| Platform        | Download                                                                                                                     |
|-----------------|------------------------------------------------------------------------------------------------------------------------------|
| **Windows x64** | [SmartPasswordManagerCsharpCli-win-x64.exe](https://github.com/smartlegionlab/SmartPasswordManagerCsharpCli/releases/latest) |
| **Linux x64**   | [SmartPasswordManagerCsharpCli-linux-x64](https://github.com/smartlegionlab/SmartPasswordManagerCsharpCli/releases/latest)   |

> Just download, run in terminal, and start using.

### Build from Source

```bash
# Clone repository
git clone https://github.com/smartlegionlab/SmartPasswordManagerCsharpCli.git
cd SmartPasswordManagerCsharpCli

# Run directly (requires .NET SDK)
dotnet run --project SmartPasswordManagerCsharpCli/

# Publish single file executable
dotnet publish -c Release -o ./publish
```

## Quick Start

### Run the Application

```bash
# Windows
SmartPasswordManagerCsharpCli-win-x64.exe

# Linux
./SmartPasswordManagerCsharpCli-linux-x64
```

### Interactive Menu

```
================================================================================
                           SMART PASSWORD MANAGER CLI
                                Version: v1.0.1
     Storage: /home/user/.config/smart_password_manager/passwords.json
                            Total smart passwords: 0
================================================================================

 MAIN MENU
 1. Add Smart Password
 2. Get Password
 3. List Smart Passwords
 4. Delete Smart Password
 5. Export Data
 6. Import Data
 7. Help
 0. Exit

Select option: 
```

### Command-Line Mode

```bash
# Add new password
SmartPasswordManagerCsharpCli add "GitHub Account" 16 "MySecretPhrase123"

# List all passwords
SmartPasswordManagerCsharpCli list

# Get password by index
SmartPasswordManagerCsharpCli get 1

# Delete password by index
SmartPasswordManagerCsharpCli delete 1

# Export all passwords to JSON
SmartPasswordManagerCsharpCli export

# Import passwords from JSON file
SmartPasswordManagerCsharpCli import ./passwords_export.json

# Show help
SmartPasswordManagerCsharpCli help
```

## Storage Locations

| Platform | Path                                                          |
|----------|---------------------------------------------------------------|
| Linux    | `~/.config/smart_password_manager/passwords.json`             |
| Windows  | `%USERPROFILE%\.config\smart_password_manager\passwords.json` |

**Export files** are saved to `~/SmartPasswordManager/` with timestamp: `spm_export_20260117_143022.json`

## Security Requirements

### Secret Phrase
- **Minimum 12 characters** (enforced)
- Case-sensitive
- Use mix of: uppercase, lowercase, numbers, symbols, emoji, or Cyrillic
- Never store digitally
- **NEVER use your password description as secret phrase**

### Strong Secret Examples
```
✅ "MyCatHippo2026"          — mixed case + numbers
✅ "P@ssw0rd!LongSecret"     — special chars + numbers + length
✅ "КотБегемот2026НаДиете"   — Cyrillic + numbers
```

### Weak Secret Examples (avoid)
```
❌ "GitHub Account"          — using description as secret (weak!)
❌ "password"                — dictionary word, too short
❌ "1234567890"              — only digits, too short
❌ "qwerty123"               — keyboard pattern
```

## Cross-Platform Compatibility

Smart Password Manager (C#) CLI produces **identical passwords** to:

| Platform       | Application                                                                               |
|----------------|-------------------------------------------------------------------------------------------|
| Python CLI     | [CLI PassMan](https://github.com/smartlegionlab/clipassman)                               |
| Python CLI Gen | [CLI PassGen](https://github.com/smartlegionlab/clipassgen)                               |
| Desktop Python | [Desktop Manager](https://github.com/smartlegionlab/smart-password-manager-desktop)       |
| Desktop C#     | [Desktop C# Manager](https://github.com/smartlegionlab/SmartPasswordManagerCsharpDesktop) |
| Web            | [Web Manager](https://github.com/smartlegionlab/smart-password-manager-web)               |
| Android        | [Android Manager](https://github.com/smartlegionlab/smart-password-manager-android)       |

## For Developers

### Prerequisites
- .NET 10.0 SDK or later

### Build Commands

```bash
# Arch Linux
sudo pacman -S dotnet-sdk

# Run
dotnet run --project SmartPasswordManagerCsharpCli/

# Build
dotnet build SmartPasswordManagerCsharpCli/

# Publish single file

# Windows
dotnet publish -c Release -o C:\publish-win\SmartPasswordManagerCsharpCli -p:AssemblyName=SmartPasswordManagerCsharpCli-win-x64 -r win-x64 --self-contained true

# Linux  
dotnet publish -c Release -o ~/.publish-linux/SmartPasswordManagerCsharpCli -p:AssemblyName=SmartPasswordManagerCsharpCli-linux-x64 -r linux-x64 --self-contained true

```

## Ecosystem

**Core Libraries:**
- **[smartpasslib](https://github.com/smartlegionlab/smartpasslib)** - Python
- **[smartpasslib-js](https://github.com/smartlegionlab/smartpasslib-js)** - JavaScript
- **[smartpasslib-kotlin](https://github.com/smartlegionlab/smartpasslib-kotlin)** - Kotlin
- **[smartpasslib-go](https://github.com/smartlegionlab/smartpasslib-go)** - Go
- **[smartpasslib-csharp](https://github.com/smartlegionlab/smartpasslib-csharp)** - C#

**CLI Applications:**
- **[CLI PassMan (Python)](https://github.com/smartlegionlab/clipassman)**
- **[CLI PassGen (Python)](https://github.com/smartlegionlab/clipassgen)**
- **[CLI Manager (C#)](https://github.com/smartlegionlab/SmartPasswordManagerCsharpCli)** (this)
- **[CLI Generator (C#)](https://github.com/smartlegionlab/SmartPasswordGeneratorCsharpCli)**

**Desktop Applications:**
- **[Desktop Manager (Python)](https://github.com/smartlegionlab/smart-password-manager-desktop)**
- **[Desktop Manager (C#)](https://github.com/smartlegionlab/SmartPasswordManagerCsharpDesktop)**

**Other:**
- **[Web Manager](https://github.com/smartlegionlab/smart-password-manager-web)**
- **[Android Manager](https://github.com/smartlegionlab/smart-password-manager-android)**

## License

**[BSD 3-Clause License](https://github.com/smartlegionlab/SmartPasswordManagerCsharpCli/blob/master/LICENSE)**

Copyright (©) 2026, [Alexander Suvorov](https://github.com/smartlegionlab)

## Author

**Alexander Suvorov** - [GitHub](https://github.com/smartlegionlab)

---

## Support

- **Issues**: [GitHub Issues](https://github.com/smartlegionlab/SmartPasswordManagerCsharpCli/issues)
- **Documentation**: This [README](https://github.com/smartlegionlab/SmartPasswordManagerCsharpCli/blob/master/README.md)

---

