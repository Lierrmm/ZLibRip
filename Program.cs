using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

namespace ZLibRip
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
            var files = ParseFilesFromArgs(args);
            Console.WriteLine($"Total of {files.Count} to process.");
            files.ForEach(DoTheRip);
        }

        private static List<string> ParseFilesFromArgs(IEnumerable<string> args)
        {
            var files = new List<string>();

            string luaExtension = "*.lua*";

            foreach (var arg in args)
            {
                if (!File.Exists(arg) && !Directory.Exists(arg))
                    continue;

                var attr = File.GetAttributes(arg);

                if (attr.HasFlag(FileAttributes.Directory))
                {
                    files.AddRange(Directory.GetFiles(arg, luaExtension, SearchOption.AllDirectories).ToList());
                }
                else if (Path.GetExtension(arg).Contains(".lua"))
                {
                    files.Add(arg);
                }
                else
                {
                    Console.WriteLine($"Invalid argument passed {arg} | {File.GetAttributes(arg)}!");
                }
            }

            // make sure to remove duplicates
            files = files.Distinct().ToList();

            // also remove any already dumped files
            files.RemoveAll(elem => elem.EndsWith(".lua.dec"));
            files.RemoveAll(elem => elem.EndsWith(".dec.lua"));
            files.RemoveAll(elem => elem.EndsWith(".luadec"));

            return files;
        }

        static void SaveMemoryStream(MemoryStream ms, string FileName)
        {
            var finalPath = Path.GetFileNameWithoutExtension(FileName);
            Directory.CreateDirectory(@"Dumped");
            FileStream outStream = File.OpenWrite($@"Dumped/{finalPath}.lua");
            ms.WriteTo(outStream);
            outStream.Flush();
            outStream.Close();
        }

        static void DoTheRip(string fileName)
        {
            var outputStream = new MemoryStream();
            Console.WriteLine($"Processing {fileName}");
            FileStream inStream = File.OpenRead($"{fileName}");
            MemoryStream storeStream = new();

            storeStream.SetLength(inStream.Length);
            inStream.Read(storeStream.GetBuffer(), 0, (int)inStream.Length);

            storeStream.Flush();
            inStream.Close();

            using var inputStream = new InflaterInputStream(storeStream);
            inputStream.CopyTo(outputStream);
            outputStream.Position = 0;
            SaveMemoryStream(outputStream, fileName);
        }
    }
}
