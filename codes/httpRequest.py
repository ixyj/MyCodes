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
import socket
import urllib.parse
import urllib.request

def httpRequest(url, method='GET', timeout=30, header={"User-Agent":"Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 6.3; WOW64; Trident/7.0)"}, repeat=3, dictForm=None):
    req = urllib.request.Request(url, None if dictForm == None else urllib.parse.urlencode(dictForm).encode('utf-8'), header)
    req.method = method
    req.timeout = timeout

    if method == 'POST' and not req.has_header("content-length"):
        req.add_header("content-length", "0")

    for i in range(0, repeat):
        response = urllib.request.urlopen(req)
        if response.status == 200:
            return response.read().decode('utf-8')

        return None
    
if __name__ == "__main__":
    #ret = httpRequest("http://asdebug/SearchThreshold/FetchData?q=360&mkt=zh-cn&ver=v307.01&tblType=Suggestion&tckt=17&_=1453292396328","POST",1)
    ret = httpRequest("http://www.baidu.com")
    writer = open("test.txt", "w", 1,"utf-8")
    writer.writelines(ret)
    writer.close()
