//////////////////////////////////////////////////////////////////////////////
// Author: Xu Yajun
// Created Time: Sat 05 May 2012 09:55:07 AM CST
// File Name: rname.cpp
// Description: rename file
//////////////////////////////////////////////////////////////////////////////

#include <string>
 
#include <cstdio>
#include <cstdlib>
#include <dirent.h>

using namespace std;
 
int main(int argc, char** argv)
{
    DIR* dirp = opendir(".");
    if (dirp == NULL)
    {
        printf("fail to open file!\n");
        return EXIT_FAILURE;
    }

    char fileName[100];
    struct dirent *dp = NULL;
    while ((dp = readdir(dirp)) != NULL)
    {
        sprintf(fileName, "%s", dp->d_name);
        string fName(fileName);

        size_t pos = fName.rfind(".f4v");
        if (pos != string::npos)
        {
            fName.replace(pos, 4, ".flv");
            
            if (rename(fileName, fName.c_str()))
                printf("file '%s' has been renamed!\n", fileName);
            else
                perror("fail to rename\n");
        }
    }
    
    closedir(dirp);

	return EXIT_SUCCESS;
}
 
