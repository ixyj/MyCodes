#include <iostream>
#include <vector>
#include <algorithm>
#include <functional>
using namespace std;

class More : public binary_function<float, float, bool>
{
public:
	More(){}
	~More(){}
	inline bool operator()(float one, float two)const
	{
		return greater<float>()(one, two);
	}
};

int main()
{
	vector<float> vf;

	copy(istream_iterator<float>(cin),istream_iterator<float>(),back_inserter<vector<float>>(vf));
	
	transform(vf.begin(), vf.end(), vf.begin(), bind2nd(More(),0.0f));
	
	copy(vf.begin(), vf.end(),ostream_iterator<float>(cout,"\n"));
	
	return 0;
}
