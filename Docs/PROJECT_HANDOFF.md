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

The project is a short 5–8 minute demo with four scenes: `00_StartMenu` → `01_Home` → `02_Harbor` → `03_EndCard`. Core mechanic: switch between Present and Memory world layers with the lantern. Keep work within the accepted scope in the shared plan. No oil, checkpoints, enemies, tides, multiple endings, or save system.

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

On 2026-11-03, the four scenes were renamed inside Unity with their GUIDs preserved: `00_StartMenu`, `01_Home`, `02_Harbor`, `03_EndCard`. Build Settings list them in that enabled order. Script routes now target `01_Home` and `00_StartMenu`; Harbor and EndCard visible labels were updated to match, while Home remains `01_Home`. The Start Menu now uses imported OGG hover, confirm, and back cues plus looping BGM and sea/wind ambience with fades. Play Mode invoked the Play button, waited for the confirm cue and fade, then reached `01_Home`; an Input System Escape event returned to `00_StartMenu`. Clip references and source playback/fade state were observed in Editor, but physical speaker listening, the 80-second loop point, and standalone build were not verified. The scene was saved in Editor after wiring. After the requested user Ctrl+S confirmation, commits `1de8084` (scene renumbering) and `be85569` (menu audio) were pushed to GitHub `main`.
## Existing local records

- `Docs/2026-11-03_Unity_Init_MainMenu.md` — original initialization/menu request and acceptance details.
- `Docs/MENU_UNITY_HANDOFF.md` — menu-specific build and visual requirements.
- `Docs/PROJECT_EXPERIENCE.md` — reusable project workflow lessons and validation boundaries.

## Handoff update rule

Update this document when verified project state or workflow constraints change. Keep intended work, observed state, and validation evidence clearly distinct. Record the date and specific evidence; do not turn planned work into a completion claim.

On 2026-11-03, removed the `01_Home` placeholder title and `Esc → menu` helper from `01_Home` at the user's request because they were appearing over the Start Menu while both scenes were loaded additively in the Editor. Cleared only `PlaceholderLabel`, saved the scene, and verified its TMP text is empty and the scene is clean. The duplicate listener/EventSystem warnings in that screenshot came from both scenes being loaded at once; each scene separately has one camera listener and one EventSystem. Commit `7415565` was pushed to GitHub `main`.

Audio follow-up on 2026-11-03: serialized the two Play On Awake music-source volumes at zero so they begin silent before `MenuMusic` fades them to 0.6 / 0.45. Commit `6f1ee1d` (`Menu audio: start fade from silence`) is on GitHub `main`. A fresh Editor probe confirmed the fade reached both targets, but afterward reported both sources stopped with zero output samples. Settings Escape closed the panel and started the SFX source. Treat sustained BGM/ambience playback and speaker audibility as unverified pending a focused Editor/listening check; do not infer a playback fault or success from the source-volume values alone.

## 2026-11-08 — Home Ground collision alignment

- Updated the serialized `Ground` Transform in `Assets/_Project/Scenes/01_Home.unity` to position `(-1.91, -3.5225, 0)`. Its `BoxCollider2D` remains offset `(0, 0)`, size `(25.2, 1)`, and Transform scale `(0.645, 0.645, 1)`.
- Resulting world bounds are X `[-10.037, 6.217]` and Y `[-3.845, -3.200]` (center `(-1.91, -3.5225)`, extents `(8.127, 0.3225)`). The top collision surface is at Y `-3.2`; the horizontal span now follows the Home ground/background layout.
- The `Crate` collider was also adjusted: Transform position `(-1.5, -2.43, 0)`, scale `(1.4, 1.4, 1)`, `BoxCollider2D` offset `(0, -0.016816527)`, and size `(1.3, 1.0663669)`. World bounds are X `[-2.41, -0.59]`, Y `[-3.2, -1.707086]`; its bottom meets the Ground top at Y `-3.2`.
- Unity MCP readback confirmed the active scene is `Assets/_Project/Scenes/01_Home.unity`, Edit Mode, scene clean, and the Ground and Crate collider values are saved. No Play Mode collision test was run for these adjustments.

## 2026-11-08 — Part 1: Core scene and additive Home loading

