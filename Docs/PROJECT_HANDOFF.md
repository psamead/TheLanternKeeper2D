# The Lantern Keeper 2D — Agent Handoff

Last reviewed: 2026-11-03

## Project location and state

- Unity project: `C:\Users\Administrator\Documents\UnityProject\TheLanternKeeper2D`
- Shared design folder: `\\haoxi-surfacep\Administrator\Documents\MyFlie\AsArtPortfolioTeaching\Game_York\SharedWorkFolder`
- Unity version: 6000.3.24f1; URP 2D; Windows PC; new Input System.
- Git repository is initialized and connected to GitHub. Local commit `7d8ffdf Before start menu v1` was pushed and verified at remote `main`.
- `AGENTS.md`, source art/font assets, and initial Docs are included in baseline commit `7d8ffdf`, which was pushed and verified. The follow-on Start Menu v1 implementation was saved and Play Mode validated on 2026-11-03; inspect `git log -1` and `git status --short` for its current commit state.
- The obsolete incomplete `Assets/_Project/Fonts/Cinzel SDF.asset` remains unreferenced in the local project and is excluded from the implementation commit. Unity MCP rejected its deletion because the operation requires interactive Editor UI; remove it through the Unity Project window when available. Imported TMP Essentials and the generated `Cinzel SDF Generated.asset` are the working font assets.

## Read first

1. Root `AGENTS.md` for project constraints and required reading order.
2. Shared `handoff.md` for project overview and scope.
3. Shared `DEMO_IMPLEMENTATION_PLAN.md` for accepted demo scope and checklist.
4. Shared `TechnicalArt/HANDPAINTED_SPEC.md` for art/scale/setup standards.
5. For menu work, read `Docs/MENU_UNITY_HANDOFF.md` and the shared `TechnicalArt/MENU_UNITY_HANDOFF.md`.
6. `Docs/2026-11-03_Unity_Init_MainMenu.md` is the original task brief; treat its claims as intended setup, then verify current files and Unity state.

## Current goal and boundary

The project is a short 5–8 minute demo with four scenes: `01_StartMenu` → `02_Home` → `03_Harbor` → `04_EndCard`. Core mechanic: switch between Present and Memory world layers with the lantern. Keep work within the accepted scope in the shared plan. No oil, checkpoints, enemies, tides, multiple endings, or save system.

Work on one implementation checklist item at a time. Mark a checklist item complete only after the required Play Mode verification. Preserve existing scene/layout edits and unrelated working-tree changes. Do not claim compile, Play Mode, build, or publication validation unless each was actually performed.

## Important technical constraints

- Use `UnityEngine.InputSystem`; do not use the legacy `Input` class. UI EventSystem uses `InputSystemUIInputModule`.
- Target 1920×1080, PPU 100, orthographic camera Size 5.4; do not add Pixel Perfect Camera.
- Text shown in game is English and stored in ScriptableObjects where applicable.
- Collision layers use tilemaps; follow the shared hand-painted specification.
- Copy art only from the shared `TechArtAsset/` source into project art folders. Do not alter shared source assets. Keep `.psd`/`.psb` sources out of Git.
- Do not inspect `Library/`, `Temp/`, `Logs/`, `obj/`, `Builds/`, or `*.meta`; inspect Unity scene/prefab serialization only when necessary and narrowly.

## Editor connection and current implementation

An initial Unity MCP call on 2026-11-03 targeted `TheFairyHill2D`. After the user closed that project, a read-only probe confirmed `Application.dataPath` is now `C:/Users/Administrator/Documents/UnityProject/TheLanternKeeper2D/Assets`, Unity 6000.3.24f1.

The four scene assets are created under `Assets/_Project/Scenes/` and Build Settings list them in the requested order. Menu and placeholder scripts compile. On 2026-11-03 the user imported TMP Essentials; Unity confirmed the standard Liberation Sans SDF, EmojiOne sprite asset, default style sheet, and default TMP font. A valid dynamic Cinzel SDF was generated at `Assets/_Project/Fonts/Cinzel SDF Generated.asset` with a populated atlas/material and 75 character records, then assigned to all six TMP labels in the four scenes. The earlier broken `Assets/_Project/Fonts/Cinzel SDF.asset` remains locally present but unreferenced: Unity MCP rejected deleting it because deletion requires interactive Editor UI.

Play Mode evidence on 2026-11-03: the start menu rendered at 1920×1080 and matched the menu mock after correcting the full-screen `BG` and `LeftDark` RectTransforms from inverted vertical anchors (negative height) to standard full-screen stretch anchors. Simulated Down/Enter selected and opened Settings; a pointer-enter event selected Settings; Play loaded `02_Home`; `BackToMenu` and its simulated Escape route returned to `01_StartMenu`; Quit was invoked in Editor and logged its expected Editor-only message. These were Editor-driven input/event checks, not a hardware mouse session or standalone build. The menu scene was saved and Play Mode was exited.

Follow-up on 2026-11-03: the underline now tracks the selected TMP label's rendered glyph bounds and sits 8 UI units beneath the glyphs. Added procedural runtime-generated hover and selection clips on the `MainMenuController` AudioSource. Play Mode checks exercised a pointer-enter event and Settings confirmation; `AudioSource.isPlaying` was true after both cues and the Settings panel opened. No hardware mouse session or speaker-level listening check was performed. Unity Editor compilation succeeded after the change. See `Docs/MENU_UNITY_HANDOFF.md` and the shared `TechnicalArt/MENU_UNITY_PROGRESS.md` for the follow-up details.

On 2026-11-03, at the user's request, changed only the placeholder text in scene `02_Home` from `02_Home` to `01_Home`, retaining the `Esc → menu` line and the actual scene filename. Saved and left `02_Home` active. The Editor reported `01_StartMenu` had unsaved in-memory scene state; it was left untouched while `02_Home` was opened additively. This correction is now also recorded in the shared menu progress file and `ART_HANDOFF.md` §10.

The first visual capture exposed a black background; inspecting the Canvas showed both full-screen images had a 1920×−1080 rect. After the anchor fix, the captured Game View showed the intended background, title, lantern selection mark, and menu labels. Unity's camera-preview MCP call failed twice; Game View screenshot capture succeeded instead. The broken asset remains the only known cleanup item before final diff review and commit.
## Existing local records

- `Docs/2026-11-03_Unity_Init_MainMenu.md` — original initialization/menu request and acceptance details.
- `Docs/MENU_UNITY_HANDOFF.md` — menu-specific build and visual requirements.
- `Docs/PROJECT_EXPERIENCE.md` — reusable project workflow lessons and validation boundaries.

## Handoff update rule

Update this document when verified project state or workflow constraints change. Keep intended work, observed state, and validation evidence clearly distinct. Record the date and specific evidence; do not turn planned work into a completion claim.
