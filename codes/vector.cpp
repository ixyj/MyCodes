#include<iostream>
#include<vector>
using namespace std;

class sample
{
public:
	void dis()
	{
		cout<<"===================\n"
			<<"int a:"<<a
			<<"\ndouble b:"<<b
			<<endl;
	}
	void set(int c,double d)
	{
		a=c;b=d;
	}
private:
	int a;
	double b;
};
int main()
{
	vector<sample> v(3);
	sample s,t;
	int i;

	s.set(12,12.5);
	t.set(100,10.01);

	cout<<"v.size:"<<v.size()<<endl;
	for(i=0;i<3;i++)
	v[i].set(i,10.5*i);

	cout<<"----------push_back----------\n";
	v.push_back(s);
	for(i=0;i<v.size();i++)
		v[i].dis();
	
	cout<<"----------resize----------\n";
	v.resize(2);
	for(i=0;i<v.size();i++)
		v[i].dis();
	cout<<"----------reserve----------\n";
	v.reserve(1);
	v.push_back(t);
	for(i=0;i<v.size();i++)
		v[i].dis();
return 0;
}