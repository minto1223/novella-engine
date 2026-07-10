# Changelog

## [1.3.0] - 2026-07-11

### Added
- Demo Project sample now ships the actual `TitleScene.unity` and `SampleScene.unity` scene files (previously only loose Resources/Data assets were included, so the sample could not actually be opened and played after import)
- `com.unity.render-pipelines.universal` added as an explicit package dependency (the demo scenes use URP 2D lighting)

### Fixed
- `.meta` files are no longer excluded from version control (`.gitignore` previously ignored `*.meta` project-wide), which meant every asset in the package — including scripts — got a brand-new random GUID on every fresh install, breaking any direct object reference (Inspector-assigned fields, prefab/scene links). All Runtime/Editor scripts, prefabs, and sample assets now ship with `.meta` files whose GUIDs are pinned to match the main project, so cross-references resolve correctly after a clean import.
- The shipped demo scenes and the `BacklogEntry`/`ChoiceButton`/`SaveSlot` prefabs no longer reference the commercially-licensed `font_1_kokugl_1` font (which was never bundled and can't be redistributed). All TextMeshPro font/material/atlas references in the shipped copies now point to Unity's bundled `LiberationSans SDF`; the main project's own font choice is unaffected.

## [1.2.0] - 2026-07-11

### Added
- Title screen Settings button support (`TitleManager._settingsButton` / `_settingsUI`) with `Novella > Rebuild Title Settings Panel` editor tool to build a full settings panel (tabs, sliders, toggles) on the title screen
- "Settings" option in the Button Builder tool's Title tab function list

## [1.1.0] - 2026-07-09

### Added
- Read-text color mode (`ReadColorMode`: None / TextOnly / TextAndName) for the message window
- Button Builder editor tool (`Novella > Button Builder`) for adding/removing UI buttons with free placement
- Title screen Reset button (clears all save data) with patcher tool
- Confirm dialog UI (`ConfirmDialogController` + `ConfirmDialogBuilder`)
- Windows build menu (`Novella > Build Windows`)
- `UIInputUtil` helper for pointer/UI raycast checks

### Fixed
- Ruby (furigana) rendering now uses actual `TMP_FontAsset` glyph widths instead of a character-count estimate

## [1.0.0] - 2026-03-24

### Added
- Initial release
- 43 commands for visual novel scripting
- JSON/CSV script support
- Save/Load system with quick save, auto save, and multiple slots
- Backlog with voice replay
- ADV/NVL display modes
- Character expression system
- Rich text (bold, italic, color, size, ruby)
- CG gallery, BGM gallery, ending list, scene recollection
- Flowchart (branching progress map)
- Ken Burns effect for backgrounds
- Particle effects (sakura, snow, firefly)
- Camera controls (zoom, pan, reset)
- UI theme system (ScriptableObject)
- Custom UI extension via interfaces
- Localization (JSON-based)
- Mobile support (SafeArea, touch)
- Movie playback
