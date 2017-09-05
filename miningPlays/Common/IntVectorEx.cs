using System.Collections.Generic;

namespace Common
{
    public class IntVectorEx : SortedList<int,int>, IIntVector
    {
        private double? _length = null;
        public double Length
        {
            get
            {
                if (_length == null) _length = Helper.GetLength(this);
                return _length.Value;
            }
            set { _length = value; }
        }

        public int Group { get; set; }

        public IntVectorEx(int group)
        {
            Group = group;
        }

        public double Distance(IIntVector other)
        {
            return VectorHelper.Distance(this, other);
        }
    }
}