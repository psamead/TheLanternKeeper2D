# The Lantern Keeper 2D — Project Experience Record

Purpose: a portable record of lessons for future agents and eventual inclusion in the shared agent knowledge base. This is a starting record, not a claim that the Unity demo or menu is complete.

Last reviewed: 2026-11-03

## Project-specific workflow lessons

- Start with the root `AGENTS.md`, then read only the relevant shared handoff/specification. For menu implementation, the local dated task brief and both menu handoffs contain specific acceptance criteria.
- Shared design documents are canonical for scope and story. Local project records should capture implementation state and evidence without silently changing the design.
- Inspect `git status --short` before edits. The initial Unity setup/menu files were present as untracked work at the time of this record; preserve them and avoid broad staging or cleanup.
- Treat the Unity Editor's current scene/layout as user work. Inspect before changing scene builders or serialized scenes, and make focused changes.
- Use the new Input System throughout, including `InputSystemUIInputModule` for UI. Do not mix in legacy `UnityEngine.Input` calls.
- Import project art from the approved shared asset folder and preserve shared source files. Exclude `.psd` and `.psb` sources from Git.
- Keep implementation, compilation/import, Play Mode, build, and remote publication evidence separate. A checklist item that requires Play Mode remains unchecked until Play Mode was actually observed.

## Scope snapshot

The intended demo has four scenes and one central mechanic: lantern-driven Present/Memory world switching. The shared `DEMO_IMPLEMENTATION_PLAN.md` is authoritative for required, optional, and excluded features. Do not expand scope based only on ideas in old drafts or adjacent projects.

## Evidence recorded so far

- On 2026-11-03, the project root contained `AGENTS.md`, `Assets/_Project/`, and `Docs/` as untracked Git paths; latest observed commit was `e0e9911 Add Unity project files`.
- The local dated initialization/menu task note reports existing menu art/font and Unity package/input configuration. These are task-note claims and should be confirmed against the project/Editor before relying on them.
- The dated task note describes a requested menu implementation and Play Mode comparison, but no evidence in the inspected records establishes that those steps were completed. No commit or push for that work was recorded.

## Updating this record

Append dated, evidence-based entries when an implementation lesson or meaningful validation result is established. Include relevant file/scene names, what was observed, and validation limits. Avoid credentials, private access tokens, and unnecessary personal details. Keep historical facts labeled by date so later agents know to re-check drift-prone state.

## Suggested knowledge-base placement

When transferring to the shared Obsidian knowledge base, preserve this as a project experience/reference note linked from that vault's project index. Keep active instructions in the repository `AGENTS.md` and current handoff; label this record as historical/reference material where appropriate.

## 2026-11-03 — Unity Editor routing mismatch

- The workspace may have a target project's Unity process open while the Unity MCP bridge is attached to a different project. Before any mutation, run a read-only Editor probe and confirm `Application.dataPath` matches the repository path.
- Here, the probe returned `TheFairyHill2D` while the requested project was `TheLanternKeeper2D`. Editor-side scene work was stopped. A failed command had created an empty `Assets/_Project/Scenes` folder plus its `.meta` in FairyHill; cleanup attempts were rejected by the UI/MCP safety layer, so verify and remove that empty accidental folder when the correct project owner can do so.
- The initial baseline commit `7d8ffdf` was pushed successfully after retrying with Git HTTP/1.1. The menu implementation itself remains uncompiled and unvalidated; do not confuse publishing the baseline with completing the feature.

## 2026-11-03 — TMP resource import requires Editor UI

- After the Unity MCP target was corrected to `TheLanternKeeper2D`, Editor APIs created and ordered the four scenes and compiled the menu scripts.
- TMP Essential Resources were absent. The MCP can open the TMP importer window but rejects Unity package import and asset deletion because those calls require user interaction. A placeholder Settings asset and generated Cinzel asset were created during diagnosis but are incomplete; do not treat that font as valid.
- Next: remove the temporary `Assets/TextMesh Pro/` folder in the Unity Project window, import TMP Essentials from `Window > TextMeshPro > Import TMP Essential Resources`, then generate the SDF font and verify its atlas before Play Mode.

## 2026-11-03 — Main menu TMP and Play Mode verification

- The user imported TMP Essentials through the Unity Editor. Verified the standard Liberation Sans SDF, EmojiOne sprite asset, default style sheet, and TMP default font through `AssetDatabase` and `TMP_Settings`.
- Generated a dynamic Cinzel SDF with a valid atlas/material and 75 character records at `Assets/_Project/Fonts/Cinzel SDF Generated.asset`; assigned it to all six TMP labels across `01_StartMenu`, `02_Home`, `03_Harbor`, and `04_EndCard`.
- Play Mode rendered an all-black background despite assigned sprites. The `BG` and `LeftDark` full-screen Canvas images each had inverted vertical anchors and a negative 1080 px rect height. Resetting anchors to `(0,0)`–`(1,1)` and offsets to zero produced a 1920×1080 Game View matching the menu mock.
- Editor-driven Play Mode checks passed: Down selected Settings, Enter opened its panel, pointer-enter selected Settings, Play loaded `02_Home`, and a simulated Escape returned to `01_StartMenu`; Quit logged its intended Editor message. No real hardware mouse session or standalone player build was run.
- The old incomplete `Assets/_Project/Fonts/Cinzel SDF.asset` remains locally present but unreferenced. Unity MCP rejected its deletion as an interactive Editor action; remove it via the Project window before staging if still present. Keep the imported TMP Essentials assets.

## 2026-11-03 — Menu underline and procedural UI cues

- The previous fixed underline offset (`-35`) crossed the lower edge of Cinzel glyphs. `TMP_Text.textBounds` after `ForceMeshUpdate()` gives the rendered glyph bounds; transform those points into the underline parent's local space and leave an 8 UI-unit gap. Play Mode measured the resulting gap as 8.
- Added small hover and selected AudioClips synthesized into memory at runtime and played via one 2D AudioSource on the menu controller. Pointer-enter and click-confirm Play Mode paths both reported active playback; the Settings confirmation also opened the panel. This verifies clip triggering, not physical speaker audibility or hardware mouse input.
- Unity refreshed the TMP fallback font asset during the Editor run. Its generated changes were restored because they were unrelated to the menu feature. Keep commits limited to the requested menu scripts and `01_StartMenu` scene; preserve the pre-existing untracked TMP Extras and obsolete font assets.

## 2026-11-03 — Home placeholder label correction

- The user asked to change the visible `02` to `01` in the screenshot. The intended edit was only `PlaceholderLabel` in `02_Home`: changed `02_Home` to `01_Home` and preserved the `Esc → menu` helper line and the scene asset name.
- `01_StartMenu` was marked dirty in the Editor despite no tracked file diff. Left that scene untouched and opened `02_Home` additively, then saved only `02_Home`. When an unrelated scene is dirty, avoid switching away or saving it while making a focused scene change.
