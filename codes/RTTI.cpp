#include <iostream>
#include <typeinfo>
using namespace std;

class A
{
public:
	A(int aa = 0):a(aa){}
	~A(){}
	virtual void Print(void){cout<<endl<<"A:a="<<a<<endl;}
private:
	int a;
};

class BA: public A
{
public:
	BA(int aa =1, char cc = 's'):A(aa), ba(cc){}
	~BA(){}
	void Print(void){A::Print();cout<<"BA:ba="<<ba<<endl;}
private:
	char ba;
};

void main()
{
	BA* pba = new BA;
	pba->Print();

	A* pa = dynamic_cast<A*>(pba);

	if (pa != NULL)
		pa->Print();
	else
		cout << "error1\n";

	A* ppa = new A;
	pba = dynamic_cast<BA*>(ppa);

		if (pba != NULL)
		pba->Print();
	else
		cout << "error2\n";

		cout<<endl<<"============================\n";
		cout << typeid("s").name() <<endl;
		cout<< typeid(string("s")).name()<<endl;
		cout<< typeid(new string("s")).name()<<endl;
		if (typeid(string("s"))==typeid(string))
			cout <<"typeid(string('s'))==typeid(string)"<<endl;
		else
			cout <<"typeid(string('s'))!=typeid(string)"<<endl;
}