namespace ChineseChess.Core
{
    public struct ChessboardPosition
    {
        public int Col;
        public int Row;

        public ChessboardPosition(int col, int row)
        {
            Col=col;
            Row=row;
        }

        public override bool Equals(object obj)
        {
            return obj is ChessboardPosition pos 
                && pos.Col == this.Col 
                && pos.Row == this.Row;
        }

        public override int GetHashCode()
        {
            return Col ^ Row;
        }

        public override string ToString()
        {
            return $"({Row}, {Col})";
        }

        public static bool operator ==(ChessboardPosition left, ChessboardPosition right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ChessboardPosition left, ChessboardPosition right)
        {
            return !(left==right);
        }

        public static ChessboardPosition operator +(ChessboardPosition left, ChessboardPosition right)
        {
            return new ChessboardPosition(left.Col + right.Col, left.Row + right.Row);
        }

        public static ChessboardPosition operator -(ChessboardPosition left, ChessboardPosition right)
        {
            return new ChessboardPosition(left.Col - right.Col, left.Row - right.Row);
        }


    }
}