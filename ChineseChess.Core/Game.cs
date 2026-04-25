using System;

namespace ChineseChess.Core
{
    /// <summary>
    /// 一局棋的状态机：维护当前阵营、胜负、将军等状态
    /// </summary>
    public class Game
    {
        public Chessboard Chessboard { get; } = new Chessboard();

        /// <summary>
        /// 当前轮到哪一方走子
        /// </summary>
        public ChessCamp CurrentCamp { get; private set; }

        /// <summary>
        /// 胜方，未分胜负时为 null
        /// </summary>
        public ChessCamp? Winner { get; private set; }

        /// <summary>
        /// 当前轮到的一方是否被将军
        /// </summary>
        public bool IsInCheck { get; private set; }

        /// <summary>
        /// 终局原因
        /// </summary>
        public GameResult Result { get; private set; }

        public bool IsGameOver => Winner.HasValue || Result != GameResult.None;

        /// <summary>
        /// 任意状态变更（落子、悔棋、复位）后触发
        /// </summary>
        public event EventHandler StateChanged;

        public Game()
        {
            Reset();
        }

        public void Reset()
        {
            Chessboard.ResetChessboard();
            CurrentCamp = ChessCamp.Red;
            Winner = null;
            Result = GameResult.None;
            IsInCheck = false;
            StateChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 尝试由当前阵营走一步棋
        /// </summary>
        /// <returns>是否成功</returns>
        public bool TryMove(ChessboardPosition start, ChessboardPosition end, out string error)
        {
            error = null;
            if (IsGameOver)
            {
                error = "对局已结束";
                return false;
            }
            var chess = Chessboard.GetChessmanByPos(start);
            if (chess == null)
            {
                error = "起点没有棋子";
                return false;
            }
            if (chess.Camp != CurrentCamp)
            {
                error = "现在不是你方走子";
                return false;
            }
            var move = ChessReferee.TryCreateMove(Chessboard, start, end);
            if (move == null)
            {
                error = "不合法的走子";
                return false;
            }

            bool kingCaptured = move.Killed == ChessType.King;
            Chessboard.PushMove(move);

            if (kingCaptured)
            {
                Winner = chess.Camp;
                Result = GameResult.Checkmate;
                IsInCheck = false;
            }
            else
            {
                var next = CurrentCamp.RivalCamp();
                CurrentCamp = next;
                IsInCheck = ChessReferee.IsInCheck(Chessboard, next);
                if (!ChessReferee.HasAnyLegalMove(Chessboard, next))
                {
                    Winner = next.RivalCamp();
                    Result = IsInCheck ? GameResult.Checkmate : GameResult.Stalemate;
                }
            }

            StateChanged?.Invoke(this, EventArgs.Empty);
            return true;
        }

        /// <summary>
        /// 悔棋一步
        /// </summary>
        public bool Undo()
        {
            if (Chessboard.MoveCount == 0) return false;
            Chessboard.PopMove();
            CurrentCamp = CurrentCamp.RivalCamp();
            Winner = null;
            Result = GameResult.None;
            IsInCheck = ChessReferee.IsInCheck(Chessboard, CurrentCamp);
            StateChanged?.Invoke(this, EventArgs.Empty);
            return true;
        }
    }

    public enum GameResult
    {
        None,
        /// <summary>将死</summary>
        Checkmate,
        /// <summary>困毙（无子可动）</summary>
        Stalemate,
    }
}
