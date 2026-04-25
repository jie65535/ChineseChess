namespace ChineseChess.Core.AI
{
    public enum TTBound : byte
    {
        None = 0,
        Exact = 1,
        Lower = 2,  // alpha-cutoff failed-high; stored score is a lower bound
        Upper = 3,  // beta-cutoff failed-low; stored score is an upper bound
    }

    /// <summary>
    /// 简化置换表：定容数组 + 索引取低位。深度优先 + 总是替换混合策略。
    /// 注意将杀分值在存/取时按 ply 修正，以保证不同搜索路径下取出的距离一致。
    /// </summary>
    internal sealed class TranspositionTable
    {
        private struct Entry
        {
            public ulong Key;
            public short Score;
            public ushort BestMove;
            public byte Depth;
            public TTBound Flag;
            public byte Generation;
        }

        private readonly Entry[] _entries;
        private readonly ulong _mask;
        private byte _generation;

        public TranspositionTable(int sizeBits = 20)
        {
            int size = 1 << sizeBits;
            _entries = new Entry[size];
            _mask = (ulong)(size - 1);
        }

        public void NewGeneration() => _generation++;

        public void Store(ulong key, int depth, int score, ushort bestMove, TTBound flag, int ply)
        {
            ref var e = ref _entries[key & _mask];
            // 替换策略：优先保留同代深度更大的，否则替换
            if (e.Key == key && e.Depth > depth && e.Generation == _generation)
                return;
            int adj = score;
            if (adj >= Evaluation.MateInMaxPly) adj += ply;
            else if (adj <= -Evaluation.MateInMaxPly) adj -= ply;
            e.Key = key;
            e.Score = (short)adj;
            e.BestMove = bestMove;
            e.Depth = (byte)depth;
            e.Flag = flag;
            e.Generation = _generation;
        }

        public bool Probe(ulong key, int ply, out int depth, out int score, out ushort move, out TTBound flag)
        {
            var e = _entries[key & _mask];
            if (e.Key != key)
            {
                depth = 0; score = 0; move = 0; flag = TTBound.None;
                return false;
            }
            depth = e.Depth;
            int s = e.Score;
            if (s >= Evaluation.MateInMaxPly) s -= ply;
            else if (s <= -Evaluation.MateInMaxPly) s += ply;
            score = s;
            move = e.BestMove;
            flag = e.Flag;
            return true;
        }
    }
}
