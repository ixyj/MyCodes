#include<iostream.h>
#include<fstream.h>
#include<stdlib.h>

void main(int argc,char *argv[])
{
fstream fin,fout;
char t;
try{
fin.open(argv[1],ios::in);
if(fin.fail())throw("open the source file error!");
fout.open(argv[2],ios::out);
if(fout.fail())throw("open the object file error!");
while(!fin.eof())
{
	fin.get(t);
	fout.put(t);
}
fin.close();
fout.close();
}
catch(char *e)
{
	cerr<<e<<endl;
}
}