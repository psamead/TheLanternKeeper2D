# Attic baseline, EndCard and platform delivery — 2026-11-15

Task: LAN `CommandForAgentToInitialRun/2026-11-15_Unity_AtticBaseline_EndCard_Windows_Web.md`.
Starting commit: `122e0132cd66b99148211aafa5fff7b485b61fda` (implementation baseline `a803050`).

## Implementation

- Lift the continuous attic floor top and landing anchor from Y 2.725 to 3.295 (+0.57). Window trigger center rises from 3.25 to 3.82. Existing Ground walls already span this height; background, downstairs geometry and camera settings are unchanged.
- Runtime comparison at the same anchor, facing and camera: camera `(5.49245,5.672,-10)`, orthographic size 3.4. Idle boot minimum viewport Y rises from .06632 to .15015 (93.368% to 84.985% from the top). Grounded collider feet/root change from 2.72999 to 3.29999. Normal contact separation accounts for .005 above the physical floor.
- EndCard opts into preserving the final storyboard frame. Natural B5 completion retains the final camera UV rectangle; finishing while already on B5 retains its current pose. Skip from earlier pages selects B5 once. The black transition remains for Prologue and Story A.
- EndCard Back to Menu and Quit share the guarded reset/load path. A release barrier prevents the advance/skip key or mouse press from activating newly shown options. Start Menu Quit retains its original platform behavior.

| Measured quantity | Before | After |
|---|---:|---:|
| Physical floor top / landing anchor Y | 2.725 | 3.295 |
| Grounded collider feet / player root Y | 2.72999 | 3.29999 |
| Visible right-boot bounds minimum Y, approximately | 2.723 | 3.293 |
| Boot line from viewport top | 93.368% | 84.985% |
| Window trigger center Y | 3.25 | 3.82 |

Boot world minima are reconstructed from the logged viewport coordinates and the measured camera, rounded to .001. The camera and floor measurements are runtime/physics values; screenshot window pixels were not used as world units.

## Validation and delivery status

Editor edge suite, complete natural route, page advances and four EndCard option cases passed. Both mouse and Enter activate each option, both return to Start Menu, and new Home sessions reset lantern, world and exploration state. Each returned menu has exactly one AudioListener and EventSystem. Held Enter/Esc cannot activate a newly shown option.

Before/after 1920×1080 screenshots and measured logs are under shared `ForDesignScreenshot/2026-11-15_AtticBaseline_EndCard_Release/after/`.
`B5_ContinuousFinish.mp4` is a 16.3333-second, 490-frame, 1920×1080 H.264/AAC short clip; hash `0017BC9049702C1D84117CF88A2325A9D4885DF0E02B0F7CFA77BA68503748E3`. Reviewed the contact sheet around completion: the caption/art remain while the input hint becomes options. All 491 sampled B5 updates had black alpha zero, and the finish UV rectangle remained unchanged. Music/wind state matched .60/.40 and retained one music voice.

Editor tests used injected Input System events. Subjective listening and owner visual approval remain pending. Development probes generated a transient MenuAudio exception when scripts reloaded during the initial measurement; fresh Play Mode regression runs completed without this issue.

Release export tooling selects the five required scenes explicitly and writes a BuildReport. Web uses a responsive 16:9 template, gzip with decompression fallback, and no threads. Windows, Web, Pages and owner acceptance will be reported separately below.

## Windows delivery

Built an isolated `git archive` export of `6adac6ca9d5532c9f646defca38c94255e55070c`; no workstation-only font, package or EndCard normalization edits were included. Source commit pushed and remote main verified equal. Unity 6000.3.24f1, Windows x86_64, Mono2x, non Development; 5 explicit scenes. BuildReport: succeeded, 0 errors, 486 warnings (primarily inference-package shader warnings and existing TMP API deprecation), 1m54s build phase after initial import.

