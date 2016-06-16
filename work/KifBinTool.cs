   namespace KifBinTool
{
    using Microsoft.Search.Kif;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Linq;

    public class Utility
    {
        private static IKifRepository KifRepository = null;

        public static string Encode(string input)
        {
            IKifReader reader = null;
            using (var memoryStream = new MemoryStream(input.Length*2))
            {
                var inputStream = new MemoryStream(Encoding.UTF8.GetBytes(input));
                XmlToKifConverter.Read(XDocument.Load(inputStream).Root, memoryStream, KifRepository);
                reader = KifBinaryReader.Create(memoryStream.ToArray());
            }

            byte[] buffer;
            int start;
            int length;
            reader.GetRawData(out buffer, out start, out length);
            return Convert.ToBase64String(buffer, Base64FormattingOptions.None);
        }

        public static string Decode(string input)
        {
            var buffer = Convert.FromBase64String(input);
            var reader = KifBinaryReader.Create(buffer);
            var doc = KifToXmlConverter.Write(KifRepository, reader, true, false);

            return doc.ToString();
        }

        public static string Json2Xml(string kifJson)
        { 
            // Json -> Kif
            var memoryStream = new MemoryStream(4096);
            JsonToKifConverter.ReadFromStream(new StringReader(kifJson), null, memoryStream, KifRepository, null);

            // Kif -> Xml
            var reader = KifBinaryReader.Create(memoryStream.ToArray());
            var doc = KifToXmlConverter.Write(KifRepository, reader, true, false);
            return doc.ToString();
        }

        public static string Xml2Json(string kifXml)
        {
            // Xml -> Kif
            var memoryStream = new MemoryStream(4096);
            var xml = XElement.Parse(kifXml);
            XmlToKifConverter.Read(xml, memoryStream, KifRepository);

            // Kif -> Json
            var reader = KifBinaryReader.Create(memoryStream.ToArray());
            var writer = new StringWriter();
            KifToJsonConverter.WriteToStream(writer, reader, KifRepository);
            return writer.ToString();
        }

        public static void Txt2bin(string txtFile, string binFile)
        {
            using (var writer = new BinaryWriter(File.Open(binFile, FileMode.Create, FileAccess.Write)))
            {
                foreach (var line in File.ReadAllLines(txtFile))
                {
                    var split = line.Split("\t".ToArray(), StringSplitOptions.RemoveEmptyEntries); 
                    writer.Write(254);
                    writer.Write(255);
                    writer.Write(1);
                    writer.Write(Encoding.UTF8.GetBytes(split[0]));
                    writer.Write(254);
                    writer.Write(255);
                    writer.Write(2);
                    writer.Write(Convert.FromBase64String(Encode(split[1])));
                }

                writer.Close();
            }
        }

        public static void Init(string kifrepository = @"\\lsdfs\shares\searchgold\deploy\builds\data\Answers\kifrepositoryV2\KifSchemas\")
        {
            KifConfigSingleton.SetConfig(new ConstKifConfig(APKifConfig.DefaultProtocol, kifrepository));
            KifRepository = new FileKifRepository();
        }
    }

	public class KifReader
    {
        private static IKifRepository KifRepository = null;
        private readonly IKifReader reader;

        public static void Init(string kifRepositoryPath)
        {
            if (!Directory.Exists(kifRepositoryPath))
            {
                throw new ArgumentException("Kif Schema Repository Path doesn't exist");
            }

            KifConfigSingleton.SetConfig(new ConstKifConfig(APKifConfig.DefaultProtocol, kifRepositoryPath));
            KifRepository = new FileKifRepository();
        }

        public KifReader(string kifBase64)
        {
            try
            {
                reader = KifBinaryReader.Create(Convert.FromBase64String(kifBase64));
            }
            catch
            {
                reader = null;
            }
        }

        public string ReadData(ushort ordinal)
        {
            string result;
            return reader.TryReadString(ordinal, out result) ? result : null;
        }

        public List<string> ReadList(ushort ordinal, ushort internalOrdinal)
        {
            var results = new List<string>();
            try
            {
                var list = reader.ReadSubTypedList(ordinal);
                for (ushort i = 0; i < list.NumberOfFields; i++)
                {
                    results.Add(list.ReadSubStruct(i).ReadString(internalOrdinal));
                }
            }
            catch
            {
            }

            return results;
        }
    }
}
