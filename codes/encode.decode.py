# coding=utf-8
import sys


if __name__ == "__main__":
    str = "练习python"
    print(str.encode("utf-8"))
    print(len(str.encode("utf-8")))

    codes = b"\xe7\xbb\x83\xe4\xb9\xa0python"
    print(codes.decode("utf-8"))
    print(codes.decode("gbk"))
