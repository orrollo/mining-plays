using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace incomeProcessing
{
    class Program
    {
        protected static string DataDirectory = Path.Combine(Directory.GetCurrentDirectory(),"..\\..\\..\\data");

        class IntDictionary : Dictionary<int,int>
        {
             
        }

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
            ReduceFiles(getBook, getUser, "books", books);

            Console.WriteLine("map-reduce: reduce files by users to vectors");
            ReduceFiles(getUser, getBook, "users", users);
        }

        private static void ReduceFiles(Func<int, int, int> getGroup, Func<int, int, int> getValue, string subdir, List<int> keys)
        {
            var srcPath = GetDataPath(subdir);
            if (!Directory.Exists(srcPath)) return;

            var fileName = subdir + ".vec";
            Console.WriteLine("making file: {0}", fileName);
            var outFile = GetDataPath(fileName);

            using (var wrt = new StreamWriter(GetDataPath(outFile)))
            {
                var files = Directory.GetFiles(srcPath, "*.src");

                Parallel.ForEach(files, file =>
                    {

                        lock (files) Console.WriteLine("processing file <{1}\\{0}>", Path.GetFileName(file), subdir);
                        var vectors = new Dictionary<int, IntDictionary>();
                        InputTextReader(file, line =>
                            {
                                var grp = line.Split('\t');
                                int book = int.Parse(grp[0]), user = int.Parse(grp[1]), rate = int.Parse(grp[2]);
                                int group = getGroup(book, user), value = getValue(book, user);
                                if (!vectors.ContainsKey(group)) vectors[group] = new IntDictionary();
                                vectors[group][value] = rate;
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

                    });
                wrt.Flush();
            }
        }

        private static void MapCsvFile(Func<int, int, int> getGroup, string subdir, int maxLimit)
        {
            Func<int, string> getName = x => string.Format("{0:d4}.src", x);

            var dstPath = GetDataPath(subdir);
            if (!Directory.Exists(dstPath)) Directory.CreateDirectory(dstPath);
            var writers = new Dictionary<int, StreamWriter>();
            InputTextReader(GetCsvDataFileName(), line =>
                {
                    var grp = line.Split('\t');
                    int book = int.Parse(grp[0]), user = int.Parse(grp[1]), rate = int.Parse(grp[2]);

                    var group = getGroup(book, user)%maxLimit;

                    if (!writers.ContainsKey(@group)) writers[group] = new StreamWriter(Path.Combine(dstPath, getName(group)));
                    writers[group].WriteLine(line);
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
                InputTextReader(GetDataPath("src.sql"), line =>
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
                    });
            }

            booksList = books.ToList();
            booksList.Sort();

            usersList = users.ToList();
            usersList.Sort();
        }

        private static void InputTextReader(string fileName, Action<string> proc)
        {
            using (var rdr = new StreamReader(fileName))
            {
                while (!rdr.EndOfStream)
                {
                    var line = (rdr.ReadLine() ?? string.Empty).Trim();
                    if (string.IsNullOrEmpty(line)) continue;
                    proc(line);
                }
            }
        }

        private static string GetCsvDataFileName()
        {
            return GetDataPath("src.csv");
        }

        private static string GetDataPath(string fileName)
        {
            return Path.Combine(DataDirectory, fileName);
        }

        private static void SaveDataList(List<int> data, string fileName)
        {
            using (var wrt = new StreamWriter(GetDataPath(fileName)))
            {
                wrt.WriteLine(data.Count);
                for (int index = 0; index < data.Count; index++) wrt.WriteLine("{0}\t{1}", index, data[index]);
            }
        }
    }
}
