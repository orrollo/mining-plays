using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Common
{
    public class IntVector : Dictionary<int,int>, IIntVector
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

        public IntVector(int group)
        {
            Group = group;
        }

        public double Distance(IIntVector other)
        {
            return VectorHelper.Distance(this, other);
        }
    }
}