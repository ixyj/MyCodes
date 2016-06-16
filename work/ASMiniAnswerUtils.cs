namespace Workflow
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Security.Cryptography;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Xml;
    using VcClient;
    using System.Web;
    using VcClientExceptions;

    public class Utils
    {
        public class TextHelper
        {
            public static IEnumerable<XmlNode> GetDataFromXml(List<string> xmlFiles, string items, string checkNode = null)
            {
                foreach (var xml in xmlFiles.SelectMany(FileUtil.EnumerateFiles).Distinct())
                {
                    Log.Logging("Start to extract data from: " + xml);

                    var xmlDoc = new XmlDocument();
                    XmlNodeList nodeList = null;
                    try
                    {
                        xmlDoc.Load(xml);

                        if (!string.IsNullOrEmpty(checkNode))
                        {
                            var checkes = checkNode.ToLower().Substring(checkNode.IndexOf("||") + 2).Split("|".ToArray(), StringSplitOptions.RemoveEmptyEntries);
                            var node = xmlDoc.SelectSingleNode(checkNode.Substring(0, checkNode.IndexOf("||"))).InnerText.ToLower();
                            if (!checkes.All(node.Contains))
                            {
                                Log.Logging("Skip data file: " + xml);
                                continue;
                            }
                        }

                        nodeList = xmlDoc.SelectNodes(items);
                    }
                    catch (Exception ex)
                    {
                        Log.Logging("Fail to extract data from: " + xml, Log.Type.Error);
                        Log.LoggingOnly(ex.Message);
                        Log.LoggingOnly(ex.StackTrace);
                        throw;
                    }

                    foreach (XmlNode node in nodeList)
                    {
                        yield return node;
                    }

                    Log.Logging("Finish to extract data from: " + xml);
                }
            }

            public static IEnumerable<string> GetDataFromTsv(List<string> tsvFiles)
            {
                foreach (var tsv in tsvFiles.SelectMany(FileUtil.EnumerateFiles).Distinct())
                {
                    Log.Logging("Start to extract data from: " + tsv);
                    foreach (var line in File.ReadAllLines(tsv))
                    {
                        yield return line;
                    }

                    Log.Logging("Finish to extract data from: " + tsv);
                }
            }

            public static string GetNodeValue(XmlNode node, string id)
            {
                var values = GetNodes(node, id);
                return values == null ? null : GetFormatText(string.Join(string.Empty, values));
            }

            public static IEnumerable<string> GetNodes(XmlNode node, string id)
            {
                try
                {
                    return node.SelectNodes(id).Cast<XmlNode>().Select(x => x.InnerText);
                }
                catch
                {
                    return null;
                }
            }

            public static string GetNodeAttribute(XmlNode node, string id, string aid)
            {
                try
                {
                    return node.SelectSingleNode(id).Attributes[aid].InnerText;
                }
                catch
                {
                    return null;
                }
            }

            public static string LimitTextLength(string text, int length)
            {
                if (string.IsNullOrEmpty(text))
                {
                    return string.Empty;
                }

                var sum = 0;
                var index = 0;
                var chars = SplitChars(Encoding.UTF8.GetBytes(text)).ToArray();
                while (index < chars.Length && sum < length)
                {
                    sum += chars[index++].Length > 1 ? 3 : 1;
                }

                var limitText = string.Join(string.Empty, chars.Take(index).Select(Encoding.UTF8.GetString));

                return limitText == text
                    ? text
                    : (" !@#$%^&*()-_=+\\|;:'\",<.>?~`！￥…（）—[{]}、@；：‘“’”，《。》/？".Contains(limitText.Last())
                        ? limitText.Substring(0, limitText.Length - 1) + "…"
                        : limitText + "…");
            }

			 private static bool IsMatch<T>(T[] arg1, T[] arg2) where T : IComparable
			 {
				var longArg = arg1.Length > arg2.Length ? arg1 : arg2;
				var shortArg = arg1.Length > arg2.Length ? arg2 : arg1;

				var l = longArg.Length - 1;
				var s = shortArg.Length - 1;
				while (l >= 0 && s >= 0)
				{
					if (shortArg[s].CompareTo(longArg[l]) == 0)
					{
						--s;
					}
					--l;
				}

				return s < 0;
			}

            public static string GetFormatText(string format, IDictionary<string, string> dicts)
            {
                format = format.Replace(@"\t", "\t").Replace(@"\r", "\r").Replace(@"\n", "\n");
                if (!(new Regex(@"\{\d+:")).IsMatch(format))
                {
                    var index = 0;
                    var sb = new StringBuilder(format);
                    for (var i = 0; i < sb.Length; i++)
                    {
                        if (sb[i] == '{')
                        {
                            var str = string.Format("{0}:", index++);
                            sb.Insert(i + 1, str);
                            i += str.Length;
                        }
                    }
                    format = sb.ToString();
                }

                var matches = (new Regex(@"(?<=\{)[^\}]+")).Matches(format).Cast<Match>().Select(x => x.Value).Select(x => x.Contains("###")
                            ? new Tuple<string[], string[]>(x.Substring(x.IndexOf("###") + 3).Split(",".ToArray()), x.Substring(x.IndexOf(':') + 1, x.LastIndexOf("###") - x.IndexOf(':') - 1).Split(",".ToArray()))
                            : new Tuple<string[], string[]>(x.Substring(x.IndexOf(':') + 1).Split(",".ToArray()), null));

                var list = matches.Select(x => x.Item2 == null
                                ? string.Join(string.Empty, x.Item1.Select(y => dicts.ContainsKey(y) ? dicts[y] : dicts["Default"]))
                                : CallBack(x.Item1.Select(y => dicts.ContainsKey(y) ? dicts[y] : dicts["Default"]), x.Item2) as string);

                return String.Format(format, list.ToArray());
            }

            public static string GetFormatText(string format)
            {
                if (string.IsNullOrEmpty(format) ||
                    format.Sum(x => x == '{' ? 1 : 0) != format.Sum(x => x == '}' ? 1 : 0))
                {
                    return format;
                }

                format = format.Replace(@"\t", "\t").Replace(@"\r", "\r").Replace(@"\n", "\n");
                if (!(new Regex(@"\{\d+:")).IsMatch(format))
                {
                    var index = 0;
                    var sb = new StringBuilder(format);
                    for (var i = 0; i < sb.Length; i++)
                    {
                        if (sb[i] == '{')
                        {
                            var str = string.Format("{0}:", index++);
                            sb.Insert(i + 1, str);
                            i += str.Length;
                        }
                    }
                    format = sb.ToString();
                }

                var matches = (new Regex(@"(?<=\{)[^\}]+")).Matches(format)
                    .Cast<Match>()
                    .Select(x => x.Value)
                    .Select(x => CallBack(x.Substring(x.IndexOf(':') + 1).Split(",".ToArray())));

                return matches.Any() ? String.Format(format, matches.ToArray()) : format;
            }

            public static Dictionary<string, string> ExtractString(string text, string format)
            {
                format = format.Replace(@"\t", "\t").Replace(@"\r", "\r").Replace(@"\n", "\n");
                var list = new List<string>();
                var start = 0;
                var splitTokens = (new Regex(@"(?<=\{)[^\{\}]+(?=\})")).Matches(format).Cast<Match>().Select(x => x.Value);
                foreach (var end in splitTokens.Select(x => text.IndexOf(x, start, StringComparison.Ordinal)))
                {
                    list.Add(text.Substring(start, end - start));
                    start = end + 1;
                }
                list.Add(text.Substring(start));

                var dict = new Dictionary<string, string>();
                var tokens = (new Regex(@"(?<!\{)[^\{\}]+(?!\})")).Matches(format).Cast<Match>().Select(x => x.Value).ToList();
                if (tokens.Count == list.Count)
                {
                    for (var i = 0; i < list.Count; i++)
                    {
                        dict[tokens[i]] = list[i];
                    }
                }

                return dict;
            }

            private static IEnumerable<byte[]> SplitChars(IList<byte> bytes)
            {
                for (var i = 0; i < bytes.Count; )
                {
                    if ((bytes[i] & (byte)0x80) == 0)
                    {
                        yield return new[] { bytes[i] };
                        ++i;
                    }
                    else if ((bytes[i] & (byte)0xFC) == (byte)0xFC)
                    {
                        yield return new[] { bytes[i], bytes[i + 1], bytes[i + 2], bytes[i + 3], bytes[i + 4], bytes[i + 5] };
                        i += 6;
                    }
                    else if ((bytes[i] & (byte)0xF8) == (byte)0xF8)
                    {
                        yield return new[] { bytes[i], bytes[i + 1], bytes[i + 2], bytes[i + 3], bytes[i + 4] };
                        i += 5;
                    }
                    else if ((bytes[i] & (byte)0xF0) == (byte)0xF0)
                    {
                        yield return new[] { bytes[i], bytes[i + 1], bytes[i + 2], bytes[i + 3] };
                        i += 4;
                    }
                    else if ((bytes[i] & (byte)0xE0) == (byte)0xE0)
                    {
                        yield return new[] { bytes[i], bytes[i + 1], bytes[i + 2] };
                        i += 3;
                    }
                    else if ((bytes[i] & (byte)0xC0) == (byte)0xC0)
                    {
                        yield return new[] { bytes[i], bytes[i + 1] };
                        i += 2;
                    }
                    else
                    {
                        throw new Exception("Invalid UTF-8 bytes :" + string.Join(" ", bytes));
                    }
                }
            }
        }

        public class ReflectHelper
        {
            public static object CreateInstance(string assemblyName, string classString)
            {
                return CreateInstance(Type.GetType(classString + "," + assemblyName, true));
            }

            public static object[] CreateArray(string assemblyName, string classString, int length)
            {
                return CreateArray(Type.GetType(classString + "," + assemblyName, true), length);
            }

            public static object CreateInstance(Type type)
            {
                return Activator.CreateInstance(type, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, null, CultureInfo.InvariantCulture);
            }

            public static object[] CreateArray(Type type, int length)
            {
                return Array.CreateInstance(type, length) as object[];
            }

            public static object GetField(object obj, string field)
            {
                while (true)
                {
                    var index = field.IndexOf(".", System.StringComparison.Ordinal);
                    if (index < 0)
                    {
                        var type = obj.GetType();
                        var fld = type.GetField(field, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                        return fld == null ? null : fld.GetValue(obj);
                    }
                    else
                    {
                        var parent = field.Substring(0, index);
                        var rest = field.Substring(index + 1);
                        var type = obj.GetType();
                        var fld = type.GetField(parent, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                        if (fld == null)
                        {
                            return null;
                        }

                        obj = fld.GetValue(obj);
                        field = rest;
                    }
                }
            }

            public static void SetField(object obj, string field, object value)
            {
                while (true)
                {
                    var index = field.IndexOf(".", StringComparison.Ordinal);
                    if (index < 0)
                    {
                        var type = obj.GetType();
                        var fld = type.GetField(field, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                        if (fld == null)
                        {
                            return;
                        }
                            
                        value = ParseValue(fld.FieldType, value);
                        fld.SetValue(obj, value);
                    }
                    else
                    {
                        var parent = field.Substring(0, index);
                        var rest = field.Substring(index + 1);
                        var type = obj.GetType();
                        var fld = type.GetField(parent, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                        if (fld == null)
                        {
                            return;
                        }

                        if (fld.GetValue(obj) == null)
                        {
                            fld.SetValue(obj, Utils.ReflectHelper.CreateInstance(fld.FieldType));
                        }
                        obj = fld.GetValue(obj);
                        field = rest;
                        continue;
                    }

                    break;
                }
            }

            public static object ParseValue(Type type, object value)
            {
                if (type == typeof (int))
                {
                    return int.Parse(value.ToString());
                }
                
                if (type == typeof (long))
                {
                    return long.Parse(value.ToString());
                }
                
                if (type == typeof (float))
                {
                    return float.Parse(value.ToString());
                }
                
                if (type == typeof (double))
                {
                    return double.Parse(value.ToString());
                }
                
                if (type == typeof (bool))
                {
                    return bool.Parse(value.ToString());
                }

                return value;
            }

            public static Type GetFieldType(object obj, string field)
            {
                while (true)
                {
                    var index = field.IndexOf(".");
                    if (index < 0)
                    {
                        var type = obj.GetType();
                        var fld = type.GetField(field, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                        return fld == null ? null : fld.FieldType;
                    }
                    else
                    {
                        var parent = field.Substring(0, index);
                        var rest = field.Substring(index + 1);
                        var type = obj.GetType();
                        var fld = type.GetField(parent, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                        if (fld == null)
                        {
                            return null;
                        }
                        else
                        {
                            if (fld.GetValue(obj) == null)
                            {
                                fld.SetValue(obj, CreateInstance(fld.FieldType));
                            }

                            obj = fld.GetValue(obj);
                            field = rest;
                        }
                    }
                }
            }

            public static object InvokeMethod(object obj, string method, object[] args)
            {
                var type = obj.GetType();
                var mth = type.GetMethod(method, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                return mth == null ? null : mth.Invoke(obj, args);
            }
        }

        public class Md5Helper
        {
            private static readonly MD5 Md5 = MD5.Create();

            public static long GetDateKey(DateTime dt)
            {
                return dt.Year * 10000 + dt.Month * 100 + dt.Day;
            }

            public static string GetMd5Hash(string input)
            {

                // Convert the input string to a byte array and compute the hash. 
                var data = Md5.ComputeHash(Encoding.UTF8.GetBytes(input));

                // Create a new Stringbuilder to collect the bytes 
                // and create a string.
                var sBuilder = new StringBuilder();

                // Loop through each byte of the hashed data  
                // and format each one as a hexadecimal string. 
                foreach (var dat in data)
                {
                    sBuilder.Append(dat.ToString("x2"));
                }

                // Return the hexadecimal string. 
                return sBuilder.ToString();
            }
        }

        public class FileUtil
        {
            public static void SafeCopy(string source, string destination, bool isMove = false, long? maxSize = null)
            {
                var directoryName = Path.GetDirectoryName(destination);
                var destinationTempFile = string.IsNullOrEmpty(directoryName) ? Path.GetRandomFileName() : directoryName + Path.DirectorySeparatorChar + Path.GetRandomFileName();

                if (maxSize == null)
                {
                    if (isMove)
                    {
                        File.Move(source, destinationTempFile);
                    }
                    else
                    {
                        File.Copy(source, destinationTempFile);
                    }
                }
                else
                {
                    using (var writer = new StreamWriter(destinationTempFile))
                    {
                        var lines = File.ReadAllLines(source)
                            .OrderBy(x => x.Split("\t".ToArray())
                                .Last()
                                .Sum(y => HttpUtility.UrlEncode(y.ToString()).Length > 1 ? 2 : 1));

                        var count = 0;
                        var stop = false;
                        foreach (var line in lines)
                        {
                            writer.WriteLine(line);

                            if (++count > 10000)
                            {
                                count = 0;
                                writer.Flush();

                                if ((new FileInfo(destinationTempFile)).Length >= maxSize)
                                {
                                    stop = stop || line.Contains("\t4294967295\t");
                                    if (stop)
                                    {
                                        break;
                                    }

                                    stop = true;
                                }
                            }
                        }

                        writer.Close();
                    }

                    if (isMove)
                    {
                        File.Delete(source);
                    }
                }

                var times = 0;
                for (; times < 3; times++)
                {
                    try
                    {
                        File.Delete(destination);
                        break;
                    }
                    catch (Exception ex)
                    {
                        if (times == 3)
                        {
                            throw ex;
                        }

                        System.Threading.Thread.Sleep(60000);
                    }
                }

                File.Move(destinationTempFile, destination);
            }

            // Regex must be in the square bracket, like: \\folder\[*]\[*].txt
            public static IEnumerable<string> EnumerateFiles(string path)
            {
                if (string.IsNullOrEmpty(path) || path.IndexOf('[') == -1)
                {
                    if (File.Exists(path))
                    {
                        yield return path;
                    }
                    yield break;
                }

                var left = path.IndexOf('[');
                var remain = path.IndexOf('\\', left);
                if (remain == -1)
                {
                    // Enumerate Files
                    var fixPath = path.Replace("[", string.Empty).Replace("]", string.Empty);
                    var current = Directory.GetParent(fixPath).FullName;
                    if (Directory.Exists(current))
                    {
                        foreach (var file in Directory.EnumerateFiles(current, Path.GetFileName(fixPath), SearchOption.TopDirectoryOnly).Distinct())
                        {
                            yield return file;
                        }
                    }
                }
                else
                {
                    // Enumerate Directories
                    var enumerates = new List<IEnumerable<string>>();
                    var current = path.Substring(0, remain).Replace("[", string.Empty).Replace("]", string.Empty);
                    if (Directory.Exists(Directory.GetParent(current).FullName))
                    {
                        var right = path.Substring(remain);
                        foreach (var file in Directory.EnumerateDirectories(Directory.GetParent(current).FullName, Path.GetFileName(current), SearchOption.TopDirectoryOnly).SelectMany(x => EnumerateFiles(x + right)))
                        {
                            yield return file;
                        }
                    }
                }
            }

            public static void MergeFile(string merged, IEnumerable<string> sources, Func<string, bool> verifyFun)
            {
                using (var writer = new StreamWriter(merged))
                {
                    var count = 0;
                    foreach (var line in sources.SelectMany(File.ReadAllLines).Where(x => verifyFun == null || verifyFun(x)))
                    {
                        writer.WriteLine(line);

                        if (++count > 2000)
                        {
                            count = 0;
                            writer.Flush();
                        }
                    }
                }
            }
        }

        public class CosmosUtil
        {
            public static bool Exists(string stream)
            {
                return VC.StreamExists(stream);
            }

            public static void DeleteStream(string stream, int retry)
            {
                for (var i = 0; i < retry; i++)
                {
                    try
                    {
                        VC.Delete(stream);
                        break;
                    }
                    catch (Exception ex)
                    {
                        if (i == retry - 1)
                        {
                            Log.Logging("Fail to delete stream: " + stream);
                            Log.Logging(ex.Message, Log.Type.Error);
                            Log.LoggingOnly(ex.StackTrace);
                            throw;
                        }
                    }
                }
            }

            public static void UploadStream(string file, string stream, bool isOverrite, int retry)
            {
                if (!IsInComplete(stream))
                {
                    if (Exists(stream) && !isOverrite)
                    {
                        return;
                    }

                    var directoryName = stream.Substring(0, stream.LastIndexOf("/", System.StringComparison.Ordinal));
                    var tempFile = directoryName + "/" + Path.GetRandomFileName();

                    var isSuccess = false;
                    for (var i = 0; i < retry; i++)
                    {
                        Log.Logging("Try to upload stream " + tempFile);
                        try
                        {
                            VC.Upload(file, tempFile, true);
                            isSuccess = true;
                            break;
                        }
                        catch (Exception ex)
                        {
                            if (i == retry - 1)
                            {
                                Log.Logging("Fail to upload stream: " + stream);
                                Log.Logging(ex.Message, Log.Type.Error);
                                Log.LoggingOnly(ex.StackTrace);
                                throw;
                            }
                        }
                    }

                    if (isSuccess)
                    {
                        if (Exists(stream))
                        {
                            VC.Delete(stream);
                        }

                        Log.Logging("Rename stream: " + tempFile + " ==> " + stream);
                        VC.Rename(tempFile, stream);
                        Log.Logging("Finish to upload stream: " + stream);
                    }
                }
            }

            public static void DownloadStream(string file, string stream, bool isOverWrite, int retry)
            {
                if (new FileInfo(file).Exists && !isOverWrite)
                {
                    Log.Logging("Not download stream: " + stream + " because: 1.file exists 2. disable overwrite");
                    return;
                }

                if (IsInComplete(stream))
                {
                    Log.Logging("Not download stream: " + stream + " because: it is incomplete");
                    return;
                }

                var directoryName = Path.GetDirectoryName(file);
                if (!string.IsNullOrEmpty(directoryName) && !Directory.Exists(directoryName))
                {
                    Directory.CreateDirectory(directoryName);
                }

                var tempFile = Path.GetRandomFileName();
                var isSuccess = false;
                for (var i = 0; i < retry; i++)
                {
                    try
                    {
                        VC.Download(stream, tempFile, true, isOverWrite);
                        isSuccess = true;
                        break;
                    }
                    catch (Exception ex)
                    {
                        if (i == retry - 1)
                        {
                            Log.Logging("Fail to download stream: " + stream);
                            Log.Logging(ex.Message, Log.Type.Error);
                            Log.LoggingOnly(ex.StackTrace);
                            throw;
                        }
                    }
                }

                if (isSuccess)
                {
                    Utils.FileUtil.SafeCopy(tempFile, file);
                }
            }

            public static bool IsInComplete(string stream)
            {
                StreamInfo info = null;
                try
                {
                    info = VC.GetStreamInfo(stream, true, false);
                }
                catch (VcClientException ex)
                {
                    if (ex.ToString().Contains("Stream is incomplete"))
                    {
                        return true;
                    }
                }
                catch
                {
                }

                return ((info != null) && (info.CommittedLength != info.Length));
            }

            public static IEnumerable<string> EnumerateFiles(string streamDir)
            {
                try
                {
                    return VC.GetDirectoryInfo(streamDir, true).Select(x => streamDir + Path.GetFileName(x.CosmosPath));
                }
                catch
                {
                    return null;
                }
            }
        }

        public static string CallBack(string[] process)
        {
            var now = DateTime.Now;

            switch (process.First().ToLower())
            {
                case "datenow":
                    return now.ToString(process.LastOrDefault());

                default:
                    return string.Empty;
            }
        }

        public static object CallBack(IEnumerable<object> data, IEnumerable<string> process)
        {
            switch (process.FirstOrDefault().ToLower())
            {
                case "urlencode":
                    return string.Join(string.Empty, data.Select(x => HttpUtility.UrlEncode(x as string)));

                case "limittext":
                    return TextHelper.LimitTextLength(string.Join(string.Empty, data),
                        Int32.Parse(process.LastOrDefault()));

                default:
                    return data;
            }
        }
    }
}
