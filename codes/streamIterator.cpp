#include <iostream>
#include <list>
#include <vector>
#include <algorithm>
using namespace std;

int main(int argc, char* argv[])
{
	list<float> l;
	vector<float> v;
	copy(istream_iterator<float>(cin), 
		istream_iterator<float>(), 
		back_inserter(l)//or back_insert_iterator<list<float>>(l)
		);// input a non-num to end input!

	copy(l.begin(), l.end(), back_inserter(v));

	sort(v.begin(), v.end()); // list will be wrong!

	copy(v.begin(), v.end(), ostream_iterator<float>(cout,"\n"));
	return 0;
}

