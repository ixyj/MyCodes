# coding=utf-8

from collections import deque

def struct():
    array = [1,2,3,4,5,6,7,8,9,0]
    print(array[1:6])
    print(array[1:6:2])
    array = "b a c".split(' ')
    array.sort()
    print(array)
    print(array[-1])
    print(len(array))
# AS List
    print("List:")
    array.append('e')
    array.insert(0, '1')
    print(array)
    del array[3] # =array.remove('c')
    print(array)
    print('c' in array)
    print(0 == array.count('0'))

# AS Stack
    print("Stack:")
    array.append("2")
    print(array)
    array.pop()
    print(array)

# Queue
    print("Queue:")
    queue = deque(array)
    queue.appendleft("3")
    print(queue)
    print('c' in queue)
    queue.popleft()
    print(queue)

# Tuple
    print("Tuple:")
    tup = tuple(array)
    print(tup)

# Set
    print("Set:")
    st = set(array)
    print(st)
    print('c' in st)
    print(st.intersection({'0', 'a'}))

# dictionary
    print("Dictionary:")
    dict = {1:"1",2:"2"}
    print(dict)
    del dict[2] #=dict.pop(2)
    dict[1]='0'
    dict[3]='3'
    print(1 in dict)
    print(dict[1])
    print(dict.values())

# Range
    print("Range:")
    print(list(range(1,6)))
    print(list(range(1,6,2)))
    for i in range(1,6):
        print(i)

if __name__ == "__main__":
    struct()