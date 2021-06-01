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
            Chessman selected = null;
            if (_CurrMouseOverPos.HasValue)
                selected = _Game.Chessboard.GetChessmanByPos(_CurrMouseOverPos.Value);
            if (_CurrSelectedChessman != selected)
            {
                _CurrSelectedChessman = selected;
                Invalidate();
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
            pevent.Graphics.DrawImage(_ChessboardBitmap, 0, 0, _ChessboardBitmap.Width, _ChessboardBitmap.Height);
        }
    }
}