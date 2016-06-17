/// <summary>
/// Only for PointTable
/// Reference: 
/// Microsoft.ObjectStore.HttpManagedClient.dll,
/// Microsoft.ObjectStore.PointDataLoader.dll
/// </summary>

namespace ObjectStoreAccess
{
    using Microsoft.Bond;
    using Microsoft.ObjectStore;
    using Microsoft.ObjectStore.HTTPInterface;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    internal class Sample
    {
        //private static void Main(string[] args)
        //{
        //    var key = new GeneralKey {Key = "Hello " + Guid.NewGuid().ToString()};  
        //    var value = new GeneralKey {Key = "World " + Guid.NewGuid().ToString()}; 
        //    //Test PROD
        //    {
        //        Console.WriteLine("Write to PROD...");
        //        var osConfig = ObjectStoreController.GetOsConfig(ObjectStoreContext.PRODs, "namespace", "tablename");
        //        ObjectStoreController.Write(osConfig, new Dictionary<GeneralKey, GeneralKey> { { key, value } });
        //        var ret = ObjectStoreController.Read<GeneralKey, GeneralKey>(ObjectStoreContext.HK2, "namespace", "tablename", new List<GeneralKey> { key });
        //    }
        //}
    }

    public class ObjectStoreContext
    {
        public static string INT = @"http://objectstoremulti.int.co.playmsn.com:83/sds/ObjectStoreQuery/V1/";
        public static string HK2 = @"http://objectstoremulti.prod.hk.binginternal.com:83/sds/ObjectStoreQuery/V1/";
        public static string CO3 = @"http://objectstoremulti.prod.co.binginternal.com:83/sds/ObjectStoreQuery/V1/";
        public static string DB4 = @"http://objectstoremulti.prod.db.binginternal.com:83/sds/ObjectStoreQuery/V1/";
        public static string BN1 = @"http://objectstoremulti.prod.bn.binginternal.com:83/sds/ObjectStoreQuery/V1/";
        public static string CH1D = @"http://objectstoremulti.prod.ch.binginternal.com:83/sds/ObjectStoreQuery/V1/";

        public static string[] INTs = { "objectstoremulti.int.co.playmsn.com:83" };

        public static string[] PRODs =
        {
            "objectstoremulti.prod.co.binginternal.com:83",
            "objectstoremulti.prod.hk.binginternal.com:83",
            "objectstoremulti.prod.bn.binginternal.com:83",
            "objectstoremulti.prod.ch.binginternal.com:83",
            "objectstoremulti.prod.db.binginternal.com:83"
        };
    }

    public class ObjectStoreController
    {
        public static DataLoadConfiguration GetOsConfig(IEnumerable<string> vips, string namespaceName, string tableName)
        {
            return new DataLoadConfiguration(vips.Select(x => new Microsoft.ObjectStore.VIP(x) as ITableLocation).ToList(),
                    namespaceName,
                    tableName,
                    maxObjectsPerRequest: 5,
                    maxSimultaneousRequests: 20,
                    retriesPerRequest: 3,
                    httpTimeoutInMs: 10000,
                    maxKeysPerSec: 100,
                    reduceTrafficWhenThrottlingOccurs: true);
        }

        public static List<IDataLoadResult> Write<TKey, TValue>(
                        DataLoadConfiguration config,
                        Dictionary<TKey, TValue> kvps)
            where TKey : IBondSerializable
            where TValue : IBondSerializable
        {
            using (var loader = new DataLoader(config))
            {
                foreach (var kvp in kvps)
                {
                    //loader.Delete(kvp.Key, "");
                    loader.Send(kvp.Key, kvp.Value, "");
                }
                loader.Flush();
                return loader.Receive(true);
            }
        }

        public static List<IDataLoadResult> Write<TKey, TValue>(
                        string[] vips,
                        string namespaceName,
                        string tableName,
                        Dictionary<TKey, TValue> kvps)
            where TKey : IBondSerializable
            where TValue : IBondSerializable
        {
            var config = GetOsConfig(vips, namespaceName, tableName);
            using (var loader = new DataLoader(config))
            {
                foreach (var kvp in kvps)
                {
                    loader.Send(kvp.Key, kvp.Value, "");
                }
                loader.Flush();
                return loader.Receive(true);
            }
        }

        public static List<TValue> Read<TKey, TValue>(
                        string osEndPointUrl,
                        string namespaceName,
                        string tableName,
                        IEnumerable<TKey> keys)
            where TKey : IBondSerializable
            where TValue : IBondSerializable, new()
        {
            var results = new List<TValue>();
            foreach (var key in keys)
            {
                var response = ObjectStoreHttp.Read(osEndPointUrl, namespaceName, tableName, new[] { key });
                var codes = ObjectStoreHttp.GetResponseCodes(response);
                if (codes.Any(code => code == ResponseCode.Failure))
                {
                    results.Add(default(TValue));
                }
                else
                {
                    var values = ObjectStoreHttp.ResponseToValues(response);
                    if (values.Count() == 1)
                    {
                        results.Add(BondFromBase64<TValue>(values.First()));
                    }
                    else
                    {
                        throw new System.ApplicationException("Should not happen: one key to multiple value");
                    }
                }
            }

            return results;
        }

        public static string BondToBase64<TBond>(TBond bond)
            where TBond : IBondSerializable
        {
            using (var ms = new MemoryStream())
            {
                var writer = new CompactBinaryProtocolWriter(ms);
                bond.Write(writer);
                var buffer = ms.ToArray();
                return Convert.ToBase64String(buffer);
            }
        }

        public static TBond BondFromBase64<TBond>(string base64)
            where TBond : IBondSerializable, new()
        {
            var buffer = Convert.FromBase64String(base64);
            using (var reader = new CompactBinaryProtocolReader(buffer))
            {
                var bond = new TBond();
                bond.Read(reader);
                return bond;
            }
        }
    }
}
