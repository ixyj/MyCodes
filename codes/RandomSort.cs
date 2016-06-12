namespace Utils
{
    using System;
    using System.IO;

    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                var input = File.ReadAllLines(args[0]);
                var output = args[1];

                RandomSort<string>(input);

                File.WriteAllLines(output, input);
            }
            catch
            {
                Console.WriteLine("this.exe input output");
            }
        }

        public static void RandomSort<T>(T[] array)
        {
            var n = array.Length;
            var rnd = new Random(unchecked((int)DateTime.Now.Ticks));

            for (var i = 0; i < n; i++)
            {
                var index = rnd.Next(i, n);
                var tmp = array[i];
                array[i] = array[index];
                array[index] = tmp;
            }
        }
    }
}
