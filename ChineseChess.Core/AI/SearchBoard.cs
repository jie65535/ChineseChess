using System;

namespace ChineseChess.Core.AI
{
    /// <summary>
    /// 搜索专用的紧凑棋盘表示。所有 90 个格子保存在 sbyte[90] 里，
    /// 棋子编码：0 空，+1..+7 红方，-1..-7 黑方，类型按 Piece.* 常量。
    /// 红方为正，黑方为负。SideToMove 也用 +1 / -1 表示。
    /// </summary>
    public sealed class SearchBoard
    {
        public const int Width = 9;
        public const int Height = 10;
        public const int Size = Width * Height;

        public const int Empty = 0;
        public const int King = 1;
        public const int Mandarin = 2;
        public const int Elephant = 3;
        public const int Knight = 4;
        public const int Rook = 5;
        public const int Cannon = 6;
        public const int Pawn = 7;

        public const int Red = 1;
        public const int Black = -1;

        public readonly sbyte[] Squares = new sbyte[Size];
        public int SideToMove;
        public int RedKingSq;
        public int BlackKingSq;
        public ulong ZobristKey;

        public static int Index(int col, int row) => row * Width + col;
        public static int Col(int sq) => sq % Width;
        public static int Row(int sq) => sq / Width;
        public static int ColorOf(int piece) => piece > 0 ? Red : (piece < 0 ? Black : 0);
        public static int TypeOf(int piece) => piece > 0 ? piece : -piece;

        public SearchBoard Clone()
        {
            var b = new SearchBoard
            {
                SideToMove = SideToMove,
                RedKingSq = RedKingSq,
                BlackKingSq = BlackKingSq,
                ZobristKey = ZobristKey,
            };
            Array.Copy(Squares, b.Squares, Size);
            return b;
        }

        /// <summary>
        /// 把 UI 用的 Chessboard 状态拷贝到搜索棋盘
        /// </summary>
        public void LoadFrom(Chessboard board, ChessCamp toMove)
        {
            Array.Clear(Squares, 0, Size);
            RedKingSq = -1;
            BlackKingSq = -1;
            foreach (var c in board.GetChessmen())
            {
                int piece = TypeFromChessType(c.Type);
                int signed = c.Camp == ChessCamp.Red ? piece : -piece;
                int sq = Index(c.Position.Col, c.Position.Row);
                Squares[sq] = (sbyte)signed;
                if (signed == King) RedKingSq = sq;
                else if (signed == -King) BlackKingSq = sq;
            }
            SideToMove = toMove == ChessCamp.Red ? Red : Black;
            ZobristKey = ComputeZobristFromScratch();
        }

        public ulong ComputeZobristFromScratch()
        {
            ulong key = 0;
            for (int sq = 0; sq < Size; sq++)
            {
                int p = Squares[sq];
                if (p != 0) key ^= Zobrist.PieceAt(p, sq);
            }
            if (SideToMove == Black) key ^= Zobrist.SideKey;
            return key;
        }

        public void Make(ref SearchMove m)
        {
            int from = m.From;
            int to = m.To;
            int piece = Squares[from];
            int captured = Squares[to];
            m.Piece = (sbyte)piece;
            m.Captured = (sbyte)captured;

            ZobristKey ^= Zobrist.PieceAt(piece, from);
            if (captured != 0) ZobristKey ^= Zobrist.PieceAt(captured, to);

            Squares[to] = (sbyte)piece;
            Squares[from] = 0;

            ZobristKey ^= Zobrist.PieceAt(piece, to);

            if (piece == King) RedKingSq = to;
            else if (piece == -King) BlackKingSq = to;
            else if (captured == King) RedKingSq = -1;
            else if (captured == -King) BlackKingSq = -1;

            SideToMove = -SideToMove;
            ZobristKey ^= Zobrist.SideKey;
        }

