# coding=utf-8

import math
import functools


def dataProcessor():
    data = list(range(1, 10))
    print(data)
	
	#sort
	rank = sorted([x for x in data if x > 1 ], key= (lambda x: float(x)), reverse=True)

    #filter(function, sequence)：对sequence中的item依次执行function(item)，输出执行结果为True的item
    l = lambda x: math.pow(math.sqrt(x), 2) == x
    result = filter(l, data)
    print(list(result))

    #map(function, sequence) ：对sequence中的item依次执行function(item)，输出执行结果
    result = map(l, data)
    print(list(result))
    print([math.pow(math.sqrt(x), 2) == x for x in data])

    print(list(map(lambda x, y, z: x + y * 2 - z, data, data, data)))

    #reduce(function, sequence, starting_value)：对sequence中的item顺序迭代调用function，如果有starting_value，还可以作为初始值调用
    print(functools.reduce(lambda x,y: x + y / 2, data, 0))
    print(functools.reduce(lambda x,y: x + y / 2, data))

if __name__ == "__main__":
        dataProcessor()
