namespace ChineseChess.Core
{
    public class Chessman
    {
        public Chessman(ChessType type, ChessCamp camp, ChessboardPosition position)
        {
            Type=type;
            Camp=camp;
            Position=position;
        }

        public ChessType Type { get; set; }
        public ChessCamp Camp { get; set; }
        public ChessboardPosition Position { get; set; }
    }
}