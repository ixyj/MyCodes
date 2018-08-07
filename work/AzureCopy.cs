namespace Helper
{
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

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
                Console.WriteLine(ex);
                Console.WriteLine(ex.StackTrace);
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
        private int _parallelTaskCount = 1;
        private object _lock = new object();
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
                    case "-p":
                        _parallelTaskCount = int.Parse(args[++i]);
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
                var files = Directory.EnumerateFiles(dir).Where(x => Regex.IsMatch(Path.GetFileName(x), pattern, RegexOptions.IgnoreCase)).ToList();
                Task.WhenAll(new int[Math.Min(files.Count(), _parallelTaskCount)].Select(async _ =>
                {
                    string file = null;
                    while (files.Any())
                    {
                        try
                        {
                            lock (_lock)
                            {
                                file = files.LastOrDefault();
                                if (string.IsNullOrEmpty(file))
                                {
                                    break;
                                }
                                files.RemoveAt(files.Count() - 1);
                            }

                            await UploadStreamAsync(file, blobContainer.GetBlockBlobReference($"{destDir}{Path.GetFileName(file)}"), _isOverwrite, 3).ConfigureAwait(false);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"Fail to upload stream {file}");
                            Console.WriteLine(e);
                        }
                    }
                })).Wait();
            }
            else
            {
                UploadStreamAsync(_source, blobContainer.GetBlockBlobReference(localPath), _isOverwrite, 3).Wait();
            }
        }

        private void Download()
        {
            if (_isRegex)
            {
                string dir, pattern;
                ExtractRegex(_source, out dir, out pattern, @"/");
                var destDir = _destination.EndsWith(@"\") ? _destination : _destination + @"\";
                var files = EnumerateFiles(dir).Where(x => Regex.IsMatch(Path.GetFileName(x.Uri.LocalPath), pattern)).ToList();
                Task.WhenAll(new int[Math.Min(files.Count(), _parallelTaskCount)].Select(async _ =>
                {
                    IListBlobItem file = null;
                    while (files.Any())
                    {
                        try
                        {
                            lock (_lock)
                            {
                                file = files.LastOrDefault();
                                if (file == null)
                                {
                                    break;
                                }
                                files.RemoveAt(files.Count() - 1);
                            }
                            await DownloadStreamAsync(destDir + Path.GetFileName(file.Uri.LocalPath), _blobClient.GetBlobReferenceFromServer(file.Uri), _isOverwrite, 3).ConfigureAwait(false);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"Fail to upload stream {file}");
                            Console.WriteLine(e);
                        }
                    }
                })).Wait();
            }
            else
            {
                DownloadStreamAsync(_destination, _blobClient.GetBlobReferenceFromServer(new Uri(_source)), _isOverwrite, 3).Wait();
            }
        }

        private async Task UploadStreamAsync(string file, ICloudBlob stream, bool isOverrite, int retry)
        {
            if (stream.Exists() && !isOverrite)
            {
                Console.WriteLine("Fail to upload stream since it exists! (overwrite?)");
                return;
            }

            for (var i = 0; i < retry; i++)
            {
                try
                {
                    await stream.DeleteIfExistsAsync().ConfigureAwait(false);
                    await stream.UploadFromFileAsync(file).ConfigureAwait(false);
                    break;
                }
                catch
                {
                    if (i != retry - 1)
                    {
                        await Task.Delay(5000);
                        continue;
                    }

                    Console.WriteLine("Fail to upload stream: " + stream);
                    throw;
                }
            }
        }

        private async Task DownloadStreamAsync(string file, ICloudBlob stream, bool isOverWrite, int retry)
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

            var tempFile = $"{Path.GetRandomFileName()}.{DateTime.Now.Ticks}";
            for (var i = 0; i < retry; i++)
            {
                try
                {
                    await stream.DownloadToFileAsync(tempFile, FileMode.Create).ConfigureAwait(false);
                    break;
                }
                catch (Exception ex)
                {
                    if (i != retry - 1)
                    {
                        await Task.Delay(5000);
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

        private IEnumerable<IListBlobItem> EnumerateFiles(string stream, int retry = 3)
        {
            for (var i = 0; i < retry; ++i)
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
                    if (i == retry - 1)
                    {
                        throw;
                    }

                    System.Threading.Thread.Sleep(3000);
                }
            }

            throw new Exception("Exception for EnumerateFiles for Blob");
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