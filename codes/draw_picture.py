# coding=utf-8

import matplotlib.pyplot as plt
import matplotlib.ticker as ticker
import matplotlib.dates as mdates 
import numpy as np

list1=[1,2,3,4,5,6,2,3,4,6,7,5,7]
list2=[2,3,4,5,8,9,2,1,3,4,5,2,4]
list3=[9,9,8,7,6,5,4,3,2,1,1,2,3]
plt.rcParams['font.sans-serif']=['SimHei'] #用来正常显示中文标签
plt.title('显示中文标题')
plt.xlabel("横坐标")
plt.ylabel("纵坐标")
x=np.arange(0,len(list1))+1
x[0]=1
my_x_ticks = np.arange(1, 14, 1)
plt.xticks(my_x_ticks)
plt.plot(x,list1,label='list1')#添加label设置图例名称
plt.plot(x,list2,label='list2')#添加label设置图例名称
plt.plot(x,list3,label='list3')#添加label设置图例名称
plt.legend()
plt.grid()#添加网格

plt.gca().xaxis.set_major_locator(ticker.MultipleLocator(5)#x轴每隔5显示一个刻度
plt.gca().xaxis.set_major_formatter(mdates.DateFormatter('%Y-%m-%d'))#x轴显示时间
plt.gcf().autofmt_xdate()#x轴旋转显示时间

plt.savefig('foo.png')  # must be called before plt.show(), or else generate a blank picture

#plt.show()

plt.close()
