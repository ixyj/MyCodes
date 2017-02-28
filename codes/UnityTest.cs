namespace Testing
{
    using Microsoft.Practices.Unity;  // Microsoft.Practices.Unity.dll
    using System;

    public interface IGraph
    {
        void Print();
    }

    public class Circle : IGraph
    {
        public void Print()
        {
            Console.WriteLine("This is a Circle!");
        }
    }

    public class Square : IGraph
    {
        public void Print()
        {
            Console.WriteLine("This is a Square!");
        }
    }

    public interface IPage
    {
        IGraph graph { get; set; }
        void Show();
    }

    public class Book : IPage
    {
        [Dependency]
        public IGraph graph { get; set; }
        [Dependency]
        public int area { get; set; }

        // Euqal to [Dependency]
        //[InjectionMethod]
        //public void Initialize(IGraph graph, int area)
        //{
        //    this.graph = graph;
        //    this.area = area;
        //}

        public void Show()
        {
            Console.WriteLine("--------------Book--------------");
            graph.Print();
            Console.WriteLine($"Area: {this.area}");
        }
    }

    public class Paint : IPage
    {
        public IGraph graph { get; set; }
        public string name;

        public Paint(string name, IGraph graph)
        {
            this.name = name;
            this.graph = graph;
        }

        public void Show()
        {
            Console.WriteLine($"--------------Paint {this.name}--------------");
            graph.Print();
        }
    }

    public class UnityTest
    {
        public static void Main(string[] argv)
        {
            var container = new UnityContainer();
            container.RegisterType<IGraph, Circle>();
            var circle = container.Resolve<IGraph>();
            circle.Print();

            container.RegisterType<IGraph, Square>();
            var square = container.Resolve<IGraph>();   // use the latest IGraph instance if not specified
            square.Print();

            container.RegisterType<IGraph, Circle>("SpecifiedA");
            container.RegisterType<IGraph, Square>("SpecifiedB");
            var specified = container.Resolve<IGraph>("SpecifiedA");
            specified.Print();

            var area = 100;
            container.RegisterInstance(area);
            container.RegisterType<IPage, Book>();
            var book = container.Resolve<IPage>();
            book.Show();

            container.RegisterType<IGraph, Circle>("SpecifiedC");
            container.RegisterType<IPage, Paint>
                (new ContainerControlledLifetimeManager(),
                 new InjectionConstructor(
                    "my_paint",
                    new ResolvedParameter<IGraph>("SpecifiedC")));
            var paint = container.Resolve<IPage>();
            paint.Show();
        }
    }
}