#include <iostream>
#include <fstream>
#include <cstdlib>
#include <vector>
#include <string>

namespace myclass
{
	using namespace std;
	class A
	{
	public:
		A(double b,string c);
		~A();
		virtual void print() const;
		A& operator = (const A &other);
		A& operator = ( double b);
		A& operator = ( string c);
		bool operator == (const A& two);
		A operator ++ (int a);
		A operator ++ ();
		friend A operator+(A& one,A& two);
		friend A operator+(A& one,double d);
		friend A operator+(string s,A& one);
		friend bool operator>=(const A& one,const A& two);
		friend ostream& operator<<(ostream& fout,const A& one);
		friend istream& operator>>(ostream& fin, A& one);
		operator string()const;
	private:
		double b;
		string c;
	};


	A::A(double b=0,string c="default")
	{
		this->c=c;
		this->b=b;
	}

	A::~A()
	{
		cout<<"-----------------------\n";
	}

	void A::print() const
	{
		cout.setf(ios::scientific);
		cout.precision(3);
		cout<<"-------class A-------\n"
		<<"double b="<<b
		<<"\nstring c=\""<<c<<"\"\n";
	}

	A& A::operator = (const A &other)
	{
		 b=other.b;
		 c=other.c;
		 return *this;
	}

	A& A::operator = ( double b)
	{
		this->b=b;
	    return *this;
	}

	A& A::operator = ( string c)
	{
		this->c=c;
		 return *this;
	}

	bool A::operator == (const A& two)
	{
		return(c==two.c && b==two.b);
	}

	A operator + (A& one,A& two)
	{	
		A add(one.b+two.b,one.c+two.c);
		return add;
	}

	 A operator + (A& one,double d)
	{
		A temp(one.b+d,one.c);
		return temp;
	}

	 A operator + (string s,A& one)
	{
		A temp(one.b,s+one.c);
		return temp;
	}
	A A::operator ++()
	{
		b++;
		c+="A";
		return *this;
	}

	A A::operator ++ (int a)
	{
		A temp(b,c);
		b++;
		c+="A";
		return temp;
	}

	bool operator>=(const A& one,const A& two)
	{
		return (one.b>=two.b);
	}

	ostream& operator<<(ostream& fout,const A& one)
	{
		fout<<"**************************************\n"
			<<"double:"<<one.b
			<<"\nstring c:"<<"\""
			<<one.c<<"\"\n"
			<<"**************************************\n";
		return fout;
	}

	istream& operator>>(istream& fin, A& one)
	{
		string t;
		double d=0;
		fin.ignore(1000,':');
		fin>>d;
		one=d;
		fin.ignore(1000,'"');
		getline(fin,t,'"');
		one=t;
		return fin;
	}

	A::operator string() const
	{
		return string(this->c);
	}
}
	
int main(void)
{
	using namespace myclass;	
	vector<A> v;
	int i;
	ofstream fout;
	ifstream fin;

    v.resize(3);

	v[0]=A(12.2,"hello");
	v[1]=A(14.6,"\tgood");

	for(i=0;i<v.size();i++)
		v[i].print();

	cout<<"========v[2]=v[0]+v[1]========\n";
	v[2]=v[0]+v[1];
	v[2].print();

	cout<<"========v[0]>=v[1]========\n";
	if(v[0]>=v[1])
		cout<<"v[0]>=v[1]\n";
	else
		cout<<"v[0]<v[1]\n";
	
	v.resize(6);
	
	cout<<"====== << ==== >> ========\n";
	fout.open("xyj.txt");
	if(fout.fail())
	{
		cout<<"open file failed!\n";
		exit(1);
	}
	fout<<v[0]<<v[1]<<v[2];
	fout.close();

	fin.open("xyj.txt");
	if(fin.fail())
	{
		cout<<"open file failed!\n";
		exit(1);
	}

	fin>>v[3]>>v[4]>>v[5];
	fin.close();
	cout<<"================\n";
	for(i=3;i<v.size();i++)
		v[i].print();
	cout<<"================\n";
	
	if(v[4]==v[1])
		cout<<"===  v[4]=v[1]  ===\n";

	cout<<"==v[3]===\n";
	v[3].print();	
	cout<<"===== v[3]++ ====\n";
	(v[3]++).print();
	v[3].print();

	cout<<"==v[5]===\n";
	v[5].print();
	cout<<"===== ++v[5] ====\n";
	(++v[5]).print();
	v[5].print();
	cout<<"===== like overload ====\n";
	v.push_back(A(100));
	v[6].print();

	cout<<"|||||||||||||||||||||||||||||||||\n";
	A a1=v[2];
	a1=a1+20;
	a1.print();
	a1="this is.."+a1;
	a1.print();
	string s1=(string)a1;
	cout<<"**s1**\t"<<s1<<endl;
	return 0;
}
