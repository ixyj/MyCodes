#include <pthread.h>

#include <iostream>
#include <string>
#include <cassert>
#include <cstdlib>
using namespace std;



const int MAX = 8;


struct Resource
{
	int num;
	pthread_mutex_t pLock;//互斥体lock，用于对缓冲区的互斥操作
	pthread_cond_t pEmpty;//缓冲区空的条件变量
	pthread_cond_t pFull;//缓冲区未满的条件变量

	Resource(int n = 0)	: num(n)
	{
		assert(num >= 0 && num <= MAX);

		pthread_mutex_init(&pLock, NULL);
		pthread_cond_init(&pEmpty, NULL);
		pthread_cond_init(&pFull, NULL);
	}

	void Get(int n)//将n个产品取出
	{		
		pthread_mutex_lock(&pLock);

		while (num < n && num >= 0)
		{
			cout << "Get (" << n << ") wait!\n";
			pthread_cond_wait(&pEmpty, &pLock);//设置空pEmpty＝1，释放pLock，阻塞
		}

		num -= n;
		cout << "Get\t\tnum:" << num+n << "->" << num << "\n";

		pthread_cond_signal(&pFull);	//激活因pFull＝1阻塞的进程
		pthread_mutex_unlock(&pLock);	//释放pLock
	}

	void Put(int n)//将n个产品放入
	{
		pthread_mutex_lock(&pLock);

		while (num + n > MAX)
		{
			cout << "Put (" << n << ") wait!\n";
			pthread_cond_wait(&pFull, &pLock);
		}

		num += n;
		cout << "Put\t\tnum:" << num-n << "->" << num << "\n";

		pthread_cond_signal(&pEmpty);
		pthread_mutex_unlock(&pLock);
	}
};





//produce和consume必须是全局函数void* Fun(void*)形式
void* produce(void* data)
{
	Resource* pRes = (Resource*)data;
	for (int i = 0; i < 40; i++)
	{
		pRes->Put(rand()%3+1);
		sleep(rand()%100/1000);
	}
	
	pRes->Put(-MAX - 1);
	return NULL;
}


void* consume(void* data)
{
	Resource* pRes = (Resource*)data;
	while (pRes->num >= 0)
	{
		pRes->Get(rand()%3+1);
		sleep(rand()%100/1000);
	}
	
	return NULL;
}


int main(int argc, char** argv)
{
	Resource res;

	pthread_t th[2];
	/*创建生产者和消费者线程*/
	pthread_create(th, NULL, produce, &res);
	pthread_create(th+1, NULL, consume, &res);

	/*等待线程结束*/
	void* status;
	pthread_join(th[0],&status);
	pthread_join(th[1],&status);

	return 0;
}


/*
线程的创建和终止
int pthread_create(pthread_t * pthread,const pthread_attr_t *attr,void *(*start_routine(*void)),void *arg);调用此函数可以创建一个新的线程，新线程创建后执行start_routine 指定的程序。其中参数attr是用户希望创建线程的属性，当为NULL时表示以默认的属性创建线程。arg是向start_routine 传递的参数。当成功创建一个新的线程时，系统会自动为新线程分配一个线程ID号，并通过pthread 返回给调用者。
void pthread_exit(void *value_ptr);调用该函数可以退出线程，参数value_ptr是一个指向返回状态值的指针。
线程控制函数
pthread_self(void);为了区分线程，在线程创建时系统为其分配一个唯一的ID号，由pthread_create()返回给调用者，也可以通过pthread_self()获取自己的线程ID。
Int pthread_join (pthread- t thread , void * *status);这个函数的作用是等待一个线程的结束。调用pthread_join()的线程将被挂起直到线程ID为参数thread 指定的线程终止。
int pthread_detach(pthread_t pthread);参数pthread代表的线程一旦终止，立即释放调该线程占有的所有资源。
线程间的互斥
互斥量和临界区类似，只有拥有互斥量的线程才具有访问资源的权限，由于互斥对象只有一个，这就决定了任何情况下共享资源（代码或变量）都不会被多个线程同时访问。使用互斥不仅能够在同一应用程序的不同线程中实现资源的安全共享，而且可以在不同应用程序的线程之间实现对资源的安全共享。Linux中通过pthread_mutex_t来定义互斥体机制完成互斥操作。具体的操作函数如下
pthread_mutex_init(pthread_mutex_t *mutex,const pthread_mutexattr_t *attr);初使化一个互斥体变量mutex，参数attr表示按照attr属性创建互斥体变量mutex，如果参数attr为NULL，则以默认的方式创建。
pthread_mutex_lock(pthread_mutex_t *mutex);给一个互斥体变量上锁，如果mutex指定的互斥体已经被锁住，则调用线程将被阻塞直到拥有mutex的线程对mutex解锁为止。
pthread_mutex_unlock(pthread_mutex_t *mutex);对参数mutex指定的互斥体变量解锁。
线程间的同步
同步就是线程等待某一个事件的发生，当等待的事件发生时，被等待的线程和事件一起继续执行。如果等待的事件未到达则挂起。在linux操作系统中是通过条件变量来实现同步的。
pthread_cond_init(pthread_cond_t *cond,const pthread_cond_t *attr);这个函数按参数attr指定的属性初使化一个条件变量cond。
pthread_cond_wait(pthread_cond_t *cond,pthread_mutex_t *mutex);等待一个事件（条件变量）的发生，发出调用的线程自动阻塞，直到相应的条件变量被置1。等待状态的线程不占用CPU时间。
pthread_cond_timedwait(pthread_cond_t *cond, pthread_mutex_t *mutex, const struct timespec *abstime);
pthread_cond_signal(pthread_cond_t *cond);解除一个等待参数cond指定的条件变量的线程的阻塞状态。
pthread_cond_destroy(pthread_cond_t *cond);
 
g++ *.c -o obj -lpthread
*/
