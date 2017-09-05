using System;

namespace Common
{
    public static class VectorHelper
    {
        public static double sq(double x)
        {
            return x * x;
        }

        public static double Distance(IIntVector a, IIntVector b, int farAway = 50)
        {
            //double dist = 0;
            //var ka = a.Keys.ToArray();
            //var kb = b.Keys.ToArray();

            //int ia = 0, ib = 0;
            //while (ia < ka.Length && ib < kb.Length)
            //{
            //    var ta = ka[ia];
            //    var tb = kb[ib];
            //    if (ta == tb)
            //    {
            //        dist += sq(a[ta] - b[tb]);
            //        ia++;
            //        ib++;
            //    }
            //    else if (ta < tb)
            //    {
            //        dist += sq(farAway - a[ta]);
            //        ia++;
            //    }
            //    else
            //    {
            //        dist += sq(farAway - b[tb]);
            //        ib++;
            //    }
            //}
            //while (ia < ka.Length)
            //{
            //    var ta = ka[ia];
            //    dist += sq(farAway - a[ta]);
            //    ia++;
            //}
            //while (ib < kb.Length)
            //{
            //    var tb = kb[ib];
            //    dist += sq(farAway - b[tb]);
            //    ib++;
            //}

            double dist = 0;
            foreach (var pp in a)
            {
                var key = pp.Key;
                var value = pp.Value;
                dist += b.ContainsKey(key) ? sq(value - b[key]) : sq(farAway - value);
            }
            foreach (var pp in b)
            {
                if (a.ContainsKey(pp.Key)) continue;
                dist += sq(farAway - pp.Value);
            }
            return Math.Sqrt(dist);
        }
    }
}