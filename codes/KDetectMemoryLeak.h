/////////////////////////////////////////////////////////
// usage:
// 1. test a method: call TestMemoryLeak as it returns
// 2. test a program : call Exit as it runs
/////////////////////////////////////////////////////////

#pragma once

#ifdef _DEBUG
#define DEBUG_CLIENTBLOCK new(_NORMAL_BLOCK, __FILE__, __LINE__)
#define _CRTDBG_MAP_ALLOC
#define new DEBUG_CLIENTBLOCK
#else
#define DEBUG_CLIENTBLOCK
#endif

#include <crtdbg.h>

#include <iostream>
#include <fstream>
#include <cstdlib>

// Reset the output
int ReportFunction(int /*nReportType*/, char* szMsg, int* /*pRetVal*/)
{
	std::cout << szMsg << "\n";

	std::fstream fout("memoryLeakLog.txt", std::ios::app);
	if (!fout.fail())
	{
		fout << szMsg << "\n";
		fout.close();
	}

	return 0;
}

// Test memory leak when a method exits
// it will output all memory leak from beginning to now
void TestMemoryLeak(const _CrtMemState * beginState = NULL)
{
	_CrtSetReportHook(ReportFunction);

	// only dump leaks when there are in fact leaks
	_CrtMemState msNow;
	_CrtMemCheckpoint(&msNow);

	if (msNow.lCounts[_CLIENT_BLOCK] != 0 || msNow.lCounts[_NORMAL_BLOCK] != 0
        || (_crtDbgFlag & _CRTDBG_CHECK_CRT_DF && msNow.lCounts[_CRT_BLOCK] != 0))
	{
		_CrtMemDumpAllObjectsSince(beginState);
	}
}

void ExitWithTestMemoryLeak()
{
	TestMemoryLeak();
}

// Test memory leak when a program exits
void Exit()
{
	std::atexit(ExitWithTestMemoryLeak);
}

