using System.Text.RegularExpressions;

class SplitToChunks
{
    const long CHUNK_RANGE = 1_000_000L;

    static Regex ifLine = new Regex(@"if\s*\(\s*input\s*==\s*(?<num>\d+)\s*\)\s*\{[^\}]*Console\.WriteLine\s*\(\s*""(?<word>Even|Odd)""\s*\)\s*;\s*return\s*;\s*\}", RegexOptions.Compiled);

    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: SplitToChunks.exe <big_source.cs>");
            return;
        }

        string sourcePath = args[0];
        if (!File.Exists(sourcePath))
        {
            Console.WriteLine("Source file not found: " + sourcePath);
            return;
        }

        var writers = new Dictionary<long, StreamWriter>();
        var counts = new Dictionary<long, long>();

        try
        {
            using (var sr = new StreamReader(sourcePath))
            {
                string line;
                long lineNo = 0;
                while ((line = sr.ReadLine()) != null)
                {
                    lineNo++;
                    var m = ifLine.Match(line);
                    if (!m.Success)
                        continue;

                    long num = long.Parse(m.Groups["num"].Value);
                    string word = m.Groups["word"].Value; // Even or Odd

                    long chunkIndex = num / CHUNK_RANGE;

                    if (!writers.TryGetValue(chunkIndex, out StreamWriter w))
                    {
                        Directory.CreateDirectory("chunks");
                        string filename = Path.Combine("chunks", $"Chunk_{chunkIndex:D5}.cs");
                        w = new StreamWriter(new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None, 1 << 20));
                        writers[chunkIndex] = w;
                        counts[chunkIndex] = 0;

                        // 构造CSharp的代码文件格式
                        w.WriteLine("using System;");
                        w.WriteLine("namespace ChunkedLookup");
                        w.WriteLine("{");
                        w.WriteLine($"    public static class Chunk_{chunkIndex:D5}");
                        w.WriteLine("    {");
                        w.WriteLine("        // Auto-generated chunk. Contains many if-checks for a numeric range.");
                        w.WriteLine("        public static string Lookup(long input)");
                        w.WriteLine("        {");
                    }

                    long localCount = counts[chunkIndex] + 1;
                    counts[chunkIndex] = localCount;
                    w.WriteLine($"            if (input == {num}) return \"{word}\";");

                    if ((localCount % 100000) == 0)
                    {
                        w.Flush();
                        Console.WriteLine($"Chunk {chunkIndex:D5}: wrote {localCount} entries (line {lineNo})");
                    }
                }
            }

            foreach (var kvp in writers)
            {
                var idx = kvp.Key;
                var w = kvp.Value;
                w.WriteLine("            return null;");
                w.WriteLine("        }");
                w.WriteLine("    }");
                w.WriteLine("}");
                w.Flush();
                w.Dispose();
                Console.WriteLine($"Finished chunk {idx:D5}, entries = {counts[idx]}");
            }

            Console.WriteLine("Split complete. Created " + writers.Count + " chunk source files.");

            // 生成代码分片编译脚本
            // 通常不使用此方法编译分片代码文件，因为其构建效率太低
            GenerateBuildScripts(writers.Keys);
        }
        finally
        {
            foreach (var w in writers.Values)
            {
                try { w.Dispose(); } catch { }
            }
        }
    }

    static void GenerateBuildScripts(IEnumerable<long> chunkIndices)
    {
        // Windows batch
        using (var bw = new StreamWriter("build_chunks.bat"))
        {
            bw.WriteLine("@echo off");
            bw.WriteLine("REM Compiles each Chunk_XXXXX.cs into a Chunk_XXXXX.dll using csc");
            foreach (var idx in chunkIndices)
            {
                string cs = $"Chunk_{idx:D5}.cs";
                string dll = $"Chunk_{idx:D5}.dll";
                bw.WriteLine($"csc -nologo -target:library -out:{dll} {cs}");
            }
            bw.WriteLine("echo Done compiling chunks.");
        }
    }
}
