# coding=utf-8

import json
import re

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

if __name__ == "__main__":
    print(getJsonNode('{"z":{"b":1,"a":[{"c":2},{"d":[6,7]}]}}', 'z'))
    print(getJsonNode('{"z":{"b":1,"a":[{"c":2},{"d":[6,7]}]}}', 'z/b'))
    print(getJsonNode('{"z":{"b":1,"a":[{"c":2},{"d":[6,7]}]}}', 'z/a[0]/'))
    print(getJsonNode('{"z":{"b":1,"a":[{"c":2},{"d":[6,7]}]}}', 'z/a[1]/d'))
    print(getJsonNode('{"z":{"b":1,"a":[{"c":2},{"d":[6,7]}]}}', 'xxx'))