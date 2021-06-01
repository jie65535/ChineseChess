using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ChineseChess.Core;
using ChineseChess.Properties;

namespace ChineseChess
{
    class ResourceHelper
    {
        /// <summary>
        /// 获取资源助手实例（单例对象）
        /// </summary>
        public static ResourceHelper Instance { get; } = new ResourceHelper();

        private Dictionary<ChessType, Bitmap> _RedChessmans = new Dictionary<ChessType, Bitmap>(7);
        private Dictionary<ChessType, Bitmap> _BlackChessmans = new Dictionary<ChessType, Bitmap>(7);
        private Bitmap[] Chessboards = new Bitmap[] { Resources.Chessboard1, Resources.Chessboard2 };



        public Size ChessmanBitmapSize { get; } = new Size(47, 46);
        public Size ChessboardCellSize { get; } = new Size(50, 50);
        public Size ChessboardSize { get; } = new Size(9, 10);
        public Size ChessboardGridSize { get; } = new Size(50 * 8, 50 * 9);
        public Size ChessboardOffset { get; } = new Size(27, 36);

        
        private ChessType[] _ChessmanOrder = new ChessType[] { ChessType.King, ChessType.Mandarins, ChessType.Elephants, ChessType.Knights, ChessType.Rooks, ChessType.Cannons, ChessType.Pawns };
        private ChessCamp[] _ChessCampOrder = new ChessCamp[] { ChessCamp.Red, ChessCamp.Black };

        private ResourceHelper()
        {
            var sprites = Resources.ChessmanSprites;
            Point offset = Point.Empty;
            foreach (var camp in _ChessCampOrder)
            {
                var chessmans = camp == ChessCamp.Red ? _RedChessmans : _BlackChessmans;
                foreach (var type in _ChessmanOrder)
                {
                    Bitmap bitmap = new Bitmap(ChessmanBitmapSize.Width, ChessmanBitmapSize.Height);
                    using (var g = Graphics.FromImage(bitmap))
                    {
                        g.DrawImage(sprites, new Rectangle(Point.Empty, ChessmanBitmapSize), new Rectangle(offset, ChessmanBitmapSize), GraphicsUnit.Pixel);
                    }
                    bitmap.MakeTransparent(Color.FromArgb(0xFF, 0x00, 0xFF));
                    chessmans.Add(type, bitmap);
                    offset.X += ChessmanBitmapSize.Width;
                }
                offset.X = 0;
                offset.Y = ChessmanBitmapSize.Height + 1;
            }
        }

        /// <summary>
        /// 获取象棋图片
        /// </summary>
        /// <param name="type">棋子类型</param>
        /// <param name="camp">棋子阵营</param>
        /// <returns>返回缓存中的图片</returns>
        public Bitmap GetChessmanBitmap(ChessType type, ChessCamp camp)
        {
            if (camp == ChessCamp.Red)
                return _RedChessmans[type];
            else if (camp == ChessCamp.Black)
                return _BlackChessmans[type];
            else
                throw new ArgumentException("无效阵营参数", nameof(camp));
        }

        /// <summary>
        /// 获取棋盘图片
        /// </summary>
        /// <param name="index">棋盘索引 0~1</param>
        /// <returns>返回缓存中的图片</returns>
        public Bitmap GetChessboardBitmap(int index)
        {
            return Chessboards[index];
        }
    }
}
