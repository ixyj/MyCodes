#coding=utf-8

import threading
import time
import re
import random
import urllib
import urllib.request
import traceback  
import sys
import os
import shutil
from os import listdir
from os.path import isdir, join

def getImports(bond):
    pattern = re.compile(r" *import +\".+\" *")
    results = []
    for line in [x for x in open(bond).readlines() if pattern.findall(x)]:
        quotation1 = line.find("\"")
        quotation2 = line.find("\"", quotation1 + 1)
        results.append(line[quotation1 + 1 : quotation2])
    return results


if __name__ == "__main__":
    missBonds = set([x.replace("\n", "").replace(" ", "").lower() for x in open("C:/Users/yajxu/Desktop/missing.bond").readlines()])
    crCached = "D:/ProgramData/CRGlobalCache/"
    outDir = "C:/Users/yajxu/Desktop/bond/"

    crCachedLength = len(crCached) 
    processing = True
    while processing:
        processing = False
        for parent,dirs,files in os.walk(crCached):
            bonds = [os.path.join(parent, x).replace("\\", "/") for x in files if x.lower().endswith(".bond")]
            for bond in bonds:
                bondStr = bond[crCachedLength : bond.lower().find("/bond/")].replace("/", ".") + ".bond"
                if bondStr.lower() in missBonds:
                    processing = True
                    shutil.copyfile(bond, outDir + bondStr)
                    missBonds.remove(bondStr.lower())
                    for new in getImports(bond):
                        if new.lower() not in missBonds:
                            missBonds.add(new.lower())
                    if len(missBonds) == 0:
                        sys.exit()  
                    
    print("Missing bonds:\n" + "\n".join(missBonds))