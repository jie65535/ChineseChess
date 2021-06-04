using System;
using System.Drawing;
using System.Windows.Forms;

using ChineseChess.Core;

namespace ChineseChess
{
    internal class CGame : Control
    {
        private Bitmap _ChessboardBitmap;
        private ResourceHelper _ResHelper;

        private Game _Game;
        private ChessboardPosition? _CurrMouseOverPos;
        private Chessman _CurrSelectedChessman;

        public CGame()
        {
            DoubleBuffered = true;
            _ResHelper = ResourceHelper.Instance;
            _ChessboardBitmap = _ResHelper.GetChessboardBitmap(0);
            MinimumSize = MaximumSize = Size = _ChessboardBitmap.Size;
            _Game = new Game();

        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            foreach (var chessman in _Game.Chessboard.GetChessmen())
                DrawChessman(pevent.Graphics, chessman);

            if (_CurrSelectedChessman != null && _CurrMouseOverPos.HasValue && _CurrSelectedChessman.Position != _CurrMouseOverPos.Value)
                DrawChessman(pevent.Graphics, _CurrSelectedChessman, _CurrMouseOverPos.Value);


            base.OnPaint(pevent);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            var pos = GetChessboardPosition(e.Location);
            if (_CurrMouseOverPos != pos)
            {
                _CurrMouseOverPos = pos;
                Invalidate();
            }
            base.OnMouseMove(e);
        }

        protected override void OnClick(EventArgs e)
        {
            try
            {
                Chessman selected = null;
                if (_CurrMouseOverPos.HasValue)
                    selected = _Game.Chessboard.GetChessmanByPos(_CurrMouseOverPos.Value);
                if (_CurrSelectedChessman == null)
                {
                    if (selected != null)
                    {
                        _CurrSelectedChessman = selected;
                        Invalidate();
                    }
                }
                else if (_CurrSelectedChessman != selected)
                {
                    _Game.Chessboard.PushMove(new ChessMove(_CurrSelectedChessman.Camp, _CurrSelectedChessman.Type, selected?.Type, _CurrSelectedChessman.Position, _CurrMouseOverPos.Value, string.Empty));
                    _CurrSelectedChessman = null;
                    Invalidate();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "异常", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            base.OnClick(e);
        }

        private void DrawChessman(Graphics g, Chessman chessman)
        {
            DrawImageByCentre(g, _ResHelper.GetChessmanBitmap(chessman.Type, chessman.Camp), GetChessboardGridPoint(chessman.Position));
        }

        private void DrawChessman(Graphics g, Chessman chessman, ChessboardPosition pos)
        {
            DrawImageByCentre(g, _ResHelper.GetChessmanBitmap(chessman.Type, chessman.Camp), GetChessboardGridPoint(pos));
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
            g.DrawImage(image, point.X - image.Width/2, point.Y - image.Height/2, image.Width, image.Height);
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            //pevent.Graphics.FillRectangle(Brushes.White, pevent.ClipRectangle);
            //DrawChessborad(pevent.Graphics, new Rectangle(((Point)_ResHelper.ChessboardOffset), _ResHelper.ChessboardGridSize), Pens.Black);
            pevent.Graphics.DrawImage(_ChessboardBitmap, 0, 0, _ChessboardBitmap.Width, _ChessboardBitmap.Height);
        }
    }
}