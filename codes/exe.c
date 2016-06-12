#include <process.h> 
#include <stdio.h> 
#include <errno.h> 

void main() 
{ 
    char* e[]={"cpuz.exe",NULL};
	 execv("e:\cpuz.exe",e); 
     perror("exec error"); 
	printf("\n");
   exit(1); 
} 
  
