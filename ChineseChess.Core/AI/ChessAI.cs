using System;
using System.Threading;
using System.Threading.Tasks;

namespace ChineseChess.Core.AI
{
    /// <summary>
    /// 一次思考的结果（已转回 UI 用的坐标）
    /// </summary>
    public class AIDecision
    {
        public ChessboardPosition From { get; set; }
        public ChessboardPosition To { get; set; }
        public int Score { get; set; }
        public int Depth { get; set; }
        public long Nodes { get; set; }
        public TimeSpan Elapsed { get; set; }
        public bool IsValid { get; set; }
    }

    /// <summary>
    /// AI 难度档位与对应搜索预算
    /// </summary>
    public class AISettings
    {
        public int MaxDepth { get; set; } = 6;
        public int TimeMs { get; set; } = 1500;

        public static AISettings Easy => new AISettings { MaxDepth = 3, TimeMs = 300 };
        public static AISettings Medium => new AISettings { MaxDepth = 7, TimeMs = 1500 };
        public static AISettings Hard => new AISettings { MaxDepth = 12, TimeMs = 5000 };
    }

    /// <summary>
    /// 公开的 AI 入口。线程安全要求：同一实例不要并发 Think。
    /// </summary>
    public sealed class ChessAI
    {
        private readonly SearchEngine _engine;

        public ChessAI(int ttSizeBits = 20)
        {
            _engine = new SearchEngine(ttSizeBits);
        }

        public AIDecision Think(Chessboard board, ChessCamp toMove, AISettings settings, CancellationToken ct = default)
        {
            if (settings == null) settings = AISettings.Medium;

            var sb = new SearchBoard();
            sb.LoadFrom(board, toMove);

            var result = _engine.Search(sb, settings.MaxDepth, settings.TimeMs, ct);

            // 0 是哨兵值（从格 0 到格 0 不是合法着法）
            if (result.BestMove == 0)
            {
                return new AIDecision { IsValid = false, Depth = result.Depth, Nodes = result.Nodes, Elapsed = result.Elapsed };
            }

            int from = result.BestMove >> 8;
            int to = result.BestMove & 0xFF;
            return new AIDecision
            {
                From = new ChessboardPosition(from % SearchBoard.Width, from / SearchBoard.Width),
                To = new ChessboardPosition(to % SearchBoard.Width, to / SearchBoard.Width),
                Score = result.Score,
                Depth = result.Depth,
                Nodes = result.Nodes,
                Elapsed = result.Elapsed,
                IsValid = true,
            };
        }

        public Task<AIDecision> ThinkAsync(Chessboard board, ChessCamp toMove, AISettings settings, CancellationToken ct = default)
        {
            // 把 UI 数据快照下来再投到线程池，避免和 UI 线程的 Chessboard 操作竞争
            var sb = new SearchBoard();
            sb.LoadFrom(board, toMove);
            return Task.Run(() => ThinkSnapshot(sb, settings ?? AISettings.Medium, ct), ct);
        }

        private AIDecision ThinkSnapshot(SearchBoard sb, AISettings settings, CancellationToken ct)
        {
            var result = _engine.Search(sb, settings.MaxDepth, settings.TimeMs, ct);
            if (result.BestMove == 0)
                return new AIDecision { IsValid = false, Depth = result.Depth, Nodes = result.Nodes, Elapsed = result.Elapsed };
            int from = result.BestMove >> 8;
            int to = result.BestMove & 0xFF;
            return new AIDecision
            {
                From = new ChessboardPosition(from % SearchBoard.Width, from / SearchBoard.Width),
                To = new ChessboardPosition(to % SearchBoard.Width, to / SearchBoard.Width),
                Score = result.Score,
                Depth = result.Depth,
                Nodes = result.Nodes,
                Elapsed = result.Elapsed,
                IsValid = true,
            };
        }
    }
}