        public void Unmake(in SearchMove m)
        {
            int from = m.From;
            int to = m.To;
            int piece = m.Piece;
            int captured = m.Captured;

            SideToMove = -SideToMove;
            ZobristKey ^= Zobrist.SideKey;

            ZobristKey ^= Zobrist.PieceAt(piece, to);

            Squares[from] = (sbyte)piece;
            Squares[to] = (sbyte)captured;

            ZobristKey ^= Zobrist.PieceAt(piece, from);
            if (captured != 0) ZobristKey ^= Zobrist.PieceAt(captured, to);

            if (piece == King) RedKingSq = from;
            else if (piece == -King) BlackKingSq = from;
            if (captured == King) RedKingSq = to;
            else if (captured == -King) BlackKingSq = to;
        }

        public void MakeNullMove()
        {
            SideToMove = -SideToMove;
            ZobristKey ^= Zobrist.SideKey;
        }

        public void UnmakeNullMove()
        {
            SideToMove = -SideToMove;
            ZobristKey ^= Zobrist.SideKey;
        }

        /// <summary>
        /// 当前一方是否被将军（用于搜索时判断是否需要将军延伸 / 过滤自将）
        /// </summary>
        public bool IsSideInCheck(int color)
        {
            int kingSq = color == Red ? RedKingSq : BlackKingSq;
            if (kingSq < 0) return true;
            return IsSquareAttacked(kingSq, -color);
        }

