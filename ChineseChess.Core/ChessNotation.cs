using System;
using System.Collections.Generic;
using System.Linq;

namespace ChineseChess.Core
{
    /// <summary>
    /// 中国象棋记谱辅助：把一步走子转换成可读的中文文本
    /// 简化版本，不处理同列两子需 前/后 区分的极少数特殊情形
    /// </summary>
    public static class ChessNotation
    {
        private static readonly string[] CnDigits = { "", "一", "二", "三", "四", "五", "六", "七", "八", "九" };

        /// <summary>
        /// 将一步走子格式化为中文记谱字符串，例如 "炮二平五"、"马８进７"
        /// </summary>
        public static string Format(Chessboard board, Chessman chess, ChessboardPosition start, ChessboardPosition end)
        {
            char name = chess.Type.GetName(chess.Camp);
            string from = ColumnLabel(chess.Camp, start.Col);

            // 同列同型棋子用 前/后 简单区分
            string prefix = "";
            var sameCol = board.GetChessmen(chess.Camp)
                                .Where(c => c.Type == chess.Type && c.Position.Col == start.Col)
                                .ToList();
            if (sameCol.Count >= 2)
            {
                bool isFront = chess.Camp == ChessCamp.Red
                    ? chess.Position.Row == sameCol.Max(c => c.Position.Row)
                    : chess.Position.Row == sameCol.Min(c => c.Position.Row);
                prefix = isFront ? "前" : "后";
                from = "";
            }

            char dir;
            int distance;
            if (start.Row == end.Row)
            {
                dir = '平';
                distance = ColumnNumber(chess.Camp, end.Col);
            }
            else
            {
                bool forward = chess.Camp == ChessCamp.Red ? end.Row > start.Row : end.Row < start.Row;
                dir = forward ? '进' : '退';

                if (chess.Type == ChessType.Rooks
                    || chess.Type == ChessType.Cannons
                    || chess.Type == ChessType.King
                    || chess.Type == ChessType.Pawns)
                {
                    distance = Math.Abs(end.Row - start.Row);
                }
                else
                {
                    // 斜走子（士、象、马）记目标列
                    distance = ColumnNumber(chess.Camp, end.Col);
                }
            }

            string distText = chess.Camp == ChessCamp.Red ? CnDigits[distance] : distance.ToString();
            return $"{prefix}{name}{from}{dir}{distText}";
        }

        private static int ColumnNumber(ChessCamp camp, int col)
        {
            // 红方从右到左为一到九，黑方从左到右为 1 到 9
            return camp == ChessCamp.Red ? (Chessboard.Width - col) : (col + 1);
        }

        private static string ColumnLabel(ChessCamp camp, int col)
        {
            int n = ColumnNumber(camp, col);
            return camp == ChessCamp.Red ? CnDigits[n] : n.ToString();
        }
    }
}
