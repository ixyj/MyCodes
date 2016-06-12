#include <iostream>
#include <hash_map>

using namespace std;

class A
{
public:
	A(int vv=-1):v(vv){}
	A(const A& a):v(a.v){}
	~A(){}		
	
	//A& operator=(const A& a){v=a.v;return *this;}
	//bool operator==(const A& a)const{return v==a.v;}
	//bool operator<(const A& a)const{return v<a.v;}
	operator size_t(void)const{return v;}
	friend ostream& operator<<(ostream& out, const A& a);
	
private:
	int v;	
};

int main(int argc, char** argv)
{
	hash_map<A,A> mA;
	mA[A(1)]=A(10);
	mA[A(2)]=A(20);
	mA[A(3)]=A(30);
	
	
	for (auto it = mA.begin(); it != mA.end(); ++it)
	{
		cout << "key:"<<(*it).first << "\tvalue:"<<(*it).second<<endl;
	}
	cout <<"---------------------------------------\n";
	
	mA.insert(mA.begin(), pair<A,A>(A(4),A(440)));
	for (auto it = mA.begin(); it != mA.end(); ++it)
	{
		cout << "key:"<<(*it).first << "\tvalue:"<<(*it).second<<endl;
	}
	cout <<"---------------------------------------\n";
	
	mA[A(4)]=A(40);
	for (auto it = mA.begin(); it != mA.end(); ++it)
	{
		cout << "key:"<<(*it).first << "\tvalue:"<<(*it).second<<endl;
	}
	cout <<"---------------------------------------\n";
	
	mA.erase(A(0));
	mA.erase(A(2));
	
	for (auto it = mA.begin(); it != mA.end(); ++it)
	{
		cout << "key:"<<(*it).first << "\tvalue:"<<(*it).second<<endl;
	}
	cout <<"---------------------------------------\n";
	cout << mA[A(2)]<<ends<<(mA.find(A(2)) != mA.end())<<endl;
	cout << mA[A(3)]<<ends<<(mA.find(A(3)) != mA.end())<<endl;
	
  return 0;
}



ostream& operator<<(ostream& out, const A& a)
{
	out<<a.v;
	return out;
}
