# Osmy

English | [日本語](README.md)

Osmy is a tool for software management using SPDX Documents.

## Overview

Osmy consists of a server program and CLI/GUI clients.

### Server

- has a list of software that is managed by Osmy using SPDX documents
- performs a vulnerability assessment and a file integrity verification periodically
- notifies users of those results by e-mail (if configured)

### CLI/GUI client

- provides a user interface to register/update software information
- provides a user interface to check automatic execution results

<img src="doc/tool-overview.svg">

## Usage

Osmy needs .NET 8 or higher, and Java 11 (for spdx format conversion with [spdx/tools-java](https://github.com/spdx/tools-java)).

### Server (Osmy.Server)

> [!NOTE]
> On Linux, Osmy needs privilege to write into `/var/run/Osmy`，`/var/lib/Osmy`, and `/etc/Osmy`.

```PowerShell
# Ubuntu
# need root privilege
sudo ./Osmy.Server

# Windows
./Osmy.Server.exe
```

About server settings, please refer [this document](doc/server-setting.en-US.md).

### GUI client (Osmy.Gui)

> [!WARNING]
> On Linux, if your system language is set to languages containing non-Latin characters,
> launch with the Latin locale to avoid bugs in dependent libraries
> (<https://github.com/rioil/Osmy/issues/20>).

```PowerShell
# Ubuntu
LC_ALL=C ./Osmy.Gui

# Windows
./Osmy.Gui
```

### CLI client (Osmy.Cli)

Osmy prints usage information when run without arguments.

```PowerShell
# Ubuntu
./Osmy.Cli

# Windows
./Osmy.Cli
```

## License

[LICENSE.md](LICENSE.md)

```Text
SPDX-License-Identifier: MIT
PackageLicenseDeclared: MIT
```
