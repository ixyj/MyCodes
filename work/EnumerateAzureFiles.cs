namespace Helper
{
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Microsoft.WindowsAzure.Storage.Table;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;

    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                int m = 1, d = 0;
                long total = 0, count = 0;
                var locker = new object();
                foreach (var year in (new[] { 2017, 2018, 2019 }))
                {
                    var tasks = new object[10].Select(_ => Task.Run(() =>
                    {
                        int dd = 0, mm = 0;
                        while (m <= 12)
                        {
                            lock (locker)
                            {
                                if (d >= 31)
                                {
                                    ++m;
                                    if (m > 12) break;
                                    d = 1;
                                }
                                else
                                    ++d;
                                dd = d;
                                mm = m;

                                Console.WriteLine($"[Info] Starting to process {year}-{mm}-{dd}");
                            }

                            try
                            {
                                AzureCopy cmd = null;
                                lock (locker) cmd = new AzureCopy();
                                var blobs = cmd.EnumerateFiles($"https://xiaobingbase.blob.core.chinacloudapi.cn/duplexlog/xiaomiiot-Yeelight/{year}/{mm}/{dd}/").ToList();
                                if (!blobs.Any()) continue;
                                var wavs = cmd.EnumerateAllFiles(blobs.First()).Where(y => y.Uri.AbsoluteUri.ToLower().EndsWith("wav")).ToList();
                                Interlocked.Add(ref count, wavs.Count);
                                foreach (var v in wavs)
                                {
                                    try
                                    {
                                        Interlocked.Add(ref total, cmd.FetchProperties(v).Length);
                                    }
                                    catch (Exception e)
                                    {
                                        lock (locker) Console.WriteLine($"[Warn]Retry {v.Uri.AbsoluteUri} again for exception {e.Message}");
                                        Task.Delay(10000);
                                        lock (locker) cmd = new AzureCopy();
                                        Interlocked.Add(ref total, cmd.FetchProperties(v).Length);
                                    }
                                }

                                Console.WriteLine($"COUNT:\t{count}\t{total}");
                            }
                            catch (Exception e)
                            {
                                lock (locker) Console.WriteLine($"[Error]Failed for {mm}-{dd}: {e.Message}");
                            }
                        }
                    }));

                    Task.WaitAll(tasks.ToArray());

                    m = 1;
                    d = 0; 
                }

                Console.WriteLine($"{count}\t{total}");
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
        private static object Locker = new object();
        private bool _isRegex = false;
        private bool _isOverwrite = false;
        private bool? _isUpload = null;
        private string _source = null;
        private string _destination = null;
        private int _parallelTaskCount = 1;
        private object _lock = new object();
        private CloudBlobClient _blobClient = null;
        private CloudTableClient _tableClient = null;

        public AzureCopy()
        {
            string key = null;
            lock (Locker)
            {
                var reader = new BinaryReader(new FileStream(@".\sharedAcessSignature", FileMode.Open));
                key = System.Text.Encoding.UTF8.GetString(reader.ReadBytes(1024000));
                reader.Close();
            }

            if (string.IsNullOrWhiteSpace(key))
            {
                throw new Exception("No sharedAcessSignature found!");
            }

            var cloudStorageAccount = CloudStorageAccount.Parse(key);
            _blobClient = cloudStorageAccount.CreateCloudBlobClient();
            _tableClient = cloudStorageAccount.CreateCloudTableClient();
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

        /***
         *  Delete:     table.ExecuteAsync(TableOperation.Delete(new TableEntity(partitionKey, rowKey) { ETag = "*" }))
         *  DeleteAll:  table.DeleteAsync(entity.PartitionKey, entity.RowKey).ConfigureAwait(false);   
         *  Get:        table.ExecuteAsync(TableOperation.Retrieve<ITableEntity>(partitionKey, rowKey))
         *  GetAll:     table.CreateQuery<ITableEntity>().Where(m => m.PartitionKey == partitionKey))
         *  InsertOrReplaceBatch: batchOperation = new TableBatchOperation();  batchOperation.Add(TableOperation.InsertOrReplace(ITableEntity); <=100
         *  table.ExecuteBatchAsync(batchOperation)
         * **/
        public async Task InsertOrReplaceTable(string tableName, string partitionKey, string rowKey, object data)
        {
            var table = _tableClient.GetTableReference(tableName);
            await table.CreateIfNotExistsAsync().ConfigureAwait(false);
            await table.ExecuteAsync(TableOperation.InsertOrReplace(new CustomTableEntity(partitionKey, rowKey, data))).ConfigureAwait(false);
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
            blobContainer.CreateIfNotExists();
            var localPath = _destination.Substring(_destination.IndexOf(container) + container.Length + 1);
            if (_isRegex)
            {
                string dir, pattern;
                ExtractRegex(_source, out dir, out pattern, @"\");
                var destDir = localPath.EndsWith("/") ? localPath : localPath + "/";
                if (destDir == "/")
                {
                    destDir = string.Empty;
                }
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

        public BlobProperties FetchProperties(IListBlobItem blob)
        {
            return (blob as CloudBlob)?.Properties;
        }

        // Enumerate all blobs in given stream recursively
        public IEnumerable<IListBlobItem> EnumerateAllFiles(IListBlobItem root)
        {
            return root is CloudBlobDirectory dir ? dir.ListBlobs(true) : new[] { root };
        }

        // Only enumerate blobs in given stream, not recursively
        public IEnumerable<IListBlobItem> EnumerateFiles(string stream, int retry = 3)
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

    public class CustomTableEntity : TableEntity
    {
        public CustomTableEntity(string partitionKey, string rowKey, object data)
            : base(partitionKey, rowKey)
        {
            Data = JsonConvert.SerializeObject(data);
        }

        public string Data { get; set; }
    }
}