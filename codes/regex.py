# coding=utf8

import re

def urlBuild(url, host):
    pattern = re.compile(r"atlahostname=[^&]+")

    match = pattern.findall(url)
    if match:
        print("matched: ".join(match))

    replace = "atlahostname=" + host
    print(pattern.subn(replace, url))
    url = pattern.sub(replace, url)
    if url.find("atlahostname=") < 0:
        url += "&atlahostname=" + host
    print(url)


if __name__ == "__main__":
    urlBuild("http://www.bing-exp.com?TrafficType=Internal_Monitor&atlahostname=CO3SCH010172530&vi=web-kirinprod", "yajxu")
    urlBuild("http://www.bing-exp.com?TrafficType=Internal_Monitor&vi=web-kirinprod", "yajxu")
