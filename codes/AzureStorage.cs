namespace Testing
{
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    public class AzureTest
    {
        public static int Main(string[] args)
        {
            var datenow = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "China Standard Time"); 
            var sharedAcessSignature = args[0].Replace("^&", "&").Replace("##", "%"); 
            var srcContainerText = args[1];
            var srcRawPath = args[2];
            var destContainerText = args[3];
            var lastTime = File.Exists(args[4]) ? DateTime.Parse(File.ReadAllText(args[4])) : new DateTime(0);
            var blobClient = InitCloudBlobClient(sharedAcessSignature);
            var srcBlobs = EnumerateBlobs(blobClient, srcContainerText, srcRawPath.Replace("#DateTime#", datenow.ToString("yyyy/M/d")));
            var srcBlobs2 = EnumerateBlobs(blobClient, srcContainerText, srcRawPath.Replace("#DateTime#", datenow.AddDays(-1).ToString("yyyy/M/d")));
            var dictIndex = new Dictionary<string, int>();
            var destContainer = blobClient.GetContainerReference(destContainerText);
            var ignorePrefix = new[] { "welcome", "proactive", "transition", "goodbye" };
            var thisNewTimestamp = lastTime;
            foreach (var srcBlob in srcBlobs.Union(srcBlobs2))
            {
                try
                {
                    var blob = srcBlob.Container.GetBlockBlobReference(GetRelativePath(srcBlob.Uri, srcContainerText));
                    var lines = blob.DownloadText().Split("\n".ToArray()).Where(x => x.Trim("\t\r\n".ToArray()).Any() && !ignorePrefix.Any(y => x.ToLower().StartsWith(y))).ToArray();
                    if (lines.Count() <= 1) continue;
                    var line1 = lines.First().Split("\t".ToArray());
                    if (!line1.Contains("TurnBeginTime") || !line1.Contains("AnswerFeed")) continue;
                    if (!dictIndex.Any())
                    {
                        for (var i = 0; i < line1.Length; ++i)
                        {
                            if (line1[i] == "Request" || line1[i] == "Response" || line1[i] == "AnswerFeed" || line1[i] == "TurnBeginTime")
                            {
                                dictIndex[line1[i]] = i;
                            }
                        }
                    }

                    var turntime = DateTime.Parse(lines[1].Split("\t".ToArray())[dictIndex["TurnBeginTime"]]);
                    if (turntime <= lastTime) continue;

                    var filter = new List<string> { "Request\tResponse\tTime" };
                    foreach (var line in lines.Where((x, i) => i > 0))
                    {
                        try
                        {
                            var separates = line.Split("\t".ToArray());
                            if (separates[dictIndex["AnswerFeed"]] != "IoTController") continue;
                            filter.Add($"{separates[dictIndex["Request"]]}\t{separates[dictIndex["Response"]]}\t{separates[dictIndex["TurnBeginTime"]]}");
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    }

                    if (filter.Count() == 1) continue;
                    var file = $"{turntime.ToString("yyyy/M/d")}/{Path.GetFileName(srcBlob.Uri.AbsolutePath)}";
                    UploadText(destContainer, string.Join("\n", filter), file);

                    if (thisNewTimestamp < turntime)
                    {
                        thisNewTimestamp = turntime;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            File.WriteAllText(args[4], TimeZoneInfo.ConvertTimeBySystemTimeZoneId(thisNewTimestamp, "China Standard Time").ToString());
            return 0;
        }

        private static CloudBlobClient InitCloudBlobClient(string connectionString)
        {
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            return storageAccount.CreateCloudBlobClient();
        }

        private static IEnumerable<IListBlobItem> EnumerateBlobs(CloudBlobClient blobClient, string container, string path)
        {
            var blobContainer = blobClient.GetContainerReference(container);  // ListContainers, ListBlobs ...
            return blobContainer.ListBlobs(path).Where(x => x.Uri.AbsolutePath.EndsWith(".txt"));
        }

        private static void UploadText(CloudBlobContainer container, string text, string filePath)
        {
            var blob = container.GetBlockBlobReference(filePath);
            using (var ms = new MemoryStream())
            {
                var bytes = Encoding.UTF8.GetBytes(text);
                ms.Write(bytes, 0, bytes.Length);
                ms.Seek(0, SeekOrigin.Begin);
                blob.UploadFromStream(ms);
            }
        }

        private static string GetRelativePath(Uri uri, string container)
        {
            var rootDir = $"/{container}/";
            return uri.LocalPath.StartsWith(rootDir) ? uri.LocalPath.Substring(rootDir.Length) : uri.LocalPath;
        }
    }
}