using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Common;

namespace randomIndexes
{
    class Program
    {

        static double CosSim(short[] a, short[] b, double vectorNorm = 100.0)
        {
            var mx = Math.Min(a.Length, b.Length);
            double rez = 0;
            for (int i = 0; i < mx; i++) rez += a[i]*b[i];
            return rez/(vectorNorm*vectorNorm);
        }

        static double CosDist(short[] a, short[] b, double vectorNorm = 100.0)
        {
            return 1.0 / (0.01 + Math.Abs(CosSim(a, b)));
        }

        static void Main(string[] args)
        {
            var subdir = "books";

            var fileName = string.Format("{0}.vec", subdir);

            int vectorSize = 200, onesSize = 10;
            var termVectors = new Dictionary<int, short[]>();

            var dataPath = Path.Combine(Helper.GetDataPath(subdir), "rnd");
            if (!Directory.Exists(dataPath)) Directory.CreateDirectory(dataPath);
            foreach (var file in Directory.GetFiles(dataPath, "*.rnd")) File.Delete(file);

            int count = 0;
            Helper.InputTextReader(Helper.GetDataPath(fileName), line =>
                {
                    var rvec = BuildRandomVector(vectorSize, onesSize);
                    var arr = line.Split(new[] { ':' }, 2);
                    var groups = arr[1].Split(',');
                    if (groups.Length >= 1)
                    {
                        var docVector = new double[vectorSize];
                        foreach (var group in groups)
                        {
                            var data = group.Split(':');
                            int id = int.Parse(data[0]);
                            //double rate = double.Parse(data[1])/5;
                            // look for random rate vector
                            if (!termVectors.ContainsKey(id)) termVectors[id] = new short[vectorSize];
                            for (int i = 0; i < vectorSize; i++) termVectors[id][i] += rvec[i];
                        }
                    }
                    count++;
                    if ((count % 1000) == 0) Console.WriteLine("lines: {0}", count);
                    return true;
                });
            // norm lengths
            Console.WriteLine("writing results");
            count = 0;
            var vectorNorm = 100.0;
            using (var wrt = new StreamWriter(Path.Combine(dataPath, "rezult.rnd")))
            {
                foreach (var vector in termVectors)
                {
                    var value = vector.Value;
                    var len = Math.Sqrt(value.Select(x => ((double)x) * x).Sum());
                    for (int i = 0; i < value.Length; i++)
                    {
                        value[i] = (short)Math.Round(vectorNorm * value[i] / len);
                    }
                    wrt.Write("{0:d7}:", vector.Key);
                    wrt.WriteLine(string.Join(";", value.Select(x => x.ToString())));
                    count++;
                    if ((count % 1000) == 0) Console.WriteLine("lines: {0}", count);
                }
            }
            // 
            var keys = termVectors.Keys.ToArray();
            var dd = new List<double>();
            for (int i = 0; i < 100; i++)
            {
                var key1 = keys[i];
                for (int j = i + 1; j < 100; j++)
                {
                    var key2 = keys[j];
                    dd.Add(CosDist(termVectors[key1], termVectors[key2]));
                }
            }
            double avg = dd.Average();
            double mx = dd.Max();
            double mn = dd.Min();
            double avgSq = dd.Select(x => x*x).Average();
            double norm = Math.Sqrt(avgSq - avg*avg);
            double maxDist = avg + 3*norm;
            //
            var centers = new List<short[]>();
            var items = new Dictionary<int, List<int>>();
            var radius = avg / 4.0;
            Console.WriteLine("collecting centers...");
            count = 0;
            foreach (var pair in termVectors)
            {
                int bestIdx = -1;
                double bestDist = 0;
                for (int i = 0; i < centers.Count; i++)
                {
                    var s = CosSim(pair.Value, centers[i]);
                    var d = CosDist(pair.Value, centers[i]);
                    if (d > radius) continue;
                    if (bestIdx != -1 && d >= bestDist) continue;
                    bestIdx = i;
                    bestDist = d;
                }
                if (bestIdx == -1)
                {
                    bestIdx = centers.Count;
                    centers.Add(pair.Value);
                }
                if (!items.ContainsKey(bestIdx)) items[bestIdx] = new List<int>();
                items[bestIdx].Add(pair.Key);
                count++;
                if ((count % 1000) == 0) Console.WriteLine("lines: {0}; centers: {1}", count, centers.Count);
            }
            //
            for (int i = 0; i < items.Count; i++) Console.WriteLine("group {0} count: {1}", i, items[i].Count);
            //
            Console.WriteLine("press enter...");
            Console.ReadLine();
        }

        static Random rnd = new Random();

        private static short[] BuildRandomVector(int vectorSize, int onesSize)
        {
            var vector = new short[vectorSize];
            for (int idx = 0; idx < onesSize; idx++)
            {
                int n = rnd.Next(0, vector.Length);
                while (vector[n] != 0) n = rnd.Next(0, vector.Length);
                vector[n] = (short) (rnd.Next(1000) < 500 ? -1 : 1);
            }
            return vector;
        }
    }
}
