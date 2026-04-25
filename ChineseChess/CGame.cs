using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using ChineseChess.Core;
using ChineseChess.Core.AI;

namespace ChineseChess
{
    /// <summary>
    /// 棋盘交互控件：负责渲染棋盘、棋子，并把鼠标操作翻译为 Game 的着法
    /// </summary>
    internal class CGame : Control
    {
        private Bitmap _ChessboardBitmap;
        private readonly ResourceHelper _ResHelper;

        private readonly Game _Game;
        private ChessboardPosition? _CurrMouseOverPos;
        private Chessman _CurrSelectedChessman;
        private List<ChessboardPosition> _LegalTargets = new List<ChessboardPosition>();
        private int _BoardIndex;

        // ===== AI 相关 =====
        private readonly ChessAI _AI = new ChessAI();
        private CancellationTokenSource _AICts;
        private ChessCamp? _AICamp;
        private AISettings _AISettings = AISettings.Medium;
        public bool IsAIThinking { get; private set; }
        public AIDecision LastAIDecision { get; private set; }

        /// <summary>对外暴露的对局实例</summary>
        public Game Game => _Game;

        /// <summary>非法走子提示</summary>
        public event EventHandler<string> InvalidMoveAttempted;

        /// <summary>AI 状态发生变化（开始/结束思考、走完棋）</summary>
        public event EventHandler AIStateChanged;

