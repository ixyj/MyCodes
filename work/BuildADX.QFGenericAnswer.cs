
namespace csTest
{
    using System;
    using System.IO;
    using System.Linq;

    class Program
    {
        private static void Main(string[] args)
        {
            GenerateQFDataFile(@"C:\Users\yajxu\Desktop\olympics_query.txt", new[] { @"C:\Users\yajxu\Desktop\olympics_query.out.txt" });
        }

        private static void GenerateQFDataFile(string inFile, string[] outFiles)
        {
            var reader = new StreamReader(inFile);
            var writers = outFiles.Select(x => new StreamWriter(x)).ToList();
            string line;
            var counts = writers.Select(x => 0UL).ToArray();
            var selector = 0;

            writers.ForEach(x => x.Write("<Items>\n\t<Item KifSchema=\"MsnJVData.EmptyAnswer[1.0]\" Id=\"Record_All\">\n"));
            // WordBreaker.Initialize();
            while ((line = reader.ReadLine()) != null)
            {
                var raw = Normalize(line);
                if (string.IsNullOrWhiteSpace(raw))
                {
                    Console.WriteLine("Empty line: " + line);
                    continue;
                }

                var writer = writers[selector];
                var cnt = ++counts[selector];
                // var normalized = Normalize(WordBreaker.BreakWords(line, "zh-CN", false));
                var normalized = Normalize(line.Trim());
                writer.Write("\t\t");
                if (string.IsNullOrWhiteSpace(normalized))
                {
                    Console.WriteLine("Empty normilized line: " + normalized + "\tRaw line: " + line);
                }
                else
                {
                    writer.Write("<Trigger DisableNormalization=\"true\">");
                    writer.Write(normalized);
                    writer.Write("</Trigger>");
                }


                // if (!string.Equals(raw, normalized))
                {
                    writer.Write("<Trigger IsTriggeredBySuggestion=\"true\">");
                    writer.Write(raw);
                    writer.Write("</Trigger>");
                }

                writer.Write("\n");
                if (cnt % 100 == 0)
                {
                    writer.Flush();
                }

                selector = (selector + 1) % writers.Count;
            }

            writers.ForEach(x => x.Write("\t</Item>\n</Items>"));
            writers.ForEach(x => x.Close());
            reader.Close();
        }

        private static string Normalize(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return text;
            }

            if (text.All(x => char.IsControl(x) || char.IsPunctuation(x) || char.IsSeparator(x) || char.IsSymbol(x) || char.IsDigit(x) || char.IsWhiteSpace(x)))
            {
                return null;
            }

            return text.Trim()
                    .ToLowerInvariant()
                    .Replace("&", "&amp;")
                    .Replace("<", "&lt;")
                    .Replace(">", "&gt;")
                    .Replace("\"", "&quot;")
                    .Replace("'", "&apos;");
        }
    }
}