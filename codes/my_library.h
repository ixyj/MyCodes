#ifndef _MY_LIBRARY_
#define _MY_LIBRARY_
 
#include <cmath>
#include <ctime>
#include <string>
#include <cstring>
#include <cassert>
#include <process.h>
#include <iostream>
#include <vector>
using namespace std;

namespace my_library
{

	unsigned long A(int max, int min)  //排列
	{ 
		assert((min >= 0) && (max >= min));
		unsigned long result=1;
		for (int i= max - min + 1; i<= max; i++)
		{
			result *= i;
		}
		return result;
	}


	unsigned long C(int max, int min)  //组合
	{ 
	return static_cast<unsigned long>(A(max, min) / static_cast<double>(A(min, min)));
	}


	double Integrator(double (*func)(double x), double lowerOfX, double upperOfX, int amount = 500000)
	{                    //积分              lowerOfX,  upperOfX----积分下限、上限  amount----积分区间的被分成的份数   
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
	{                                                                             //求方程的根
		assert(maxX >= minX);
		double start = minX;                                        
		double end = maxX;                                          //根的范围[minX, maxX]
		double key = (start + end) / 2;
		if((*func)(start) * (*func)(end) > 0)
		{
			cerr<<"此方程可能无根！\n";
			return -1;
		}
		while (fabs((*func)(key)) > 1e-6)                         //二分法求解
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

	int CreateProcess(const char* path, char* argv[] = NULL, char* envp[] = NULL, int mode = _P_NOWAIT)  //创建一个新进程
	{                        //path----指定新程序的路径(包括程序自身，仅限exe类型)，argv----新进程的参数，envp----环境变量，
							  //mode----运行模式      成功返回一正整数，否则返回-1
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

	std::vector<std::string> SplitString(const std::string& rawString, const std::string& delimiter, bool enableEmpty = false)
	{
		std::vector<std::string> splitResults;

		size_t begin = 0;
		size_t split = rawString.find_first_of(delimiter, begin);

		for (; split != std::string::npos; begin = split + 1, split = rawString.find_first_of(delimiter, begin))
		{
			if (enableEmpty || split != begin)
			{
				splitResults.push_back(std::move(rawString.substr(begin, split - begin)));
			}
		}

		if (enableEmpty || !rawString.substr(begin, split - begin).empty())
		{
			splitResults.push_back(std::move(rawString.substr(begin, split - begin)));
		}

		return splitResults;
	}
}

	void TrimString(string& rawString, char charToTrim = ' ')
	{
		rawString.erase(0, rawString.find_first_not_of(charToTrim));
		
		rawString.erase(rawString.find_last_not_of(charToTrim) + 1, std::string::npos);
	}


#endif		
