using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace PinYinTool
{
    public class PinYinTool
    {
        public static void Main(string[] args)
        {
        }
            
        private static bool IsMatch<T>(T[] arg1, T[] arg2, Func<T, T, bool> equal)
        {
            var longArg = arg1.Length > arg2.Length ? arg1 : arg2;
            var shortArg = arg1.Length > arg2.Length ? arg2 : arg1;

            var l = longArg.Length - 1;
            var s = shortArg.Length - 1;
            while (l >= 0 && s >= 0)
            {
                if (equal(shortArg[s], longArg[l]))
                {
                    --s;
                }
                --l;
            }

            return s < 0;
        }

        private static bool IsMatch<T>(T[] arg1, T[] arg2) where T : IComparable
        {
            var longArg = arg1.Length > arg2.Length ? arg1 : arg2;
            var shortArg = arg1.Length > arg2.Length ? arg2 : arg1;

            var l = longArg.Length - 1;
            var s = shortArg.Length - 1;
            while (l >= 0 && s >= 0)
            {
                if (shortArg[s].CompareTo(longArg[l]) == 0)
                {
                    --s;
                }
                --l;
            }

            return s < 0;
        }
    }
}