using System;

namespace ChineseChess.Core.AI
{
    /// <summary>
    /// Zobrist 哈希：为每个 (棋子类型 × 阵营 × 格子) 分配一个固定 64-bit 随机数，用于增量计算棋面 hash key
    /// </summary>
    internal static class Zobrist
    {
        // 编码：piece 取值 ±1..±7。通过 PieceIndex 映射到 0..13
        public static readonly ulong[,] PieceKeys = new ulong[14, SearchBoard.Size];
        public static readonly ulong SideKey;

        static Zobrist()
        {
            // 用确定性种子，便于调试与置换表稳定
            var rng = new Random(0x5A17B0A6);
            for (int p = 0; p < 14; p++)
            {
                for (int sq = 0; sq < SearchBoard.Size; sq++)
                {
                    PieceKeys[p, sq] = NextULong(rng);
                }
            }
            SideKey = NextULong(rng);
        }

        public static int PieceIndex(int pieceVal)
        {
            // 1..7 → 0..6; -1..-7 → 7..13
            return pieceVal > 0 ? pieceVal - 1 : 6 - pieceVal;
        }

        public static ulong PieceAt(int pieceVal, int sq)
        {
            if (pieceVal == 0) return 0UL;
            return PieceKeys[PieceIndex(pieceVal), sq];
        }

        private static ulong NextULong(Random rng)
        {
            Span<byte> buf = stackalloc byte[8];
            rng.NextBytes(buf);
            return BitConverter.ToUInt64(buf);
        }
    }
}
