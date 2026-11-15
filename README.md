# The Lantern Keeper 2D

A hand-painted Unity 6.3 LTS demo: Start Menu → Prologue → Home and attic → EndCard.

## Play

[Play the Web demo](https://psamead.github.io/TheLanternKeeper2D/) — verified in Chrome through B5, both EndCard menu returns, replay reset and refresh. Click the game to activate sound/input. Subjective listening remains pending.

Windows ZIP on the shared LAN: `\\haoxi-surfacep\Administrator\Documents\MyFlie\AsArtPortfolioTeaching\Game_York\SharedWorkFolder\DemoBuilds\2026-11-15_FinalDemo\TheLanternKeeper2D_Windows_x64.zip`. Extract the entire ZIP and run `Windows/TheLanternKeeper2D.exe`. Build, package integrity and actual startup passed; Windows gameplay/EndCard controls need manual verification because native window capture failed.

[Web export download](https://github.com/psamead/TheLanternKeeper2D/releases/tag/web-2026-11-15). Both packages use game source `6adac6ca9d5532c9f646defca38c94255e55070c`.

Controls: A/D or arrows to run, Shift to walk, Space/W/Up to jump, T to pick up the lantern, E to raise it and switch worlds after exploration, Esc to skip a story or return to the menu. Both EndCard options return to the Start Menu. Start Menu Quit keeps its platform behavior.

## Build

Use Unity **6000.3.24f1** with Windows x64 and Web Build Support. `DemoReleaseBuilder` exports non Development builds outside Assets, using only Start Menu, Prologue, Core, Home and EndCard. Harbor is excluded.

- `Tools > Lantern Keeper > Release > Build Windows x64`
- For Web, configure gzip with decompression fallback and threads disabled, switch the active build target to Web, then use `Tools > Lantern Keeper > Release > Build Web`.
- Build reports and packages go under `Builds/FinalDemo/`, which is ignored by Git.

See [the release record](Docs/2026-11-15_Attic_EndCard_Release.md) and [project handoff](Docs/PROJECT_HANDOFF.md) for evidence, limits and source provenance.

## Publish an updated Web build

Build locally and ZIP the Web output with `index.html` and `Build/` at the ZIP root. Upload it as `web-pages.zip` to a GitHub release tagged at the build source commit, recording its SHA256. Run **Deploy Unity Web demo** in Actions with that release tag, then verify the actual HTTPS game. The workflow publishes a Pages artifact; binaries stay outside main source history.
