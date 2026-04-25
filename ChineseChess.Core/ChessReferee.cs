using System;
using System.Collections.Generic;
using System.Linq;

namespace ChineseChess.Core
{
    /// <summary>
    /// 中国象棋裁判：负责走子合法性、将军/将死判定与着法生成
    /// </summary>
    public static class ChessReferee
    {
        private static readonly ChessboardPosition[] OrthDirs = new[]
        {
            new ChessboardPosition( 1, 0), new ChessboardPosition(-1, 0),
            new ChessboardPosition( 0, 1), new ChessboardPosition( 0,-1),
        };

        private static readonly ChessboardPosition[] DiagDirs = new[]
        {
            new ChessboardPosition( 1, 1), new ChessboardPosition(-1, 1),
            new ChessboardPosition( 1,-1), new ChessboardPosition(-1,-1),
        };

        /// <summary>
        /// 判断坐标是否落在指定阵营的九宫格内
        /// </summary>
        public static bool InPalace(ChessboardPosition pos, ChessCamp camp)
        {
            if (pos.Col < 3 || pos.Col > 5) return false;
            return camp == ChessCamp.Red ? (pos.Row >= 0 && pos.Row <= 2)
                                         : (pos.Row >= 7 && pos.Row <= 9);
        }

        /// <summary>
        /// 判断坐标是否在指定阵营的本方半场（未过河）
        /// </summary>
        public static bool OnOwnSide(ChessboardPosition pos, ChessCamp camp)
            => camp == ChessCamp.Red ? pos.Row <= 4 : pos.Row >= 5;

        /// <summary>
        /// 列出某棋子所有合法落点（已过滤掉自将的步法）
        /// </summary>
        public static IEnumerable<ChessboardPosition> GetLegalTargets(Chessboard board, Chessman chess)
        {
            if (board == null || chess == null) yield break;
            var pseudo = GetPseudoLegalTargets(board, chess).ToList();
            foreach (var t in pseudo)
            {
                if (!WouldExposeKing(board, chess, t))
                    yield return t;
            }
        }

        /// <summary>
        /// 仅依据棋子走法规则给出落点（不考虑自将）
        /// </summary>
        public static IEnumerable<ChessboardPosition> GetPseudoLegalTargets(Chessboard board, Chessman chess)
        {
            switch (chess.Type)
            {
                case ChessType.King:      return KingTargets(board, chess);
                case ChessType.Mandarins: return MandarinsTargets(board, chess);
                case ChessType.Elephants: return ElephantsTargets(board, chess);
                case ChessType.Knights:   return KnightsTargets(board, chess);
                case ChessType.Rooks:     return RooksTargets(board, chess);
                case ChessType.Cannons:   return CannonsTargets(board, chess);
                case ChessType.Pawns:     return PawnsTargets(board, chess);
                default:                  return Enumerable.Empty<ChessboardPosition>();
            }
        }

        /// <summary>
        /// 根据起点和终点构造合法的 ChessMove，若不合法则返回 null
        /// </summary>
        public static ChessMove TryCreateMove(Chessboard board, ChessboardPosition start, ChessboardPosition end)
        {
            var chess = board.GetChessmanByPos(start);
            if (chess == null) return null;
            if (!GetLegalTargets(board, chess).Contains(end)) return null;
            var killed = board.GetChessmanByPos(end);
            var text = ChessNotation.Format(board, chess, start, end);
            return new ChessMove(chess.Camp, chess.Type, start, end, killed?.Type, text);
        }

