# coding=utf-8

import json
import random
import urllib.parse
import urllib.request

def HttpPost(url, body, timeout=30, repeat=3):
    try:
        req = urllib.request.Request(url, None, {'Content-Type':'application/json'})
        req.method = 'POST'
        req.timeout = timeout

        if not req.has_header("content-length"):
            req.add_header("content-length", "0")

        for i in range(0, repeat):
            response = urllib.request.urlopen(req, body.encode('utf-8'))
            if response.status == 200:
                return response.read().decode('utf-8')
        return None
    except Exception as e:
        print(e)
        return None

def RunTest(queryFile, bodyFile, outputFile):
    url = 'http://int-xiaoicecore-duplex.chinacloudapp.cn/api/chitchat/reply?workflow=Duplex'
    body = json.loads(''.join(open(bodyFile, 'r', 1,'utf-8').readlines()))
    queries = open(queryFile, 'r', 1, 'utf-8').readlines()
    writer = open(outputFile, 'w' , 1, 'utf-8')
    count = 0
    for query in queries:
        if count % 10 == 0:
            print('Sent %d requests!' % count)

        body['Content']['Text'] = query.replace('\n', '')

        response = HttpPost(url, json.dumps(body))
        res = json.loads(response)[0]['Content']['Text']
        writer.write('%s\t%s\n' % (body['Content']['Text'], res))

        if res.startswith('好多设备，选择一个吧！'):
            devices = res[len('好多设备，选择一个吧！')::].split('，')
            dev = random.choice(devices)
            body['Content']['Text'] = dev
            response = HttpPost(url, json.dumps(body))
            res = json.loads(response)[0]['Content']['Text']
            writer.write('%s\t%s\n' % (body['Content']['Text'], res))
        writer.flush()

    writer.close()
    print('Finished!")



if __name__ == "__main__":
    try:
        RunTest('D:/WorkSpace/pyTest/testQueries.txt', 'D:/WorkSpace/pyTest/IotBody.txt', 'D:/WorkSpace/pyTest/IotOutput.txt')
    except Exception as e:
        print(traceback.format_exc())
        print(e)