using System.Collections.Generic;

namespace ChineseChess.Core
{
    public static class ChessTypeExtensions
    {
        private readonly static Dictionary<ChessType, char> RedChess;
        private readonly static Dictionary<ChessType, char> BlackChess;
        private readonly static Dictionary<char, ChessType> ChessNames;

        static ChessTypeExtensions()
        {
            RedChess = new Dictionary<ChessType, char>
            {
                [ChessType.King     ] = '帅',
                [ChessType.Mandarins] = '仕',
                [ChessType.Elephants] = '相',
                [ChessType.Knights  ] = '马',
                [ChessType.Rooks    ] = '车',
                [ChessType.Cannons  ] = '炮',
                [ChessType.Pawns    ] = '兵',
            };
            BlackChess = new Dictionary<ChessType, char>
            {
                [ChessType.King     ] = '将',
                [ChessType.Mandarins] = '士',
                [ChessType.Elephants] = '象',
                [ChessType.Knights  ] = '马',
                [ChessType.Rooks    ] = '车',
                [ChessType.Cannons  ] = '炮',
                [ChessType.Pawns    ] = '卒',
            };

            ChessNames = new Dictionary<char, ChessType>
            {
                ['帅'] = ChessType.King,
                ['将'] = ChessType.King,
                ['仕'] = ChessType.Mandarins,
                ['士'] = ChessType.Mandarins,
                ['相'] = ChessType.Elephants,
                ['象'] = ChessType.Elephants,
                ['马'] = ChessType.Knights,
                ['车'] = ChessType.Rooks,
                ['炮'] = ChessType.Cannons,
                ['兵'] = ChessType.Pawns,
                ['卒'] = ChessType.Pawns,
            };
        }

        /// <summary>
        /// 获取棋子名称
        /// </summary>
        /// <param name="type">棋子类型</param>
        /// <param name="camp">棋子阵营</param>
        /// <returns></returns>
        public static char GetName(this ChessType type, ChessCamp camp)
        {
            return camp == ChessCamp.Red ? RedChess[type] : BlackChess[type];
        }

        /// <summary>
        /// Parses the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public static ChessType Parse(char name)
        {
            return ChessNames[name];
        }

        /// <summary>
        /// Tries the parse.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static bool TryParse(char name, out ChessType type)
        {
            return ChessNames.TryGetValue(name, out type);
        }
    }
}