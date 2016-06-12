# coding=utf-8

import xml.etree.ElementTree
import xml.dom.minidom 
import io
import os
import datetime
import sys
import math
import functools
import re


def Test(path):
    for parent,dirs,files in os.walk(path):
        fs = [os.path.join(parent,x) for x in files]
        for f in fs:
            dat = os.stat(f).st_ctime
            day= datetime.datetime.fromtimestamp(dat)  
            if day.year != 2016:
                os.remove(f)

def WriteFile(results, file):
    writer = open(file, "w", 1,"utf-8")

    for result in results:
        writer.write("\t".join(result))
        writer.write("\n")
        writer.flush()

if __name__ == "__main__":
     Test("D:\\BingKnows\\Monitor\\QA\\")