        /// <summary>
        /// 判断指定格子是否被指定颜色的棋子攻击。
        /// 包含飞将（双方将帅同列且中间无子）。
        /// </summary>
        public bool IsSquareAttacked(int sq, int byColor)
        {
            int sqCol = Col(sq);
            int sqRow = Row(sq);

            // 兵 / 卒
            int pawnVal = (sbyte)(byColor * Pawn);
            int srcRow = sqRow - byColor; // 红：来自南；黑：来自北
            if (srcRow >= 0 && srcRow < Height)
            {
                if (Squares[Index(sqCol, srcRow)] == pawnVal) return true;
            }
            // 过河兵的横走
            bool sidewaysOk = byColor > 0 ? sqRow >= 5 : sqRow <= 4;
            if (sidewaysOk)
            {
                if (sqCol > 0 && Squares[Index(sqCol - 1, sqRow)] == pawnVal) return true;
                if (sqCol < Width - 1 && Squares[Index(sqCol + 1, sqRow)] == pawnVal) return true;
            }

            // 将 / 帅 一步邻接（仅当被攻击格在攻击方九宫内才有意义）
            int kingVal = (sbyte)(byColor * King);
            if (InPalace(sqCol, sqRow, byColor))
            {
                if (sqCol > 0 && Squares[Index(sqCol - 1, sqRow)] == kingVal) return true;
                if (sqCol < Width - 1 && Squares[Index(sqCol + 1, sqRow)] == kingVal) return true;
                if (sqRow > 0 && Squares[Index(sqCol, sqRow - 1)] == kingVal) return true;
                if (sqRow < Height - 1 && Squares[Index(sqCol, sqRow + 1)] == kingVal) return true;
            }

            // 飞将：被攻击格上若有己方将帅，且与对方将帅同列、中间无子，视为受攻击
            int defenderKing = (sbyte)(-byColor * King);
            if (Squares[sq] == defenderKing)
            {
                int otherKingSq = byColor == Red ? RedKingSq : BlackKingSq;
                if (otherKingSq >= 0 && Col(otherKingSq) == sqCol)
                {
                    int r1 = Math.Min(sqRow, Row(otherKingSq)) + 1;
                    int r2 = Math.Max(sqRow, Row(otherKingSq)) - 1;
                    bool clear = true;
                    for (int r = r1; r <= r2; r++)
                    {
                        if (Squares[Index(sqCol, r)] != 0) { clear = false; break; }
                    }
                    if (clear) return true;
                }
            }

            // 士 / 仕（仅九宫内对角一步）
            int mandarinVal = (sbyte)(byColor * Mandarin);
            if (InPalace(sqCol, sqRow, byColor))
            {
                for (int i = 0; i < 4; i++)
                {
                    int dc = (i & 1) == 0 ? -1 : 1;
                    int dr = (i & 2) == 0 ? -1 : 1;
                    int c = sqCol + dc, r = sqRow + dr;
                    if (c < 0 || c >= Width || r < 0 || r >= Height) continue;
                    if (!InPalace(c, r, byColor)) continue;
                    if (Squares[Index(c, r)] == mandarinVal) return true;
                }
            }

            // 象 / 相（不过河，象眼无子）
            bool sqOnAttackerSide = byColor > 0 ? sqRow <= 4 : sqRow >= 5;
            if (sqOnAttackerSide)
            {
                int elephantVal = (sbyte)(byColor * Elephant);
                int[] dcs = { -2, -2, 2, 2 };
                int[] drs = { -2, 2, -2, 2 };
                for (int i = 0; i < 4; i++)
                {
                    int dc = dcs[i], dr = drs[i];
                    int c = sqCol + dc, r = sqRow + dr;
                    if (c < 0 || c >= Width || r < 0 || r >= Height) continue;
                    if (Squares[Index(c, r)] != elephantVal) continue;
                    int eyeC = sqCol + dc / 2, eyeR = sqRow + dr / 2;
                    if (Squares[Index(eyeC, eyeR)] == 0) return true;
                }
            }

            // 马（蹩马腿在 sq + (sign(dc), sign(dr)) 处）
            int knightVal = (sbyte)(byColor * Knight);
            // 8 个候选源格相对于 sq 的偏移
            ReadOnlySpan<int> kdc = stackalloc int[] { 1, 1, -1, -1, 2, 2, -2, -2 };
            ReadOnlySpan<int> kdr = stackalloc int[] { 2, -2, 2, -2, 1, -1, 1, -1 };
            for (int i = 0; i < 8; i++)
            {
                int c = sqCol + kdc[i], r = sqRow + kdr[i];
                if (c < 0 || c >= Width || r < 0 || r >= Height) continue;
                if (Squares[Index(c, r)] != knightVal) continue;
                int legC = sqCol + Math.Sign(kdc[i]);
                int legR = sqRow + Math.Sign(kdr[i]);
                if (Squares[Index(legC, legR)] == 0) return true;
            }

            // 车 / 炮 直线扫描
            int rookVal = (sbyte)(byColor * Rook);
            int cannonVal = (sbyte)(byColor * Cannon);
            ReadOnlySpan<int> ddc = stackalloc int[] { 1, -1, 0, 0 };
            ReadOnlySpan<int> ddr = stackalloc int[] { 0, 0, 1, -1 };
            for (int i = 0; i < 4; i++)
            {
                int dc = ddc[i], dr = ddr[i];
                int c = sqCol + dc, r = sqRow + dr;
                bool foundFirst = false;
                while (c >= 0 && c < Width && r >= 0 && r < Height)
                {
                    int p = Squares[Index(c, r)];
                    if (p != 0)
                    {
                        if (!foundFirst)
                        {
                            if (p == rookVal) return true;
                            foundFirst = true;
                        }
                        else
                        {
                            if (p == cannonVal) return true;
                            break;
                        }
                    }
                    c += dc; r += dr;
                }
            }

            return false;
        }

        public static bool InPalace(int col, int row, int color)
        {
            if (col < 3 || col > 5) return false;
            return color > 0 ? (row >= 0 && row <= 2) : (row >= 7 && row <= 9);
        }

        public static bool OnOwnSide(int row, int color)
            => color > 0 ? row <= 4 : row >= 5;

        private static int TypeFromChessType(ChessType t) => t switch
        {
            ChessType.King => King,
            ChessType.Mandarins => Mandarin,
            ChessType.Elephants => Elephant,
            ChessType.Knights => Knight,
            ChessType.Rooks => Rook,
            ChessType.Cannons => Cannon,
            ChessType.Pawns => Pawn,
            _ => 0,
        };
    }
}
