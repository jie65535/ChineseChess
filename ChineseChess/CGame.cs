using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using ChineseChess.Core;

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

        /// <summary>对外暴露的对局实例</summary>
        public Game Game => _Game;

        /// <summary>非法走子提示</summary>
        public event EventHandler<string> InvalidMoveAttempted;

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
                // 改变光标形状以提供反馈
                if (pos.HasValue)
                {
                    if (_CurrSelectedChessman != null && _LegalTargets.Contains(pos.Value))
                        Cursor = Cursors.Hand;
                    else if (_Game.Chessboard.GetChessmanByPos(pos.Value)?.Camp == _Game.CurrentCamp && !_Game.IsGameOver)
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
