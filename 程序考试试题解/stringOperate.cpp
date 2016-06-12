#include <iostream>
#include <vector>
#include <string>
#include <algorithm>
#include <functional>


using namespace std;

 void print(string s);

int  main()
{
	vector<string> vs;
	string str = "";

	cout << "please input the string(end with '\\'):";
	getline(cin,str,'\\');

	vs.clear();
	size_t index = 0;
	size_t start = 0;
	while (index < str.length())
	{
		if (str.at(index) == '#' && index > 0)
		{
			vs.push_back(str.substr(start,index - start));
			start = index + 1;
		}
		index++;
	}

	if (str.at(str.length() - 1) != '#')
		vs.push_back(str.substr(start,str.length() - start));

	sort(vs.begin(), vs.end());
	
	cout<<"total strings:"<<vs.size()<<endl;
	for_each(vs.begin(), vs.end(), print);

	str = vs.at(0);
	for(vector<string>::iterator it =vs.begin()+1; it != vs.end();++it)
	{
		if ((*it).length() > str.length())
			str = *it;
	}
	cout<<"string(of max length):"<<str<<endl;

	str = vs.at(0);
	for(vector<string>::iterator it =vs.begin()+1; it != vs.end();++it)
	{
		if ((*it).length() < str.length())
			str = *it;
	}
	cout<<"string(of min length):"<<str<<endl;

	system("pause");
	return 0;
}

void print(string str)
{
	cout << str <<endl;
}
