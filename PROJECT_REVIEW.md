# EMT — Project Review

_Review date: 2026-10-03. Last commit at time of review: 2025-02-25 (`be78df6`)._

> **Note:** this reviews the original WPF version, which has since been replaced by the Avalonia rewrite.
> The issues listed here were addressed in the rewrite: saving patches the original text, localisation is
> edited line by line, and the Avalonia + .NET 10 plan below is what was implemented.

## What it is

EU4 Mission Tool: a WPF desktop editor for Europa Universalis IV mission trees.

- **Stack:** .NET 5 (`net5.0-windows`) + WPF, Caliburn.Micro (MVVM), Pdoxcl2Sharp (Paradox script parsing), Pfim (DDS decoding), gong-wpf-dragdrop, WindowsAPICodePack (file dialogs), AutoCompleteTextBox, log4net, Newtonsoft.Json.
- **Size:** ~3,600 lines of C#, ~500 lines of XAML, 52 commits.
- **Flow:** the start screen asks for the mission file, localisation file, vanilla folder and mod folder (saved to `paths.json`). The app parses the mission file, localisation and all `.gfx` files, then shows a branch/mission tree with drag-and-drop, a preview rendered with the game's own DDS icons and arrows, per-mission trigger/effect/highlight node editors, title/description editing and an icon picker.
- **Build status:** builds cleanly; only unused-variable warnings.

## Bugs (most serious first)

### 1. Saving is lossy for mission files
- `MissionModel.TokenCallback` ([Models/MissionModel.cs](Models/MissionModel.cs)) only handles `position`, `icon`, `required_missions`, `provinces_to_highlight`, `trigger` and `effect`. Any other mission key (e.g. `ai_weight`, `completed_by`) is silently dropped on save.
- `MissionBranchModel.TokenCallback` ([Models/MissionBranchModel.cs](Models/MissionBranchModel.cs)) treats any unrecognised branch key as a **mission** (`default:` case).
- Comments are lost on save.
- The writer always emits ` = `, so comparison operators (`>`, `<`) may not survive a round trip. **Unverified — needs a test.**

### 2. The localisation file is regenerated rather than edited
`Localisation.Write` ([Models/Localisation.cs](Models/Localisation.cs)):
- always writes the `l_english:` header, whatever language the file was,
- forces `:0` on every key,
- loses comments and ordering.

`Localisation.Read` assumes lines exactly like ` key:0 "value"` and fails on trailing comments, tab indentation, or no space before the opening quote.

### 3. Drag-and-drop doesn't update `MissionModel.Branch`
In [Handlers/DropTargetHandler.cs](Handlers/DropTargetHandler.cs), moving a mission to another branch never reassigns `mission.Branch`. If you drag that mission again, it is removed from the old branch (a no-op) and inserted into the new one, so it ends up in two branches. In `IsSelfOrChild`, `target as MissionModel` should be `source as MissionModel`; as written the check does nothing.

### 4. "Create new file" leaves the file open
`File.Create(...)` in `StartViewModel.CreateMissionFile` / `CreateLocalisationFile` ([ViewModels/StartViewModel.cs](ViewModels/StartViewModel.cs)) returns a `FileStream` that is never disposed, so the following load can fail with "file in use". All loads use `new FileStream(path, FileMode.Open)`, which asks for read/write access, so read-only files fail too; use `File.OpenRead`.

### 5. Icons fail silently on path mismatch
In `ShellViewModel.LoadGfx` ([ViewModels/ShellViewModel.cs](ViewModels/ShellViewModel.cs)), the root folder is found by walking up parents and comparing with `string.Equals`. A trailing slash or a case difference in the chosen folder makes it reach the drive root and throw `NullReferenceException`. The exception is caught per file, so the user just ends up with no icons and no message.

