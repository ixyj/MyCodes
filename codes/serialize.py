# coding=utf-8

import json,pickle,base64

if __name__ == '__main__':
    # json only supports built-in type
    dic = {"1":1, "2":"222"}
    text = json.dumps(dic)
    print(type(text))
    print(text)
    dic2 = json.loads(text)
    print(type(dic2))
    print(dic2)

    print('--------pickle------')
    # pickle
    # file = open('data.pkl', 'wb')
    # pickle.dump(obj, file, [,protocol])
    byteArray = pickle.dumps(dic2)
    print(type(byteArray))
    print(byteArray)
    text = base64.b64encode(byteArray)
    print(type(text))
    print(text)
    byteArray2 = base64.b64decode(text)
    dic3 = pickle.loads(byteArray2)
    print(type(dic3))
    print(dic3)