        /// <summary>AI 执子方；为 null 时不启用 AI</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ChessCamp? AICamp
        {
            get => _AICamp;
            set
            {
                if (_AICamp == value) return;
                _AICamp = value;
                CancelAI();
                MaybeStartAI();
                AIStateChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>AI 难度设置</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public AISettings AISettings
        {
            get => _AISettings;
            set
            {
                _AISettings = value ?? AISettings.Medium;
                // 若 AI 正在思考，让它用新的设置重启
                if (IsAIThinking)
                {
                    CancelAI();
                    MaybeStartAI();
                }
            }
        }

        public CGame()
        {
            DoubleBuffered = true;
            _ResHelper = ResourceHelper.Instance;
            _BoardIndex = 0;
            _ChessboardBitmap = _ResHelper.GetChessboardBitmap(_BoardIndex);
            MinimumSize = MaximumSize = Size = _ChessboardBitmap.Size;
            _Game = new Game();
            _Game.StateChanged += OnGameStateChanged;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                CancelAI();
                _Game.StateChanged -= OnGameStateChanged;
            }
            base.Dispose(disposing);
        }

        /// <summary>棋盘模式总数：位图1、位图2、自绘</summary>
        private const int BoardModeCount = 3;

        public void SwitchBoard()
        {
            _BoardIndex = (_BoardIndex + 1) % BoardModeCount;
            if (_BoardIndex < 2)
                _ChessboardBitmap = _ResHelper.GetChessboardBitmap(_BoardIndex);
            Invalidate();
        }

        private void OnGameStateChanged(object sender, EventArgs e)
        {
            ClearSelection();
            Invalidate();
            // 棋面变了，先取消可能尚未结束的旧 AI 思考，再决定是否启动新一轮
            CancelAI();
            MaybeStartAI();
        }

        private void MaybeStartAI()
        {
            if (_AICamp == null) return;
            if (_Game.IsGameOver) return;
            if (_Game.CurrentCamp != _AICamp.Value) return;
            if (IsAIThinking) return;
            if (!IsHandleCreated) return; // 等控件就绪后再启动
            StartAI();
        }

        private void StartAI()
        {
            var cts = new CancellationTokenSource();
            _AICts = cts;
            IsAIThinking = true;
            AIStateChanged?.Invoke(this, EventArgs.Empty);

            var settings = _AISettings;
            var task = _AI.ThinkAsync(_Game.Chessboard, _Game.CurrentCamp, settings, cts.Token);
            task.ContinueWith(t =>
            {
                if (IsDisposed || !IsHandleCreated) return;
                try
                {
                    BeginInvoke(new Action(() => OnAIDone(t, cts)));
                }
                catch (ObjectDisposedException) { }
                catch (InvalidOperationException) { }
            });
        }

        private void OnAIDone(Task<AIDecision> t, CancellationTokenSource cts)
        {
            // 如果当前 cts 已被替换，说明这次思考已被取消，丢弃结果
            if (cts != _AICts) return;
            _AICts = null;
            IsAIThinking = false;

            if (t.IsCanceled || t.IsFaulted)
            {
                AIStateChanged?.Invoke(this, EventArgs.Empty);
                return;
            }

            var decision = t.Result;
            LastAIDecision = decision;
            AIStateChanged?.Invoke(this, EventArgs.Empty);

            if (decision == null || !decision.IsValid) return;
            // 防御性校验：确保起点有 AI 阵营的棋子（如果在等待期间发生了 Reset，AICamp 可能已变）
            if (_AICamp == null || _Game.CurrentCamp != _AICamp.Value || _Game.IsGameOver) return;
            if (!_Game.TryMove(decision.From, decision.To, out var err))
            {
                InvalidMoveAttempted?.Invoke(this, $"AI 走子失败：{err}");
            }
        }

        private void CancelAI()
        {
            if (_AICts == null) return;
            try { _AICts.Cancel(); } catch (ObjectDisposedException) { }
            _AICts = null;
            if (IsAIThinking)
            {
                IsAIThinking = false;
                AIStateChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            // 控件就绪后再尝试启动 AI（针对 AI 执红、首步即由 AI 出招的情况）
            MaybeStartAI();
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            var g = pevent.Graphics;

            // 最近一步标记（起点 + 终点，红色细框）
            var last = _Game.Chessboard.LastMove;
            if (last != null)
            {
                DrawLastMoveMark(g, last.Start);
                DrawLastMoveMark(g, last.End);
            }

            // 棋子
            foreach (var chessman in _Game.Chessboard.GetChessmen())
                DrawChessman(g, chessman);

            // 选中棋子的边框
            if (_CurrSelectedChessman != null)
            {
                var border = _CurrSelectedChessman.Camp == ChessCamp.Red
                    ? _ResHelper.SelectBorderRed
                    : _ResHelper.SelectBorderGreen;
                DrawImageByCentre(g, border, GetChessboardGridPoint(_CurrSelectedChessman.Position));
            }

            // 合法落点提示
            foreach (var t in _LegalTargets)
            {
                DrawTargetHint(g, t);
            }

            // 鼠标悬停在合法落点时画半透明预览
            if (_CurrSelectedChessman != null
                && _CurrMouseOverPos.HasValue
                && _LegalTargets.Contains(_CurrMouseOverPos.Value))
            {
                DrawChessmanGhost(g, _CurrSelectedChessman, _CurrMouseOverPos.Value);
            }

            base.OnPaint(pevent);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            var pos = GetChessboardPosition(e.Location);
            if (_CurrMouseOverPos.Equals(pos) == false)
            {
                _CurrMouseOverPos = pos;
                bool humanTurn = !_Game.IsGameOver
                    && !IsAIThinking
                    && (!_AICamp.HasValue || _Game.CurrentCamp != _AICamp.Value);
                // 改变光标形状以提供反馈
                if (pos.HasValue && humanTurn)
                {
                    if (_CurrSelectedChessman != null && _LegalTargets.Contains(pos.Value))
                        Cursor = Cursors.Hand;
                    else if (_Game.Chessboard.GetChessmanByPos(pos.Value)?.Camp == _Game.CurrentCamp)
                        Cursor = Cursors.Hand;
                    else
                        Cursor = Cursors.Default;
                }
                else
                {
                    Cursor = Cursors.Default;
                }
                Invalidate();
            }
            base.OnMouseMove(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            if (_CurrMouseOverPos.HasValue)
            {
                _CurrMouseOverPos = null;
                Cursor = Cursors.Default;
                Invalidate();
            }
            base.OnMouseLeave(e);
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if (e.Button != MouseButtons.Left) return;
            if (_Game.IsGameOver) return;
            if (IsAIThinking) return;
            // 不是人类一方时，禁止操作
            if (_AICamp.HasValue && _Game.CurrentCamp == _AICamp.Value) return;
            var clicked = GetChessboardPosition(e.Location);
            if (!clicked.HasValue) return;

            // 已有选中棋子，且点击合法落点 → 走子
            if (_CurrSelectedChessman != null && _LegalTargets.Contains(clicked.Value))
            {
                string error;
                if (!_Game.TryMove(_CurrSelectedChessman.Position, clicked.Value, out error))
                {
                    InvalidMoveAttempted?.Invoke(this, error);
                }
                return; // StateChanged 事件会触发 ClearSelection + Invalidate
            }

            // 否则按当前阵营是否能选中处理
            var target = _Game.Chessboard.GetChessmanByPos(clicked.Value);
            if (target != null && target.Camp == _Game.CurrentCamp)
            {
                Select(target);
            }
            else
            {
                ClearSelection();
                Invalidate();
            }
        }

        private void Select(Chessman chess)
        {
            _CurrSelectedChessman = chess;
            _LegalTargets = ChessReferee.GetLegalTargets(_Game.Chessboard, chess).ToList();
            Invalidate();
        }

        private void ClearSelection()
        {
            _CurrSelectedChessman = null;
            _LegalTargets.Clear();
        }

        private void DrawChessman(Graphics g, Chessman chessman)
        {
            DrawImageByCentre(g, _ResHelper.GetChessmanBitmap(chessman.Type, chessman.Camp), GetChessboardGridPoint(chessman.Position));
        }

        private void DrawChessmanGhost(Graphics g, Chessman chessman, ChessboardPosition pos)
        {
            var bitmap = _ResHelper.GetChessmanBitmap(chessman.Type, chessman.Camp);
            var pt = GetChessboardGridPoint(pos);
            // 用 ColorMatrix 做半透明绘制
            using (var attrs = new System.Drawing.Imaging.ImageAttributes())
            {
                var matrix = new System.Drawing.Imaging.ColorMatrix { Matrix33 = 0.55f };
                attrs.SetColorMatrix(matrix);
                var rect = new Rectangle(pt.X - bitmap.Width / 2, pt.Y - bitmap.Height / 2, bitmap.Width, bitmap.Height);
                g.DrawImage(bitmap, rect, 0, 0, bitmap.Width, bitmap.Height, GraphicsUnit.Pixel, attrs);
            }
        }

        private void DrawTargetHint(Graphics g, ChessboardPosition pos)
        {
            var pt = GetChessboardGridPoint(pos);
            int radius = 8;
            var occupied = _Game.Chessboard.GetChessmanByPos(pos);
            using (var brush = new SolidBrush(Color.FromArgb(140, occupied != null ? Color.OrangeRed : Color.LimeGreen)))
            {
                if (occupied != null)
                {
                    // 以空心圆框表示可吃子
                    using (var pen = new Pen(brush.Color, 3))
                    {
                        int s = _ResHelper.ChessmanBitmapSize.Width / 2 + 2;
                        g.DrawEllipse(pen, pt.X - s, pt.Y - s, s * 2, s * 2);
                    }
                }
                else
                {
                    g.FillEllipse(brush, pt.X - radius, pt.Y - radius, radius * 2, radius * 2);
                }
            }
        }

        private void DrawLastMoveMark(Graphics g, ChessboardPosition pos)
        {
            var pt = GetChessboardGridPoint(pos);
            int s = _ResHelper.ChessboardCellSize.Width / 2 - 2;
            using (var pen = new Pen(Color.FromArgb(180, Color.DodgerBlue), 2))
            {
                g.DrawRectangle(pen, pt.X - s, pt.Y - s, s * 2, s * 2);
            }
        }

        private void DrawChessborad(Graphics g, Rectangle rect, Pen pen)
        {
            Size blockCount = new Size(8, 9);
            Size cellSize = new Size(rect.Width / blockCount.Width, rect.Height / blockCount.Height);
            Point begin = default, end = default;

            // 绘制横线
            begin.X = rect.Left;
            end.X = rect.Right;
            for (int i = 0; i <= blockCount.Height; i++)
            {
                end.Y = begin.Y = rect.Top + cellSize.Height * i;
                g.DrawLine(pen, begin, end);
            }

            // 绘制纵线
            begin.Y = rect.Top;
            end.Y = rect.Bottom;
            g.DrawLine(pen, rect.Left, begin.Y, rect.Left, end.Y);
            g.DrawLine(pen, rect.Right, begin.Y, rect.Right, end.Y);
            int upEndY = rect.Top + cellSize.Height * (blockCount.Height / 2);
            int dnBegY = rect.Top + cellSize.Height * (blockCount.Height / 2 + 1);
            for (int i = 1; i < blockCount.Width; i++)
            {
                begin.X = end.X = rect.Left + cellSize.Width * i;
                g.DrawLine(pen, begin.X, begin.Y, end.X, upEndY);
                g.DrawLine(pen, begin.X, dnBegY, end.X, end.Y);
            }

            // 绘制斜线
            Point centerPoint = new Point(rect.Left + rect.Width / 2, rect.Top + cellSize.Height);
            int left = centerPoint.X - cellSize.Width;
            int right = centerPoint.X + cellSize.Width;
            int up = centerPoint.Y - cellSize.Height;
            int down = centerPoint.Y + cellSize.Height;
            g.DrawLine(pen, left, up, right, down);
            g.DrawLine(pen, right, up, left, down);

            centerPoint.Y = rect.Bottom - cellSize.Height;
            up = centerPoint.Y - cellSize.Height;
            down = centerPoint.Y + cellSize.Height;
            g.DrawLine(pen, left, up, right, down);
            g.DrawLine(pen, right, up, left, down);

            // 绘制对位
            Size offsetSize = new Size(cellSize.Width / 10, cellSize.Height / 10);
            Point[] begOffset = new Point[4]
            {
                new Point(-offsetSize.Width, -offsetSize.Height),
                new Point(offsetSize.Width,  -offsetSize.Height),
                new Point(offsetSize.Width,  offsetSize.Height),
                new Point(-offsetSize.Width, offsetSize.Height),
            };
            Size lineSize = new Size(cellSize.Width / 5, cellSize.Height / 5);
            Point[,] endOffsets = new Point[4, 2]
            {
                { new Point(0, -lineSize.Height), new Point(-lineSize.Width, 0) },
                { new Point(0, -lineSize.Height), new Point(lineSize.Width, 0)  },
                { new Point(0, lineSize.Height),  new Point(lineSize.Width, 0)  },
                { new Point(0, lineSize.Height),  new Point(-lineSize.Width, 0) },
            };

            void drawTarget(Point point)
            {
                for (int i = 0; i < 4; i++)
                {
                    begin = new Point(point.X + begOffset[i].X, point.Y + begOffset[i].Y);
                    for (int j = 0; j < 2; j++)
                    {
                        end = new Point(begin.X + endOffsets[i, j].X, begin.Y + endOffsets[i, j].Y);
                        if (rect.Contains(begin) && rect.Contains(end))
                            g.DrawLine(pen, begin, end);
                    }
                }
            }

            // 画卒线 兵位
            for (int i = 0; i < 5; i++)
            {
                drawTarget(new Point(rect.Left + i*2 * cellSize.Width, rect.Top + 3 * cellSize.Height));
                drawTarget(new Point(rect.Left + i*2 * cellSize.Width, rect.Top + 6 * cellSize.Height));
            }
            // 画三线 炮位
            drawTarget(new Point(rect.Left + cellSize.Width, rect.Top + 2 * cellSize.Height));
            drawTarget(new Point(rect.Right - cellSize.Width, rect.Top + 2 * cellSize.Height));
            drawTarget(new Point(rect.Left + cellSize.Width, rect.Bottom - 2 * cellSize.Height));
            drawTarget(new Point(rect.Right - cellSize.Width, rect.Bottom - 2 * cellSize.Height));
        }

        private Point GetChessboardGridPoint(ChessboardPosition position)
        {
            return new Point(position.Col * _ResHelper.ChessboardCellSize.Width + _ResHelper.ChessboardOffset.Width,
                _ResHelper.ChessboardGridSize.Height - position.Row * _ResHelper.ChessboardCellSize.Height + _ResHelper.ChessboardOffset.Height);
        }

        private ChessboardPosition? GetChessboardPosition(Point point)
        {
            int x = point.X - _ResHelper.ChessboardOffset.Width + _ResHelper.ChessboardCellSize.Width / 2;
            int y = point.Y - _ResHelper.ChessboardOffset.Height + _ResHelper.ChessboardCellSize.Height / 2;
            if (x < 0 || y < 0
                || x >= _ResHelper.ChessboardSize.Width * _ResHelper.ChessboardCellSize.Width
                || y >= _ResHelper.ChessboardSize.Height * _ResHelper.ChessboardCellSize.Height)
                return null;
            x /= _ResHelper.ChessboardCellSize.Width;
            y /= _ResHelper.ChessboardCellSize.Height;
            return new ChessboardPosition(x, _ResHelper.ChessboardSize.Height - y - 1);
        }

        private void DrawImageByCentre(Graphics g, Image image, Point point)
        {
            g.DrawImage(image, point.X - image.Width / 2, point.Y - image.Height / 2, image.Width, image.Height);
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            if (_BoardIndex < 2)
            {
                pevent.Graphics.DrawImage(_ChessboardBitmap, 0, 0, _ChessboardBitmap.Width, _ChessboardBitmap.Height);
            }
            else
            {
                pevent.Graphics.FillRectangle(Brushes.White, pevent.ClipRectangle);
                DrawChessborad(pevent.Graphics, new Rectangle(((Point)_ResHelper.ChessboardOffset), _ResHelper.ChessboardGridSize), Pens.Black);
            }
        }
    }
}
