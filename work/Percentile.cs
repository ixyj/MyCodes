namespace Percentile
{
    using System;
    using System.Linq;

    class Helper<T>
    {
        public static dynamic Percentile(T[] array, double percentile)
        {
            Array.Sort(array);
            var n = (array.Length - 1) * percentile + 1;
            // Another method: n = (array.Length + 1) * percentile;
            if (n == 1d) return array.First();
            else if (n == array.Length) return array.Last();

            var k = (int)n;
            return array[k - 1] + (n - k) * ((dynamic)array[k] - array[k - 1]);
        }
    }
}
