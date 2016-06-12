#include <iostream>
#include <string>
using namespace std;

template <class t>
t max(t& one,t& two)
{
	if (one >= two)
		return one;
	else
		return two;
}

template <class r,class s,int n=3>
class my
{
public:
	my(r a[n],s b);
	virtual ~my();
	void display();
	r result();
	friend bool operator > (my<r,s,n> one,my<r,s,n> two);
private:
	r a[n];
	s b;
};

template <class r,class s,int n>
my<r,s,n>::my(r a[n],s b)
{
	for(int i=0;i<n;i++)
		this->a[i]=a[i];
	this->b=b;
}

template <class r,class s,int n>
my<r,s,n>::~my()
{
	cout<<"--------------------\n";
}

template <class r,class s,int n>
void my<r,s,n>::display()
{
	cout<<"==================\n";
		for(int i=0;i<n;i++)
		{
			cout<<a[i]<<'\t';
			if(i%4==3)
				cout<<endl;
		}
		cout<<"result:"<<result()<<endl;
		cout<<"\n"<<b<<"\n==================\n";
}

template <class r,class s,int n>
r my<r,s,n>::result()
{
	r y=a[0];
	for(int i=1;i<n;i++)
		y+=a[i];
	return y;
}

template <class r,class s,int n>
 bool operator > (my<r,s,n> one,my<r,s,n> two)
 {
	 return ( one.result() > two.result() );
 }

 void main()
 {
	 double z1[]={12.2,32.3,2.5},z2[]={21.2,11.5,16.6};
	 int y1[]={4,3,5}, y2[]={2,6,8};

	 my<double,string,3> a1(z1,"hello"),a2(z2,"good");
	 my<int,double> b1(y1,10.2),b2(y2,11.1);

	 a1.display();
	 a2.display();

	 if(b1>b2)
		 b1.display();
	 else
		b2.display();

 cout<<"====== max =========\n"
	 <<max(12.4,23.3)<<endl
	 <<max('q','Z')<<endl;
 }


/*
template <class T>
T min(const T t1, const T t2)  //模板函数
{
	return t1 < t2 ? t2 : t2;
}

template		//模板实例化
const float min<const float>(const float, const float);

template<> //模板特化
const char* min<const char*>(const char* s1, const char* s2)
{
	if (std::strcmp(s1, s2) < 0)
		return s1;
	else
		return s2;
}
*/