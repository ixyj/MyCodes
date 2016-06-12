namespace Delegate
{
    using System;

    class Program
    {
        public static void Hello(string person)
        {
            Console.WriteLine("Hello " + person);
        }

        public static void 你好(string 某人)
        {
            Console.WriteLine("你好 " + 某人);
        }

        public delegate void Greet(string someone);

        private static void Main(string[] args)
        {
            Greet greet = Hello;
            greet += 你好;

            greet("Jack");
        }
    }
}
