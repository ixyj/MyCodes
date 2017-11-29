
import traceback
import time
import clr

clr.AddReference(r'System.Core')
clr.AddReference(r'mscorlib')

import System
from System import Linq, Func
from System.Collections.Generic import IEnumerable
from System.Threading.Tasks import Task
clr.ImportExtensions(System.Linq)

def CatchException(task):
    try:
        print('Step into Catch Exception...')
        if task.Exception:
            print('Catched Exception: %s' % task.Exception.ToString())
            return None
    except:
        print(traceback.format_exc())
    return task

def GetDevices(input):
    print(input)
    raise Exception('throw exception!')
    #return len(input)

def Run(request):
    try:
        return Task.Run[int](Func[int](lambda: GetDevices(request)))\
            .ContinueWith[int](Func[Task[int], int](lambda task: CatchException(task)))
    except:
        print(traceback.format_exc())
        return None

if __name__ == '__main__':
    try:
        Run('hello')
        print()
    except:
        print(traceback.format_exc())