﻿using System;

namespace ChineseChess.Core
{
    /// <summary>
    /// 棋子移动步骤
    /// </summary>
    public class ChessMove
    {
        private ChessMove()
        {

        }

        public static ChessMove GenMove(Chessboard chessboard, string move)
        {
            throw new NotImplementedException();
        }

        public static ChessMove GenMove(Chessboard chessboard, ChessCamp camp, ChessboardPosition start, ChessboardPosition end)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 阵营
        /// </summary>
        public ChessCamp Camp { get; }

        /// <summary>
        /// 移动的棋子
        /// </summary>
        public ChessType Chess { get; }

        /// <summary>
        /// 击杀的棋子 可为空
        /// </summary>
        public ChessType? Killed { get; }

        /// <summary>
        /// 起点位置
        /// </summary>
        public ChessboardPosition Start { get; }

        /// <summary>
        /// 终点位置
        /// </summary>
        public ChessboardPosition End { get; }

        /// <summary>
        /// 文本格式
        /// </summary>
        public string Text { get; }
    }
}