namespace Testing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    // Avoid to use "async Void";
    // Could not catch exception from "async Void";
    // Always use "async", since it may lead to deadlock (root cause: await context), though nor repro in Console app
    public class AsyncHelper
    {
        public static int Main(string[] args)
        {
            //Testing(); // Not wait for Testing()
            //Testing().Wait();       // it still does not wait "Doing 3". Others will complete if no Thread.Sleep(Timeout.Infinite)
            //Console.WriteLine(Testing2());   // Console.WriteLine("System.Threading.Tasks.Task`1[System.String]", not "1;2;3;4;5"), Testing2() is still ongoing but WriteLine does not wait
            var result = Testing2().Result;  // wait for Testing2() completion
            Console.WriteLine(result);
            Console.WriteLine("After Main.Testing ... ");
            //Thread.Sleep(Timeout.Infinite);  //program exists without Doing() completion if no this line
            return 0;
        }

        static async Task Testing()
        {
            Console.WriteLine("Before Testing ...");
            await Doing();
            Console.WriteLine("After Testing ...");
        }
        static async Task Doing()
        {
            Console.WriteLine("Before doing ...");
            await Task.Run(() =>
            {
                Thread.Sleep(2000);
                Console.WriteLine("Doing 1");
                return 1;
            });

            await Task.WhenAny(new[]
            {
                Task.Run(() =>
                {
                    Thread.Sleep(1000);
                    Console.WriteLine("Doing 2");
                    return 2;
                }),
                Task.Run(() =>
                {
                Thread.Sleep(6000);
                Console.WriteLine("Doing 3");
                return 3;
                })
            });

            await Task.Run(() =>
            {
                Thread.Sleep(2000);
                Console.WriteLine("Doing 4");
                return 4;
            });
            Console.WriteLine("After doing ...");
        }


        static async Task<string> Testing2()
        {
            Console.WriteLine("Before Testing2 ...");
            var result = await Doing2();
            var text = string.Join(";", result.Select(x => x.ToString()));
            Console.WriteLine("After Testing2 ...");

            return text;
        }
        static async Task<List<int>> Doing2()
        {
            Console.WriteLine("Before Doing2 ...");
            var result = await Task.Run(() =>
            {
                var list = new List<int>();
                for (var i = 0; i < 5; ++i)
                {
                    Console.WriteLine("doing ... " + i);
                    Thread.Sleep(i * 1000);
                    list.Add(i);
                }

                return list;
            });
            Console.WriteLine("After Doing2 ...");

            return result; // List<int>
        }
    }

}