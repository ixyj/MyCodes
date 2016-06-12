namespace Helper
{
    using System;
    using System.IO;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using VcClient;
    using VcClientExceptions;

    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                var cmd = new CosmosUtil();
                cmd.Excute(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.WriteLine("this.exe -r/regex -o/overwrite -u/upload/d/download src dest  {partial match}");
                Console.WriteLine(@"Regex path is like : '..\\\d+.*' or '..\as.*'");
            }
        }
    }

    public class CosmosUtil
    {
        private static bool _isRegex = false;
        private static bool _isOverwrite = false;
        private static bool? _isUpload = null;
        private static string _source = null;
        private static string _destination = null;

        public void Excute(string[] args)
        {
            foreach (var arg in args.Select(x => x.StartsWith("-") || x.StartsWith("/") ? x.Replace("/", "-").Trim().ToLower() : x))
            {
                switch (arg)
                {
                    case "-u":
                    case "-upload":
                        _isUpload = true;
                        break;
                    case "-d":
                    case "-download":
                        _isUpload = false;
                        break;
                    case "-r":
                    case "-regex":
                        _isRegex = true;
                        break;
                    case "-o":
                    case "-overwrite":
                        _isOverwrite = true;
                        break;
                    default:
                        if (string.IsNullOrWhiteSpace(_source))
                        {
                            _source = arg.Replace("\"", string.Empty);
                        }
                        else
                        {
                            _destination = arg.Replace("\"", string.Empty);
                        }
                        break;
                }
            }

            if (_isUpload == null)
            {
                throw new ArgumentException("No upload/download argment!");
            }
            
            if (_isUpload ?? false)
            {
                Upload();
            }
            else
            {
                Download();
            }
        }


        private void ExtractRegex(string path, out string dir, out string pattern, string separator)
        {
            if (path.Contains(@"\\"))
            {
                var index = path.Substring(0, path.IndexOf(@"\\"));
                dir = index + @"\";
                pattern = path.Substring(dir.Length + 1);
            }
            else
            {
                dir = _source.Substring(0, _source.LastIndexOf(separator, StringComparison.Ordinal) + 1);
                pattern = _source.Substring(dir.Length);
            }
        }

        private void Upload()
        {
            if (_isRegex)
            {
                string dir, pattern;
                ExtractRegex(_source, out dir, out pattern, @"\");
                var destDir = _destination.EndsWith("/") ? _destination : _destination + "/";

                Directory.EnumerateFiles(dir)
                    .Select(x => x.Substring(x.LastIndexOf(@"\", StringComparison.Ordinal) + 1))
                    .Where(x => Regex.IsMatch(x, pattern, RegexOptions.IgnoreCase))
                    .ToList()
                    .ForEach(x => UploadStream(dir + x, destDir + x, _isOverwrite, 3));
            }
            else
            {
                UploadStream(_source, _destination, _isOverwrite, 3);
            }
        }

        private void Download()
        {
            if (_isRegex)
            {
                string dir, pattern;
                ExtractRegex(_source, out dir, out pattern, @"/");
                var destDir = _destination.EndsWith(@"\") ? _destination : _destination + @"\";

                EnumerateFiles(dir).Select(x => x.Substring(x.LastIndexOf("/", StringComparison.Ordinal) + 1))
                    .Where(x => Regex.IsMatch(x, pattern, RegexOptions.CultureInvariant))
                    .ToList()
                    .ForEach(x => DownloadStream(destDir + x, dir + x, _isOverwrite, 3));
            }
            else
            {
                DownloadStream(_destination, _source, _isOverwrite, 3);
            }
        }

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
                catch
                {
                    if (i != retry - 1)
                    {
                        continue;
                    }

                    Console.WriteLine("Fail to delete stream: " + stream);
                    throw;
                }
            }
        }

        public static void UploadStream(string file, string stream, bool isOverrite, int retry)
        {
            if (Exists(stream) && !isOverrite)
            {
                Console.WriteLine("Fail to upload stream since it exists! (overwrite?)");
                return;
            }

            var directoryName = stream.Substring(0, stream.LastIndexOf("/", System.StringComparison.Ordinal));
            var tempFile = directoryName + "/" + Path.GetRandomFileName();

            for (var i = 0; i < retry; i++)
            {
                try
                {
                    VC.Upload(file, tempFile, true);
                    break;
                }
                catch
                {
                    if (i != retry - 1)
                    {
                        continue;
                    }

                    Console.WriteLine("Fail to upload stream: " + stream);
                    throw;
                }
            }

            if (Exists(stream))
            {
                VC.Delete(stream);
            }

            VC.Rename(tempFile, stream);
        }

        public static void DownloadStream(string file, string stream, bool isOverWrite, int retry)
        {
            if (new FileInfo(file).Exists && !isOverWrite)
            {
                Console.WriteLine("Fail to download stream since it exists! (overwrite?)");
                return;
            }

            if (IsInComplete(stream))
            {
                Console.WriteLine("Fail to download the incomplete stream!");
                return;
            }

            var directoryName = Path.GetDirectoryName(file);
            if (!string.IsNullOrEmpty(directoryName) && !Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }

            var tempFile = Path.GetRandomFileName();
            for (var i = 0; i < retry; i++)
            {
                try
                {
                    VC.Download(stream, tempFile, true, isOverWrite);
                    break;
                }
                catch
                {
                    if (i != retry - 1)
                    {
                        continue;
                    }

                    Console.WriteLine("Fail to download stream: " + stream);
                    throw;
                }
            }

            if (isOverWrite && File.Exists(file))
            {
                File.SetAttributes(file,FileAttributes.Normal);
                File.Delete(file);
            }

            File.Move(tempFile, file);
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

                Console.WriteLine("Fail to get info for: " + stream);
                throw;
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
                Console.WriteLine("Fail to get dir info in: " + streamDir);
                throw;
            }
        }
    }
}