### 6. Smaller issues
- `GfxDialogViewModel.CanOk` is inverted (`IsNullOrWhiteSpace` instead of `!IsNullOrWhiteSpace`). It's masked because Caliburn never resolves the `selectedIcon` parameter. Pressing OK with nothing selected sets `Icon = null`, and the next save throws `IconException`.
- `MissionDetailsViewModel.PickGfx` blocks on `.Result` on the UI thread; it should be awaited.
- The tree preview is fully rebuilt and every DDS file re-read from disk on each `Name`/`Position` change, i.e. on every keystroke. It needs a decoded-image cache.
- `DDSConverter` uses `Marshal.UnsafeAddrOfPinnedArrayElement` on an array that isn't pinned. Use the `byte[]` overload of `BitmapSource.Create`.
- `GfxDialogViewModel.FilterGfxFiles` has a dead early branch (no `return`), and filtering is case-sensitive.
- Duplicate localisation keys are reported via `Console.WriteLine`, which a WinExe never shows.
- `_unconnectedLocalisation` is never cleared (fine only because a file is loaded once per run).
- `MissionTreeView.AddIcon` throws `KeyNotFoundException` if an arrow GFX is missing.

## Tech debt and cleanup
- **Outdated .NET:** .NET 5 has been out of support since May 2022. Leftovers from .NET Framework 4.7.2 remain: ClickOnce/bootstrapper properties in the `.csproj`, `supportedRuntime` in `App.config`, and the Upgrade Assistant analyzer package.
- **Dead code:**
  - `Converters/ParadoxConfigParser.cs` is about 350 lines, fully commented out, and contains hardcoded `C:\Users\Mati\...` paths.
  - `Models/ComplexTypeValues.cs` is only used by that dead code.
  - The `cwtools-eu4-config` submodule is declared but not checked out and unused.
- **Copy-paste:** `AddValue`/`AddGroup`/`RemoveValue`/`RemoveGroup` are duplicated across `StartViewModel`, `MissionDetailsViewModel` and `BranchDetailsViewModel`; the `StartViewModel` copies are unused.
- **Relative paths:** `paths.json` and `logs\` are relative to the current working directory, not the executable's folder.
- **No tests and no CI:** `.github/workflows` is empty.

## Opinion

A useful, focused tool with a sensible MVVM structure. Rendering the preview with the game's real arrow and icon assets is a nice touch. Its weak point is that saves aren't faithful to the original files. For a tool that overwrites files inside a mod, that matters most, because users won't notice until a mission loses its `ai_weight` or a comment block vanishes.

### Suggested priorities
1. Keep unknown keys when saving, and add round-trip tests (load and save real mission files, then compare).
2. Edit localisation keys in place instead of regenerating the file.
3. Fix the drag-and-drop `Branch` bug and the undisposed `File.Create` handles.
4. Modernise the runtime and remove dead code.

## Cross-platform plan (Avalonia + .NET 10)

Upgrading to .NET 10 alone does **not** make the app cross-platform: WPF is Windows-only on every .NET version (`net10.0-windows`). Supporting Linux and macOS means replacing the UI framework, and Avalonia is the closest match to WPF.

| Dependency | Cross-platform? | Replacement |
|---|---|---|
| WPF | No | Avalonia 11 |
| Caliburn.Micro | No (WPF/UWP only) | CommunityToolkit.Mvvm |
| gong-wpf-dragdrop | No | Avalonia built-in `DragDrop` |
| WindowsAPICodePack-Shell | No | Avalonia `StorageProvider` |
| AutoCompleteTextBox | No | Avalonia `AutoCompleteBox` |
| Pdoxcl2Sharp | Yes (netstandard2.0) | keep |
| Pfim | Yes (netstandard2.0) | keep; output to Avalonia `WriteableBitmap` |
| log4net, Newtonsoft.Json | Yes | keep (or move to `System.Text.Json`) |

Platform-specific pitfalls:
- **Case-sensitive file systems** (Linux): texture paths in `.gfx` files often don't match the real file's capitalisation. File lookup needs to be case-insensitive.
- **Config and log locations:** use the per-user app data folder (`Environment.SpecialFolder.ApplicationData`) instead of the current directory.
- **Path handling:** compare paths after normalising them (full path, trimmed separators).
