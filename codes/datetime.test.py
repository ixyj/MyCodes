# coding=utf-8

import datetime
from datetime import timedelta

# python3

timeStr = '2018-12-20T06:53:20.504'
ParsedDateTime = datetime.datetime.strptime(timeStr, '%Y-%m-%dT%H:%M:%S.%f')
print(ParsedDateTime)
print('{0:%Y-%m-%d %H:%M:%S}'.format(ParsedDateTime))

updated=ParsedDateTime + timedelta(hours=12)
print('{0:%Y-%m-%d %H:%M:%S}'.format(updated))

diff = datetime.datetime.strptime('20181220T065430.505123', '%Y%m%dT%H%M%S.%f') - ParsedDateTime
print(diff)
print(diff.total_seconds())
print(diff.days)
print(diff.seconds)
print(diff.microseconds)   #   0.000001s 
