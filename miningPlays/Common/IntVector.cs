using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Common
{
    public class IntVector : Dictionary<int,int>
    {
        public int Group { get; set; }

        public IntVector(int group)
        {
            Group = group;
        }

        public static double sq(double x)
        {
            return x*x;
        }

        public double Distance(IntVector other)
        {
            double dist = 0;
            foreach (var pp in this)
            {
                var key = pp.Key;
                var value = this[key];
                dist += other.ContainsKey(key) ? sq(value - other[key]) : sq(50 - value);
            }
            foreach (var pp in other)
            {
                if (this.ContainsKey(pp.Key)) continue;
                dist += sq(50 - pp.Value);
            }
            return Math.Sqrt(dist);
        }
    }
}