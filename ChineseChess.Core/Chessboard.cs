using System;
using System.Collections.Generic;
using System.Linq;

namespace ChineseChess.Core
{
    public class Chessboard
    {
        /// <summary>
        /// 棋盘宽度（列数）
        /// </summary>
        public const int Width = 9;

        /// <summary>
        /// 棋盘高度（行数）
        /// </summary>
        public const int Height = 10;

        private readonly Stack<ChessMove> Moves = new Stack<ChessMove>();

        private readonly List<Chessman> AlivingChessman = new List<Chessman>(32);

        public event EventHandler<ChessmanMovedEventArgs> ChessmanMovedEvent;

        public event EventHandler ChessboardResetEvent;

        private void OnChessmanMoved(Chessman chessman, Chessman chessmanKilled)
            => ChessmanMovedEvent?.Invoke(this, new ChessmanMovedEventArgs(chessman, chessmanKilled));

        public Chessman GetChessmanByPos(ChessboardPosition position)
            => AlivingChessman.FirstOrDefault(c => c.Position == position);

        public IEnumerable<Chessman> GetChessmenByType(ChessType type, ChessCamp camp)
            => AlivingChessman.Where(chess => chess.Type == type && chess.Camp == camp);

        public IEnumerable<Chessman> GetChessmen() => AlivingChessman;

        public IEnumerable<Chessman> GetChessmen(ChessCamp camp) => AlivingChessman.Where(c => c.Camp == camp);

        public IEnumerable<ChessMove> GetMoves() => Moves;

        public ChessMove LastMove => Moves.Count == 0 ? null : Moves.Peek();

        public int MoveCount => Moves.Count;

        /// <summary>
        /// 静态判断坐标是否在棋盘内
        /// </summary>
        public static bool InBounds(ChessboardPosition pos)
            => pos.Col >= 0 && pos.Col < Width && pos.Row >= 0 && pos.Row < Height;

