using System;

namespace ChineseChess.Core.AI
{
    /// <summary>
    /// 高速伪合法走子生成器。结果写入调用方提供的 Span，避免内存分配。
    /// "伪合法"指未过滤"自将"的步法；search 在 Make 之后用 IsSquareAttacked 排除非法。
    /// </summary>
    internal static class MoveGenerator
    {
        // 4 个正交方向的偏移
        private static readonly int[] OrthDc = { 1, -1, 0, 0 };
        private static readonly int[] OrthDr = { 0, 0, 1, -1 };

        // 4 个对角方向
        private static readonly int[] DiagDc = { 1, 1, -1, -1 };
        private static readonly int[] DiagDr = { 1, -1, 1, -1 };

        // 8 个马步
        private static readonly int[] KnightDc = { 1, 1, -1, -1, 2, 2, -2, -2 };
        private static readonly int[] KnightDr = { 2, -2, 2, -2, 1, -1, 1, -1 };

        public static int Generate(SearchBoard board, int color, Span<SearchMove> buffer, bool capturesOnly = false)
        {
            int count = 0;
            for (int sq = 0; sq < SearchBoard.Size; sq++)
            {
                int piece = board.Squares[sq];
                if (piece == 0) continue;
                int pieceColor = piece > 0 ? 1 : -1;
                if (pieceColor != color) continue;
                int type = piece > 0 ? piece : -piece;

                switch (type)
                {
                    case SearchBoard.King: GenKing(board, sq, color, buffer, ref count, capturesOnly); break;
                    case SearchBoard.Mandarin: GenMandarin(board, sq, color, buffer, ref count, capturesOnly); break;
                    case SearchBoard.Elephant: GenElephant(board, sq, color, buffer, ref count, capturesOnly); break;
                    case SearchBoard.Knight: GenKnight(board, sq, color, buffer, ref count, capturesOnly); break;
                    case SearchBoard.Rook: GenRook(board, sq, color, buffer, ref count, capturesOnly); break;
                    case SearchBoard.Cannon: GenCannon(board, sq, color, buffer, ref count, capturesOnly); break;
                    case SearchBoard.Pawn: GenPawn(board, sq, color, buffer, ref count, capturesOnly); break;
                }
            }
            return count;
        }

        private static void Add(Span<SearchMove> buf, ref int count, int from, int to, int piece, int captured, bool capturesOnly)
        {
            if (capturesOnly && captured == 0) return;
            buf[count++] = new SearchMove(from, to, piece, captured);
        }

        private static void GenKing(SearchBoard b, int sq, int color, Span<SearchMove> buf, ref int count, bool capOnly)
        {
            int sCol = SearchBoard.Col(sq), sRow = SearchBoard.Row(sq);
            int piece = b.Squares[sq];
            for (int i = 0; i < 4; i++)
            {
                int c = sCol + OrthDc[i], r = sRow + OrthDr[i];
                if (!SearchBoard.InPalace(c, r, color)) continue;
                int t = SearchBoard.Index(c, r);
                int target = b.Squares[t];
                if (target != 0 && (target > 0 ? 1 : -1) == color) continue;
                Add(buf, ref count, sq, t, piece, target, capOnly);
            }
        }

        private static void GenMandarin(SearchBoard b, int sq, int color, Span<SearchMove> buf, ref int count, bool capOnly)
        {
            int sCol = SearchBoard.Col(sq), sRow = SearchBoard.Row(sq);
            int piece = b.Squares[sq];
            for (int i = 0; i < 4; i++)
            {
                int c = sCol + DiagDc[i], r = sRow + DiagDr[i];
                if (!SearchBoard.InPalace(c, r, color)) continue;
                int t = SearchBoard.Index(c, r);
                int target = b.Squares[t];
                if (target != 0 && (target > 0 ? 1 : -1) == color) continue;
                Add(buf, ref count, sq, t, piece, target, capOnly);
            }
        }

        private static void GenElephant(SearchBoard b, int sq, int color, Span<SearchMove> buf, ref int count, bool capOnly)
        {
            int sCol = SearchBoard.Col(sq), sRow = SearchBoard.Row(sq);
            int piece = b.Squares[sq];
            for (int i = 0; i < 4; i++)
            {
                int dc = DiagDc[i] * 2, dr = DiagDr[i] * 2;
                int c = sCol + dc, r = sRow + dr;
                if (c < 0 || c >= SearchBoard.Width || r < 0 || r >= SearchBoard.Height) continue;
                if (!SearchBoard.OnOwnSide(r, color)) continue; // 象不过河
                // 象眼
                int eye = SearchBoard.Index(sCol + DiagDc[i], sRow + DiagDr[i]);
                if (b.Squares[eye] != 0) continue;
                int t = SearchBoard.Index(c, r);
                int target = b.Squares[t];
                if (target != 0 && (target > 0 ? 1 : -1) == color) continue;
                Add(buf, ref count, sq, t, piece, target, capOnly);
            }
        }

