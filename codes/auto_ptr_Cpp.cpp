#include <iostream>
#include <memory>

using namespace std;
/*
auto_ptr不能指向非new出来的内存
两个auto_ptr不能指向同一个内存区域如auto_ptr<type> auto_ptr2(auto_ptr1.get());
auto_ptr不支持数组
*/
int main()
{
	//auto_ptr<int> pr1 = new int(1);   error!
	auto_ptr<int> pr1(new int(1));

	{
		auto_ptr<int> pr2(new int(3));

		//cout<<pr1; error!
		cout<<pr1.get()<<endl;  //pr1.get()返回pr1底层的指针
		cout<<*pr2<<endl;
	
			pr1=pr2;//this will delete pr2

		//cout<<*pr2<<endl; error
		cout<<*pr1<<endl;
	}

	cout<<*pr1 << endl;
	pr1.reset(new int(11));
	cout<<*pr1 << endl;
	*pr1=12;
	cout<<*pr1 << endl;
cout<<pr1.release() << endl;//返回pr1底层指针并释放

	return 0;
}