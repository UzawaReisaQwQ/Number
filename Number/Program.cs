using System.Text;

class numberBuild
{
    static void Main()
    {
        // n: 0 ≤ n ≤ 10⁹
        const long a = 1_000_000_000L;
        string b = "number.cs";

        var c = new StreamWriter(new FileStream(b, FileMode.Create, FileAccess.Write, FileShare.None, 1 << 20), Encoding.UTF8);

        //构造标准CSharp格式的代码文件
        c.WriteLine("using System;");
        c.WriteLine();
        c.WriteLine("class Program");
        c.WriteLine("{");
        c.WriteLine("    static void Main()");
        c.WriteLine("    {");
        c.WriteLine("        if (!long.TryParse(Console.ReadLine()?.Trim(), out long input)) return;");
        c.WriteLine();

        const int d = 10_000_000;
        for (long i = 0; i <= a; i++)
        {
            string e = (i % 2 == 0) ? "Even" : "Odd";
            c.WriteLine($"        if (input == {i}) {{ Console.WriteLine(\"{e}\"); return; }}");

            if ((i % d) == 0 && i != 0)
            {
                c.Flush();
            }
        }

        c.WriteLine("    }");
        c.WriteLine("}");
        c.Flush();

        Console.WriteLine($"Build Success：{b}");
        Console.ReadLine();
    }
}
