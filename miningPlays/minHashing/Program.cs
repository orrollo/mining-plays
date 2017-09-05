using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;

namespace minHashing
{
    class Program
    {
        static void Main(string[] args)
        {
            string subdir = "books";
            var fileName = string.Format("{0}.vec", subdir);

            var mh = new MinHashing(16, 15);

            var cnts = new List<int>();

            var dic = new Dictionary<int, MinHashSign>();
            Helper.InputTextReader(Helper.GetDataPath(fileName), line =>
                {
                    var arr = line.Split(new[] { ':' }, 2);
                    var groups = arr[1].Split(',');
                    cnts.Add(groups.Length);

                    int code = int.Parse(arr[0]);

                    mh.ResetSign();

                    foreach (var group in groups)
                    {
                        var data = group.Split(':');
                        int id = int.Parse(data[0]), rate = int.Parse(data[1]);
                        mh.Calculate(id, rate);
                    }
                    dic[code] = mh.ToSign();
                    return true;
                });

            Console.WriteLine("average group cnt: {0}", cnts.Average());

            var keys = dic.Keys.ToArray();
            var dd = new List<double>();
            var limit = 1000;
            for (int i = 0; i < limit; i++)
            {
                var k1 = keys[i];
                for (int j = i + 1; j < limit; j++)
                {
                    var k2 = keys[j];
                    var d = dic[k1].Distance(dic[k2]);
                    dd.Add(d);
                }
            }
            var avg = dd.Average();
            var avgSq = dd.Select(x => x*x).Average();
            var norm = Math.Sqrt(avgSq - avg * avg);

            var centers = new List<MinHashSign>();
            var items = new Dictionary<int, List<int>>();
            var radius = avg / 2.0;

            Console.WriteLine("radius: {0}; norm: {1}", radius, norm);
            Console.WriteLine("collecting centers...");
            int count = 0;

            foreach (var pair in dic)
            {
                int bestIdx = -1;
                double bestDist = 0;
                for (int i = 0; i < centers.Count; i++)
                {
                    var d = centers[i].Distance(pair.Value);
                    if (d > radius) continue;
                    if (bestIdx != -1 && d >= bestDist) continue;
                    bestIdx = i;
                    bestDist = d;
                }
                if (bestIdx == -1)
                {
                    bestIdx = centers.Count;
                    centers.Add(pair.Value);
                    items[bestIdx] = new List<int>();
                }
                items[bestIdx].Add(pair.Key);

                count++;
                if ((count % 1000) == 0) Console.WriteLine("lines: {0}; centers: {1}", count, centers.Count);
            }

            for (int i = items.Count-1; i >= 0; i--) Console.WriteLine("group {0} count: {1}", i, items[i].Count);
            //
            Console.WriteLine("press enter...");
            Console.ReadLine();
        }
    }
}
