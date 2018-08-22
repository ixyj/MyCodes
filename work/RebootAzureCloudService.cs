namespace AzureCloudService
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Cache;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml;

    class AzureCloudService
    {
        private static string[] ParameterKeys = new[] { "SubscriptionId", "ServiceName", "RoleInstance", "Thumbprint", "Slot", "IsChina", "TotalInst", "TimesToComplete" };

        static void Main(string[] args)
        {
            try
            {
                var parms = ParseParameters(args);
                string value;
                var totalInst = Convert.ToInt32(parms["TotalInst"]);
                var curInst = -1;
                var cocurrency = (int)Math.Ceiling(totalInst / (parms.TryGetValue("TimesToComplete", out value) ? Convert.ToSingle(value) : 3.0f));
                var isChina = parms.TryGetValue("IsChina", out value) ? Convert.ToBoolean(value) : true;
                var slot = parms.TryGetValue("Slot", out value) ? value : "Production";

                var finishInsts = new ConcurrentQueue<int>();
                var failedInsts = new ConcurrentQueue<int>();
                var instanceName = parms["RoleInstance"].TrimEnd("0123456789".ToArray());
                var tasks = new object[cocurrency].Select(_ => Task.Run(async () =>
                {
                    while (curInst < totalInst)
                    {
                        var instSnapshot = GetInstanceStatus(parms["SubscriptionId"], parms["ServiceName"], parms["Thumbprint"], isChina);
                        if (instSnapshot == null || !instSnapshot.Any())
                        {
                            await Task.Delay(30 * 1000);
                            continue;
                        }

                        var readyCnt = instSnapshot.Sum(x => x.Item2 == "ReadyRole" ? 1 : 0);
                        if (readyCnt <= totalInst - cocurrency)
                        {
                            Console.WriteLine("No enough Ready instances, waiting for a while ...");
                            await Task.Delay(30 * 1000);
                            continue;
                        }

                        var inst = Interlocked.Increment(ref curInst);
                        if (inst >= totalInst)
                        {
                            break;
                        }

                        if (RebootServer(parms["SubscriptionId"], parms["ServiceName"], $"{instanceName}{inst}", parms["Thumbprint"], slot, isChina))
                        {
                            finishInsts.Enqueue(inst);
                        }
                        else
                        {
                            failedInsts.Enqueue(inst);
                        }

                        if (inst == totalInst - 1)
                        {
                            break;
                        }

                        await Task.Delay(10 * 60 * 1000);
                    }
                }));

                Task.WaitAll(tasks.ToArray());

                Console.WriteLine($"Reboot completed, Succeeded: {finishInsts.Count}/{totalInst}, Failed Instances: {string.Join(",", failedInsts.Select(x => x.ToString()))}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static IDictionary<string, string> ParseParameters(string[] args)
        {
            var parms = new Dictionary<string, string>();
            foreach (var arg in args)
            {
                var sp = arg.Split(":".ToArray());
                if (sp.Length == 2)
                {
                    var key = ParameterKeys.FirstOrDefault(x => x.Equals(sp[0], StringComparison.InvariantCultureIgnoreCase));
                    if (key != null)
                    {
                        parms[key] = sp[1];
                    }
                }
            }

            return parms;
        }

        private static X509Certificate2 GetCertificate(string thumbprint)
        {
            var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly);
            var certs = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);
            store.Close();

            if (certs.Count == 0)
            {
                Console.WriteLine("Fail to fetch Certificate!");
                return null;
            }

            return certs[0];
        }

        private static IEnumerable<Tuple<string, string>> GetInstanceStatus(string subscriptionId, string serviceName, string thumbprint, bool isChina = true)
        {
            IEnumerable<Tuple<string, string>> instStatus = null;
            try
            {
                var managementUrl = isChina ? "https://management.core.chinacloudapi.cn" : "https://management.core.windows.net";
                var url = $"{managementUrl}/{subscriptionId}/services/hostedservices/{serviceName}?embed-detail=true";
                string result;
                var code = SendHttp(url, thumbprint, out result, "GET");
                if (code != HttpStatusCode.OK)
                {
                    Console.WriteLine($"Failed to Get status for {serviceName}");
                    return instStatus;
                }

                var xml = new XmlDocument();
                xml.LoadXml(result);
                var nsmgr = new XmlNamespaceManager(xml.NameTable);
                nsmgr.AddNamespace("abc", "http://schemas.microsoft.com/windowsazure");
                instStatus = xml.SelectNodes("/abc:HostedService/abc:Deployments/abc:Deployment/abc:RoleInstanceList/abc:RoleInstance", nsmgr).Cast<XmlNode>().Select(x =>
                    new Tuple<string, string>(x.SelectSingleNode("abc:InstanceName", nsmgr).InnerText, x.SelectSingleNode("abc:InstanceStatus", nsmgr).InnerText));
                if (!instStatus.Any())
                {
                    Console.WriteLine($"Not found any instance for {serviceName}");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception for {serviceName}: {e}");
            }

            return instStatus;
        }

        private static bool RebootServer(string subscriptionId, string serviceName, string roleInstance, string thumbprint, string slot, bool isChina = true)
        {
            try
            {
                var managementUrl = isChina ? "https://management.core.chinacloudapi.cn" : "https://management.core.windows.net";
                var url = $"{managementUrl}/{subscriptionId}/services/hostedservices/{serviceName}/deploymentslots/{slot}/roleinstances/{roleInstance}?comp=reboot";
                string result;
                var code = SendHttp(url, thumbprint, out result);
                if (code == HttpStatusCode.OK || code == HttpStatusCode.Accepted)
                {
                    Console.WriteLine($"Succeeded to reboot {roleInstance}");
                    return true;
                }

                Console.WriteLine($"Failed to reboot : {roleInstance}. HttpCode: {code}, Info: {result}");
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception for {roleInstance}: {e}");
                return false;
            }
        }

        private static HttpStatusCode SendHttp(string url, string thumbprint, out string result, string method = "POST")
        {
            try
            {
                var request = WebRequest.CreateHttp(url);
                request.ClientCertificates.Add(GetCertificate(thumbprint));
                request.Headers.Add("x-ms-version", "2012-03-01");
                request.ContentLength = 0;
                request.Method = method;
                request.CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);

                var response = (HttpWebResponse)request.GetResponse();
                result = null;
                try
                {
                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        result = reader.ReadToEnd();
                    }
                }
                catch
                {
                }

                return response.StatusCode;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception for url {url}: {e}");
                result = null;
                return HttpStatusCode.ExpectationFailed;
            }
        }
    }
}