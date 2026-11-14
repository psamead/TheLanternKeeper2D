# Pickup gate, story audio and attic safety — 2026-11-15 (round 2)

Source: LAN `CommandForAgentToInitialRun/2026-11-15_Unity_PickupGate_StoryAudio_Attic.md`.
Baseline: `c7ba7f23627ce072206ac7e68b019f42a85228fa`.
This supersedes the earlier permission to explore before pickup and the immediate hint disappearance on E.

## Implementation

- State and physical-entry gates require HasLantern AND StoryACompleted. No queued exploration; one fresh occupied-zone entry advances one stage, with the existing same-frame guard. A completion never grants a stage.
- A retains its four pages and captions. Its ending fades black once, then runs a return coroutine while StoryboardPlayer still blocks gameplay. All story graphics except black are disabled together, Home is revealed, and only then is control unlocked and completion recorded. Esc and individual advances use the same return. A one-frame gameplay-input barrier also prevents exit-frame input leakage.
- `ReturnAfterStoryA` is a Home scene anchor at (-4.7, -3.2, 0), outside discovery x[-3.6,-2.4]. Placement measures the player's physical collider feet and raycasts actual Ground support; it clears velocity and jump/coyote buffers and updates grounded animation before reveal. Facing and held lantern are retained.
- First accepted E consumes the input immediately. Both fixed hints remain visible through the full bell. At Memory change the two exploration components/colliders retire and clear occupancy; hint alpha fades over 0.5 s. The ladder trigger is separately owned and waits for tutorial exit AND its renderer to reach full alpha. An overlap query recognizes a waiting player and multiple colliders without requiring re-entry. Subsequent world switches cannot revive retired exploration zones.
- Attic landing anchor: (4.7, 2.725, 0); actual root/feet are measured at runtime, not inferred from screenshot pixels. The former x[2.2,3.4] floor hole is covered by a single continuous collider spanning x[1.8,10.2], surface y=2.725. Both side walls and roof colliders use Ground, shared by both worlds. Art, current backgrounds, camera framing and EndCard placement are preserved. Attic camera framing latches after the one-way transition.
- Unsupported landings keep the black overlay and control lock, report an error and never fall through to reveal. Normal placement synchronizes Rigidbody/Transform/physics, checks grounding again after a physics step, then reveals.
- Page audio lives in `StoryboardData.Frame.audioCue`. Core music changes gain on the existing voice/clip/sample position; A no longer starts a competing whole-sequence duck. B restores its music handoff and applies page cues with one local ambience source and one cancellable fade. B5 keeps music and wind until leaving EndCard.
- Independent wave and wind stems were generated from existing Menu helpers, archived with WAV masters, OGG deliverables and reproducible source in shared `TechArtAsset/Audio/Storyboard/`. No new music. Source/parameters: that directory's README. Story A4 stops the wave voice after its fade; Home restores its own ambience.

## Audio cue table

| Page/segment | Music gain | Independent ambience | Gain | Fade | Stop behavior |
|---|---:|---|---:|---:|---|
| A1 | .25 → .60 | Existing Home interior | .18 | 1 s | No new sea stem |
| A2 | .60 | Existing Home interior | .18 | 1 s | Retain current clip |
| A3 | .38 | Story Waves v1 | .55 | 1 s | Crossfade interior out |
| A4 / skip target | .38 | None | 0 | 1 s | Wave voice stops |
| Home return | .26 | Existing Home interior | .50 | 1 s | No story waves |
| First bell | Home .26 × existing .48 duck multiplier | Home interior | .50 | Existing .25 s duck ramps | Existing duck release timing retained |
| B1–B4 | .60 | Story Wind v1 | .40 | 1 s | Continuous music/wind |
| B5 natural or skip | .60 | Story Wind v1 | .40 | 1 s | Keep both until scene exit |

The original Home interior ambience contains distant mixed elements; it is not described as a pure wind stem. The new wind stem uses no wave/foam helper. Cue/playback observations and file non-silence do not establish subjective mix quality. Owner listening remains required.

## Validation and delivery

