# coding=utf-8

import json

class JsonConvert:
    @staticmethod
    def Obj2Dict(obj):
        if isinstance(obj, object):   # Only support class inheriting from object
            clone = dict(obj.__dict__)
            clone['__class__'] = obj.__class__.__name__
            clone['__module__'] = obj.__class__.__module__
            return clone
        else:
            return json.dumps(obj)

    @staticmethod
    def Dict2Obj(dictObj):
        if isinstance(dictObj, dict) and '__class__' in dictObj:
            className = dictObj.pop('__class__')
            moduleName = dictObj.pop('__module__')
            module = __import__(moduleName)
            cls = getattr(module, className)
            return cls(**dictObj)
        else:
            return dictObj

    @staticmethod
    def Dumps(obj, **kwargs): return json.dumps(obj, default=JsonConvert.Obj2Dict, **kwargs)

    @staticmethod
    def Loads(text, **kwargs): return json.loads(text, object_hook=JsonConvert.Dict2Obj, **kwargs)
