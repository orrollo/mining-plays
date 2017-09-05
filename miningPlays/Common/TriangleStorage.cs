using System;

namespace Common
{
    public class TriangleStorage
    {
        private readonly double[] _data;
        private readonly int _width;
        private readonly int _k;

        protected int getIndex(int x, int y)
        {
            if (x == y) throw new ArgumentException();
            if (x >= _width) throw new ArgumentException();
            if (y >= _width) throw new ArgumentException();
            if (x < y) return getIndex(y, x);
            return ((_k - (y - 1))*y >> 1) + (x - y - 1);
        }

        public TriangleStorage(int width)
        {
            _width = width;
            int size = (width*(width - 1)) >> 1;
            _k = 2*(width - 1);
            _data = new double[size];
        }

        public double this[int row, int col]
        {
            get { return  col==row ? 0 : _data[getIndex(col, row)]; }
            set { _data[getIndex(col, row)] = value; }
        }
    }
}