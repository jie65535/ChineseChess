namespace ChineseChess.Core
{
    /// <summary>
    /// 棋子阵营
    /// </summary>
    public enum ChessCamp
    {
        /// <summary>
        /// 红方
        /// </summary>
        Red,

        /// <summary>
        /// 黑方
        /// </summary>
        Black,
    }

    public static class ChessCampExtensions
    {
        public static ChessCamp RivalCamp(this ChessCamp camp)
            => camp == ChessCamp.Red ? ChessCamp.Black : ChessCamp.Red;
    }
}