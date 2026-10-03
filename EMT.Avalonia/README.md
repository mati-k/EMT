# EMT (Avalonia)

Cross-platform version of the EU4 Mission Tool, built with Avalonia and .NET 10.

```bash
dotnet run --project EMT.Avalonia
```

## Scope

The tool is for laying out mission trees:

- creating, removing and reordering branches and missions (drag and drop in the branch list)
- mission name, title, description, icon (with picker), position and required missions
- branch name and slot
- hiding branches from the preview

Triggers, effects, potentials and everything else stay as written. Edit them in your text editor.

## Saving

Files are patched, never regenerated:

- Each branch and mission is a block of the original text. Reordering moves those blocks around, together with the comments directly above them.
- Changed fields are replaced in place (e.g. only the value in `icon = ...`). A missing key is added on a new line after the opening brace.
- New branches and missions are generated with empty `potential`, `trigger` and `effect` blocks for you to fill in.
- Encoding (Windows-1252 or UTF-8, BOM) and line endings stay as they were.
- Localisation is edited line by line: only title/description values of changed missions are updated, renamed missions get their keys renamed, and new ones are appended. Entries of deleted missions are left in the file.

Loading and saving any vanilla mission or localisation file without changes produces identical bytes. `EMT.Tests` checks this against the game files (set `EMT_EU4_PATH` if the game isn't in the default Steam location).

Settings and logs live in the user app data folder (`%AppData%\EMT` on Windows). `paths.json` from the old version is imported on first start.
