namespace Helper
{
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                var cmd = new AzureCopy(args);
                cmd.Excute();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.WriteLine("this.exe -r/regex -o/overwrite -u/upload/d/download [-k/key sharedAcessSignature] src dest {partial match}");
                Console.WriteLine(@"Regex must not exist in folder path (absolute path), and start with '@@' (not included) if containing char '\'");
            }
        }
    }

    public class AzureCopy
    {
        private bool _isRegex = false;
        private bool _isOverwrite = false;
        private bool? _isUpload = null;
        private string _source = null;
        private string _destination = null;
        private CloudBlobClient _blobClient = null;

        public AzureCopy(string[] args)
        {
            string key = null;
            for (var i = 0; i < args.Length; ++i)
            {
                switch (args[i])
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
                    case "-k":
                    case "-key":
                        key = args[++i];
                        break;
                    default:
                        if (string.IsNullOrWhiteSpace(_source))
                        {
                            _source = args[i].Replace("\"", string.Empty);
                        }
                        else
                        {
                            _destination = args[i].Replace("\"", string.Empty);
                        }
                        break;
                }
            }

            if (string.IsNullOrWhiteSpace(key))
            {
                if (File.Exists("sharedAcessSignature"))
                {
                    var reader = new BinaryReader(new FileStream("sharedAcessSignature", FileMode.Open));
                    key = System.Text.Encoding.UTF8.GetString(reader.ReadBytes(1024000));
                    reader.Close();
                }
            }
            else
            {
                var writer = new BinaryWriter(new FileStream("sharedAcessSignature", FileMode.Create));
                writer.Write(System.Text.Encoding.UTF8.GetBytes(key));
                writer.Close();
            }

            if (string.IsNullOrWhiteSpace(key))
            {
                throw new Exception("No sharedAcessSignature found!");
            }

            _blobClient = CloudStorageAccount.Parse(key).CreateCloudBlobClient();
        }

        public void Excute()
        {
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
            if (path.Contains("@@"))
            {
                var index = path.Substring(0, path.IndexOf("@@"));
                dir = index.Substring(0, index.LastIndexOf(separator, StringComparison.Ordinal) + 1);
                pattern = path.Substring(dir.Length).Replace("@@", String.Empty);
            }
            else
            {
                dir = path.Substring(0, path.LastIndexOf(separator, StringComparison.Ordinal) + 1);
                pattern = path.Substring(dir.Length);
            }
        }

        private void Upload()
        {
            var container = GetContainer(_destination);
            var blobContainer = _blobClient.GetContainerReference(container);
            var localPath = _destination.Substring(_destination.IndexOf(container) + container.Length + 1);
            if (_isRegex)
            {
                string dir, pattern;
                ExtractRegex(_source, out dir, out pattern, @"\");
                var destDir = localPath.EndsWith("/") ? localPath : localPath + "/";
                Directory.EnumerateFiles(dir)
                .Where(x => Regex.IsMatch(Path.GetFileName(x), pattern, RegexOptions.IgnoreCase))
                .ToList()
                .ForEach(x => UploadStream(x, blobContainer.GetBlockBlobReference($"{destDir}{Path.GetFileName(x)}"), _isOverwrite, 3));
            }
            else
            {
                UploadStream(_source, blobContainer.GetBlockBlobReference(localPath), _isOverwrite, 3);
            }
        }

        private void Download()
        {
            if (_isRegex)
            {
                string dir, pattern;
                ExtractRegex(_source, out dir, out pattern, @"/");
                var destDir = _destination.EndsWith(@"\") ? _destination : _destination + @"\";

                EnumerateFiles(dir)
                    .Where(x => Regex.IsMatch(Path.GetFileName(x.Uri.LocalPath), pattern))
                    .ToList()
                    .ForEach(x => DownloadStream(destDir + Path.GetFileName(x.Uri.LocalPath), _blobClient.GetBlobReferenceFromServer(x.Uri), _isOverwrite, 3));
            }
            else
            {
                DownloadStream(_destination, _blobClient.GetBlobReferenceFromServer(new Uri(_source)), _isOverwrite, 3);
            }
        }

        private void UploadStream(string file, ICloudBlob stream, bool isOverrite, int retry)
        {
            if (stream.Exists() && !isOverrite)
            {
                Console.WriteLine("Fail to upload stream since it exists! (overwrite?)");
                return;
            }

            stream.DeleteIfExists();
            for (var i = 0; i < retry; i++)
            {
                try
                {
                    stream.UploadFromFile(file);
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
        }

        private void DownloadStream(string file, ICloudBlob stream, bool isOverWrite, int retry)
        {
            if (new FileInfo(file).Exists && !isOverWrite)
            {
                Console.WriteLine("Fail to download stream since it exists! (overwrite?)");
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
                    stream.DownloadToFile(tempFile, FileMode.Create);
                    break;
                }
                catch (Exception ex)
                {
                    if (i != retry - 1)
                    {
                        continue;
                    }

                    Console.WriteLine($"Fail to download stream: {stream}, exception: {ex}");
                    throw;
                }
            }

            if (File.Exists(file))
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            File.Move(tempFile, file);
        }

        private IEnumerable<IListBlobItem> EnumerateFiles(string stream)
        {
            try
            {
                var container = GetContainer(stream);
                var blobContainer = _blobClient.GetContainerReference(container);  // ListContainers, ListBlobs ...
                return blobContainer.ListBlobs(stream.Substring(stream.IndexOf(container) + container.Length + 1));
            }
            catch
            {
                Console.WriteLine("Fail to get dir info in: " + stream);
                throw;
            }
        }

        private string GetContainer(string stream)
        {
            var start = stream.IndexOf("//");
            start = stream.IndexOf('/', start == -1 ? 0 : start + "//".Length);
            var end = stream.IndexOf('/', start + 1);
            if (start == -1 || end == -1)
            {
                throw new Exception("No Container found!");
            }

            return stream.Substring(start + 1, end - start - 1);
        }
    }
}