#ifndef _MY_LIBRARY_
#define _MY_LIBRARY_
 
#include <cmath>
#include <ctime>
#include <string>
#include <cstring>
#include <cassert>
#include <process.h>
#include <iostream>
using namespace std;

namespace my_library
{

	unsigned long A(int max, int min)  //����
	{ 
		assert((min >= 0) && (max >= min));
		unsigned long result=1;
		for (int i= max - min + 1; i<= max; i++)
		{
			result *= i;
		}
		return result;
	}


	unsigned long C(int max, int min)  //���
	{ 
	return static_cast<unsigned long>(A(max, min) / static_cast<double>(A(min, min)));
	}


	double Integrator(double (*func)(double x), double lowerOfX, double upperOfX, int amount = 500000)
	{                    //����              lowerOfX,  upperOfX----�������ޡ�����  amount----��������ı��ֳɵķ���   
		double result = 0;
		double avg = (upperOfX - lowerOfX) / amount;            
		double valueOfX = lowerOfX;
		for(int i=0; i < amount; i++)
		{
			result += (*func)(valueOfX) * avg;
			valueOfX += avg;
		}
		return result;
	}


	double CalcKeyOfEquation(double (*func)(double x), double minX, double maxX)
	{                                                                             //�󷽳̵ĸ�
		assert(maxX >= minX);
		double start = minX;                                        
		double end = maxX;                                          //���ķ�Χ[minX, maxX]
		double key = (start + end) / 2;
		if((*func)(start) * (*func)(end) > 0)
		{
			cerr<<"�˷��̿����޸���\n";
			return -1;
		}
		while (fabs((*func)(key)) > 1e-6)                         //���ַ����
		{
			if((*func)(start) * (*func)(key) <= 0)
			{
				end = key;
			}
			else
			{
				start = key;
			}
			key = (start + end) / 2;
		}
		return key;
	}

	template <class type>
	void Sort(type array[], int num)    //����
	{                            //array----����������飬num----�����Ԫ��
		assert(num > 0);
		type tmp;
		for (int i = 0; i < num - 1; i++)
		{
			for (int j = 0; j < num - i - 1; j++)
			{
				if (array[j] > array[j + 1])
				{
					tmp = array[j];
					array[j] = array[j+1];
					array[j+1] = tmp;
				}
			}
		}
	}


	template <class type>
	int HalfSeek(type array[], int num, type key) //�۰����   ���ҳɹ��������������е�λ�ã����򷵻�-1
	{                 //array----�����ҵ�����	num----������Ԫ����Ŀ	 key----�����ҵ�Ԫ��
		assert( num > 0);
		int first = 0;
		int last = num - 1;
		int mid = (first + last) / 2;
		int count = 0;                        //���Ҵ���
		if (key == array[num - 1])
		{
			return num;
		}
		while (first <= last)
		{
			if (key < array[mid])
			{
				last = mid;
			}
			else if (key > array[mid])
			{
				first = mid;
			}
			else
			{
				return mid+1;
			}
			mid = (first + last) / 2;
			count++;
			if (count > num)
			{
				break;
			}
		}
		return -1;
	}


	int CreateProcess(char* path, char* argv[] = NULL, char* envp[] = NULL, int mode = _P_NOWAIT)  //����һ���½���
	{                        //path----ָ���³����·��(����������������exe����)��envp----����������
							  //mode----����ģʽ      �ɹ�����һ�����������򷵻�-1
		assert(path != NULL);	
		bool flag = false;
		if (argv == NULL)
		{
			argv = new char*[2];
			argv[0] = new char[2];
			strcpy(argv[0]," ");
			argv[1] = NULL;
			flag = true;
		}

		int result = static_cast<int>(spawnvpe( mode, path, argv, envp));
		if (flag)
		{
			delete[] argv;
		}
		return result;
	}
		

	char* GetTime(char* getTime, const char* formatString = "%c", size_t Size0fTime = 4)
	{
		assert(getTime != NULL);
		time_t t = time(NULL);

		assert(strftime( getTime, Size0fTime, formatString, localtime(&t) ) != 0); 
	
		return getTime;
	}


}



#endif		