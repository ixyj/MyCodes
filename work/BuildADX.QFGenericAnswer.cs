namespace csTest
{
    using System.IO;

    class Program
    {
        static void Main(string[] args)
        {
            GenerateQFDataFile(@"\\STCVM-A25\Users\v-dawsun\Desktop\Share\TableAResult2.txt", @"C:\Users\yajxu\Desktop\adx.qf.generic.xml");
        }

        private static void GenerateQFDataFile(string inFile, string outFile)
        {
            var reader = new StreamReader(inFile);
            var writer = new StreamWriter(outFile);
            string line;
            ulong count = 0;

            WordBreaker.Initialize();
            writer.Write("<Items>\n");
            while ((line = reader.ReadLine()) != null)
            {
                writer.Write("<Item KifSchema=\"MsnJVData.EmptyAnswer[1.0]\" Id=\"Record");
                writer.Write(count++);
                writer.Write("\"><Trigger IsTriggeredBySuggestion=\"true\">");
                writer.Write(Normalize(line));
                writer.Write("</Trigger><Trigger>");
                writer.Write(Normalize(WordBreaker.BreakWords(line, "zh-CN", false)));
                writer.Write("</Trigger></Item>\n");

                if (count % 100 == 0)
                {
                    writer.Flush();
                }
            }

            writer.Write("</Items>");
            writer.Close();
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
