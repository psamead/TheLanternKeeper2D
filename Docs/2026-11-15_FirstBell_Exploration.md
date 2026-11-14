# First bell exploration and EndCard position — 2026-11-15

Source: LAN `CommandForAgentToInitialRun/2026-11-15_Unity_FirstBell_Exploration_EndCard.md`.
Baseline: `a84fa6814ddd7f52693b1c6322c65fc88fcfd35f`.

## Implementation

- `GameState` owns session-only `Unexplored -> Discovered -> Inspected -> Used` progress and Story A completion. Menu reset and subsystem registration clear both. Nothing is written to disk.
- Two `BrokenLadderHintZone` instances use actual Player collider entry and per-zone occupancy sets. An out-of-order visit remains occupied until all Player colliders exit; another child entering cannot count as a new visit. Progress cannot advance twice in one frame. Disabled player, story playback, Memory, and player control locks reject progression.
- Discovery: center `(-3,-2.9)`, size `(1.2,1.3)`, world bounds x `[-3.6,-2.4]`, y `[-3.55,-2.25]`. Separate root `LanternDiscoveryZone` survives pickup.
- Ladder inspection: center `(2.8,-2.9)`, size `(1.6,1.3)`, world bounds x `[2,3.6]`, y `[-3.55,-2.25]`. Both zones are triggers, not solid obstacles.
- `FirstBellExplorationHints` displays two independent scene texts, using the existing `LanternFlowText` ScriptableObject and the current 2.15-point world text / rounded frame style. Ordinary Transform anchors prevent TMP layout from resetting their world locations: Press E `(-3,-0.65,0)`, Broken ladder `(2.8,-0.65,0)`.
- Step one persists the remote ladder hint; step two adds fixed E. Both survive exit and story completion, suppress during story/control locks, and hide on the first accepted bell. If exploration is completed before pickup, existing Press T is spaced 0.45 world units below E to avoid overlapping text.
- `RequestSwitch()` checks lantern ownership, Story A completion, exploration, active player, story and control locks before any side effect. Input remains edge-triggered with no deferred E. Existing first bell duration, movement lock, layer/light transition, and subsequent switching remain in place. The direct Player raise fallback uses the same readiness rule.
- `FirstBellSceneBuilder` is a focused repeatable authoring command. The older Home builder no longer configures the obsolete ladder text field.
- EndCard changes only the entire `EndCardButtons` RectTransform position, from `(470,120)` to `(670,-43)` relative to its left/mid-height anchor. Root screen center is `(670,497)` at 1920x1080; visible content is left of that root center. All labels, icon, underline, end dot, and button hit areas move together. Art, fonts, sizes, colors, and callbacks are retained.
- Background textures, colors, lantern v4, and ladder v5 were not changed. Existing unrelated EndCard/TMP/Packages edits are excluded from the task commit.

## Validation evidence

Evidence directory:
`\\haoxi-surfacep\Administrator\Documents\MyFlie\AsArtPortfolioTeaching\Game_York\SharedWorkFolder\ForDesignScreenshot\2026-11-15_Home_FirstBell_Exploration\after\`

- `edge_cases_checks.txt`: actual Input System key events plus physics placements for corner cases. Initial hidden hints/E rejection; non-Player rejection; ladder-first/multiple colliders; no retroactive count while a child remains inside; new reentry; exploration before pickup; no lantern denial; exact fixed anchors; A suppression/skip recovery; held E through A does not replay; first new E locks and switches once; duplicate request rejection; later switching; menu resets; natural A does not automatically unlock E; exploration after pickup.
- `continuous_route_checks.txt`: continuous normal Menu -> natural Prologue -> Home -> T -> natural A -> ordered exploration -> E -> Memory -> automatic attic fade -> attic window -> natural B5. Movement/jump/pickup/E use actual Input System events; this route does not teleport the player or force progression booleans. One first-bell world switch was counted. EndCard natural/skip root positions match; raycasts, actual mouse hover, Quit click (Editor only), Back click, keyboard selection and Enter all passed.
- `additional_edge_checks.txt`: one Player collider spanning both trigger zones advanced only one step; locked entry and remaining inside after unlock did not advance; both hints hid under the lock. Held E across actual second-zone entry did not execute when exploration became ready.
- All eleven PNGs are 1920x1080. Visual review confirmed separated anchored hints and EndCard left of the girl/lamp and below the caption. Back glyph bounds are x444.00-754.21, bottom-origin y501.68-558.29; Quit x444.00-546.94, y431.48-488.10. Visible menu group including icon is centered approximately x570, y585 in top-origin screen coordinates, close to the reference target (566,583).
- `FirstBell_ContinuousRoute.mp4`: 121.400 s, H.264 1920x1080, 30 fps, 3642 frames, AAC stereo 48 kHz, 113460809 bytes. FFmpeg decoded the complete file successfully. Audio mean -23.0 dB and peak -5.4 dB confirm non-silent audio; this is not an audible mix judgment.
- Local fallback and delivered shared video SHA256 matched: `45DBEDF20B1EB4FA90506DCC8D918CCFF590F3A78D55169AE79AC4AEA98BB613`. Recording used the permitted project `_CodexCaptures/2026-11-15_Home_FirstBell_Exploration/` fallback because UNC Recorder had previously failed. The verified copy is delivered on the share; the task's local video was then removed. No captures from this run remain locally.
- Temporary Editor validation/Recorder driver was archived in evidence `_work/FirstBellValidation.cs` and removed from Assets. No new project compile/runtime error was observed during the successful rerun; the Console retained the earlier corrected driver timeout. No package changes were included.
- Finished in Edit Mode with one clean `00_StartMenu`, no runtime Player/GameState or test objects. Editor 已回到只开 00_StartMenu.
- Initial screenshot review caught TMP resetting cloned text positions; dedicated anchor parents fixed it and the acceptance pass was repeated. First route driver reached EndCard, but its position wait incorrectly timed out after Player destruction; the driver was corrected and a new full recording requested. These were corrected validation/authoring issues, not unresolved gameplay failures.

## Review limits

Editor input is injected through Unity Input System; it is not physical keyboard/mouse hardware testing. Standalone build and standalone Quit are outside this task and remain untested. Audible mix quality and final placement/ladder art acceptance remain for the user to review in the delivered recording.

