using System;
using System.Windows.Forms;

using ChineseChess.Core;

namespace ChineseChess
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            HookGame();
            UpdateStatus();
        }

        private void HookGame()
        {
            Game.Game.StateChanged += OnGameStateChanged;
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

        private void UpdateStatus()
        {
            string camp = Game.Game.CurrentCamp == ChessCamp.Red ? "红方" : "黑方";
            string state;
            if (Game.Game.IsGameOver)
            {
                string winner = Game.Game.Winner == ChessCamp.Red ? "红方" : "黑方";
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
            StatusGameLabel.Text = $"已走 {Game.Game.Chessboard.MoveCount} 步";
        }

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
                string winner = Game.Game.Winner == ChessCamp.Red ? "红方" : "黑方";
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
            if (!Game.Game.Undo())
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
                "    菜单可悔棋、开新局、切换棋盘外观。",
                "关于", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
