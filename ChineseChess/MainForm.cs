using System;
using System.Windows.Forms;

using ChineseChess.Core;
using ChineseChess.Core.AI;

namespace ChineseChess
{
    public partial class MainForm : Form
    {
        private AILevel _aiLevel = AILevel.Off;
        private ChessCamp _aiSide = ChessCamp.Black;

        public MainForm()
        {
            InitializeComponent();
            HookGame();
            UpdateAIMenuChecks();
            UpdateStatus();
        }

        private void HookGame()
        {
            Game.Game.StateChanged += OnGameStateChanged;
            Game.AIStateChanged += OnAIStateChanged;
            Game.InvalidMoveAttempted += (s, msg) =>
            {
                StatusGameLabel.Text = "提示：" + msg;
            };
        }

        private void OnGameStateChanged(object sender, EventArgs e)
        {
            RefreshMoveList();
            UpdateStatus();
            ShowGameOverIfNeeded();
        }

        private void OnAIStateChanged(object sender, EventArgs e)
        {
            UpdateStatus();
        }

        private void UpdateStatus()
        {
            if (Game.IsAIThinking)
            {
                StatusCampLabel.Text = $"AI（{CampText(Game.AICamp ?? _aiSide)}）思考中…";
                StatusGameLabel.Text = $"难度：{LevelText(_aiLevel)}";
                return;
            }

            string camp = CampText(Game.Game.CurrentCamp);
            string state;
            if (Game.Game.IsGameOver)
            {
                string winner = CampText(Game.Game.Winner ?? ChessCamp.Red);
                string reason = Game.Game.Result == GameResult.Stalemate ? "困毙" : "将死";
                state = $"对局结束：{winner}胜（{reason}）";
            }
            else if (Game.Game.IsInCheck)
            {
                state = $"{camp}被将军！";
            }
            else
            {
                state = $"轮到 {camp} 走子";
            }
            StatusCampLabel.Text = state;

            var decision = Game.LastAIDecision;
            if (decision != null && decision.IsValid)
            {
                StatusGameLabel.Text =
                    $"已走 {Game.Game.Chessboard.MoveCount} 步 ｜ AI：第{decision.Depth}层 / {decision.Nodes:N0} 节点 / {decision.Elapsed.TotalMilliseconds:F0}ms / 评分 {decision.Score}";
            }
            else
            {
                StatusGameLabel.Text = $"已走 {Game.Game.Chessboard.MoveCount} 步";
            }
        }

        private static string CampText(ChessCamp camp) => camp == ChessCamp.Red ? "红方" : "黑方";
        private static string LevelText(AILevel l) => l switch
        {
            AILevel.Easy => "简单",
            AILevel.Medium => "中等",
            AILevel.Hard => "困难",
            _ => "关闭",
        };

        private void RefreshMoveList()
        {
            MoveListBox.BeginUpdate();
            MoveListBox.Items.Clear();
            int idx = 1;
            // GetMoves 返回栈结构（栈顶为最近一步），反转得到顺序
            var moves = Game.Game.Chessboard.GetMoves();
            var ordered = new System.Collections.Generic.List<ChessMove>(moves);
            ordered.Reverse();
            foreach (var m in ordered)
            {
                string side = m.Camp == ChessCamp.Red ? "红" : "黑";
                MoveListBox.Items.Add($"{idx,3}. {side}  {m.Text}");
                idx++;
            }
            if (MoveListBox.Items.Count > 0)
            {
                MoveListBox.TopIndex = MoveListBox.Items.Count - 1;
            }
            MoveListBox.EndUpdate();
        }

