#coding=utf-8
# python3
import re
import json
import codecs
import random
import sys
import traceback
import time
from datetime import datetime
import wave
import random

BytesPerMs = 32

def Splice(wavs, outfile, silentMs=[1000, 5000]):
    data, timestamps, offset = [], [], 0
    for wav in wavs:
        w = wave.open(wav, 'rb')
        data.append([w.getparams(), w.readframes(w.getnframes())])
        w.close()
        offset += len(data[-1][1])
        timestamps.append(offset)

        data.append([0, b'\x00' * BytesPerMs * random.randint(silentMs[0], silentMs[1])]) # silience
        offset += len(data[-1][1])
        timestamps.append(offset)

    output = wave.open(outfile, 'wb')
    output.setparams(data[0][0])
    for w in data:
        output.writeframes(w[1])
    output.close()
    tt = [t/BytesPerMs for t in timestamps]
    print(tt)

try:
    Splice([r"\\stcsrv-e31\Share\Speech.Evaluation\Elder Wav Test cases\wav_far\006S01_F_001.wav",\
        r"\\stcsrv-e31\Share\Speech.Evaluation\Elder Wav Test cases\wav_far\006S01_F_002.wav",\
        r"\\stcsrv-e31\Share\Speech.Evaluation\Elder Wav Test cases\wav_far\006S01_F_003.wav"],\
        r"C:\Users\yajxu\Desktop\测试录音2.wav")
except:
    print(traceback.format_exc())
    input('Press to continue ...')