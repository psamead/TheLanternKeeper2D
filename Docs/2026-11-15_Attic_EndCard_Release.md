# Attic baseline, EndCard and platform delivery — 2026-11-15

Task: LAN `CommandForAgentToInitialRun/2026-11-15_Unity_AtticBaseline_EndCard_Windows_Web.md`.
Starting commit: `122e0132cd66b99148211aafa5fff7b485b61fda` (implementation baseline `a803050`).

## Implementation

- Lift the continuous attic floor top and landing anchor from Y 2.725 to 3.295 (+0.57). Window trigger center rises from 3.25 to 3.82. Existing Ground walls already span this height; background, downstairs geometry and camera settings are unchanged.
- Runtime comparison at the same anchor, facing and camera: camera `(5.49245,5.672,-10)`, orthographic size 3.4. Idle boot minimum viewport Y rises from .06632 to .15015 (93.368% to 84.985% from the top). Grounded collider feet/root change from 2.72999 to 3.29999. Normal contact separation accounts for .005 above the physical floor.
- EndCard opts into preserving the final storyboard frame. Natural B5 completion retains the final camera UV rectangle; finishing while already on B5 retains its current pose. Skip from earlier pages selects B5 once. The black transition remains for Prologue and Story A.
- EndCard Back to Menu and Quit share the guarded reset/load path. A release barrier prevents the advance/skip key or mouse press from activating newly shown options. Start Menu Quit retains its original platform behavior.

## Validation and delivery status

Editor edge suite, complete natural route, page advances and four EndCard option cases passed. Both mouse and Enter activate each option, both return to Start Menu, and new Home sessions reset lantern, world and exploration state. Each returned menu has exactly one AudioListener and EventSystem. Held Enter/Esc cannot activate a newly shown option.

Before/after 1920×1080 screenshots and measured logs are under shared `ForDesignScreenshot/2026-11-15_AtticBaseline_EndCard_Release/after/`.
`B5_ContinuousFinish.mp4` is a 16.3333-second, 490-frame, 1920×1080 H.264/AAC short clip; hash `0017BC9049702C1D84117CF88A2325A9D4885DF0E02B0F7CFA77BA68503748E3`. Reviewed the contact sheet around completion: the caption/art remain while the input hint becomes options. All 491 sampled B5 updates had black alpha zero, and the finish UV rectangle remained unchanged. Music/wind state matched .60/.40 and retained one music voice.

Editor tests used injected Input System events. Subjective listening and owner visual approval remain pending. Development probes generated a transient MenuAudio exception when scripts reloaded during the initial measurement; fresh Play Mode regression runs completed without this issue.

Release export tooling selects the five required scenes explicitly and writes a BuildReport. Web uses a responsive 16:9 template, gzip with decompression fallback, and no threads. Windows, Web, Pages and owner acceptance will be reported separately below.
