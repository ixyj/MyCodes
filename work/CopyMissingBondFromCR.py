#coding=utf-8

import threading
import time
import random
import urllib
import urllib.request
import traceback  
import sys
import os
import shutil
from os import listdir
from os.path import isdir, join

if __name__ == "__main__":
    missBonds = set([x.replace("\n", "").replace(" ", "").lower() for x in open("C:/Users/yajxu/Desktop/missing.bond").readlines()])
    crCached = "D:/ProgramData/CRGlobalCache/"
    outDir =  "C:/Users/yajxu/Desktop/bond/"

    crCachedLength = len(crCached)
    for parent,dirs,files in os.walk(crCached):
        bonds = [os.path.join(parent,x).replace("\\", "/") for x in files if x.lower().endswith(".bond")]
        for bond in bonds:
            bondStr = bond[crCachedLength : bond.lower().find("/bond/")].replace("/", ".") + ".bond"
            if bondStr.lower() in missBonds:
                shutil.copyfile(bond, outDir + bondStr)
                missBonds.remove(bondStr.lower())
                if len(missBonds) == 0:
                    sys.exit()

                    
    print("Missing bonds:\n" + "\n".join(missBonds))
