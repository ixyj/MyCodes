namespace Helper
{
    using IronPython.Hosting;
    using Microsoft.Scripting.Hosting;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class App
    {
        public static int Main(string[] args)
        {
            try
            {
                var result = IpyEngine.ExecuteFile(@"test.py");     // python code:  def Add() ...  ... out = "hello python"
                Console.WriteLine(result.GetVariable<string>("out"));

                var AddFunc = result.GetVariable<Func<int, int, int>>("Add");
                Console.WriteLine(AddFunc(12, 23));

                var times1 = IpyEngine.Execute<int>("print(a+b)\nTimes(a,b)", new Dictionary<string, object> { { "a", 2 }, { "b", 3 }, { "Times", new Func<int, int, int>((x, y) => x * y) } });
                var times2 = IpyEngine.Execute<int>("print(a+b)\nApi.Times(a,b)", new Dictionary<string, object> { { "a", 2 }, { "b", 3 }, { "Api", new Utils() } });
                Console.WriteLine(times1 == times2);

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return -1;
            }
        }

        public class Utils   // IPY only access public class/members
        {
            public int Times(int a, int b)
            {
                return a * b;
            }
        }
    }

    /// <summary>
    /// Require IronPython/IronPython.StdLib (2.7.9) Nuget packages
    /// </summary>
    public class IpyEngine
    {
        private static readonly Lazy<ScriptEngine> Engine = new Lazy<ScriptEngine>(Generate, true);

        public static T Execute<T>(string pyCodes, IDictionary<string, object> inArgs = null, IEnumerable<string> moduleDirs = null)
        {
            var engine = Engine.Value;
            var scope = engine.CreateScope();

            if (moduleDirs != null) engine.SetSearchPaths(engine.GetSearchPaths().Union(moduleDirs).ToArray());

            if (inArgs != null) foreach (var kv in inArgs) scope.SetVariable(kv.Key, kv.Value);

            return engine.Execute<T>(pyCodes, scope);
        }

        public static ScriptScope ExecuteFile(string pyFile, IDictionary<string, object> inArgs = null, IEnumerable<string> moduleDirs = null)
        {
            var engine = Engine.Value;
            var scope = engine.CreateScope();

            if (moduleDirs != null) engine.SetSearchPaths(engine.GetSearchPaths().Union(moduleDirs).ToArray());

            if (inArgs != null) foreach (var kv in inArgs) scope.SetVariable(kv.Key, kv.Value);

            return engine.ExecuteFile(pyFile, scope);
        }

        private static ScriptEngine Generate()
        {
            var options = new Dictionary<string, object> { { "Frames", true }, { "FullFrames", true } };
#if DEBUG
            options["Debug"] = true;
#endif
            return Python.CreateEngine(options);
        }
    }
}
