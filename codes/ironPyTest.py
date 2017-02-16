# coding=utf-8

import clr
clr.AddReferenceToFileAndPath(r'C:\Windows\Microsoft.NET\Framework\v2.0.50727\System.dll')
clr.AddReferenceToFileAndPath(r'C:\Windows\Microsoft.NET\Framework\v2.0.50727\mscorlib.dll')
clr.AddReference(r'System.Core')
import System
from System import Console
from System.Collections.Generic import List
from System.Linq import Enumerable
clr.ImportExtensions(System.Linq)


if __name__ == '__main__':
    Console.WriteLine("C# Console.WriteLine ...")
    lst = List[int]()
    print(type(lst))
    lst.Add(1)
    lst.Add(2)
    Console.WriteLine(lst.Count)
    Console.WriteLine(lst.Sum())