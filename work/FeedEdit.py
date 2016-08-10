#coding=utf-8

import threading
import time
import random
import urllib
import urllib.request
import traceback  
import sys
import os
from os import listdir
from os.path import isdir, join

if __name__ == "__main__":
    finalList1 = [x.replace('\r', '').replace('\n', '') for x in open("C:/Users/yajxu/Desktop/list.txt", "r", 1, "utf-8").readlines()]
    finalList = ["." + x[x.find(':') + 1:].lower() + "." for x in finalList1]
    base_path = 'D:/Branches/Searchgold/deploy/builds/data/latest/Satori/AnswerProd/zhCN/'
    fo = open('Feeds.txt', 'w')

    for parent,dirs,files in os.walk(base_path):
        feeds = [os.path.join(parent,x) for x in files if x.lower() == "feed.ini"]
        for feed in feeds:
            file = open(feed, "r", 1, "utf-8")
            try:
                text = file.readlines()
                file.close()

                if all(x.find("mobile2cortana") == -1 for x in text):
                    continue

                if any(feed.lower().replace('\\', '.').replace('/', '.').find(x) != -1 for x in finalList):
                    print(feed)
                    continue

                fo.write(feed + '\n')

                os.system("sd edit " + feed)
                file = open(feed, "w", 1, "utf-8")
                file.writelines([x.replace("mobile2cortana", "ae2cortana") for x in text])
            except Exception as ex:
                print(ex)
            finally:
                file.close()

    fo.close()