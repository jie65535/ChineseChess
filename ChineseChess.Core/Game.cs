using System;

namespace ChineseChess.Core
{
    public class Game
    {
        public Chessboard Chessboard { get; } = new Chessboard();

        public Game()
        {
            Chessboard.ResetChessboard();
        }

    }
}
