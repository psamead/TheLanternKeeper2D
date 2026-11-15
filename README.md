# The Lantern Keeper 2D

A hand-painted Unity 6.3 LTS demo: Start Menu → Prologue → Home and attic → EndCard.

## Play

Windows and Web release verification is in progress. Confirmed download paths and a tested Pages link will be recorded here after delivery.

Controls: A/D or arrows to run, Shift to walk, Space/W/Up to jump, T to pick up the lantern, E to raise it and switch worlds after exploration, Esc to skip a story or return to the menu. Both EndCard options return to the Start Menu. Start Menu Quit keeps its platform behavior.

## Build

Use Unity **6000.3.24f1** with Windows x64 and Web Build Support. `DemoReleaseBuilder` exports non Development builds outside Assets, using only Start Menu, Prologue, Core, Home and EndCard. Harbor is excluded.

- `Tools > Lantern Keeper > Release > Build Windows x64`
- For Web, configure gzip with decompression fallback and threads disabled, switch the active build target to Web, then use `Tools > Lantern Keeper > Release > Build Web`.
- Build reports and packages go under `Builds/FinalDemo/`, which is ignored by Git.

See [the release record](Docs/2026-11-15_Attic_EndCard_Release.md) and [project handoff](Docs/PROJECT_HANDOFF.md) for evidence, limits and source provenance.