        private static void GenKnight(SearchBoard b, int sq, int color, Span<SearchMove> buf, ref int count, bool capOnly)
        {
            int sCol = SearchBoard.Col(sq), sRow = SearchBoard.Row(sq);
            int piece = b.Squares[sq];
            for (int i = 0; i < 8; i++)
            {
                int dc = KnightDc[i], dr = KnightDr[i];
                int c = sCol + dc, r = sRow + dr;
                if (c < 0 || c >= SearchBoard.Width || r < 0 || r >= SearchBoard.Height) continue;
                // 蹩马腿：从源格朝 (sign(dc), sign(dr)) 一步
                int legC, legR;
                if (Math.Abs(dr) == 2)
                {
                    legC = sCol;
                    legR = sRow + dr / 2;
                }
                else
                {
                    legC = sCol + dc / 2;
                    legR = sRow;
                }
                if (b.Squares[SearchBoard.Index(legC, legR)] != 0) continue;
                int t = SearchBoard.Index(c, r);
                int target = b.Squares[t];
                if (target != 0 && (target > 0 ? 1 : -1) == color) continue;
                Add(buf, ref count, sq, t, piece, target, capOnly);
            }
        }

        private static void GenRook(SearchBoard b, int sq, int color, Span<SearchMove> buf, ref int count, bool capOnly)
        {
            int sCol = SearchBoard.Col(sq), sRow = SearchBoard.Row(sq);
            int piece = b.Squares[sq];
            for (int i = 0; i < 4; i++)
            {
                int dc = OrthDc[i], dr = OrthDr[i];
                int c = sCol + dc, r = sRow + dr;
                while (c >= 0 && c < SearchBoard.Width && r >= 0 && r < SearchBoard.Height)
                {
                    int t = SearchBoard.Index(c, r);
                    int target = b.Squares[t];
                    if (target == 0)
                    {
                        Add(buf, ref count, sq, t, piece, 0, capOnly);
                    }
                    else
                    {
                        if ((target > 0 ? 1 : -1) != color)
                            Add(buf, ref count, sq, t, piece, target, capOnly);
                        break;
                    }
                    c += dc; r += dr;
                }
            }
        }

        private static void GenCannon(SearchBoard b, int sq, int color, Span<SearchMove> buf, ref int count, bool capOnly)
        {
            int sCol = SearchBoard.Col(sq), sRow = SearchBoard.Row(sq);
            int piece = b.Squares[sq];
            for (int i = 0; i < 4; i++)
            {
                int dc = OrthDc[i], dr = OrthDr[i];
                int c = sCol + dc, r = sRow + dr;
                bool hopped = false;
                while (c >= 0 && c < SearchBoard.Width && r >= 0 && r < SearchBoard.Height)
                {
                    int t = SearchBoard.Index(c, r);
                    int target = b.Squares[t];
                    if (!hopped)
                    {
                        if (target == 0)
                        {
                            Add(buf, ref count, sq, t, piece, 0, capOnly);
                        }
                        else
                        {
                            hopped = true;
                        }
                    }
                    else
                    {
                        if (target != 0)
                        {
                            if ((target > 0 ? 1 : -1) != color)
                                Add(buf, ref count, sq, t, piece, target, capOnly);
                            break;
                        }
                    }
                    c += dc; r += dr;
                }
            }
        }

        private static void GenPawn(SearchBoard b, int sq, int color, Span<SearchMove> buf, ref int count, bool capOnly)
        {
            int sCol = SearchBoard.Col(sq), sRow = SearchBoard.Row(sq);
            int piece = b.Squares[sq];
            int forwardR = sRow + color; // 红 +1 黑 -1
            if (forwardR >= 0 && forwardR < SearchBoard.Height)
            {
                int t = SearchBoard.Index(sCol, forwardR);
                int target = b.Squares[t];
                if (target == 0 || (target > 0 ? 1 : -1) != color)
                    Add(buf, ref count, sq, t, piece, target, capOnly);
            }
            bool acrossRiver = color > 0 ? sRow >= 5 : sRow <= 4;
            if (acrossRiver)
            {
                if (sCol > 0)
                {
                    int t = SearchBoard.Index(sCol - 1, sRow);
                    int target = b.Squares[t];
                    if (target == 0 || (target > 0 ? 1 : -1) != color)
                        Add(buf, ref count, sq, t, piece, target, capOnly);
                }
                if (sCol < SearchBoard.Width - 1)
                {
                    int t = SearchBoard.Index(sCol + 1, sRow);
                    int target = b.Squares[t];
                    if (target == 0 || (target > 0 ? 1 : -1) != color)
                        Add(buf, ref count, sq, t, piece, target, capOnly);
                }
            }
        }
    }
}
