# EU4 Mission Tool

A tool for laying out mission trees for Europa Universalis IV. It shows the tree the way the game does, has an icon picker and edits mission titles and descriptions in your localisation file. Runs on Windows, Linux and macOS.

## Installation

Download the zip for your system from the [latest release](https://github.com/mati-k/EMT/releases/latest), unpack it and launch `EMT` (`EMT.exe` on Windows). Nothing else needs to be installed.

| System | File |
|---|---|
| Windows | `EMT-win-x64.zip` |
| Linux | `EMT-linux-x64.zip` |
| macOS (Apple Silicon) | `EMT-osx-arm64.zip` |
| macOS (Intel) | `EMT-osx-x64.zip` |

The app isn't signed. On macOS, allow it on first launch with right click > Open (or `xattr -d com.apple.quarantine EMT`), on Windows SmartScreen may ask to confirm running it.

## Building

Requires the [.NET 10 SDK](https://dotnet.microsoft.com/download).

```bash
dotnet run --project EMT.csproj
```

Tests:

```bash
dotnet test --project EMT.Tests
```

`EMT.Tests` also checks against the game files, set `EMT_EU4_PATH` if the game isn't in the default Steam location.

Single-file build for one system (`win-x64`, `linux-x64`, `osx-x64` or `osx-arm64`):

```bash
dotnet publish EMT.csproj -c Release -r win-x64 -o publish/win-x64
```

## Releases

GitHub Actions build and test every push (`ci.yml`). Pushing a version tag builds all four systems and publishes a release with the zips (`release.yml`):

```bash
git tag v2.0.0
git push origin v2.0.0
```

## Scope

The tool is for laying out mission trees:

- creating, removing and reordering branches and missions (drag and drop in the branch list)
- mission key, title, description, icon (with picker, animated icons included), position and required missions
- branch key and slot
- hiding branches from the preview

Triggers, effects, potentials and everything else stay as written. Edit them in your text editor.

Keys may only contain letters, numbers, `_` and `-`.

## Saving

Files are patched, never regenerated:

- Each branch and mission is a block of the original text. Reordering moves those blocks around, together with the comments directly above them.
- Changed fields are replaced in place (e.g. only the value in `icon = ...`). A missing key is added on a new line after the opening brace.
- New branches and missions are generated with empty `potential`, `trigger` and `effect` blocks for you to fill in.
- Encoding (Windows-1252 or UTF-8) and line endings of the mission file stay as they were. Localisation is always saved as UTF-8 with BOM, which the game requires.
- Localisation is edited line by line: only title/description values of changed missions are updated, renamed missions get their keys renamed, and new ones are appended. Entries of deleted missions are left in the file.
- Before each save both files are copied to the backups folder (can be turned off on the start page, File > Open backups folder).
- If a file was changed outside the tool since loading, you're asked before it's overwritten.

Loading and saving any vanilla mission or localisation file without changes produces identical bytes.

Settings, logs and backups live in the user app data folder (`%AppData%\EMT` on Windows). `paths.json` from the old version is imported on first start.
