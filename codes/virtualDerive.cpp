#include <iostream>
#include <string>
using namespace std;

class A
{
public:
	A(int aa = 0):a(aa){}
	~A(){}

	void Print(int aa = 0){cout<<endl<<"A:a="<<a<<endl;}

protected:
	int a;
};

class BA: virtual public A
{
public:
	BA(int aa, char cc):A(aa), ba(cc){}
	~BA(){}

	void Print(char c = 0){cout<<endl<<"A:a="<<a<<"\tBA:ba="<<ba<<endl;}

protected:
	BA(char cc):ba(cc){}

	char ba;
};

class CA: virtual public A
{
public:
	CA(int aa, string ss):A(aa), ca(ss){}
	~CA(){}

public:
	void Print(string s = "io"){cout<<endl<<"A:a="<<a<<"\tCA:ca="<<ca<<endl;}
protected:
	CA(string cc):ca(cc){}

	string ca;
};

class D : public BA, public CA
{
public:
	D(int aa, char c, string s, float ff)
		:A(aa), BA(c), CA(s), f(ff)
	{}
	~D(){}
	void Print(void){BA::Print();CA::Print();cout<<endl<<"D:f="<<ca<<endl;}
private:
	float f;
};

void main()
{
	D dd(1,'c', "string", 2.5f);
	dd.Print();
	dd.BA::Print('d');
}