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
cout<<"�����������Ϣ��ѧ�ţ��Ա����������ſγ̳ɼ�\n";
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
cout<<"������ѧ�ţ��Ա�����;\n�꼶���������ַ;"<<endl;
cin>>num>>sex>>name>>grade>>motherland>>address;
cout<<"���������ſγɼ�\n";
cin>>score[0]>>score[1]>>score[2];
	cout<<"\tѧ��:"<<num
		<<"\t�Ա�:"<<sex
		<<"\t����:"<<name<<endl
		<<"\t�꼶��"<<grade
		<<"\tƽ���ɼ���"<<calc()<<endl
		<<"\t�����"<<motherland
		<<"\t��ַ��"<<address<<endl;
}

void main()
{
class stu xyj;
class people xu; 
char choice;
cout<<"\t\t\t��ʼ����!\a"<<endl;
do
{
xyj.put_stu();
cout<<"ѧ��:"<<xyj.num<<"\t�Ա�:"<<xyj.sex<<"\t����:"<<xyj.name<<endl;
cout<<"ƽ���ɼ���"<<xyj.calc()<<endl;
cout<<"������ʼ����"<<endl;
xu.informain();
cout<<"������ʼ������[y/n]\a"<<endl;
	cin>>choice;
}while(choice=='y'||choice=='Y');
}