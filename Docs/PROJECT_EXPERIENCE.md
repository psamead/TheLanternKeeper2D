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
