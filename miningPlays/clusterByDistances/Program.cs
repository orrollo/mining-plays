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

            var minVectorLength = 5;
            Console.WriteLine("average distance = {0}; minimum vector length = {1}", dist, minVectorLength);

            // collecting centers
            var centers = new List<VectorCenter>();
            var count = 0;
            var list = new List<VectorCenter>();
            Helper.InputTextVectors(fileName, vector =>
                {
                    if (vector.Count >= minVectorLength)
                    {
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
                            centers.Add(new VectorCenter(vector));
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
                            center.Items.Add(vector.Group);
                        }
                    }
                    count++;
                    if ((count % 1000) == 0)
                        Console.WriteLine("processed {0} vectors, found {1} centers", count, centers.Count);
                    //return count < 30000;
                    return true;
                });
            // now we have "centers" set
            Console.WriteLine("extracted {0} centers", centers.Count);

            // need reduce (join) centers here
            var distMax = 3 * dist;
            Console.WriteLine("processing vector distances matrix...");
            var matr = CalcCentersDistances(centers);

            var groupCount = new List<int>();
            for (int steps = 0; steps < 30; steps++)
            {
                Console.WriteLine("distance {0}; process grouping...", distMax);
                var grouped = GroupByDistance(centers, distMax, matr);
                groupCount.Add(grouped.Count);
                Console.WriteLine("groups with distance limit {0} is {1}", distMax, groupCount.Last());
                if (steps > 0 && groupCount[steps] == groupCount[steps - 1])
                {
                    Console.WriteLine("group count is not changed, so stop grouping.");
                    break;
                }
                distMax = distMax * 1.1;
            }
            Console.WriteLine("enter to exit...");
            Console.ReadLine();
        }

        private static Dictionary<int, List<VectorCenter>> GroupByDistance(List<VectorCenter> centers, double distMax, TriangleStorage matr)
        {
            int cgroups = 0;
            var groups = new Dictionary<VectorCenter, int>();
            
            double bestDist = 0;
            for (int idx = 0; idx < centers.Count; idx++)
            {
                if (groups.ContainsKey(centers[idx])) continue;
                int bestIndex = -1;
                // looking for closest
                for (int sidx = 0; sidx < centers.Count; sidx++)
                {
                    if (idx == sidx) continue;
                    var d = matr[idx, sidx];
                    if (d > distMax || (bestIndex != -1 && bestDist <= d)) continue;
                    bestIndex = sidx;
                    bestDist = d;
                }
                // if not found - new group
                if (bestIndex == -1)
                {
                    cgroups++;
                    groups[centers[idx]] = cgroups;
                }
                else if (!groups.ContainsKey(centers[bestIndex]))
                {
                    cgroups++;
                    groups[centers[idx]] = cgroups;
                    groups[centers[bestIndex]] = cgroups;
                }
                else
                {
                    groups[centers[idx]] = groups[centers[bestIndex]];
                }
            }
            var grouped = new Dictionary<int, List<VectorCenter>>();
            foreach (var pair in groups)
            {
                if (!grouped.ContainsKey(pair.Value)) grouped[pair.Value] = new List<VectorCenter>();
                grouped[pair.Value].Add(pair.Key);
            }
            int sum = grouped.Sum(x => x.Value.Count);

            return grouped;
        }

        private static TriangleStorage CalcCentersDistances(List<VectorCenter> centers)
        {
            var matr = new TriangleStorage(centers.Count);
            var processorCount = Environment.ProcessorCount;
            Parallel.For(0, processorCount, new ParallelOptions() {MaxDegreeOfParallelism = processorCount}, index =>
                {
                    for (int i = 0; i < centers.Count; i++)
                    {
                        if ((i%processorCount) != index) continue;
                        for (int j = i + 1; j < centers.Count; j++) matr[i, j] = centers[i].Distance(centers[j].Center);
                    }
                });
            return matr;
        }

        private static void TeachDistances(string fileName, int sampleCount, out double distance, out double norm)
        {
            var vectors = new List<IIntVector>();
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
