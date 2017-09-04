using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Common;

namespace incomeProcessing
{
    class Program
    {
        static void Main(string[] args)
        {
            List<int> books, users;

            Console.WriteLine("processing sql to csv");
            ProcessSql(out books, out users);

            Console.WriteLine("saving books and users lists");
            SaveDataList(books, "books.lst");
            SaveDataList(users, "users.lst");

            Func<int, int, int> getUser = (b, u) => u, getBook = (b, u) => b;

            Console.WriteLine("map-reduce: map source files by books");
            MapCsvFile(getBook, "books", 1000);
            Console.WriteLine("map-reduce: map source files by users");
            MapCsvFile(getUser, "users", 1000);

            Console.WriteLine("map-reduce: reduce files by books to vectors");
            ReduceCsvFiles(getBook, getUser, "books", books);

            Console.WriteLine("map-reduce: reduce files by users to vectors");
            ReduceCsvFiles(getUser, getBook, "users", users);
        }

        private static void ReduceCsvFiles(Func<int, int, int> getGroup, Func<int, int, int> getValue, string subdir, List<int> keys)
        {
            var srcPath = Helper.GetDataPath(subdir);
            if (!Directory.Exists(srcPath)) return;

            var fileName = subdir + ".vec";
            Console.WriteLine("making file: {0}", fileName);
            var outFile = Helper.GetDataPath(fileName);

            using (var wrt = new StreamWriter(Helper.GetDataPath(outFile)))
            {
                var files = Directory.GetFiles(srcPath, "*.src");

                Parallel.ForEach(files, file =>
                    {

                        lock (files) Console.WriteLine("processing file <{1}\\{0}>", Path.GetFileName(file), subdir);
                        var vectors = new Dictionary<int, IntVector>();
                        Helper.InputTextReader(file, line =>
                            {
                                var grp = line.Split('\t');
                                int book = int.Parse(grp[0]), user = int.Parse(grp[1]), rate = int.Parse(grp[2]);
                                int group = getGroup(book, user), value = getValue(book, user);
                                if (!vectors.ContainsKey(group)) vectors[group] = new IntVector(group);
                                vectors[group][value] = rate;
                                return true;
                            });
                        var sb = new StringBuilder();
                        foreach (var pp in vectors)
                        {
                            sb.Clear();
                            sb.AppendFormat("{0:d7}", pp.Key);
                            var first = true;
                            foreach (var ratePair in pp.Value)
                            {
                                sb.Append(first ? ":" : ",");
                                sb.AppendFormat("{0}:{1}", ratePair.Key, ratePair.Value);
                                first = false;
                            }
                            lock (wrt)
                            {
                                wrt.WriteLine(sb.ToString());
                            }
                        }
                        File.Delete(file);
                    });
                wrt.Flush();
            }
        }

        private static void MapCsvFile(Func<int, int, int> getGroup, string subdir, int maxLimit)
        {
            Func<int, string> getName = x => string.Format("{0:d4}.src", x);

            var dstPath = Helper.GetDataPath(subdir);
            if (!Directory.Exists(dstPath)) Directory.CreateDirectory(dstPath);
            var writers = new Dictionary<int, StreamWriter>();
            Helper.InputTextReader(GetCsvDataFileName(), line =>
                {
                    var grp = line.Split('\t');
                    int book = int.Parse(grp[0]), user = int.Parse(grp[1]), rate = int.Parse(grp[2]);

                    var group = getGroup(book, user)%maxLimit;

                    if (!writers.ContainsKey(@group)) writers[group] = new StreamWriter(Path.Combine(dstPath, getName(group)));
                    writers[group].WriteLine(line);
                    return true;
                });
            var keys = writers.Keys.ToArray();
            foreach (var key in keys)
            {
                writers[key].Flush();
                writers[key].Close();
                writers.Remove(key);
            }
        }

        private static void ProcessSql(out List<int> booksList, out List<int> usersList)
        {
            HashSet<int> books = new HashSet<int>(), users = new HashSet<int>();

            var re = new Regex(@"\(\d+,(\d+),(\d+),'(\d)'\)", RegexOptions.Compiled | RegexOptions.CultureInvariant);
            using (var wrt = new StreamWriter(GetCsvDataFileName()))
            {
                var fileName = Helper.GetDataPath("src.sql.gz");
                Helper.InputTextReader(fileName, line =>
                    {
                        var mc = re.Matches(line);
                        foreach (Match m in mc)
                        {
                            if (!m.Success) continue;

                            var book = m.Groups[1].Value;
                            var user = m.Groups[2].Value;

                            var bookId = int.Parse(book);
                            var userId = int.Parse(user);

                            if (!books.Contains(bookId)) books.Add(bookId);
                            if (!users.Contains(userId)) users.Add(userId);

                            var record1 = new[] { book, user, m.Groups[3].Value };
                            line = string.Join("\t", record1);
                            wrt.WriteLine(line);
                        }
                        return true;
                    });
            }

            booksList = books.ToList();
            booksList.Sort();

            usersList = users.ToList();
            usersList.Sort();
        }

        private static string GetCsvDataFileName()
        {
            return Helper.GetDataPath("src.csv");
        }

        private static void SaveDataList(List<int> data, string fileName)
        {
            using (var wrt = new StreamWriter(Helper.GetDataPath(fileName)))
            {
                wrt.WriteLine(data.Count);
                for (int index = 0; index < data.Count; index++) wrt.WriteLine("{0}\t{1}", index, data[index]);
            }
        }
    }
}
