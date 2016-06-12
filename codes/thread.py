#coding=utf-8

import threading
import time
import random

# join(timeout)  在join()位置等待另一线程结束后再继续运行join()后的操作,timeout是可选项，表示最大等待时间
# setDaemon(bool) True:当父线程结束时，子线程立即结束；False:父线程等待子线程结束后才结束。默认为False
# isDaemon()	判断子线程是否和父线程一起结束，即setDaemon()设置的值
# isAlive() 判断线程是否在运行
####### Methods of threading.Condition######
# acquire()/release()：获得/释放 Lock
# wait([timeout]):线程挂起，直到收到一个notify通知或者超时（可选的，浮点数，单位是秒s）才会被唤醒继续运行。wait()必须在已获得Lock前提下才能调用，否则会触发RuntimeError。调用wait()会释放Lock，直至该线程被Notify()、NotifyAll()或者超时线程又重新获得Lock.
# notify(n=1):通知其他线程，那些挂起的线程接到这个通知之后会开始运行，默认是通知一个正等待该condition的线程,最多则唤醒n个等待的线程。notify()必须在已获得Lock前提下才能调用，否则会触发RuntimeError。notify()不会主动释放Lock。
# notifyAll(): 如果wait状态线程比较多，notifyAll的作用就是通知所有线程（这个一般用得少）

class myTask:
    source = 0
    con = threading.Condition()

    def __init__(self, src):
        self.source = src

    def task(self, tid):
        print("Task %d is executing" % tid)
        while self.source > 0:
            if self.con.acquire():
                if self.source > 0:
                    print("Task %d consumes resource: %d" % (tid, self.source))
                    self.source -= 1
                self.con.notify()
                self.con.release()
                time.sleep(random.randint(0, 10))
            else:
                print("Task %d is waiting" % tid)
                self.con.wait()
        print("Task %d finished" % tid)

    def run(self):
        threads = []
        for i in range(0, 4):
            t = threading.Thread(target=self.task, args=(i,))
            threads.append(t)

        start = time.time()

        for t in threads:
            t.setDaemon(False)
            t.start()

        while self.source > 0:
            if self.con.acquire():
                self.con.notify()
                self.con.release()
                time.sleep(random.randint(0, 10))
            else:
                self.con.wait()

        end = time.time()
        total = end - start
        print(u"总时间: {0:.5f}秒".format(total))

if __name__ == "__main__":
    th = myTask(10)
    th.run()
