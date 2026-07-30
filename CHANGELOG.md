# Changelog

## [1.9.0] - 2026-07-30

### Added
- **Sample theme applied to the gallery screens** — CG gallery, CG full view, scene recollection, chapter select, BGM gallery and the ending list now match the rest of the themed demo (sky gradient background, ink headings, centered accent close pill, tinted scrollbars). The CG full view intentionally keeps a dark scrim so artwork is not washed out. New menu item `Tools > Novella > Apply Sample Theme Gallery Skin` re-applies it to any scene.
- **List item colors are now editable in the Inspector.** `BGMGalleryUIController` and `EndingListUIController` gained `List Item Appearance` fields (row sprite, row/hover/pressed colors, text colors), and `ChapterSelectUIController` / `SceneRecollectionUIController` gained the hover, pressed, placeholder and empty-state colors that were previously hard-coded. Rows are generated at runtime, so these were the only colors a project could not change without editing engine source. **All defaults match the previous hard-coded values, so existing scenes are unaffected.**

### Fixed
- **The ending list never displayed anything.** Its scroll viewport combined a `Mask` with a fully transparent `Image`; because the mask derives its stencil from the graphic's alpha, every row inside was clipped away — in the editor and at runtime. The viewport now uses `RectMask2D`, matching every other panel in the engine. `EndingListBuilder` was fixed too, so rebuilding the panel no longer reintroduces it.
- List rows collapsed to the height of a single line of text, because the content layout group controlled child heights while the controllers set row heights explicitly. Rows now keep their intended height.
- Close buttons in gallery panels stretched into full-width bars; they are now fixed-size pills centered in the panel.
- The bundled skin tools (`Apply Sample Theme ... Skin`) looked up sprites, styles and `SaveSlot.prefab` by hard-coded `Assets/Novella/...` paths, so they found nothing when the engine was installed as a package. Asset lookup now falls back to searching the project by name, making the tools usable from an installed package.

## [1.8.2] - 2026-07-30

