﻿namespace XiaoIce.LiveStreamHub
{
    using Microsoft.ApplicationInsights;
    using System;
    using System.Threading;
    using XiaoIce.Common.Library.Cache;
    using XiaoIce.Utility.Cache;

    public class ai
    {
        public static void Main(string[] argv)
        {
            var duplexRedisCacheConnectionString = argv[0];
            var ipListConnectionString = argv[1];
            var ipConnectionStringPrefix = argv[2];
            var InstrumentationKey = argv[3];
            var interval = int.Parse(argv[4]) * 1000;
            var cache = new RedisCache(duplexRedisCacheConnectionString);
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
                        if (string.IsNullOrEmpty(data))
                        {
                            throw new Exception($"Fail to fetch connect count for: {ipConnectionStringPrefix + ip}");
                        }

                        count += int.Parse(data);
                    }

                    var telemetry = new TelemetryClient();
                    telemetry.InstrumentationKey = InstrumentationKey;
                    telemetry.TrackMetric("curAllConn", count);
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
}