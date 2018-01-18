# coding=utf-8

import json
import re

import clr
clr.AddReference(r'System.Core')
clr.AddReference(r'mscorlib')
clr.AddReference(r'Newtonsoft.Json')
import System
from System.Reflection import BindingFlags
from Newtonsoft.Json import JsonConvert

# node=z/a[1]/d ==>jsonData['z']['a'][1]['d']
def getJsonNode(jsonData, node):
    try:
        js = json.loads(jsonData, encoding='utf-8')
        for n in re.split(r"/|\[", node):
            if n != "" and js is not None:
                if n[-1] == "]":
                    n = int(n[0:-1])
                js = js[n]
    except:
        js = None

    return js

mids = None
#def JsonDumps(obj):
#    mids = {}
#    # return JsonConvert.SerializeObject(o)
#    return json.dumps(obj, default=lambda o: Utility.ToDict(o), ensure_ascii=False, separators=(',', ':'))

def ToDict(obj):
    if hasattr(obj, '__dict__'):
        return obj.__dict__
    maps = {}
    for p in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance):
        m = p.GetGetMethod()
        if m != None and m.IsPublic and m.GetParameters().Count == 0:
            o = m.Invoke(obj, None)
            mid = id(o)
            if mid not in mids:
                mids[mid] = o
                maps[p.Name] = o
    return maps

def Obj2Dict(obj):
    if isinstance(obj, MyClass):
        clone = dict(obj.__dict__)
        clone['__class__'] = obj.__class__.__name__
        clone['__module__'] = obj.__class__.__module__
        return clone
    else:
        return json.dumps(obj)

def Dict2Obj(dictObj):
    if isinstance(dictObj, dict) and '__class__' in dictObj:
        className = dictObj.pop('__class__')  
        moduleName = dictObj.pop('__module__')  
        module = __import__(moduleName)  
        cls = getattr(module, className)
        return cls(**dictObj)
    else:  
        return dictObj

if __name__ == "__main__":
    #json.dumps(obj, default=Obj2Dict)
    #json.loads(js, object_hook=Dict2Obj)
    print(getJsonNode('{"z":{"b":1,"a":[{"c":2},{"d":[6,7]}]}}', 'z'))
    print(getJsonNode('{"z":{"b":1,"a":[{"c":2},{"d":[6,7]}]}}', 'z/b'))
    print(getJsonNode('{"z":{"b":1,"a":[{"c":2},{"d":[6,7]}]}}', 'z/a[0]/'))
    print(getJsonNode('{"z":{"b":1,"a":[{"c":2},{"d":[6,7]}]}}', 'z/a[1]/d'))
    print(getJsonNode('{"z":{"b":1,"a":[{"c":2},{"d":[6,7]}]}}', 'xxx'))
