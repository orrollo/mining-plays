using System.Collections.Generic;

namespace Common
{
    public class IntVectorEx : SortedList<int,int>, IIntVector
    {
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