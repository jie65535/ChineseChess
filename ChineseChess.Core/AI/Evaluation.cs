namespace ChineseChess.Core.AI
{
    /// <summary>
    /// 静态局面评估：物质 + 各兵种 piece-square table。
    /// 所有 PST 以红方视角给出（row 0 = 红方底线，row 9 = 黑方底线）；
    /// 黑方使用按 row 镜像后的格子。
    /// </summary>
    internal static class Evaluation
    {
        // 物质分（按 type 索引：1=King, 2=Mandarin, 3=Elephant, 4=Knight, 5=Rook, 6=Cannon, 7=Pawn）
        public static readonly int[] PieceValue = new int[8]
        {
            0,        // 0: empty (unused)
            10000,    // 1: King （仅用于必杀计算时引用，正常评估不计）
            120,      // 2: Mandarin
            120,      // 3: Elephant
            270,      // 4: Knight
            600,      // 5: Rook
            285,      // 6: Cannon
            30,       // 7: Pawn (基准值，过河后由 PST 大幅加成)
        };

        // Mate 分值，远大于任何盘面物质评估的可能上限。
        public const int MateValue = 30000;
        public const int MateInMaxPly = MateValue - 1024;

        // ====== Piece-Square Tables (红方视角，[row, col]，共 90 项) ======

        // 兵卒：跨河后大幅升值，进入对方腹地最佳；中央列略优
        private static readonly int[] PawnPST = new int[]
        {
            // Row 0
            0,0,0,0,0,0,0,0,0,
            // Row 1
            0,0,0,0,0,0,0,0,0,
            // Row 2
            0,0,0,0,0,0,0,0,0,
            // Row 3 (兵起始线)
            -2,0,4,0,6,0,4,0,-2,
            // Row 4
            -2,0,8,0,8,0,8,0,-2,
            // Row 5 (刚过河)
            14,18,20,26,28,26,20,18,14,
            // Row 6
            22,30,32,34,38,34,32,30,22,
            // Row 7
            28,36,42,46,50,46,42,36,28,
            // Row 8 (近九宫)
            32,42,48,54,58,54,48,42,32,
            // Row 9 (敌方底线)
            18,22,26,30,32,30,26,22,18,
        };

        // 马：中心强，被困边角弱
        private static readonly int[] KnightPST = new int[]
        {
            -4,4,4,6,4,6,4,4,-4,
            4,8,16,12,4,12,16,8,4,
            4,12,18,18,12,18,18,12,4,
            6,8,16,16,16,16,16,8,6,
            8,12,18,16,20,16,18,12,8,
            12,16,20,20,24,20,20,16,12,
            14,18,24,28,30,28,24,18,14,
            16,22,30,32,34,32,30,22,16,
            14,20,28,30,32,30,28,20,14,
            6,14,16,20,20,20,16,14,6,
        };

        // 车：到处都强，向前推进略加分；中路、肋道更优
        private static readonly int[] RookPST = new int[]
        {
            -2,8,4,12,16,12,4,8,-2,
            8,12,16,18,18,18,16,12,8,
            6,10,12,14,14,14,12,10,6,
            6,8,10,16,16,16,10,8,6,
            8,12,16,18,18,18,16,12,8,
            10,14,18,20,20,20,18,14,10,
            14,18,20,22,22,22,20,18,14,
            16,20,22,24,26,24,22,20,16,
            16,20,22,26,28,26,22,20,16,
            14,18,20,24,26,24,20,18,14,
        };

        // 炮：中线和宫顶炮位重要，避免低效边路
        private static readonly int[] CannonPST = new int[]
        {
            6,4,0,-10,-12,-10,0,4,6,
            2,2,0,-4,-14,-4,0,2,2,
            2,2,0,-10,-8,-10,0,2,2,
            0,0,-2,4,10,4,-2,0,0,
            0,0,0,4,8,4,0,0,0,
            -2,0,4,6,10,6,4,0,-2,
            0,0,0,2,8,2,0,0,0,
            -4,-4,0,4,10,4,0,-4,-4,
            -6,-6,0,4,12,4,0,-6,-6,
            -4,-4,0,8,16,8,0,-4,-4,
        };

        // 将/帅：留在九宫、最好窝在底线中位
        private static readonly int[] KingPST = new int[]
        {
            0,0,0,12,18,12,0,0,0,
            0,0,0,4,10,4,0,0,0,
            0,0,0,-6,-8,-6,0,0,0,
            0,0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,0,
        };

        // 士：仅九宫合法格有微小区分（中心比四角略优）
        private static readonly int[] MandarinPST = new int[]
        {
            0,0,0,0,0,0,0,0,0,
            0,0,0,0,4,0,0,0,0,
            0,0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,0,
        };

        // 象：典型三七象、田字眼略优，象心略弱
        private static readonly int[] ElephantPST = new int[]
        {
            0,0,4,0,0,0,4,0,0,
            0,0,0,0,0,0,0,0,0,
            2,0,0,0,6,0,0,0,2,
            0,0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,0,
        };

        private static readonly int[][] PSTByType = new int[][]
        {
            null,           // 0
            KingPST,        // 1
            MandarinPST,    // 2
            ElephantPST,    // 3
            KnightPST,      // 4
            RookPST,        // 5
            CannonPST,      // 6
            PawnPST,        // 7
        };

        /// <summary>
        /// 评估局面，返回相对 SideToMove 的分值（越大越对当前一方有利）
        /// </summary>
        public static int Evaluate(SearchBoard board)
        {
            int score = 0;
            for (int sq = 0; sq < SearchBoard.Size; sq++)
            {
                int p = board.Squares[sq];
                if (p == 0) continue;
                int type = p > 0 ? p : -p;
                int idx = p > 0 ? sq : MirrorSq(sq);
                int v = PieceValue[type] + PSTByType[type][idx];
                score += p > 0 ? v : -v;
            }
            return board.SideToMove > 0 ? score : -score;
        }

        /// <summary>
        /// 返回除士、象、将以外是否还有其它子（用于决定空着裁剪是否安全）
        /// </summary>
        public static bool HasMajorPiece(SearchBoard board, int color)
        {
            for (int sq = 0; sq < SearchBoard.Size; sq++)
            {
                int p = board.Squares[sq];
                if (p == 0) continue;
                if ((p > 0 ? 1 : -1) != color) continue;
                int type = p > 0 ? p : -p;
                if (type == SearchBoard.Rook || type == SearchBoard.Cannon
                    || type == SearchBoard.Knight || type == SearchBoard.Pawn)
                    return true;
            }
            return false;
        }

        private static int MirrorSq(int sq)
        {
            int col = sq % SearchBoard.Width;
            int row = sq / SearchBoard.Width;
            return (SearchBoard.Height - 1 - row) * SearchBoard.Width + col;
        }
    }
}
