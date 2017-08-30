namespace AiHelper
{
    using Microsoft.ApplicationInsights;
    using Newtonsoft.Json;
    using StackExchange.Redis;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    public class ai
    {
        public static void Main(string[] argv)
        {
            var duplexRedisCacheConnectionString = argv[0];
            var ipListConnectionString = argv[1];
            var ipConnectionStringPrefix = argv[2];
            var InstrumentationKey = argv[3];
            var interval = int.Parse(argv[4]) * 1000;
            var partner = argv[5];
            var cache = new RedisCache(duplexRedisCacheConnectionString);
            var metadata = new Dictionary<string, string> { { "partner", partner } };
            var telemetry = new TelemetryClient();
            telemetry.InstrumentationKey = InstrumentationKey;
            while (true)
            {
                try
                {
                    var count = 0;
                    foreach (var ip in cache.DefaultCache.SetMembers(ipListConnectionString))
                    {
                        CacheItem item;
                        cache.TryGetData(ipConnectionStringPrefix + ip, out item);
                        var data = item?.Data?.ToString();
                        count += int.Parse(data ?? "0");
                    }
                                            
                    telemetry.TrackMetric("curAllConn", count, metadata);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                finally
                {
                    Thread.Sleep(interval);
                }
            }
        }
    }

    public sealed class RedisCache : IDisposable
    {    
        private string _connectionString;

        private Lazy<ConnectionMultiplexer> _lazyDefaultConnection;
        private Lazy<IDatabase> _lazyDefaultCache;

        private ConnectionMultiplexer DefaultConnection
        {
            get
            {
                return _lazyDefaultConnection.Value;
            }
        }

        public IDatabase DefaultCache
        {
            get
            {
                return _lazyDefaultCache.Value;
            }
        }

        public bool TrySetData(string key, object data, TimeSpan timeout)
        {
            return TrySetData(key, data, string.Empty, timeout);
        }

        public bool TrySetData(string key, object data, string metaData, TimeSpan timeout)
        {
            try
            {
                CacheItem cacheItem = new CacheItem();
                cacheItem.Key = key;
                cacheItem.Data = data;
                cacheItem.MetaData = metaData;
                var serializeObj = JsonConvert.SerializeObject(cacheItem);
                return DefaultCache.StringSet(key, serializeObj, timeout);
            }
            catch (Exception e)
            {
                Console.WriteLine($"TrySetData error: {e}");
            }

            return false;
        }

        public bool TryGetData(string key, out CacheItem cacheItem)
        {
            try
            {
                string serializedObj = DefaultCache.StringGet(key);
                if (serializedObj != null)
                {
                    cacheItem = JsonConvert.DeserializeObject<CacheItem>(serializedObj);
                    if (cacheItem != null)
                    {
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"TryGetData error: {e}");
            }

            cacheItem = null;
            return false;
        }

        public object GetData(string key)
        {
            try
            {
                CacheItem cacheItem = null;
                if (TryGetData(key, out cacheItem))
                {
                    return cacheItem.Data;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"GetData error: {e}");
            }

            return null;
        }

        public T GetData<T>(string key)
        {
            try
            {
                var serializedObj = GetData(key);
                return serializedObj == null ? default(T) : (T)serializedObj;
            }
            catch (Exception e)
            {

                Console.WriteLine("GetData(T) error: {0}", e.ToString());
            }

            return default(T);
        }

        public bool DeleteKey(string key)
        {
            try
            {
                return DefaultCache.KeyDelete(key);
            }
            catch (Exception e)
            {
                Console.WriteLine($"DeleteKey error: {e}");
            }

            return false;
        }

        public bool SortedSetAppend(string key, object data, double score)
        {
            RedisValue value = JsonConvert.SerializeObject(data);
            return DefaultCache.SortedSetAdd(key, value, score);
        }

        public string[] SortedSetPop(string key, double start, double stop, int take)
        {
            var trans = DefaultCache.CreateTransaction();

            var results = trans.SortedSetRangeByScoreAsync(key, start, stop, Exclude.None, Order.Ascending, 0, take, CommandFlags.None);
            var deletes = trans.SortedSetRemoveRangeByRankAsync(key, 0, take);
            trans.Execute();

            return results.Result.Select(m => m.ToString()).ToArray();

        }

        public bool SetAdd(string key, string value)
        {
            return DefaultCache.SetAdd(key, value);
        }

        public bool SetRemove(string key, string value)
        {
            return DefaultCache.SetRemove(key, value);
        }

        public bool SetContains(string key, string value)
        {
            return DefaultCache.SetContains(key, value);
        }

        public void Dispose()
        {
            DefaultConnection.Close();
        }

        public RedisCache(string connectionString)
        {
            this._connectionString = connectionString;
            _lazyDefaultConnection = new Lazy<ConnectionMultiplexer>(() =>
            {
                Exception lastEx = null;
                for (int i = 0; i < 2; i++)
                {
                    try
                    {
                        return ConnectionMultiplexer.Connect(this._connectionString);
                    }
                    catch (Exception e)
                    {
                        lastEx = e;
                        Console.WriteLine("Connect error, tried time(s): " + (i + 1) + ", exception: " + e.ToString());
                    }
                }
                throw lastEx;
            });

            _lazyDefaultCache = new Lazy<IDatabase>(() => DefaultConnection.GetDatabase());
        }

        public bool KeyExpire(string key, TimeSpan timeout)
        {
            return DefaultCache.KeyExpire(key, timeout);
        }

        public bool TrySetHashSetData(string key, string hashKey, object data, TimeSpan timeout)
        {
            try
            {
                CacheItem cacheItem = new CacheItem();
                cacheItem.Key = key;
                cacheItem.Data = data;
                var serializeObj = JsonConvert.SerializeObject(cacheItem);
                if (!DefaultCache.KeyExists(key))
                {
                    DefaultCache.KeyExpire(key, timeout);
                }
                return DefaultCache.HashSet(key, hashKey, serializeObj);
            }
            catch (Exception e)
            {
                Console.WriteLine("TrySetData error: {0}", e.ToString());
            }

            return false;
        }

        public bool TryGetHashSetData(string key, string hashKey, out CacheItem cacheItem)
        {
            try
            {
                string serializedObj = DefaultCache.HashGet(key, hashKey);
                Console.WriteLine(serializedObj);
                object deserializedObj = null;
                if (serializedObj != null)
                {
                    try
                    {
                        deserializedObj = JsonConvert.DeserializeObject(serializedObj);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"fail to deserialize the data {e}");
                    }

                    cacheItem = deserializedObj != null ? deserializedObj as CacheItem : null;
                    if (cacheItem == null)
                    {
                        cacheItem = new CacheItem()
                        {
                            Data = serializedObj
                        };
                    }

                    return true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("TryGetData error: {0}", e.ToString());
            }

            cacheItem = null;
            return false;
        }

        public bool TrySetCounter(string key, long start)
        {
            try
            {
                return DefaultCache.StringSet(key, start);
            }
            catch (Exception e)
            {
                Console.WriteLine("TrySetCounter failed for redis exception!", e);
            }
            return false;
        }

        public bool TrySetCounter(string key, long start, TimeSpan timeout)
        {
            try
            {
                return DefaultCache.StringSet(key, start, timeout);
            }
            catch (Exception e)
            {
                Console.WriteLine("TrySetCounter failed for redis exception!", e);
            }
            return false;
        }

        public long TryIncrementCounter(string key)
        {
            try
            {
                return DefaultCache.StringIncrement(key);
            }
            catch (Exception e)
            {
                Console.WriteLine("TryIncrementCounter failed for redis exception!", e);
                throw;
            } 
        }

        public long TryIncrementCounter(string key, long step)
        {
            try
            {
                return DefaultCache.StringIncrement(key, step);
            }
            catch (Exception e)
            {
                Console.WriteLine("TryIncrementCounter failed for redis exception!", e);
                throw;
            }     
        }

        public long TryDecrementCounter(string key)
        {
            try
            {
                return DefaultCache.StringDecrement(key);
            }
            catch (Exception e)
            {
                Console.WriteLine("TryDecrementCounter failed for redis exception!", e);
                throw;
            }     
        }

        public long TryDecrementCounter(string key, long step)
        {
            try
            {
                return DefaultCache.StringDecrement(key, step);
            }
            catch (Exception e)
            {
                Console.WriteLine("TryDecrementCounter failed for redis exception!", e);
                throw;
            }       
        }

        public long TryGetCounter(string key, out bool isExist)
        {
            isExist = false;
            try
            {
                var value = DefaultCache.StringGet(key);
                if (!value.IsNullOrEmpty)
                {
                    isExist = true;
                }
                return (long)value;
            }
            catch (Exception e)
            {
                Console.WriteLine("TryGetCounter failed for redis exception!", e);
                throw;
            }  
        }

        public long TryGetCounter(string key)
        {
            try
            {
                return (long)DefaultCache.StringGet(key);
            }
            catch (Exception e)
            {
                Console.WriteLine("TryGetCounter failed for redis exception!", e);
                throw;
            }       
        }
    }

    public class CacheItem
    {
        public string Key { get; set; }

        public object Data { get; set; }

        public string MetaData { get; set; }

        public DateTime LastUpdateUtc { get; set; }

        public DateTime LastAccessUtc { get; set; }

        public DateTime ExpireUtc { get; set; }

        ~CacheItem()
        {
            var disposable = Data as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
            }
        }
    }
}