# coding=utf-8
# python3

import sys
import traceback

def Calc(base, years, rate):
    result = 0
    for i in range(int(years)):
        result = (result + base) * (1 + rate / 100.0)
    return result

def ParseCmd(args):
    parameters = [p.split('=') for p in args if '=' in p]
    return dict((p[0], float(p[1])) for p in parameters)
try:                         
    parm = ParseCmd(sys.argv)
    result = Calc(parm['b'], parm['y'], parm['r'])
    print(f'Total income={result:9.3f}')
except:
    print(traceback.format_exc())
    print('Example: this.py b=100 y=5, r=3.25\n\t b=baseDeposit/year; y=years; r=rate(%)')

