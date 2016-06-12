# coding=utf-8


def lambdaTest():
    l = lambda x: x * x
    print(l(2))

    l = lambda x: "Test info: %s" % x
    print(l("Hello"))

if __name__ == "__main__":
    lambdaTest()