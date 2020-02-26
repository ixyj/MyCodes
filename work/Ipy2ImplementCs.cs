/**
 * Nuget Package IronPython & DynamicLanguageRuntime required 
**/
namespace Ipy2ImplementCs
{
    using IronPython.Hosting;
    using System;
    using System.Reflection;

    public interface ICsInterface
    {
        int GetLength(string text);
        int Length { get; set; }
    }

    public class CsClass : ICsInterface
    {
        public virtual int Length { get => 0; set => throw new NotImplementedException(); }
        public virtual int GetLength(string text)
        {
            Console.WriteLine($"csClass: {text}");
            return 0;
        }
    }

    public class Program
    {
        static void Main(string[] args)
        {
            var engine = Python.CreateEngine();
            engine.Runtime.LoadAssembly(Assembly.GetExecutingAssembly());

            var scope = engine.CreateScope();
            var source = engine.CreateScriptSourceFromFile("pyClass.py");

            source.Execute(scope);

            var pyClass = scope.GetVariable("PyClass");
            var pyInstance = pyClass();
            ICsInterface csInterface = pyInstance;
            Console.WriteLine(csInterface.GetLength("Test GetLength"));
            Test(csInterface);

            var pyClass2 = scope.GetVariable("PyClass2");
            var pyInstance2 = pyClass2();
            CsClass csClass2 = pyInstance2;
            Console.WriteLine(csClass2.GetLength("Test GetLength"));
            Console.WriteLine(csClass2.Length);
            Test(csClass2);
        }

        static void Test(ICsInterface it)
        {
            Console.WriteLine(it.GetLength("Test..."));
            Console.WriteLine(it.Length);
            it.Length = 100;
            Console.WriteLine(it.Length);
        }
    }
}

/**** file pyClass content ****
import Ipy2ImplementCs

class PyClass(Ipy2ImplementCs.ICsInterface):
def __init__(self):
    self.length = 0

def GetLength(self, text):
    return len(text) if text else 0

@property
def Length(self):
    return self.length

@Length.setter
def Length(self, value):
    self.length = value

class PyClass2(Ipy2ImplementCs.CsClass):
def __init__(self): pass

def GetLength(self, text):
    Ipy2ImplementCs.CsClass.GetLength(self, text)
    print('PyClass2: ' + text)
    return -1

@property
def Length(self):
    #print(Ipy2ImplementCs.CsClass.Length)  # I don't know how to call super.property
    return -100

@Length.setter
def Length(self, value):
    print('set value to PyClass2: %r') % value
*/
