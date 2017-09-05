using System;
using System.Linq;
using System.Collections.Generic;

namespace Common
{
    public class MinHashSign : HashSet<uint>
    {
        public MinHashSign()
        {
            
        }

        public MinHashSign(IEnumerable<uint> src) : base(src)
        {
            
        }

        public double Similarity(MinHashSign other)
        {
            return Similarity(this, other);
        }

        public static double Similarity(MinHashSign a, MinHashSign b)
        {
            int ch = a.Count(b.Contains), zn = a.Count + b.Count - ch;
            return zn == 0 ? 0 : ((double)ch)/zn;
        }

        public double Distance(MinHashSign other)
        {
            return Distance(this, other);
        }

        public static double Distance(MinHashSign a, MinHashSign b)
        {
            return (1.0/(0.001 + Similarity(a, b))) - 1.0/1.001;
        }
    }

    public class MinHashing
    {
        public int BitSize { get; protected set; }

        protected delegate uint HashFunction(int value);

        public int Count { get; protected set; }

        protected HashFunction[] Hashes;
        protected uint[] values;

        public uint this[int idx]
        {
            get
            {
                if (idx < 0 || idx >= values.Length) throw new ArgumentException();
                return values[idx];
            }
        }

        public MinHashSign ToSign()
        {
            return new MinHashSign(values);
        }

        // wiki universal hash 
        protected static uint UnivHash(int value, uint a, uint b, int m) { return (a*((uint) value) + b) >> (32 - m); }

        public MinHashing(int bitSize, int signSize) : this(bitSize, signSize, 100)
        {
            
        }

        public MinHashing(int bitSize, int signSize, int rndSeed)
        {
            BitSize = bitSize;
            Count = signSize;
            Hashes = new HashFunction[signSize];
            values = new uint[signSize];
            //
            var rnd = new Random(rndSeed);
            for (int idx = 0; idx < signSize; idx++)
            {
                uint a = 0, b = 0;
                while (a <= 0 || (a & 1) == 0) a = (uint) ((rnd.Next() << 1) + 1);
                while (b <= 0) b = (uint) rnd.Next(0, 1 << bitSize);
                Hashes[idx] = x => UnivHash(x, a, b, bitSize);
            }
            ResetSign();
        }

        public void ResetSign()
        {
            for (int i = 0; i < values.Length; i++) values[i] = uint.MaxValue;
        }

        public void Calculate(params int[] args)
        {
            if (args == null || args.Length == 0) return;
            foreach (var arg in args)
            {
                for (int idx = 0; idx < Hashes.Length; idx++)
                {
                    var hash = Hashes[idx](arg);
                    values[idx] = Math.Min(values[idx], hash);
                }
            }
        }
    }
}