- `Assets/_Project/Scenes/Core_Persistent.unity` now owns the existing Player root and nested Maela prefab, Main Camera/AudioListener, Cinemachine camera/confiner, global Light2D, EventSystem, BackToMenu and existing Canvas. It also owns the new AudioManager, GameState and LevelLoader. These objects remain in Core while gameplay levels load additively; they do not use DontDestroyOnLoad.
- `01_Home` contains only Background, Ground, WallL/WallR, Crate, CameraBounds, LevelInfo/LevelBootstrap and Spawn_Start. The prior geometry/colliders are unchanged. Spawn_Start is `(-8, -3.20)`; LevelInfo keeps ortho `3.45`, the existing bounds and light, Home ambience volume `0.5`, and a two-second audio transition. Null music stops the prior music unless LevelInfo explicitly requests keeping it.
- Flow is `00_StartMenu` -> `00b_Prologue` -> Core (single) -> Home (additive and active). Prologue playback clip, position and volume transfer into Core AudioManager's two music/two ambience voices, replacing the old PrologueAudio DontDestroyOnLoad object. Escape in Home loads the menu as a single scene, unloading both Home and Core. Build Settings include Core; Harbor remains excluded.
- Prologue received one EventSystem with the existing Input System UI module to satisfy the plan's requirement for one EventSystem throughout the route; its storyboard content/layout is unchanged.
- Verified in Editor Play Mode: the complete menu/prologue/Home/Escape/menu route, 6,846 samples with exactly one enabled AudioListener and EventSystem, ground rest, both wall contacts, actual jump and landing on Crate, Core teardown, and direct-open Home bootstrap. The final bounded route capture and bootstrap capture contained zero project warnings/errors. Home rest was Player `(-8, -3.19502)`, camera `(-3.90703, -0.88240, -10)`, ortho `3.45`; the small floor offset is the existing physics contact separation.
- Delivery: `\\haoxi-surfacep\Administrator\Documents\MyFlie\AsArtPortfolioTeaching\Game_York\SharedWorkFolder\ForDesignScreenshot\2026-11-08_CoreSplit\Home_Rest_after.png` (verified 1920x1080). This is a runtime camera render with the existing overlay Canvas temporarily rendered through that camera; target texture and Canvas settings were restored immediately. Compared against the provided cropped Home_Rest reference and the unchanged checkpoint camera/geometry.
- Limits: Editor-driven Input System events and runtime playback-state/sample/volume observations; no physical keyboard/speaker listening or standalone build validation. An initial temporary test probe generated a null-clip timeSamples warning, corrected before the passing rerun; an initial Unity account API warning was unrelated to project runtime code. Console clearing by reflection was rejected by MCP, so the accepted rerun used a bounded Application.logMessageReceived subscription that was detached afterward. Temporary test assets were removed; fonts, packages, art, AGENTS.md and Parts 2-4 were excluded.

## 2026-11-08 — Maela v1.6_m PSB and draw order

- Replaced `Assets/_Project/Art/Characters/Maela/Maela.psb` with the shared v1.6_m source (SHA256 `3D171F9A96C9C5C0DCCA0683A07F3E6D9E2496D714F3608C1FD2BE8DB26ADD67`) and used the supplied importer metadata, which changes the `Leg_Upper_L` rect width from 26 to 33 px. Other existing importer settings and stable layer IDs were preserved; the supplied PSB layer order was carried through.
- `Check Maela Sprite Rects` passed for all 17 layers and the prefab has zero missing sprite references. Ran `Realign Maela Sprites` once; `Leg_Upper_L_Sprite` moved +3.5 px / 200 PPU = +0.0175 local X. Other sprite transforms were unchanged.
- Updated prefab renderer sorting only: Leg_Lower_L 6, Boot_R 7, Boot_L 8, Leg_Upper_R 9, Leg_Lower_R 10. `MaelaAnimBuilder.cs` does not set sorting order. Reviewed magenta captures for Rest, Run, JumpUp, Fall and Lantern; no visible boot/leg gaps, overlaps or seams appeared. In Play Mode, all five Animator states were reached directly, all 17 sprites remained valid, and the specified orders read back correctly. Exited without saving runtime state.
- Screenshots: `ForDesignScreenshot/2026-11-08_Maela_v1.6m/RestPose_Magenta.png`, `Run.png`, `JumpUp.png`, `Fall.png`, and `Lantern.png` (600×600 each). The camera, renderer enabled states, Animator, PlayerController and CinemachineBrain were restored after capture.

## 2026-11-08 — Maela Footstep v4

- Imported `SFX_Maela_Step_Wood_01_v4.ogg` through `04_v4.ogg` from the shared TechArtAsset folder. Their Unity importer settings match the existing v1 files except for unique asset GUIDs: Decompress On Load, Vorbis quality 0.7, preserve sample rate, stereo, and the same preload/background/ambisonic flags.
- Updated only the `MaelaAudio.footstepClips` array in `Core_Persistent` to the four v4 clips in order. A Unity readback confirmed all four references, no nulls, and a clean saved scene. The original v1 files remain in place.
- Source/imported hashes: 01 `A61F4BAE4B700F9203D5DB8C442A971E5AA29B228FA58DC36D7983B151F6C461`; 02 `1C830B8A2F4DD725969A0BD8284EDD18EA8B5D0EA4A1AD7AD4DC5651BE313D06`; 03 `4318991F44412E2FC6D6B75DF03B6BDD541B83F0B3027BF63805788E2B73EE92`; 04 `0D13A41C4D2AA2ACAE5170AC4042321116F7506E33A3DED3BA298CBA7C688D89`.
