using System;
using System.Diagnostics;
using System.Threading;

namespace ChineseChess.Core.AI
{
    public struct SearchResult
    {
        public ushort BestMove;
        public int BestFrom => BestMove >> 8;
        public int BestTo => BestMove & 0xFF;
        public int Score;
        public int Depth;
        public long Nodes;
        public TimeSpan Elapsed;
    }

    /// <summary>
    /// 带迭代加深的 alpha-beta + PVS 搜索引擎。
    /// </summary>
    internal sealed class SearchEngine
    {
        private const int Inf = 32000;
        private const int MaxPly = 64;
        private const int MaxBuffer = 128;

        private SearchBoard _board;
        private readonly TranspositionTable _tt;
        private CancellationToken _ct;
        private long _nodes;
        private Stopwatch _sw;
        private int _timeLimitMs;

        // 杀手着法和历史启发表
        private readonly ushort[,] _killers = new ushort[MaxPly, 2];
        private readonly int[,] _history = new int[14, SearchBoard.Size];

        public SearchEngine(int ttSizeBits = 20)
        {
            _tt = new TranspositionTable(ttSizeBits);
        }

        public SearchResult Search(SearchBoard root, int maxDepth, int timeLimitMs, CancellationToken ct)
        {
            _board = root.Clone();
            _ct = ct;
            _nodes = 0;
            _sw = Stopwatch.StartNew();
            _timeLimitMs = timeLimitMs;
            _tt.NewGeneration();
            Array.Clear(_killers, 0, _killers.Length);
            Array.Clear(_history, 0, _history.Length);

            ushort bestMove = 0;
            int bestScore = 0;
            int completedDepth = 0;

            for (int depth = 1; depth <= maxDepth; depth++)
            {
                ushort iterBest = bestMove;
                int score;
                try
                {
                    score = AlphaBetaRoot(depth, ref iterBest);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                bestMove = iterBest;
                bestScore = score;
                completedDepth = depth;

                if (Math.Abs(score) >= Evaluation.MateInMaxPly) break;
                if (TimeUp(true)) break;
            }

            return new SearchResult
            {
                BestMove = bestMove,
                Score = bestScore,
                Depth = completedDepth,
                Nodes = _nodes,
                Elapsed = _sw.Elapsed,
            };
        }

        private bool TimeUp(bool soft)
        {
            if (_ct.IsCancellationRequested) return true;
            if (_timeLimitMs <= 0) return false;
            long elapsed = _sw.ElapsedMilliseconds;
            // 软停时（完成一层后）：超过预算的 60% 即停，避免下一层吞超
            if (soft) return elapsed >= (_timeLimitMs * 60) / 100;
            return elapsed >= _timeLimitMs;
        }

        private void CheckTime()
        {
            if ((_nodes & 4095) == 0 && TimeUp(false))
                throw new OperationCanceledException();
        }

        private int AlphaBetaRoot(int depth, ref ushort bestMove)
        {
            int alpha = -Inf, beta = Inf;
            int ply = 0;
            int color = _board.SideToMove;

            Span<SearchMove> moves = stackalloc SearchMove[MaxBuffer];
            int nMoves = MoveGenerator.Generate(_board, color, moves);
            OrderMoves(moves[..nMoves], bestMove, ply);

            int legalCount = 0;
            int bestScore = -Inf;
            ushort iterBest = bestMove;

            for (int i = 0; i < nMoves; i++)
            {
                var m = moves[i];
                _board.Make(ref m);
                if (_board.IsSideInCheck(color))
                {
                    _board.Unmake(m);
                    continue;
                }
                legalCount++;

                int score;
                if (legalCount == 1)
                {
                    score = -AlphaBeta(depth - 1, -beta, -alpha, ply + 1, true);
                }
                else
                {
                    score = -AlphaBeta(depth - 1, -alpha - 1, -alpha, ply + 1, true);
                    if (score > alpha && score < beta)
                        score = -AlphaBeta(depth - 1, -beta, -alpha, ply + 1, true);
                }
                _board.Unmake(m);

                if (score > bestScore)
                {
                    bestScore = score;
                    iterBest = m.Encode();
                    if (score > alpha)
                    {
                        alpha = score;
                        if (alpha >= beta) break;
                    }
                }
            }

            if (legalCount == 0)
            {
                // 无子可动：被困毙或已被绝杀
                return -Evaluation.MateValue + ply;
            }
            bestMove = iterBest;
            _tt.Store(_board.ZobristKey, depth, bestScore, iterBest, TTBound.Exact, ply);
            return bestScore;
        }

        private int AlphaBeta(int depth, int alpha, int beta, int ply, bool allowNull)
        {
            _nodes++;
            CheckTime();

            int color = _board.SideToMove;
            bool inCheck = _board.IsSideInCheck(color);
            if (inCheck) depth++; // 被将军延伸

            if (depth <= 0) return Quiescence(alpha, beta, ply);

            // TT 探查
            ulong key = _board.ZobristKey;
            ushort ttMove = 0;
            if (_tt.Probe(key, ply, out var ttDepth, out var ttScore, out var ttMv, out var ttFlag))
            {
                ttMove = ttMv;
                if (ttDepth >= depth)
                {
                    if (ttFlag == TTBound.Exact) return ttScore;
                    if (ttFlag == TTBound.Lower && ttScore >= beta) return ttScore;
                    if (ttFlag == TTBound.Upper && ttScore <= alpha) return ttScore;
                }
            }

            // 空着裁剪
            if (allowNull && !inCheck && depth >= 3 && Evaluation.HasMajorPiece(_board, color)
                && Math.Abs(beta) < Evaluation.MateInMaxPly)
            {
                int R = depth >= 6 ? 3 : 2;
                _board.MakeNullMove();
                int nullScore = -AlphaBeta(depth - 1 - R, -beta, -beta + 1, ply + 1, false);
                _board.UnmakeNullMove();
                if (nullScore >= beta)
                {
                    if (nullScore >= Evaluation.MateInMaxPly) nullScore = beta;
                    return nullScore;
                }
            }

            Span<SearchMove> moves = stackalloc SearchMove[MaxBuffer];
            int nMoves = MoveGenerator.Generate(_board, color, moves);
            OrderMoves(moves[..nMoves], ttMove, ply);

            int legalCount = 0;
            int bestScore = -Inf;
            ushort bestMv = 0;
            TTBound bound = TTBound.Upper;

            for (int i = 0; i < nMoves; i++)
            {
                var m = moves[i];
                _board.Make(ref m);
                if (_board.IsSideInCheck(color))
                {
                    _board.Unmake(m);
                    continue;
                }
                legalCount++;

                int score;
                if (legalCount == 1)
                {
                    score = -AlphaBeta(depth - 1, -beta, -alpha, ply + 1, true);
                }
                else
                {
                    score = -AlphaBeta(depth - 1, -alpha - 1, -alpha, ply + 1, true);
                    if (score > alpha && score < beta)
                        score = -AlphaBeta(depth - 1, -beta, -alpha, ply + 1, true);
                }
                _board.Unmake(m);

                if (score > bestScore)
                {
                    bestScore = score;
                    bestMv = m.Encode();
                    if (score > alpha)
                    {
                        alpha = score;
                        bound = TTBound.Exact;
                        if (alpha >= beta)
                        {
                            if (m.Captured == 0)
                            {
                                if (_killers[ply, 0] != bestMv)
                                {
                                    _killers[ply, 1] = _killers[ply, 0];
                                    _killers[ply, 0] = bestMv;
                                }
                                int pIdx = Zobrist.PieceIndex(m.Piece);
                                _history[pIdx, m.To] += depth * depth;
                            }
                            bound = TTBound.Lower;
                            break;
                        }
                    }
                }
            }

            if (legalCount == 0)
            {
                // 无子可动 = 输（中国象棋无和）
                return -Evaluation.MateValue + ply;
            }

            _tt.Store(key, depth, bestScore, bestMv, bound, ply);
            return bestScore;
        }

        private int Quiescence(int alpha, int beta, int ply)
        {
            _nodes++;
            CheckTime();

            int standPat = Evaluation.Evaluate(_board);
            if (standPat >= beta) return beta;
            if (alpha < standPat) alpha = standPat;
            if (ply >= MaxPly - 1) return alpha;

            int color = _board.SideToMove;
            Span<SearchMove> moves = stackalloc SearchMove[MaxBuffer];
            int nMoves = MoveGenerator.Generate(_board, color, moves, capturesOnly: true);
            OrderCaptures(moves[..nMoves]);

            for (int i = 0; i < nMoves; i++)
            {
                var m = moves[i];
                _board.Make(ref m);
                if (_board.IsSideInCheck(color))
                {
                    _board.Unmake(m);
                    continue;
                }
                int score = -Quiescence(-beta, -alpha, ply + 1);
                _board.Unmake(m);

                if (score >= beta) return beta;
                if (score > alpha) alpha = score;
            }
            return alpha;
        }

        private void OrderMoves(Span<SearchMove> moves, ushort ttMove, int ply)
        {
            for (int i = 0; i < moves.Length; i++)
            {
                ref var m = ref moves[i];
                ushort code = m.Encode();
                int score;
                if (code == ttMove)
                {
                    score = 10_000_000;
                }
                else if (m.Captured != 0)
                {
                    int victim = m.Captured > 0 ? m.Captured : -m.Captured;
                    int attacker = m.Piece > 0 ? m.Piece : -m.Piece;
                    score = 1_000_000 + Evaluation.PieceValue[victim] * 16 - Evaluation.PieceValue[attacker];
                }
                else if (code == _killers[ply, 0])
                {
                    score = 900_000;
                }
                else if (code == _killers[ply, 1])
                {
                    score = 800_000;
                }
                else
                {
                    score = _history[Zobrist.PieceIndex(m.Piece), m.To];
                }
                m.Score = score;
            }
            SortByScoreDescending(moves);
        }

        private static void OrderCaptures(Span<SearchMove> moves)
        {
            for (int i = 0; i < moves.Length; i++)
            {
                ref var m = ref moves[i];
                int victim = m.Captured > 0 ? m.Captured : -m.Captured;
                int attacker = m.Piece > 0 ? m.Piece : -m.Piece;
                m.Score = Evaluation.PieceValue[victim] * 16 - Evaluation.PieceValue[attacker];
            }
            SortByScoreDescending(moves);
        }

        private static void SortByScoreDescending(Span<SearchMove> moves)
        {
            // 着法数通常在 30~80 之间，插入排序足够
            for (int i = 1; i < moves.Length; i++)
            {
                var key = moves[i];
                int j = i - 1;
                while (j >= 0 && moves[j].Score < key.Score)
                {
                    moves[j + 1] = moves[j];
                    j--;
                }
                moves[j + 1] = key;
            }
        }
    }
}
