using System;
using System.IO;
using System.IO.Compression;

namespace Common
{
    public static class Helper
    {
        private static string dataDirectory = null;

        public static string GetDataDirectory()
        {
            return dataDirectory ?? (dataDirectory = Path.Combine(Directory.GetCurrentDirectory(), "..\\..\\..\\data"));
        }

        public static void InputTextReader(string fileName, Func<string, bool> proc)
        {
            if (!fileName.ToLower().EndsWith(".gz"))
            {
                ProcessTextReader(proc, new StreamReader(fileName));
            }
            else
            {
                using (var fileStream = new FileStream(fileName, FileMode.Open))
                {
                    using (var ds = new GZipStream(fileStream, CompressionMode.Decompress))
                    {
                        ProcessTextReader(proc, new StreamReader(ds));
                    }
                }
            }
        }

        public static void ProcessTextReader(Func<string, bool> proc, StreamReader streamReader)
        {
            using (var rdr = streamReader)
            {
                while (!rdr.EndOfStream)
                {
                    var line = (rdr.ReadLine() ?? String.Empty).Trim();
                    if (String.IsNullOrEmpty(line)) continue;
                    if (!proc(line)) break;
                }
            }
        }

        public static string GetDataPath(string fileName)
        {
            return Path.Combine(GetDataDirectory(), fileName);
        }

        public static void InputTextVectors(string fileName, Func<IIntVector, bool> proc)
        {
            InputTextReader(GetDataPath(fileName), line =>
                {
                    var arr = line.Split(new[] {':'}, 2);

                    var vector = new IntVectorEx(int.Parse(arr[0]));

                    var groups = arr[1].Split(',');
                    foreach (var group in groups)
                    {
                        var data = @group.Split(':');
                        int id = int.Parse(data[0]), rate = int.Parse(data[1]);
                        vector[id] = rate;
                    }
                    return proc(vector);
                });
        }
    }
}