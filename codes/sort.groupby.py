#coding=utf-8

import threading
import time
import random
import urllib
import urllib.request
import traceback
import os
from datetime import *
from itertools import groupby

if __name__ == "__main__":
    lines = ['a\tTrue\t-2','B\tTrue\t-2','A\tFalse\t-1','b\tTrue\t-3','C\tFalse\t-2']

    tokens = [x.split('\t') for x in lines]

    rank = sorted(tokens, key = lambda x: x[0].lower(), reverse=True)
    # must sort before groupby
    group = groupby([x for x in rank if x[2] != "0"], key = lambda x: x[0].lower()) 
    groupArray = [[key, list(items)] for key,items in group]

    ret = [[x[0], any(y[1] == "True" for y in x[1]), max(float(y[2]) for y in x[1])] for x in groupArray]

    print("\n")