        /// <summary>
        /// 指定阵营是否处于被将军状态
        /// </summary>
        public static bool IsInCheck(Chessboard board, ChessCamp camp)
        {
            var king = board.GetChessmenByType(ChessType.King, camp).FirstOrDefault();
            if (king == null) return true;
            var enemies = board.GetChessmen().Where(c => c.Camp != camp).ToList();
            foreach (var enemy in enemies)
            {
                foreach (var p in GetPseudoLegalTargets(board, enemy))
                {
                    if (p == king.Position) return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 指定阵营是否仍存在合法着法（无则将死或困毙）
        /// </summary>
        public static bool HasAnyLegalMove(Chessboard board, ChessCamp camp)
        {
            var pieces = board.GetChessmen().Where(c => c.Camp == camp).ToList();
            foreach (var c in pieces)
            {
                if (GetLegalTargets(board, c).Any()) return true;
            }
            return false;
        }

        private static bool WouldExposeKing(Chessboard board, Chessman chess, ChessboardPosition target)
        {
            var start = chess.Position;
            var captured = board.ApplyMoveSilent(start, target);
            try
            {
                return IsInCheck(board, chess.Camp);
            }
            finally
            {
                board.UndoMoveSilent(start, target, captured);
            }
        }

        // ======= 各兵种伪合法落点 =======

        private static IEnumerable<ChessboardPosition> KingTargets(Chessboard board, Chessman chess)
        {
            foreach (var d in OrthDirs)
            {
                var t = chess.Position + d;
                if (!InPalace(t, chess.Camp)) continue;
                var occ = board.GetChessmanByPos(t);
                if (occ == null || occ.Camp != chess.Camp) yield return t;
            }

            // 飞将：双方将帅同列且中间无子时可视作将吃将
            var enemyKing = board.GetChessmenByType(ChessType.King, chess.Camp.RivalCamp()).FirstOrDefault();
            if (enemyKing != null && enemyKing.Position.Col == chess.Position.Col)
            {
                int minR = Math.Min(chess.Position.Row, enemyKing.Position.Row);
                int maxR = Math.Max(chess.Position.Row, enemyKing.Position.Row);
                bool clear = true;
                for (int r = minR + 1; r < maxR; r++)
                {
                    if (board.GetChessmanByPos(new ChessboardPosition(chess.Position.Col, r)) != null)
                    {
                        clear = false;
                        break;
                    }
                }
                if (clear) yield return enemyKing.Position;
            }
        }

        private static IEnumerable<ChessboardPosition> MandarinsTargets(Chessboard board, Chessman chess)
        {
            foreach (var d in DiagDirs)
            {
                var t = chess.Position + d;
                if (!InPalace(t, chess.Camp)) continue;
                var occ = board.GetChessmanByPos(t);
                if (occ == null || occ.Camp != chess.Camp) yield return t;
            }
        }

        private static IEnumerable<ChessboardPosition> ElephantsTargets(Chessboard board, Chessman chess)
        {
            foreach (var d in DiagDirs)
            {
                var step = new ChessboardPosition(d.Col * 2, d.Row * 2);
                var t = chess.Position + step;
                if (!Chessboard.InBounds(t)) continue;
                if (!OnOwnSide(t, chess.Camp)) continue; // 象不过河
                // 象眼：对角线中点不可有子
                var eye = chess.Position + d;
                if (board.GetChessmanByPos(eye) != null) continue;
                var occ = board.GetChessmanByPos(t);
                if (occ == null || occ.Camp != chess.Camp) yield return t;
            }
        }

        private static IEnumerable<ChessboardPosition> KnightsTargets(Chessboard board, Chessman chess)
        {
            // 8 种 L 型走法，每个走法都对应一个蹩马腿位置
            var moves = new[]
            {
                new ValueTuple<ChessboardPosition, ChessboardPosition>(new ChessboardPosition( 1,  2), new ChessboardPosition( 0,  1)),
                new ValueTuple<ChessboardPosition, ChessboardPosition>(new ChessboardPosition(-1,  2), new ChessboardPosition( 0,  1)),
                new ValueTuple<ChessboardPosition, ChessboardPosition>(new ChessboardPosition( 1, -2), new ChessboardPosition( 0, -1)),
                new ValueTuple<ChessboardPosition, ChessboardPosition>(new ChessboardPosition(-1, -2), new ChessboardPosition( 0, -1)),
                new ValueTuple<ChessboardPosition, ChessboardPosition>(new ChessboardPosition( 2,  1), new ChessboardPosition( 1,  0)),
                new ValueTuple<ChessboardPosition, ChessboardPosition>(new ChessboardPosition( 2, -1), new ChessboardPosition( 1,  0)),
                new ValueTuple<ChessboardPosition, ChessboardPosition>(new ChessboardPosition(-2,  1), new ChessboardPosition(-1,  0)),
                new ValueTuple<ChessboardPosition, ChessboardPosition>(new ChessboardPosition(-2, -1), new ChessboardPosition(-1,  0)),
            };
            foreach (var pair in moves)
            {
                var t = chess.Position + pair.Item1;
                if (!Chessboard.InBounds(t)) continue;
                var leg = chess.Position + pair.Item2;
                if (board.GetChessmanByPos(leg) != null) continue; // 蹩马腿
                var occ = board.GetChessmanByPos(t);
                if (occ == null || occ.Camp != chess.Camp) yield return t;
            }
        }

        private static IEnumerable<ChessboardPosition> RooksTargets(Chessboard board, Chessman chess)
        {
            foreach (var d in OrthDirs)
            {
                var t = chess.Position;
                while (true)
                {
                    t += d;
                    if (!Chessboard.InBounds(t)) break;
                    var occ = board.GetChessmanByPos(t);
                    if (occ == null)
                    {
                        yield return t;
                    }
                    else
                    {
                        if (occ.Camp != chess.Camp) yield return t;
                        break;
                    }
                }
            }
        }

        private static IEnumerable<ChessboardPosition> CannonsTargets(Chessboard board, Chessman chess)
        {
            foreach (var d in OrthDirs)
            {
                var t = chess.Position;
                bool hopped = false;
                while (true)
                {
                    t += d;
                    if (!Chessboard.InBounds(t)) break;
                    var occ = board.GetChessmanByPos(t);
                    if (!hopped)
                    {
                        if (occ == null) yield return t;
                        else hopped = true;
                    }
                    else
                    {
                        if (occ != null)
                        {
                            if (occ.Camp != chess.Camp) yield return t;
                            break;
                        }
                    }
                }
            }
        }

        private static IEnumerable<ChessboardPosition> PawnsTargets(Chessboard board, Chessman chess)
        {
            int forward = chess.Camp == ChessCamp.Red ? 1 : -1;
            // 向前
            var f = chess.Position + new ChessboardPosition(0, forward);
            if (Chessboard.InBounds(f))
            {
                var occ = board.GetChessmanByPos(f);
                if (occ == null || occ.Camp != chess.Camp) yield return f;
            }
            // 过河后可横走
            bool acrossRiver = chess.Camp == ChessCamp.Red ? chess.Position.Row >= 5 : chess.Position.Row <= 4;
            if (acrossRiver)
            {
                foreach (var dCol in new[] { 1, -1 })
                {
                    var t = chess.Position + new ChessboardPosition(dCol, 0);
                    if (!Chessboard.InBounds(t)) continue;
                    var occ = board.GetChessmanByPos(t);
                    if (occ == null || occ.Camp != chess.Camp) yield return t;
                }
            }
        }
    }
}
