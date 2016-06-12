namespace Request
{
    using System;
    using System.IO;
    using System.Net;
    using System.Threading;
    using System.Web;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;

    class Program
    {
        private const string userAgent = "Mozilla/5.0 (Windows NT 6.3; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/43.0.2357.81 Safari/537.36";
        private static StreamReader reader = null;
        private static int threads = 1;
        private static int runningThread = 0;
        private static int finishThread = 0;
        private static bool isFinish = false;

        static void Main(string[] args)
        {
            try
            {
                GenerateRequest("new.request.txt", args[0], args[1]);
                reader = new StreamReader("new.request.txt");
                threads = Convert.ToInt32(args[2]);
                var repeat = Convert.ToInt32(args[3]);

                Console.WriteLine("Begin...");

                for (var j = 0; j < repeat; j++)
                {
                    MainThread(threads);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.WriteLine("this.exe request_file host_file thread_count repeat_count");
            }
        }

        private static void GenerateRequest(string output, string requestFile, string hostFile)
        {
            var writer = new StreamWriter(output);

            var hosts = File.ReadAllLines(hostFile);
            var requests = File.ReadAllLines(requestFile);
            foreach (var host in hosts)
            {
                foreach (var req in requests)
                {
                    var atlahostMatxh = Regex.Match(req, @"atlahostname=[^&]+", RegexOptions.IgnoreCase);
                    if (atlahostMatxh.Success)
                    {
                        var newReq = Regex.Replace(req, @"atlahostname=[^&]+", "&atlahostname=" + host);
                        writer.WriteLine(newReq);
                    }
                    else
                    {
                        writer.WriteLine(req + "&atlahostname=" + host);
                    }

                    writer.Flush();
                }
            }

            writer.Close();
        }

        private static readonly List<ManualResetEvent> ManualEvents = new List<ManualResetEvent>(); 
        private static void MainThread(int threads = 5)
        {
            for (var i = 0; i < threads; i++)
            {
                var mre = new ManualResetEvent(false);
                ManualEvents.Add(mre);
                ThreadPool.QueueUserWorkItem(SendRequest, mre);
            }

            WaitHandle.WaitAll(ManualEvents.ToArray());
            Console.WriteLine("Thread Finished!");
        }

        private static void SendRequest(object obj)
        {
            lock (reader)
            {
                ++runningThread;
            }

            string line = null;
            while (!isFinish)
            {
                try
                {
                    lock (reader)
                    {
                        if ((line = reader.ReadLine()) == null)
                        {
                            --runningThread;
                            ++finishThread;

                            if (runningThread == 0 && finishThread == threads)
                            {
                                reader.Close();
                                reader.Dispose();
                                Console.Write("All test queries have been sent!");
                                isFinish = true;
                            }
                        }
                    }

                    if (isFinish)
                    {
                        var mre = (ManualResetEvent)obj;
                        mre.Set();
                        Environment.Exit(0);
                    }

                    if (!string.IsNullOrEmpty(line))
                    {
                        var res = SendHttpRequest(line);      //HttpUtility.UrlEncode(line)
                    }
                }
                catch (Exception ex)
                {
                    lock (reader)
                    {
                        Console.WriteLine("failed request:" + line);
                        Console.WriteLine(ex.ToString());
                    }
                }
            }
        }

        private static string SendHttpRequest(string url)
        {
            var interval = 1; //seconds
            string res = null;

            if (string.IsNullOrEmpty(url))
            {
                return null;
            }

            for (int i = 0; i < 10; i++)
            {
                var startTime = DateTime.Now;
                HttpWebRequest request = null;
                try
                {
                    request = WebRequest.Create(url) as HttpWebRequest;
                    request.Method = "GET";
                    request.UserAgent = userAgent;
                    request.Timeout = 3000;

                    var httpWebResponse = request.GetResponse() as HttpWebResponse;
                    if (httpWebResponse != null && httpWebResponse.StatusCode == HttpStatusCode.OK)
                    {
                        using (var response = new StreamReader(httpWebResponse.GetResponseStream()))
                        {
                            res = response.ReadToEnd();
                        }

                        break;
                    }
                }
                catch
                {
                    // retry
                    continue;
                }
                finally
                {        
                    request.Abort(); // release resource used by request

                    var endTime = DateTime.Now;
                    if (startTime.AddSeconds(interval) > endTime)
                    {
                        var remain = interval * 1000 + startTime.Millisecond - endTime.Millisecond;
                        Console.WriteLine(remain);
                        Thread.Sleep(remain);
                    }
                }
            }

            return res;
        }
    }
}
