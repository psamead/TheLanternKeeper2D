# Task: Unity project setup + Start Menu v1 (2026-11-03)

Shared folder = `SharedWorkFolder` (if it is mapped to a drive letter on your computer, use that path). The Unity project is **`TheLanternKeeper2D`** (already created, already connected to GitHub, and you are already connected to Unity). Work inside it; do not create a new project.

## Step 0 — before anything else
Fill in the actual path you use to reach `SharedWorkFolder` in the empty slot in `AGENTS.md` (project root), replacing `<在 Unity 那台电脑上填写网络共享路径>`. Do the same in `SharedWorkFolder/UNITY_AGENTS.md`. From now on, always use this path to find the shared documents.

## Already done by Claude (2026-11-03, not yet committed — include them in your "Before" commit)
- Art and font are **already in the project**: `Assets/_Project/Art/Menu/` (MENU_L0_BG_1920, MENU_L1_left_dark, TITLE_FRAMED_v1, MENU_v1_mock = target image, do not use in the scene), `Assets/_Project/Art/UI/ICON_lantern_128.png` / `_64.png`, `Assets/_Project/Fonts/Cinzel-Variable.ttf` (+ OFL). Do not copy them again; only set import settings.
- `AGENTS.md` is already in the project root.
- Project facts: Unity 6000.3.24f1; 2D Animation / PSD Importer / Input System already installed; **Active Input Handling = Input System Package (new)** — use `UnityEngine.InputSystem` (Keyboard.current / Mouse.current or an InputAction), not the legacy `Input` class. Add `InputSystemUIInputModule` on the EventSystem.

## Read first (only these)
1. `SharedWorkFolder/UNITY_AGENTS.md` — rules (copy it into the Unity project root as `AGENTS.md` if it is not there yet).
2. `SharedWorkFolder/handoff.md` §1, §4, §5.
3. `SharedWorkFolder/TechnicalArt/MENU_UNITY_HANDOFF.md` — the menu spec.
4. Look at `SharedWorkFolder/TechArtAsset/Menu/MENU_v1_mock.png` — your result must match it.

## Rules
- First: check the existing Git repo has a Unity `.gitignore` (Library/, Temp/, Logs/, obj/, Builds/, UserSettings/); add the missing lines if needed. Commit "Before start menu v1" and push. Push again after the final commit.
- Unity 6.3 LTS, URP 2D. Do not install extra packages except TextMeshPro, 2D Animation and 2D PSD Importer.
- Never move, edit or delete files in the shared folder (except the two progress files in step 6).
- Do not read `Library/`, `Temp/`, `Logs/` or `*.meta` files. Do not run automated tests; the user tests by hand.

## Steps
1. Folders: `Assets/_Project/{Art/Menu, Art/UI, Fonts, Scenes, Scripts}`.
2. Create 4 scenes: `01_StartMenu`, `02_Home`, `03_Harbor`, `04_EndCard`, all in Build Settings in that order. `02`–`04` are empty placeholders with a TMP label showing the scene name and "Esc → menu".
3. Build `01_StartMenu` exactly as `MENU_UNITY_HANDOFF.md` §2–§4 (files already in the project — import settings in §3, create the Cinzel TMP font asset). The existing `Assets/Scenes/SampleScene` can stay untouched; remove it from Build Settings.
4. Scripts (small, commented): `MainMenuController` (keyboard + mouse selection, icon + underline follow the selected item, Play / Settings / Quit), `SceneLoader` (load by name), and a tiny `BackToMenu` (Esc in placeholder scenes → `01_StartMenu`).
5. Enter Play Mode, compare with `MENU_v1_mock.png`, fix positions if they differ. Commit "Start menu v1".
6. Report:
   - Create `SharedWorkFolder/TechnicalArt/MENU_UNITY_PROGRESS.md` (if the shared folder is not reachable from your computer, write it to `Docs/MENU_UNITY_PROGRESS.md` in the project instead and say so): what was done, what you checked in Play Mode, what is not verified, any differences from the mock.
   - Add one dated line to `SharedWorkFolder/ART_HANDOFF.md` §10 (edit the current file; do not overwrite other changes).
   - Tell the user the Unity project path and how to open the scene.
