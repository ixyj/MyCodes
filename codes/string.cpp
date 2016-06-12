#include<iostream>
#include<cstring>
#include<string>
using namespace std;

int main()
{
	string a="hello!",b("good"),c="Hi",d,e;
	char *p=new char[30];
cout<<"c:\t"<<c<<endl;
c=a+b;
cout<<"a:\t"<<a<<"\nc=a+b:\n"<<c<<endl;
cout<<"please input:";
getline(cin,d)>>e;
cout<<"d:\t"<<d<<"\ne:\t"<<e<<endl;
e=c.substr(1,c.size());
e.at(e.size()-1)='*';
cout<<"e:\t"<<e<<endl;
strcpy(p,a.c_str());
cout<<"char*:\t"<<p<<endl;
delete p;
return 0;
}