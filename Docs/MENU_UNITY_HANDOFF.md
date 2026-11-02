# 开始菜单 → Unity 搭建交接（给 Codex / Claude）

> 2026-11-03。项目《守灯人 The Lantern Keeper》，2D 手绘（见 `TechnicalArt/HANDPAINTED_SPEC.md`），先做 PC 版。
> 先读：`handoff.md`（项目与 Demo 范围）、`DEMO_IMPLEMENTATION_PLAN.md`、`UNITY_AGENTS.md`（规则）。本文件只讲开始菜单。
> 视觉目标图：`TechArtAsset/Menu/MENU_v1_mock.png`（1920×1080）——Unity 结果应与它一致。

## 1. 素材（都在 `SharedWorkFolder/` 下，只复制，不改源文件）

| 用途 | 文件 | Unity 内位置 |
|---|---|---|
| 背景 | `TechArtAsset/Menu/MENU_L0_BG_1920.png`（1920×1080） | `Assets/_Project/Art/Menu/` |
| 左侧压暗 | `TechArtAsset/Menu/MENU_L1_left_dark.png`（透明渐变） | 同上 |
| 标题（含框） | `TechArtAsset/Menu/TITLE_FRAMED_v1.png`（透明底） | 同上 |
| 选中图标 | `TechArtAsset/UI/ICON_lantern_128.png`（另有 64 px） | `Assets/_Project/Art/UI/` |
| 按钮字体 | `TechnicalArt/fonts/Cinzel-Variable.ttf`（+ OFL_cinzel.txt） | `Assets/_Project/Fonts/` |

## 2. 布局（1920×1080 像素坐标，左上为原点；照 mock）

| 元素 | 位置 / 尺寸 |
|---|---|
| 背景 | 铺满 |
| 左侧压暗 | 铺满（图本身是从左往右的渐变） |
| 标题 | 左上角 x=100, y=70，宽 720（高按比例，约 439） |
| 按钮 Play / Settings / Quit | 左对齐 x=220，第一行 y≈570，行距 86；Cinzel 54 px |
| 选中图标 | 在选中文字左边，中心约 x=176，与文字垂直居中；显示约 40–48 px |
| 选中下划线 | 文字下方约 8 px，线宽 3 px，长度 = 文字宽，线尾一个 8 px 小圆点；暖金 |

## 3. 视觉规范

| 状态 | 颜色 | 效果 |
|---|---|---|
| 普通 | 米白 `#F2EBDD` | 无 |
| 选中 / 悬停 | 暖金 `#E8B45A` | 左侧小灯笼图标 + 金色下划线 |

- Canvas：Screen Space - Overlay；Canvas Scaler = Scale With Screen Size，Reference 1920×1080，Match 0.5。
- 按钮**不要**导出图片，用 Button + TextMeshPro（Cinzel 生成 TMP SDF 字体，只需英文字符集）。
- 导入设置：Sprite (2D and UI)，Filter = Bilinear，Mip Maps 关，Compression = None（菜单图都不压缩）。

## 4. 场景结构

```
Scene: 01_StartMenu
├─ Main Camera（正交，唯一 AudioListener）
└─ Canvas
    ├─ BG（Image，MENU_L0_BG_1920）
    ├─ LeftDark（Image，MENU_L1_left_dark）
    ├─ Title（Image，TITLE_FRAMED_v1，Preserve Aspect）
    └─ MenuButtons（Vertical Layout Group，左对齐）
        ├─ Play / Settings / Quit（Button + TMP）
        ├─ SelectionIcon（Image，跟随选中项）
        └─ SelectionUnderline（Image 或 UI 线，跟随选中项，宽度 = 文字宽）
```

- 交互：W/S 或 ↑/↓ 切换；鼠标悬停切换选中；Enter / 空格 / 左键确认。
- 功能：Play → 加载 `02_Home`（空场景占位即可）；Settings → 空面板占位（Esc 关闭）；Quit → `Application.Quit()`（编辑器里打印日志）。

## 5. 暂时不做

- 背景视差分层与动效（雾、灯笼呼吸光）——背景目前只有一整张。
- 菜单音乐、音效。
- Continue（Demo 无存档）。

## 6. 完成后请记录

- 在 `TechnicalArt/MENU_UNITY_PROGRESS.md`（新建）写：完成了什么、Play Mode 里实际看过哪些、还没验证什么。
- 在 `ART_HANDOFF.md` §10 加一行日期记录；在 `DEMO_IMPLEMENTATION_PLAN.md` §7 只给 Play Mode 验证过的项打 ☑。

## 7. 后续交互反馈更新（2026-11-03）

- 用户要求选中下划线位于字形下方，并为鼠标悬停与确认选择添加程序化音效。`MainMenuController` 根据 TMP 渲染字形边界定位下划线，字形与线之间留 8 UI 单位。
- `ProceduralMenuAudio` 在运行时生成短悬停音和较暖的确认音，通过菜单控制器上的单个 2D `AudioSource` 播放；没有导入外部音频文件。
- Play Mode 中调用指针进入与 Settings 确认路径，观察到 AudioSource 播放且 Settings 面板打开。未用真实鼠标或扬声器做人工听测，也未构建独立 Player。