Build preprocessing in the isolated export clears the two dynamic font caches (`m_ClearDynamicDataOnBuild=1`, with source-font references retained), adds Input System actions to preloaded assets, and serializes target batching defaults. These are generated Unity build changes, not imported workstation edits. The archived `build_source_check.json` records actual hashes; gameplay scripts/scenes and package manifest match the commit after normalizing Git's CRLF export conversion.

- Complete player directory: `\\haoxi-surfacep\Administrator\Documents\MyFlie\AsArtPortfolioTeaching\Game_York\SharedWorkFolder\DemoBuilds\2026-11-15_FinalDemo\Windows\`
- ZIP: `\\haoxi-surfacep\Administrator\Documents\MyFlie\AsArtPortfolioTeaching\Game_York\SharedWorkFolder\DemoBuilds\2026-11-15_FinalDemo\TheLanternKeeper2D_Windows_x64.zip`
- ZIP size: 153,525,239 bytes; directory: 370,664,586 bytes including BuildReport/source record. ZIP integrity passed for all 209 files, including EXE, matching data directory, UnityPlayer.dll and MonoBleedingEdge.
- SHA256: `6cff576047ffed445aa1b3daf304c69c11e5c7ba2fb82681f1f10d20e57441d2`.
- Extract the whole ZIP, then run `Windows/TheLanternKeeper2D.exe`; keep all neighboring runtime files together.

Actual EXE startup was checked, including Direct3D 12 and an additional windowed Direct3D 11 launch. Both initialized graphics, Mono and input without a runtime exception in the launch log. The native screenshot helper repeatedly failed with `FrameArrived timed out` / `window capture timed out`, including windowed retries. Therefore Windows gameplay, both EndCard return paths, reset behavior, visual output and listening are **not verified**. Test processes were stopped after collecting logs; this was not an in-game Quit test. Logs and the exact limitation are archived in `after/_work/Windows_runtime_status.txt` and `Windows_Player*.log`.

Supplemental Editor slow walk passed: Shift+D displacement .58835 over .75 seconds, grounded/IsWalking true at Y 3.30. Temporary check archived and removed.

## Hosting references checked

- [Unity 6000.3 decompression fallback](https://docs.unity.com/en-us/engine/6000.3/script-reference/unityeditor/playersettings/webgl/decompressionfallback): supports compressed files when the host cannot set Content-Encoding.
- [GitHub Pages deployment workflows](https://docs.github.com/en/pages/getting-started-with-github-pages/using-custom-workflows-with-github-pages) and [Pages limits](https://docs.github.com/en/pages/getting-started-with-github-pages/github-pages-limits): deployment method and published-site size must be checked separately from whether the game plays.

## Web build and deployment

Web build from the same isolated source commit succeeded: IL2CPP, non Development, 0 errors, 346 warnings, 9m47.415s build phase. The generated site contains five files totaling 127,268,811 bytes. The data file is 113,314,885 bytes, so the build is stored as a release asset rather than a Git blob. Gzip with Unity decompression fallback works without custom Content-Encoding; threads are disabled. The template uses relative Build paths and a responsive 16:9 canvas.

- Shared unpacked site: `\\haoxi-surfacep\Administrator\Documents\MyFlie\AsArtPortfolioTeaching\Game_York\SharedWorkFolder\DemoBuilds\2026-11-15_FinalDemo\Web\`
- Shared ZIP alongside it: `web-pages.zip`, 126,633,577 bytes, five files, ZIP CRC and copied-file hash verified.
- SHA256: `d38f1053f570755b394e0fe3311c9afffdd0adf6f5b1df12e1c33fb2e4d48206`.
- [Web build release](https://github.com/psamead/TheLanternKeeper2D/releases/tag/web-2026-11-15).
- [Pages deployment run](https://github.com/psamead/TheLanternKeeper2D/actions/runs/43675728069) succeeded from workflow commit `1fb2df14a6c4ca316690375fc2449725c2c978f0` using the release asset built from `6adac6ca9d5532c9f646defca38c94255e55070c`. The workflow commit adds deployment configuration only; it does not change game code or assets.
- Pages configured with workflow builds and HTTPS enforced. No existing Pages site was replaced. Repository API was used because the connector did not support the Pages endpoint.

Local HTTP browser route passed: menu, Prologue skip, pre-pickup E, T/Story A, exploration, jump over crate, E, full bell/hint handoff, grounded raised attic, window, Story B skip to B5 and mouse Quit back to menu. Screenshots and browser logs are archived with this release. Browser input used Playwright keyboard/mouse events, not physical hardware.

## Published browser validation and final status

Actual HTTPS URL verified: https://psamead.github.io/TheLanternKeeper2D/ in Chrome 156 on Windows, 1280x720. A complete route passed through natural Prologue, natural Story A, exploration, crate jump, E/bell, raised attic, window and natural Story B. A second route exercised advance/Skip and held Esc at B5. Browser runs used normal keyboard/mouse controls, with no game-state injections or teleports.

- Mouse Back to Menu and keyboard ArrowDown/Enter on Quit both returned to Start Menu. Restarting after each showed the uncollected ground lantern, empty hand, present world and no earned exploration hints; pre-pickup E had no visible effect. Local HTTP also passed mouse Quit. Editor separately passed all four mouse/keyboard option cases.
- Run, Shift slow walk, Space jump, T pickup, E switch, keyboard focus and both attic worlds worked. Physical keyboard hardware was not separately tested.
- Twenty B5 screenshots spanning natural completion show retained art/caption and directly appearing options, with no black sample (mean luma 99.317–101.606). These are sampled screenshots, not an every-frame browser audit; the separate Editor MP4 and 491-update log provide continuous finish evidence.
- Refresh from Home restarted at Start Menu. Relative project Build URLs loaded framework/WASM/data; refresh reused the data cache. Canvas focus was `unity-canvas`, with an observed running AudioContext after interaction and refresh. This establishes audio activation, not subjective listening approval.
- Logs retain favicon 404s, WebGL internal-format capability warnings, App UI default-settings/unused-shader messages and transient sound-length-before-load messages. No gameplay exception was observed in the completed routes; a clean Console is not claimed.
- Deployment ID 7798352032 succeeded; [deployment job](https://github.com/psamead/TheLanternKeeper2D/actions/runs/43675728069/job/132695338869). API evidence is archived in `_work/pages-status.json` and `_work/pages-environment.json`.

| Deliverable/check | Result |
|---|---|
| Three requested changes and Editor regression | Implemented and passed |
| Windows complete package/ZIP | Built, copied, integrity/hash verified |
| Real Windows EXE | Startup passed; gameplay/EndCard/reset/visual/listening unverified because native capture timed out |
| Local Web HTTP | Route through B5 and Quit return passed |
| Pages deployment | Successful workflow and deployment |
| Actual HTTPS game | Natural menu-to-B5, both returns, reset, advance/Skip and refresh passed |
| Owner visual/audio acceptance | Pending |

Final Editor check: exact project dataPath, Edit Mode, not compiling, one clean `00_StartMenu`, zero PlayerController/GameState runtime objects. Temporary validation scripts removed. Editor 已回到只开 00_StartMenu。

Test browser and HTTP server stopped. All 58 capture files have matching shared copies, recorded in `_work/capture_copy_hashes.json`. Automatic approval review blocked deletion of local duplicate captures without a more specific reason. They remain at `C:\Users\Administrator\Documents\UnityProject\TheLanternKeeper2D\_CodexCaptures\2026-11-15_AtticBaseline_EndCard_Release\`, ignored by Git. Final evidence is on the shared `ForDesignScreenshot/2026-11-15_AtticBaseline_EndCard_Release/after/` path.

Implementation and deployment commits are pushed. The final documentation commit changes only records/README, so both packages still correspond to game source 6adac6ca9d5532c9f646defca38c94255e55070c. Final local/remote hash verification is archived in `_work/final_git_verification.txt`. Unrelated AGENTS/TMP/fonts/EndCard scene/package edits remain unstaged.
