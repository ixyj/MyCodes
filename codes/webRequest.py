# coding=utf-8

import urllib.parse
import urllib.request

def webRequest(url):
    headers = {"Content-Type":"text/html; charset=UTF-8", "User-Agent":"Mozilla/5.0 (Windows NT 6.3; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/43.0.2357.134 Safari/537.36"}
    headers["Cookie"] = "user info data"
    req = urllib.request.Request(url, headers=headers, method="GET")
    return urllib.request.urlopen(req).read().decode("utf-8")

if __name__ == "__main__":
    page = webRequest("http://www.bing.com/search?%s&mkt=zh-cn"%(urllib.parse.urlencode({"q":"+++"})))
    print(page)