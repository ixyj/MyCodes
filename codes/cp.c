#include<stdio.h>
#include<stdlib.h>
int main(int c,char **v)
{
	FILE *in,*out;
	char temp[1024];
	int count;
	if(c!=3)
	{
		printf("input error!\n");
		exit(0);
	}
  
	if((in=fopen(*(v+1),"ra"))==NULL)
	{
		printf("source file error!\n");
		exit(0);
	}
	if((out=fopen(*(v+2),"wa"))==NULL)
	{
		printf("destination file error!\n");
		exit(0);
	}
	while(!feof(in))
	{
		if((count=fgets(temp,1024,in))<0)
		{
			printf("copy failed!\n");
			exit(0);
		}
		fputs(temp,out);
	}
	fclose(in);
	fclose(out);
printf("copy succeed!\n");
return 0;
}