- `edge_cases_checks.txt`: PASS. Actual repeated movement through both zones before pickup + E; story-time gate; Esc return; held E across A exit and exploration unlock; out-of-order and multiple child colliders; fresh re-entry; full bell/hint handoff; waiting-in-ladder entry once; left/right running and jumping; Present/Memory floor support; spent zones remain disabled; menu reset; individual page advances return grounded with stage zero.
- `continuous_route_checks.txt`: PASS. Normal Menu → natural P → Home → T → natural A → door → discovery → ladder inspection → E → attic → window → natural B5 with eight-second dwell. Real Input System events, Time.timeScale=1, no test teleports or forced progress in this video. Natural and skipped B5 each retain exactly one ArMorDu voice at .60 and wind at .40. Back to Menu works on both paths.
- `_work/supplemental_checks.txt`: injected isolated state test confirms the same-frame second exploration step is rejected. Unsupported anchor placement returns false without moving the player; the actual ladder coroutine with that deliberately invalid anchor remains at black alpha 1 with controls locked and the player downstairs. Runtime-only fixture discarded.
- `_work/frame_audit.csv`: 1,801 consecutive runtime frames, no frame-number gaps. A images disabled at Unity time 142.4481 under black alpha 1. First Home reveal at 142.5148: root (-4.7000,-3.1958), grounded and locked. First attic reveal at 157.4815: root (4.7000,2.7288), grounded and locked. All 44 samples across the two reveal windows remain grounded. These root values include normal contact separation from surfaces -3.2 and 2.725.
- `FRAME_REVIEW.md`: reviewed all 92 consecutive full-resolution frames covering the final two seconds of A4 and first second of Home. A4 retains its caption into its single fade to black; Home is the next visible scene. No A4 reappearance, old prompt or extra image layer. Also reviewed every frame in the attic reveal excerpt; the first visible pose already stands on support.
- `_work/audio_cue_observations.txt`: actual page observations match the cue table. A4 wave source is stopped (not merely volume zero); B1–B5 music and wind continue, with normal ArMorDu loop wrap. Home restores .26 music/.50 interior ambience. The existing first-bell duck is retained. No competing whole-A duck or B5 Stop remains.
- `PickupStory_ContinuousRoute.mp4`: 133.666633 s, H.264 1920×1080, 30 fps, 4,010 frames, AAC stereo 48 kHz, 121,478,915 bytes. Recorded simulation interval 133.64 s matches file duration; no acceleration/retiming. Entire file decoded successfully. Audio mean -21.9 dB, peak -5.2 dB (non-silence, not listening acceptance).
- Local fallback/shared SHA256 match: `064DCF205B7D4BEBBA9A16B47463FA4EA77000D0FF2E73ED4D8C509E50BADA7F`. Final path: `\\haoxi-surfacep\Administrator\Documents\MyFlie\AsArtPortfolioTeaching\Game_York\SharedWorkFolder\ForDesignScreenshot\2026-11-15_Home_Pickup_Story_Attic\after\PickupStory_ContinuousRoute.mp4`. Verified local video removed; no capture files remain in this task's `_CodexCaptures` folder.
- All delivered standalone PNGs are 1920×1080. Intermediate contact sheets and all-frame extracts are under `after/_work/`. Temporary Editor driver and runtime frame probe were archived there and removed from Assets, including their metadata.
- No new product compile error or unexpected runtime error occurred in the passing runs. Console retains one corrected temporary-driver exception (world switch re-enabled the test-disabled window collider) and one deliberately induced unsupported-landing error. The boundary test was corrected to deactivate the window root, then the complete edge suite passed. The full route always uses the normal window trigger.
- Unrelated TMP/font, EndCard scene and package changes are excluded from the task commit. Current Home background, EndCard placement and original art sources remain unchanged.

## Review limits

Owner listening and final placement/art acceptance remain pending; runtime gains and a non-silent video do not prove subjective audio quality. Input was injected through Unity Input System, not physical hardware. No standalone build or standalone Quit validation was performed. Editor cleanup is checked separately before handoff: only `00_StartMenu`, no runtime or temporary test objects.

Final Editor verification: Edit Mode, not compiling, exactly one clean `00_StartMenu`, zero runtime Player/GameState objects, temporary test scripts absent. Editor 已回到只开 00_StartMenu。
