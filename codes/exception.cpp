#include<iostream>
#include<cmath>
#include<string>
#include<exception>
#include<cassert>
using namespace std;

void run() throw (int,char);
void excep()	throw (exception);

int main()
{
	try
	{
		run();
	}
	catch(int i)
	{
		cout<<"除数("<<i<<")不能是零！\n";
	}

	catch(char c)
	{
		cout<<"您选择"<<c<<"(退出)！\n";
	}

	catch(exception e)
	{
		cout<<e.what()<<endl;
	}
cout<<"thanks for using!\n";
	return 0;
}

void run() throw (int,char)
{
	float a,b;
	char choice;
do{
	cout<<"\n===============================\n"
		<<"1.calc\t2.exit\n"
		<<"please choose:";
	cin>>choice;
	if(choice!='1' && choice!='2')
		excep();
	if(choice=='2')
		throw '2' ;
	cout<<"\na/b\tplease input a and b:";
	cin>>a>>b;
	if(fabs(b)<1e-4)throw 0;

	//assert(fabs(b)<1e-4);

	cout<<a<<"/"<<b<<"="<<a/b<<endl;
}while(choice=='1');
}

void excep()	throw (exception)
{
	throw exception("You input error!\n");
}