        public void ResetChessboard()
        {
            Moves.Clear();
            AlivingChessman.Clear();
            AlivingChessman.Add(new Chessman(ChessType.King     , ChessCamp.Red, new ChessboardPosition(4, 0)));
            AlivingChessman.Add(new Chessman(ChessType.Mandarins, ChessCamp.Red, new ChessboardPosition(3, 0)));
            AlivingChessman.Add(new Chessman(ChessType.Mandarins, ChessCamp.Red, new ChessboardPosition(5, 0)));
            AlivingChessman.Add(new Chessman(ChessType.Knights  , ChessCamp.Red, new ChessboardPosition(1, 0)));
            AlivingChessman.Add(new Chessman(ChessType.Knights  , ChessCamp.Red, new ChessboardPosition(7, 0)));
            AlivingChessman.Add(new Chessman(ChessType.Elephants, ChessCamp.Red, new ChessboardPosition(2, 0)));
            AlivingChessman.Add(new Chessman(ChessType.Elephants, ChessCamp.Red, new ChessboardPosition(6, 0)));
            AlivingChessman.Add(new Chessman(ChessType.Rooks    , ChessCamp.Red, new ChessboardPosition(0, 0)));
            AlivingChessman.Add(new Chessman(ChessType.Rooks    , ChessCamp.Red, new ChessboardPosition(8, 0)));
            AlivingChessman.Add(new Chessman(ChessType.Cannons  , ChessCamp.Red, new ChessboardPosition(1, 2)));
            AlivingChessman.Add(new Chessman(ChessType.Cannons  , ChessCamp.Red, new ChessboardPosition(7, 2)));
            AlivingChessman.Add(new Chessman(ChessType.Pawns    , ChessCamp.Red, new ChessboardPosition(0, 3)));
            AlivingChessman.Add(new Chessman(ChessType.Pawns    , ChessCamp.Red, new ChessboardPosition(2, 3)));
            AlivingChessman.Add(new Chessman(ChessType.Pawns    , ChessCamp.Red, new ChessboardPosition(4, 3)));
            AlivingChessman.Add(new Chessman(ChessType.Pawns    , ChessCamp.Red, new ChessboardPosition(6, 3)));
            AlivingChessman.Add(new Chessman(ChessType.Pawns    , ChessCamp.Red, new ChessboardPosition(8, 3)));

            AlivingChessman.Add(new Chessman(ChessType.King     , ChessCamp.Black, new ChessboardPosition(4, 9)));
            AlivingChessman.Add(new Chessman(ChessType.Mandarins, ChessCamp.Black, new ChessboardPosition(3, 9)));
            AlivingChessman.Add(new Chessman(ChessType.Mandarins, ChessCamp.Black, new ChessboardPosition(5, 9)));
            AlivingChessman.Add(new Chessman(ChessType.Knights  , ChessCamp.Black, new ChessboardPosition(1, 9)));
            AlivingChessman.Add(new Chessman(ChessType.Knights  , ChessCamp.Black, new ChessboardPosition(7, 9)));
            AlivingChessman.Add(new Chessman(ChessType.Elephants, ChessCamp.Black, new ChessboardPosition(2, 9)));
            AlivingChessman.Add(new Chessman(ChessType.Elephants, ChessCamp.Black, new ChessboardPosition(6, 9)));
            AlivingChessman.Add(new Chessman(ChessType.Rooks    , ChessCamp.Black, new ChessboardPosition(0, 9)));
            AlivingChessman.Add(new Chessman(ChessType.Rooks    , ChessCamp.Black, new ChessboardPosition(8, 9)));
            AlivingChessman.Add(new Chessman(ChessType.Cannons  , ChessCamp.Black, new ChessboardPosition(1, 7)));
            AlivingChessman.Add(new Chessman(ChessType.Cannons  , ChessCamp.Black, new ChessboardPosition(7, 7)));
            AlivingChessman.Add(new Chessman(ChessType.Pawns    , ChessCamp.Black, new ChessboardPosition(0, 6)));
            AlivingChessman.Add(new Chessman(ChessType.Pawns    , ChessCamp.Black, new ChessboardPosition(2, 6)));
            AlivingChessman.Add(new Chessman(ChessType.Pawns    , ChessCamp.Black, new ChessboardPosition(4, 6)));
            AlivingChessman.Add(new Chessman(ChessType.Pawns    , ChessCamp.Black, new ChessboardPosition(6, 6)));
            AlivingChessman.Add(new Chessman(ChessType.Pawns    , ChessCamp.Black, new ChessboardPosition(8, 6)));

            ChessboardResetEvent?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 移动棋子到目标位置，若目标位置存在棋子，则移除
        /// </summary>
        /// <param name="chessman">棋子</param>
        /// <param name="target">目标位置</param>
        /// <exception cref="MoveException">无法将棋子移动到己方棋子上</exception>
        private Chessman MoveChessman(Chessman chessman, ChessboardPosition target)
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
            return tar;
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
        public ChessMove PopMove()
        {
            if (Moves.Count == 0)
                throw new MoveException("已经退回到初始局面");
            var move = Moves.Pop();

            var chessman = GetChessmanByPos(move.End);
            if (chessman == null)
                throw new MoveException("未找到要进行移动的棋子");
            MoveChessman(chessman, move.Start);
            if (move.Killed != null)
            {
                var revived = new Chessman(move.Killed.Value, chessman.Camp.RivalCamp(), move.End);
                AlivingChessman.Add(revived);
                OnChessmanMoved(revived, null);
            }
            return move;
        }

        /// <summary>
        /// 静默地应用一步移动（不记录、不触发事件），用于规则模拟
        /// </summary>
        internal Chessman ApplyMoveSilent(ChessboardPosition start, ChessboardPosition end)
        {
            var chess = GetChessmanByPos(start);
            var captured = GetChessmanByPos(end);
            if (captured != null) AlivingChessman.Remove(captured);
            chess.Position = end;
            return captured;
        }

        /// <summary>
        /// 撤销 ApplyMoveSilent 产生的修改
        /// </summary>
        internal void UndoMoveSilent(ChessboardPosition start, ChessboardPosition end, Chessman captured)
        {
            var chess = GetChessmanByPos(end);
            chess.Position = start;
            if (captured != null) AlivingChessman.Add(captured);
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
