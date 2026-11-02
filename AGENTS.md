<!-- 草稿（2026-11-02）：复制到 Unity 项目根目录并改名为 AGENTS.md，供 Codex 每次对话自动读取。保持简短；细节放在下列文档里，需要时再读。 -->

# AGENTS.md — The Lantern Keeper（Unity Demo）

## 先读（按需，不要一次全读）
1. `SharedWorkFolder/handoff.md` — 项目速览、代码结构、约定
2. `SharedWorkFolder/DEMO_IMPLEMENTATION_PLAN.md` — 范围、机制、台词、实施清单（§7 打勾）
3. `SharedWorkFolder/TechnicalArt/HANDPAINTED_SPEC.md` — 分辨率、尺度、场景层级（§5）、导入设置（§6）、灰盒色块（§7）
4. 改设定前：`SharedWorkFolder/DESIGN_CHANGES.md`

> Unity 项目：`TheLanternKeeper2D`（Unity 6000.3.24f1，已连 GitHub；输入用新 Input System）。SharedWorkFolder 路径：\\haoxi-surfacep\Administrator\Documents\MyFlie\AsArtPortfolioTeaching\Game_York\SharedWorkFolder

## 固定规格
- Unity 6.3 LTS，URP 2D，PC（Windows）。游戏内文字只用英文。
- 1920×1080，PPU 100，正交相机 Size 5.4；**不用** Pixel Perfect Camera。
- 碰撞用 Tilemap：`Collision_Both / _Present / _Memory`（Grid Cell Size 1）；画面用分层美术（L0–L4）。
- 两层世界：`LanternSwitch` 发事件 `OnWorldSwitched(bool isMemory)`；物体挂 `WorldLayerObject`（Present / Memory / Both）；冷暖改 Global Light 2D（现在 #8FA3BF 0.8，记忆 #FFD9A0 1.0）。
- 角色：2D Animation + PSD Importer（`.psb` 骨骼）。美术未到位前用白色胶囊 0.8×1.6。

## 范围
- 只做 Demo：4 个 Scene（01_StartMenu → 02_Home → 03_Harbor → 04_EndCard），5–8 分钟。
- 不做：灯油、检查点、敌人、潮汐、多结局、存档。新增内容先对照 DEMO_IMPLEMENTATION_PLAN §2。

## 工作规则
- 一次只做实施清单里的一项；在 Play Mode 验证后才在 §7 打勾，并 git commit 一次。
- 文本放 ScriptableObject，不写死在脚本里。
- 美术资产只从 `SharedWorkFolder/TechArtAsset/` 拷入 `Assets/Art/`，不改源文件；美术源文件（.psd/.psb）不进 Git。
- 不读、不搜索：`Library/`、`Temp/`、`Logs/`、`obj/`、`Builds/`、`*.meta`；`.unity` / `.prefab` 文件只在必要时读相关片段。
- 报错时只看 Console 里相关的几行。

