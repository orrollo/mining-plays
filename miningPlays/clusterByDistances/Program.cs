using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;

namespace clusterByDistances
{
    class Program
    {
        static void Main(string[] args)
        {
            double dist, norm;
            var fileName = "users.vec";
            TeachDistances(fileName, 100, out dist, out norm);

            dist += 2*norm;

            Console.WriteLine("average distance = {0}", dist);

            // collecting centers
            var centers = new List<IntVector>();
            var groups = new Dictionary<int, List<int>>();
            var count = 0;
            var list = new List<IntVector>();
            Helper.InputTextVectors(fileName, vector =>
                {
                    if (vector.Count >= 5)
                    {
                        bool found = false;
                        var vectorGroup = vector.Group;

                        list.Clear();
                        var processorCount = Environment.ProcessorCount;
                        Parallel.For(0, processorCount,
                                     new ParallelOptions() {MaxDegreeOfParallelism = processorCount},
                                     index =>
                                         {
                                             for (int i = 0; i < centers.Count; i++)
                                             {
                                                 if ((i%processorCount) != index) continue;
                                                 var center = centers[i];
                                                 var d = center.Distance(vector);
                                                 if (d > dist) continue;
                                                 lock (list) list.Add(center);
                                                 break;
                                             }
                                         });
                        if (list.Count == 0)
                        {
                            centers.Add(vector);
                            groups[vectorGroup] = new List<int> {vectorGroup};
                        }
                        else
                        {
                            int idx = 0;
                            double best = list[0].Distance(vector);
                            for (int j = 1; j < list.Count; j++)
                            {
                                var d = list[j].Distance(vector);
                                if (d >= best) continue;
                                idx = j;
                                best = d;
                            }
                            var center = list[idx];
                            groups[center.Group].Add(vectorGroup);
                        }

                    }
                    count++;
                    if ((count % 1000) == 0)
                        Console.WriteLine("processed {0} vectors, found {1} centers", count, centers.Count);
                    return true;
                });
            // now we have "centers" set
            Console.WriteLine("extracted {0} centers", centers.Count);

            // need reduce (join) centers here

            Console.ReadLine();
        }

        private static void TeachDistances(string fileName, int sampleCount, out double distance, out double norm)
        {
            var vectors = new List<IntVector>();
            Helper.InputTextVectors(fileName, vec =>
                {
                    vectors.Add(vec);
                    return vectors.Count < sampleCount;
                });
            int count = 0;
            distance = 0;
            double sqr = 0;
            for (int i = 0; i < vectors.Count; i++)
            {
                for (int j = i + 1; j < vectors.Count; j++)
                {
                    var d = vectors[i].Distance(vectors[j]);
                    distance += d;
                    sqr += d*d;
                    count++;
                }
            }
            if (count > 0)
            {
                distance /= count;
                sqr /= count;
            }
            norm = Math.Sqrt(sqr - distance*distance);
        }
    }
}