        private bool _GameOverShown;
        private void ShowGameOverIfNeeded()
        {
            if (Game.Game.IsGameOver && !_GameOverShown)
            {
                _GameOverShown = true;
                string winner = CampText(Game.Game.Winner ?? ChessCamp.Red);
                string reason = Game.Game.Result == GameResult.Stalemate ? "困毙" : "将死";
                BeginInvoke(new Action(() =>
                {
                    MessageBox.Show(this, $"{winner}获胜（{reason}）", "对局结束",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }));
            }
            else if (!Game.Game.IsGameOver)
            {
                _GameOverShown = false;
            }
        }

        private void NewGameMenuItem_Click(object sender, EventArgs e)
        {
            if (Game.Game.Chessboard.MoveCount > 0 && !Game.Game.IsGameOver)
            {
                var result = MessageBox.Show(this, "当前对局尚未结束，确定要开始新对局吗？",
                    "新对局", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result != DialogResult.Yes) return;
            }
            Game.Game.Reset();
        }

        private void UndoMenuItem_Click(object sender, EventArgs e)
        {
            // 人机对战时悔棋应一次回退两步：把己方上一步和 AI 应招都退掉
            int undoCount = (Game.AICamp.HasValue && Game.Game.Chessboard.MoveCount >= 2) ? 2 : 1;
            int actual = 0;
            for (int i = 0; i < undoCount; i++)
            {
                if (Game.Game.Undo()) actual++;
                else break;
            }
            if (actual == 0)
            {
                StatusGameLabel.Text = "提示：已经回到初始局面";
            }
        }

        private void SwitchBoardMenuItem_Click(object sender, EventArgs e)
        {
            Game.SwitchBoard();
        }

        private void ExitMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void AboutMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(this,
                "中国象棋\n" +
                "\n" +
                "操作：\n" +
                "    左键单击己方棋子选中，再点击合法落点完成走子。\n" +
                "    再次点击其他己方棋子可改选。\n" +
                "    菜单可悔棋、开新局、切换棋盘外观、开启 AI 对战。\n" +
                "\n" +
                "AI：alpha-beta + 置换表 + 杀手 / 历史启发 + 静态搜索。",
                "关于", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ===== AI 菜单 =====

        private void AIOffMenuItem_Click(object s, EventArgs e) => SetAILevel(AILevel.Off);
        private void AIEasyMenuItem_Click(object s, EventArgs e) => SetAILevel(AILevel.Easy);
        private void AIMediumMenuItem_Click(object s, EventArgs e) => SetAILevel(AILevel.Medium);
        private void AIHardMenuItem_Click(object s, EventArgs e) => SetAILevel(AILevel.Hard);

        private void AISideBlackMenuItem_Click(object s, EventArgs e) => SetAISide(ChessCamp.Black);
        private void AISideRedMenuItem_Click(object s, EventArgs e) => SetAISide(ChessCamp.Red);

        private void SetAILevel(AILevel level)
        {
            _aiLevel = level;
            ApplyAI();
        }

        private void SetAISide(ChessCamp side)
        {
            if (_aiSide == side) return;
            _aiSide = side;
            ApplyAI();
        }

        private void ApplyAI()
        {
            UpdateAIMenuChecks();
            if (_aiLevel == AILevel.Off)
            {
                Game.AICamp = null;
                return;
            }
            Game.AISettings = _aiLevel switch
            {
                AILevel.Easy => AISettings.Easy,
                AILevel.Hard => AISettings.Hard,
                _ => AISettings.Medium,
            };
            Game.AICamp = _aiSide;
        }

        private void UpdateAIMenuChecks()
        {
            AIOffMenuItem.Checked = _aiLevel == AILevel.Off;
            AIEasyMenuItem.Checked = _aiLevel == AILevel.Easy;
            AIMediumMenuItem.Checked = _aiLevel == AILevel.Medium;
            AIHardMenuItem.Checked = _aiLevel == AILevel.Hard;
            AISideBlackMenuItem.Checked = _aiSide == ChessCamp.Black;
            AISideRedMenuItem.Checked = _aiSide == ChessCamp.Red;
            // 难度关闭时执方选项暂无实际作用，仍允许选择
        }

        private enum AILevel
        {
            Off, Easy, Medium, Hard,
        }
    }
}
