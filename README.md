# 中国象棋 ChineseChess

一个用 C# / WinForms 写的中国象棋游戏，自带搜索 AI。

## 特性

- 完整的中国象棋走子规则：将/帅、士、象、马、车、炮、兵/卒，以及 **蹩马腿、塞象眼、隔山打牛、飞将、九宫、过河兵** 等所有限制
- 自动判定将军、将死、困毙；中国象棋无和棋——困毙也判负
- 走子记录用中文记谱（炮二平五、马８进７ 这种形式）
- 三种棋盘外观（位图×2 + 纯线条自绘），可在菜单切换
- **内置象棋 AI**：alpha-beta + PVS 搜索，置换表、杀手着法、历史启发、空着裁剪、静态搜索、被将军延伸
- 难度分简单 / 中等 / 困难三档；可选 AI 执红或执黑
- 悔棋（人机对战时一次回退两步）

## 编译运行

需要 .NET 9 SDK，仅支持 Windows（依赖 WinForms）。

```sh
dotnet build ChineseChess.sln
dotnet run --project ChineseChess/ChineseChess.csproj
```

或者用 Visual Studio 直接打开 `ChineseChess.sln`。

## 操作

- 左键单击己方棋子选中，再点击高亮的合法落点完成走子
- 再次点击其他己方棋子可改选；点击空白处取消选中
- 顶部菜单：
  - **对局** — 新对局（Ctrl+N）/ 悔棋（Ctrl+Z）/ 退出
  - **视图** — 切换棋盘外观
  - **AI** — 关闭 / 简单 / 中等 / 困难，以及 AI 执黑 / AI 执红
  - **帮助** — 关于

底部状态栏显示当前轮次、将军提示、对局结果，以及 AI 上一次思考的搜索深度、节点数、用时和评分。

## 项目结构

```
ChineseChess.sln
├── ChineseChess.Core/        # 与 UI 解耦的核心逻辑（netstandard2.1）
│   ├── Chessboard.cs         # 棋盘状态与走子撤销栈
│   ├── ChessReferee.cs       # 走子合法性、将军、将死/困毙判定
│   ├── ChessNotation.cs      # 中文记谱
│   ├── Game.cs               # 对局状态机（轮次、胜负、悔棋）
│   └── AI/                   # 搜索引擎
│       ├── SearchBoard.cs        # 紧凑棋盘 + IsSquareAttacked + Zobrist
│       ├── MoveGenerator.cs      # 高速伪合法走子生成
│       ├── Evaluation.cs         # 物质 + piece-square 表
│       ├── TranspositionTable.cs # 置换表（默认 2^20 项）
│       ├── SearchEngine.cs       # alpha-beta + PVS + 静态搜索
│       └── ChessAI.cs            # 公开门面（同步 / 异步两种调用）
└── ChineseChess/             # WinForms 外壳（net9.0-windows）
    ├── MainForm.cs           # 菜单、状态栏、走子记录列表
    ├── CGame.cs              # 棋盘控件：渲染、交互、AI 调度
    ├── ResourceHelper.cs     # 棋盘 / 棋子位图加载
    ├── AIBench.cs            # AI 自检工具（--ai-bench 触发）
    └── Resources/            # 棋盘 / 棋子 / 选中框等资源
```

## AI 简介

AI 子系统位于 `ChineseChess.Core/AI/`，是个单线程的 alpha-beta 搜索引擎，跑在 `SearchBoard`（`sbyte[90]` 平铺数组）上而非 UI 用的 `Chessboard`，避免 LINQ 与对象分配。

主要技术：

| 技术 | 文件 |
|---|---|
| 迭代加深、PVS（主要变例搜索） | `SearchEngine.cs` |
| 置换表 + Zobrist 增量哈希 | `TranspositionTable.cs` / `Zobrist.cs` |
| 杀手着法（每层 2 个）+ 历史启发表 | `SearchEngine.cs` |
| 空着裁剪（剩有大子时启用，R=2/3） | `SearchEngine.cs` |
| 被将军延伸 | `SearchEngine.cs` |
| 静态搜索（仅吃子） | `SearchEngine.cs` |
| 走子排序：TT > MVV-LVA 吃子 > Killer > 历史 | `SearchEngine.cs::OrderMoves` |

参考性能（i7 级别 CPU 单线程，开局位置）：

- 第 7 层 ≈ 0.5 秒，约 200 万节点
- 第 8 层 ≈ 5 秒，约 2000 万节点
- NPS 约 300–450 万

可在 `Evaluation.cs` 调整 `PieceValue` 和各兵种 piece-square 表来微调棋风；调 `AISettings.Easy/Medium/Hard` 改难度档位。

## 自检

```sh
ChineseChess/bin/Release/net9.0-windows/ChineseChess.exe --ai-bench
```

会跑三个标准局面（开局红方、对方应招、困难档位深度搜索），把结果写到同目录下的 `ai_bench.log`。

## 规则备注

- **没有和棋**：中国象棋规则下，被困毙（无子可动但未被将军）也算负方。引擎里 `Game.GameResult` 枚举区分了 `Checkmate` 和 `Stalemate` 两种结束原因，但都是"被困一方负"。
- **长打 / 长将不允许重复**：当前未实现重复局面检测，AI 不会自觉避免长将循环；需要时在 `Chessboard.GetMoves()` 上做三次重复检查即可。

## 致谢

棋盘和棋子图片来自互联网公开素材；