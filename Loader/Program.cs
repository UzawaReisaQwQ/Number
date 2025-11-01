using System.Reflection;

class MainLoader
{
    const long CHUNK_RANGE = 1_000_000L;

    static Dictionary<long, Assembly> loaded = new Dictionary<long, Assembly>();
    static Dictionary<long, Type> chunkTypes = new Dictionary<long, Type>();

    static void Main()
    {
        Console.Write("输入数字: ");
        string line;
        while ((line = Console.ReadLine()) != null)
        {
            if (!long.TryParse(line.Trim(), out long input))
            {
                Console.WriteLine("Invalid");
                continue;
            }

            if (input < 0)
            {
                Console.WriteLine("OutOfRange");
                continue;
            }

            long chunkIndex = input / CHUNK_RANGE;
            string dllName = Path.Combine("dlls", $"Chunk_{chunkIndex:D5}.dll");
            if (!File.Exists(dllName))
            {
                Console.WriteLine("MissingChunk");
                continue;
            }

            try
            {
                if (!loaded.TryGetValue(chunkIndex, out Assembly asm))
                {
                    asm = Assembly.LoadFrom(dllName);
                    loaded[chunkIndex] = asm;
                }

                if (!chunkTypes.TryGetValue(chunkIndex, out Type t))
                {
                    string typeName = $"ChunkedLookup.Chunk_{chunkIndex:D5}";
                    t = asm.GetType(typeName);
                    if (t == null)
                    {
                        Console.WriteLine("TypeNotFound");
                        continue;
                    }
                    chunkTypes[chunkIndex] = t;
                }

                var mi = chunkTypes[chunkIndex].GetMethod("Lookup", BindingFlags.Public | BindingFlags.Static);
                if (mi == null)
                {
                    Console.WriteLine("MethodNotFound");
                    continue;
                }

                var res = mi.Invoke(null, new object[] { input }) as string;
                if (res == null)
                    Console.WriteLine("NotFound");
                else
                    Console.WriteLine(res);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error:" + ex.Message);
            }
        }
    }
}
