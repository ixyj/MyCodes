# coding=utf-8

import datetime

# python3

timeStr = '2018-12-20T06:53:20.504'
ParsedDateTime = datetime.datetime.strptime(timeStr, '%Y-%m-%dT%H:%M:%S.%f')
print(ParsedDateTime)


diff = datetime.datetime.strptime('20181220T065430.505123', '%Y%m%dT%H%M%S.%f') - ParsedDateTime
print(diff)
print(diff.total_seconds())
print(diff.days)
print(diff.seconds)
print(diff.microseconds)   #   0.000001s 
