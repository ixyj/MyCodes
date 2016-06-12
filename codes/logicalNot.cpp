#include <algorithm>
#include <functional>
#include <iostream>
using namespace std;

class A
{
public:
	A(int aa = 0):a(aa){}
	~A(){}
	bool operator<(const A& one) {return a < one.a;}
	bool operator>=(const A& one) {return logical_not<bool>()(operator<(one));}
	friend bool operator<(const A& one, const A& two){return one.a < two.a;}

private:
	int a;
};

int main()
{
	A a1(1), a2(2), a3(3);

	cout << (a3>=a2) << "\n";
	cout << logical_or<bool>()(a1>=a2, a2>=a3) << "\n";
	cout << less<A>()(a2, a3) <<endl;

	return 0;
}
