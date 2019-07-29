# coding=utf-8
import sys
import json
import urllib
import urllib2
import traceback
import ssl

def httpRequest(url, method='GET', timeout=30, header={"User-Agent":"Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 6.3; WOW64; Trident/7.0)"}, repeat=3, dictForm=None):
    req = urllib2.Request(url, None if dictForm == None else urllib.urlencode(dictForm).encode('utf-8'), header)
    req.method = method
    req.timeout = timeout

    if method == 'POST' and not req.has_header("content-length"):
        req.add_header("content-length", "0")

    for i in range(0, repeat):
        try:
            response = urllib2.urlopen(req, context=ssl._create_unverified_context())
            return response.getcode(), response.read().decode('utf-8')
        except urllib2.HTTPError as err:
            return err.code, err.msg

    return 404, None
  
def SetOutlet(on):
    header = { 'App-Id':'2882303761517406057',\
        'Access-Token':'d~7_11671947_1564365904239534866~dbs2zsw33x', 'Spec-NS': 'miot-spec-v2', 'Content-Type': 'application/json'}
    body = { "properties": [{ "pid": "M1GAxtaW9A0LXNwZWMtdjIVhIAFGAtjaHVhbmdtaS12MRUUGAgxMTUxNTkxNhUcAA.2.1", "value": on  }]   }

    return httpRequest(url='https://api.home.mi.com/api/v1/properties', method='PUT', header=header, dictForm=body)

try:
    ret = SetOutlet(False)
    print(ret)
except:
    print(traceback.format_exc())
print()
