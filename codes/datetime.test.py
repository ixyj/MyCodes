# coding=utf-8

import datetime

# python3

timeStr = '2018-12-20T06:53:20.504'

print(datetime.datetime.strptime(timeStr, "%Y-%m-%dT%H:%M:%S.%f"))
