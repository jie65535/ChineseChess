namespace ChineseChess.Core.AI
{
    /// <summary>
    /// 搜索引擎内部使用的紧凑走子表示
    /// </summary>
    public struct SearchMove
    {
        public byte From;
        public byte To;
        /// <summary>被吃子（搜索内部编码：0 表示无吃子，正数为红方棋子，负数为黑方棋子）</summary>
        public sbyte Captured;
        /// <summary>被移动的棋子编码（用于撤销）</summary>
        public sbyte Piece;
        /// <summary>移动排序得分（不参与正确性，仅用于排序）</summary>
        public int Score;

        public SearchMove(int from, int to, int piece, int captured)
        {
            From = (byte)from;
            To = (byte)to;
            Piece = (sbyte)piece;
            Captured = (sbyte)captured;
            Score = 0;
        }

        public ushort Encode() => (ushort)((From << 8) | To);

        public override string ToString() => $"{From}->{To}";
    }
}
