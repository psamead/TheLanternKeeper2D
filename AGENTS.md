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
- 相机跟随用 **Cinemachine 3.1.7**（已安装）：Main Camera 挂 `CinemachineBrain`；每个场景一个 `CinemachineCamera`（Lens 正交 5.4）+ `CinemachinePositionComposer` + `CinemachineConfiner2D`。不要自己写跟随脚本。
- 碰撞用 Tilemap：`Collision_Both / _Present / _Memory`（Grid Cell Size 1）；画面用分层美术（L0–L4）。
- 两层世界：`LanternSwitch` 发事件 `OnWorldSwitched(bool isMemory)`；物体挂 `WorldLayerObject`（Present / Memory / Both）；冷暖改 Global Light 2D（现在 #8FA3BF 0.8，记忆 #FFD9A0 1.0）。
- 角色：2D Animation + PSD Importer（`.psb` 骨骼，PPU 200）；剪纸式 Transform 骨骼，动作由 `Assets/_Project/Editor/MaelaAnimBuilder.cs` 生成（数据源 `SharedWorkFolder/TechArtAsset/Characters/Maela/_work/anim_v2/poses.py`），不要手改 .anim。
- **场景结构（11-08）**：`Core_Persistent` 常驻（Maela、Main Camera = 唯一 AudioListener、Cinemachine + Confiner2D、Global Light 2D、EventSystem、`GameState` / `AudioManager` / `LevelLoader` 单例）；关卡（`01_Home` …）叠加加载，只放关卡内容 + `LevelInfo` + `SpawnPoint` + `LevelBootstrap`。菜单、序章、EndCard 是普通场景。不要再用 DontDestroyOnLoad 带音频。
- **Maela PSB 导入规则**：图层 ID / 名字 / 画布尺寸不能变；外框变大时按 Claude 准备的 patched meta（A 路）导入，跑 Tools ▸ Lantern Keeper ▸ **Check Maela Sprite Rects** 必须 PASS，再跑一次 **Realign Maela Sprites**；前后顺序只改 prefab 的 sortingOrder。
- 操作：A/D 或方向键移动（默认跑），Shift 慢走，Space/W 跳，E 举灯（规则见 DEMO_IMPLEMENTATION_PLAN 灯笼切换），Esc 回菜单。

## 范围
- 只做 Demo（2026-11-07 缩小）：00_StartMenu → 序章（故事版 P）→ 01_Home（含故事版 A）→ 03_EndCard（故事版 B），4–6 分钟。`02_Harbor` 留给完整版，不进 Build Settings。
- 不做：灯油、检查点、敌人、潮汐、多结局、存档。新增内容先对照 DEMO_IMPLEMENTATION_PLAN §2。

## 工作规则
- **录屏 / 截图 / 导出文件的存放位置（11-11 用户要求，必须遵守）**：一律放 `SharedWorkFolder/ForDesignScreenshot/YYYY-MM-DD_主题/`（中间文件放同目录下 `_work/` 子文件夹）。**不要**存到 Videos、Desktop、Downloads、Temp 或其他任何地方——用户找不到。
  - 写共享文件夹失败时（例如 Unity Recorder 不能直接写 UNC 网络路径），改存 Unity 工程根目录的 `TheLanternKeeper2D/_CodexCaptures/YYYY-MM-DD_主题/`（在 `Assets/` 外面；`_CodexCaptures/` 要写进 Unity 的 `.gitignore`，不进 Git），然后自己复制到上面的共享位置。
  - 汇报时写出文件的完整最终路径；如果有文件留在 `_CodexCaptures/`，也要说明。
- 一次只做实施清单里的一项；在 Play Mode 验证后才在 §7 打勾，并 git commit 一次。
- 文本放 ScriptableObject，不写死在脚本里。
- 美术资产只从 `SharedWorkFolder/TechArtAsset/` 拷入 `Assets/Art/`，不改源文件；美术源文件（.psd/.psb）不进 Git。
- 不读、不搜索：`Library/`、`Temp/`、`Logs/`、`obj/`、`Builds/`、`*.meta`；`.unity` / `.prefab` 文件只在必要时读相关片段。
- 报错时只看 Console 里相关的几行。

