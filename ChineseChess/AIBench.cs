using System;
using System.IO;

using ChineseChess.Core;
using ChineseChess.Core.AI;

namespace ChineseChess
{
    /// <summary>
    /// AI 自检工具：跑几个标准局面，记录到 ai_bench.log。
    /// 通过 "ChineseChess.exe --ai-bench" 触发。
    /// </summary>
    internal static class AIBench
    {
        public static void Run()
        {
            string logPath = Path.Combine(AppContext.BaseDirectory, "ai_bench.log");
            using (var sw = new StreamWriter(logPath, false))
            {
                sw.WriteLine($"AIBench @ {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                sw.WriteLine();

                // 1. 起手红方
                var game = new Game();
                RunCase(sw, "Opening / Red to move", game.Chessboard, ChessCamp.Red, AISettings.Medium);

                // 2. 起手黑方（AI 应招）
                game.Chessboard.PushMove(ChessReferee.TryCreateMove(game.Chessboard,
                    new ChessboardPosition(1, 2), new ChessboardPosition(4, 2)));
                RunCase(sw, "After 炮二平五 / Black to move", game.Chessboard, ChessCamp.Black, AISettings.Medium);

                // 3. 困难档位也跑一遍，看是否能在限时内多搜几层
                var game3 = new Game();
                RunCase(sw, "Opening / Red / HARD", game3.Chessboard, ChessCamp.Red, AISettings.Hard);
            }
            // 同步把路径输出到 stderr（不一定能看到，但试一下）
            try { Console.Error.WriteLine($"ai_bench.log written to {logPath}"); } catch { }
        }

        private static void RunCase(StreamWriter sw, string label, Chessboard board, ChessCamp toMove, AISettings settings)
        {
            sw.WriteLine($"== {label} (depth<={settings.MaxDepth}, time<={settings.TimeMs}ms) ==");
            var ai = new ChessAI();
            var d = ai.Think(board, toMove, settings);
            if (!d.IsValid)
            {
                sw.WriteLine("  (AI 没有给出走子)");
            }
            else
            {
                var chess = board.GetChessmanByPos(d.From);
                string name = chess != null ? chess.Type.GetName(chess.Camp).ToString() : "?";
                sw.WriteLine($"  Move : {name} ({d.From.Col},{d.From.Row}) -> ({d.To.Col},{d.To.Row})");
                sw.WriteLine($"  Score: {d.Score}");
                sw.WriteLine($"  Depth: {d.Depth}");
                sw.WriteLine($"  Nodes: {d.Nodes:N0}");
                sw.WriteLine($"  Time : {d.Elapsed.TotalMilliseconds:F0} ms");
                sw.WriteLine($"  NPS  : {(d.Elapsed.TotalSeconds > 0 ? (long)(d.Nodes / d.Elapsed.TotalSeconds) : 0):N0} nodes/s");
            }
            sw.WriteLine();
        }
    }
}
