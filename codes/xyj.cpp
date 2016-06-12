#include<iostream.h>
class stu
{
public:
int num;
char sex;
char name[8];
protected:
int score[3];
public:
	void put_stu()
{
cout<<"请输入相关信息：学号，性别，姓名，三门课程成绩\n";
cin>>num;
cin>>sex;
cin>>name;
cin>>score[0]>>score[1]>>score[2];
}
double calc();
};

double stu::calc()
{
double average;
average=(score[0]+score[1]+score[2])/3.0;
return average;
}


class people:protected stu
{
public:
	char motherland[8];
	char address[12];
private:
	int grade;
public:
	void informain();
};
void people::informain()
{
cout<<"请输入学号，性别，姓名;\n年级，祖国，地址;"<<endl;
cin>>num>>sex>>name>>grade>>motherland>>address;
cout<<"请输入三门课成绩\n";
cin>>score[0]>>score[1]>>score[2];
	cout<<"\t学号:"<<num
		<<"\t性别:"<<sex
		<<"\t姓名:"<<name<<endl
		<<"\t年级："<<grade
		<<"\t平均成绩："<<calc()<<endl
		<<"\t祖国："<<motherland
		<<"\t地址："<<address<<endl;
}

void main()
{
class stu xyj;
class people xu; 
char choice;
cout<<"\t\t\t开始运行!\a"<<endl;
do
{
xyj.put_stu();
cout<<"学号:"<<xyj.num<<"\t性别:"<<xyj.sex<<"\t姓名:"<<xyj.name<<endl;
cout<<"平均成绩："<<xyj.calc()<<endl;
cout<<"继续开始运行"<<endl;
xu.informain();
cout<<"继续开始运行吗？[y/n]\a"<<endl;
	cin>>choice;
}while(choice=='y'||choice=='Y');
}