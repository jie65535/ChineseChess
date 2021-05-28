using System;
using System.Collections.Generic;
using System.Linq;

namespace ChineseChess.Core
{
    public class Chessboard
    {
        private readonly Stack<ChessMove> Moves = new Stack<ChessMove>();

        private readonly List<Chessman> AlivingChessman = new List<Chessman>(32);

        public event EventHandler<ChessmanMovedEventArgs> ChessmanMovedEvent;

        private void OnChessmanMoved(Chessman chessman, Chessman chessmanKilled)
            => ChessmanMovedEvent?.Invoke(this, new ChessmanMovedEventArgs(chessman, chessmanKilled));

        public Chessman GetChessmanByPos(ChessboardPosition position)
            => AlivingChessman.FirstOrDefault(c => c.Position == position);

        public IEnumerable<Chessman> GetChessmenByType(ChessType type, ChessCamp camp)
            => AlivingChessman.Where(chess => chess.Type == type && chess.Camp == camp);

        public IEnumerable<Chessman> GetChessmen() => AlivingChessman;

        public IEnumerable<ChessMove> GetMoves() => Moves;

        /// <summary>
        /// 移动棋子到目标位置，若目标位置存在棋子，则移除
        /// </summary>
        /// <param name="chessman">棋子</param>
        /// <param name="target">目标位置</param>
        /// <exception cref="MoveException">无法将棋子移动到己方棋子上</exception>
        private void MoveChessman(Chessman chessman, ChessboardPosition target)
        {
            var tar = GetChessmanByPos(target);
            if (tar != null)
            {
                if (chessman.Camp == tar.Camp)
                    throw new MoveException("无法将棋子移动到己方棋子上");

                AlivingChessman.Remove(tar);
            }
            chessman.Position = target;
            OnChessmanMoved(chessman, tar);
        }

        /// <summary>
        /// 移动一步
        /// </summary>
        /// <param name="move">移动方式</param>
        /// <exception cref="MoveException">未找到要进行移动的棋子</exception>
        public void PushMove(ChessMove move)
        {
            var chessman = GetChessmanByPos(move.Start);
            if (chessman == null)
                throw new MoveException("未找到要进行移动的棋子");
            MoveChessman(chessman, move.End);
            Moves.Push(move);
        }

        /// <summary>
        /// 退回上一步
        /// </summary>
        /// <exception cref="MoveException">已经退回到初始局面 or 未找到要进行移动的棋子</exception>
        public void PopMove()
        {
            if (Moves.Count == 0)
                throw new MoveException("已经退回到初始局面");
            var move = Moves.Pop();

            var chessman = GetChessmanByPos(move.End);
            if (chessman == null)
                throw new MoveException("未找到要进行移动的棋子");
            MoveChessman(chessman, move.Start);
            if (move.Killed != null)
                AlivingChessman.Add(new Chessman((ChessType)move.Killed, chessman.Camp.RivalCamp(), move.End));
        }
    }

    public class ChessmanMovedEventArgs : EventArgs
    {
        public ChessmanMovedEventArgs(Chessman chessman, Chessman chessmanKilled)
        {
            Chessman = chessman;
            ChessmanKilled = chessmanKilled;
        }

        public Chessman Chessman { get; set; }
        public Chessman ChessmanKilled { get; set; }
    }

    public class MoveException : Exception
    {
        public MoveException(string message) : base(message)
        {
        }

        public MoveException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public MoveException() : this("移动失败")
        {
        }
    }
}