### Fixed
- **Broken font references in the shipped prefabs and demo scenes.** `BacklogEntry.prefab`, `ChoiceButton.prefab`, `SaveSlot.prefab` and both demo scenes pointed at a font asset that was never part of the package (it lives only in the engine's own development project), so every text element fell back to whatever TextMeshPro's default font provided — Japanese text in particular had no guaranteed glyph coverage. All 181 references now resolve to the bundled `NotoSansJP SDF` (OFL, full CJK coverage). This has been the case since the prefabs were first shipped.

> Note: Noto Sans JP has a taller line height than the font used during development, so line spacing in the demo scenes is looser than in the screenshots. Text content and layout are otherwise unchanged.

## [1.8.1] - 2026-07-30

### Changed
- The sample theme is now presented as a neutral placeholder: the demo title screen uses generic placeholder branding, and the skin menu items were renamed from `Apply Sorairo ... Skin` to `Tools > Novella > Apply Sample Theme Settings Skin` / `Apply Sample Theme SaveLoad Skin`.

## [1.8.0] - 2026-07-30

### Added
- **Sample sky-blue UI theme** for the bundled demo — a light school-days look applied across every screen: title (stacked pill buttons, scrim, catch copy), in-game HUD (white message window, gradient name plate, pill quick bar), main menu (card layout), settings (pill tabs, themed sliders/toggles), and save/load panels + `SaveSlot.prefab`. Ships 14 nine-slice sprites under `Runtime/UI/Sprites/Theme/` and a new `MenuButtonStyle` asset; `PrimaryButtonStyle`, `DangerButtonStyle`, `IconButtonStyle` and `DefaultUITheme` are updated to match.
- **Click-wait marker animation** — the ▼ marker in the message window now fades and bobs (1.1s cycle) via the new `NextMarkerBlinker` component, and hides automatically while text is typing.
- **Editor utilities** (Tools > Novella): `Capture Game View` (render overlay canvases to PNG without relying on Game View repaint), `Generate Theme Gradients`, `Apply Sample Theme Settings Skin`, and `Apply Sample Theme SaveLoad Skin` (re-apply the theme to settings / save-load UIs in any scene).

### Fixed
- `HUDController` quick-bar labels: the QL label used a hard-coded white color that was invisible on the new white pill buttons; all label colors now come from the controller's on/off color fields.
- `SaveUIController`: empty save-slot thumbnails used a hard-coded dark color; now a light blue that fits the theme.

### Added
- **Per-state button sprites** — each `ButtonStateStyle` (Normal / Hover / Pressed / Disabled) now has its own `Sprite` slot, so hand-drawn button images can swap per state (states without a sprite fall back to the shared `BackgroundSprite`). Sprites swap at the start of a transition; color and scale still tween.
- `SpriteTint` per state (default white). When a sprite is used, the image is tinted with `SpriteTint` instead of `BackgroundColor`, so custom art shows in its original colors without any color setup.
- `ShowBorder` toggle on `NovellaButtonStyle` — turn off the code-drawn border for designs that already include a frame in the image.

## [1.6.0] - 2026-07-20

### Added
- **Styled buttons (4-state)** — a new `NovellaButtonStyle` ScriptableObject (`Create > Novella > Button Style`) defines a button's look per state (Normal / Hover / Pressed / Disabled): background, border and text colors, corner-bracket decoration, a sweeping sheen highlight, scale, and an optional per-state SE. The new `NovellaButton` component reads the style and animates transitions with a built-in tween (no external dependencies); border, corner brackets and sheen are generated at runtime, so no manual object setup is needed. Keyboard/gamepad focus shows the same visuals as mouse hover. `NovellaUITheme` gains `PrimaryButtonStyle` / `IconButtonStyle` / `DangerButtonStyle` slots — assign a style there and every themed button picks it up; leave them empty and everything renders with the previous flat colors.
- **Button Builder style option** — the Button Builder window can now attach `NovellaButton` to generated buttons: theme-driven (default), a custom style asset, or none (legacy flat color).
- The bundled demo ships three ready-made styles (`PrimaryButtonStyle`, `IconButtonStyle`, `DangerButtonStyle`) applied to the title menu, in-game HUD, and choice buttons.

### Fixed
- `UIThemeApplicator` could not find `HUDPanel`, `SavePanel` or `SettingsPanel` when the scene uses the `CameraRoot` wrapper (the panels intentionally live outside it, so HUD/save/settings theming silently did nothing since the wrapper was introduced). Panel lookup now falls back from `CameraRoot` to the canvas root.
- Clicking a styled button no longer leaves it stuck in its hover look: uGUI keeps a clicked button selected, and `NovellaButton` treats selection as hover for gamepad support, so pointer-initiated selection is now released on pointer-up (navigation selection is unaffected).

## [1.5.0] - 2026-07-16

### Added
- **Diced character sprites** — a new `Novella > Diced Character Builder` editor tool slices a character's expression PNGs into cells, de-duplicates identical cells, and packs the unique ones into a single atlas (`DicedCharacterData` + `DicedImage`). Expression variants share most of their pixels, so memory use drops dramatically for characters with many expressions. When `Resources/Characters/Diced/{id}` exists, `show_char` renders through the atlas automatically — scenarios need no changes, and crossfades, movement, blink (`_blink`) and lip-sync (`_talk`) all keep working. The bundled demo ships diced data for the sample character.

### Fixed
- Typewriter character counting no longer breaks when dialogue text contains a literal `<` (e.g. "A < B"). Only strings shaped like TextMeshPro rich-text tags are treated as tags now.

## [1.4.0] - 2026-07-16

### Removed
- The `ai_say` command, the bundled Claude API client, and the `NovellaEngine.AIApiKey` Inspector field. Generating dialogue at runtime against a metered, key-protected web API doesn't fit a redistributable offline engine (the key would ship in plain text inside every build). Scripts that still contain `ai_say` now log an "Unknown command" warning and skip the line; remove or replace those lines with `say`. The command count is now 42.

## [1.3.4] - 2026-07-16

### Added
- `IChoiceUI.Hide()` is now part of the interface (previously the engine looked up a `Hide` method via reflection and silently did nothing if absent). `CustomChoiceUI` provides a no-op `virtual` default, so existing custom choice UIs keep compiling; override it to dismiss your UI when a load interrupts a pending choice.

### Fixed
- Corrupt or truncated save files no longer throw unhandled exceptions on load. All save reads (slots, quick save, auto save, slot info) now validate and fail gracefully with a warning, treating the broken file as an empty slot. Save writes are also guarded so an I/O failure can't halt the game.
- `FlagManager.Set` no longer logs every flag assignment in release builds (editor and development builds still log).

## [1.3.3] - 2026-07-16

### Fixed
- Script execution no longer grows the call stack when many instantly-completing commands (`set_flag`, `label`, `jump`, etc.) run back-to-back. `NovellaEngine.ExecuteNext` now drains synchronous continuations in a loop (trampoline) instead of recursing, preventing potential stack overflow in long scripts.
- Command errors are now logged with the full stack trace and the correct command index.

### Changed
- Read-state tracking no longer serializes the entire read set to `PlayerPrefs` on every command advance. `ReadManager` marks entries dirty and flushes in batch on scene teardown, application quit/pause, and every save (manual, quick, auto), removing per-click serialization and disk I/O.
- Save thumbnail capture is much cheaper: the 320px downscale is done on the GPU via `Graphics.Blit` (previously a per-pixel `GetPixelBilinear` loop on the CPU), capture textures are reused instead of reallocated per shot, and captures are throttled to at most one per 0.5s so fast-clicking/skipping doesn't re-capture every message.

## [1.3.2] - 2026-07-11

### Changed
- `Runtime/Fonts/NotoSansJP SDF.asset` now covers a comprehensive Japanese character set (~13,500 characters - hiragana, katakana, and the full common-use kanji range, matching Utage 4's coverage) instead of only the ~600 characters used by Novella's own demo scenario. The font is usable for any Japanese VN content, not just the bundled demo.
- Regenerated at `pointSize` 30 / padding 3 / 4096x4096 atlas (matching Utage 4's settings) instead of the Font Asset Creator's default 90/9/1024x1024, which packed the same character count into 2 atlas pages (~73 MB) instead of the ~147 tiny pages (~300+ MB) the defaults produced.

## [1.3.1] - 2026-07-11

### Fixed
- v1.3.0 replaced the shipped demo scenes' font with `LiberationSans SDF`, which only covers Latin script and does not render Japanese at all - breaking the entire Japanese-language demo it was meant to fix. Switched to `Noto Sans JP` (SIL Open Font License, safe to redistribute; the same font Utage 4 bundles for the same purpose), pre-populated with every hiragana/katakana/kanji character actually used by the demo content plus ASCII. Applies to the shipped demo scenes and the `BacklogEntry`/`ChoiceButton`/`SaveSlot` prefabs.
- `Runtime/Fonts/` added to the package so the font is available even if the Demo Project sample isn't imported (the `BacklogEntry`/`ChoiceButton`/`SaveSlot` prefabs are core Runtime assets, not sample-only).

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
