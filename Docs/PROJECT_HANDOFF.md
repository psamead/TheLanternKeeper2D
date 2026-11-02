# The Lantern Keeper 2D — Agent Handoff

Last reviewed: 2026-11-03

## Project location and state

- Unity project: `C:\Users\Administrator\Documents\UnityProject\TheLanternKeeper2D`
- Shared design folder: `\\haoxi-surfacep\Administrator\Documents\MyFlie\AsArtPortfolioTeaching\Game_York\SharedWorkFolder`
- Unity version: 6000.3.24f1; URP 2D; Windows PC; new Input System.
- Git repository is initialized and connected to GitHub. Latest observed commit: `e0e9911 Add Unity project files`.
- At this handoff, `AGENTS.md`, `Assets/_Project/`, and `Docs/` are untracked. Inspect `git status --short` before editing or committing; do not discard or overwrite them.
- No Play Mode validation, commit, or push is recorded for the 2026-11-03 initialization/menu task. Do not infer completion from the plan.

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

## Existing local records

- `Docs/2026-11-03_Unity_Init_MainMenu.md` — original initialization/menu request and acceptance details.
- `Docs/MENU_UNITY_HANDOFF.md` — menu-specific build and visual requirements.
- `Docs/PROJECT_EXPERIENCE.md` — reusable project workflow lessons and validation boundaries.

## Handoff update rule

Update this document when verified project state or workflow constraints change. Keep intended work, observed state, and validation evidence clearly distinct. Record the date and specific evidence; do not turn planned work into a completion claim.
