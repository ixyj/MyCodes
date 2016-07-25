namespace csTest
{
    using System.IO;
	using System.Linq;
	using System.Linq.Expressions;

    class Program
    {
        private static void Main(string[] args)
        {
            GenerateQFDataFile(@"\\STCVM-A25\Users\v-dawsun\Desktop\Share\TableAResult2.txt", new [] { @"C:\Users\yajxu\Desktop\adx.qf.generic1.xml", @"C:\Users\yajxu\Desktop\adx.qf.generic2.xml" , @"C:\Users\yajxu\Desktop\adx.qf.generic3.xml" });
        }

        private static void GenerateQFDataFile(string inFile, string[] outFiles)
        {
            var reader = new StreamReader(inFile);
            var writers = outFiles.Select(x => new StreamWriter(x)).ToList();
            string line;
            var counts = writers.Select(x => 0UL).ToArray();
            var selector = 0;

            writers.ForEach(x => x.Write("<Items>\n"));
            WordBreaker.Initialize();    
            while ((line = reader.ReadLine()) != null)
            {
                var writer = writers[selector];
                var cnt = ++counts[selector];
                writer.Write("<Item KifSchema=\"MsnJVData.EmptyAnswer[1.0]\" Id=\"Record");
                writer.Write(cnt);
                writer.Write("\"><Trigger IsTriggeredBySuggestion=\"true\">");
                writer.Write(Normalize(line));
                writer.Write("</Trigger><Trigger>");
                writer.Write(Normalize(WordBreaker.BreakWords(line, "zh-CN", false)));
                writer.Write("</Trigger></Item>\n");

                if (cnt % 100 == 0)
                {
                    writer.Flush();
                }
                 
                selector = (selector + 1) % writers.Count;
            }

            writers.ForEach(x => x.Write("</Items>"));
            writers.ForEach(x => x.Close()); 
            reader.Close();
        }

        private static string Normalize(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return text;
            }

            return text.ToLowerInvariant()
                    .Replace("&", "&amp;")
                    .Replace("<", "&lt;")
                    .Replace(">", "&gt;")
                    .Replace("\"", "&quot;")
                    .Replace("'", "&apos;");
        }
    }
}