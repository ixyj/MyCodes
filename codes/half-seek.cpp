#include <iostream.h>
#include <stdlib.h>

typedef int* intArray;
int position=0,count=1;

void search(intArray array,int num,int first,int last);
void sort(intArray array,int first,int last);
void display(intArray array,int first,int last);

void main()
{
	int i,count=0,n,num;
	char choose,choice; 
	intArray array;
	cout<<"���������ݿ������ݸ�����";
	cin>>n;
	array=new int[n];

	do{
		cout<<"===================================\n"
			<<"��ѡ��\nA.�Լ��������ݣ�\tB.�漴������\n";
		cin>>choose;
		if(choose!='a' && choose!='A' && choose!='b' && choose!='B')
		cout<<"\a�����������������룡\n";
	}while(choose!='a' && choose!='A' && choose!='b' && choose!='B');

	if(choose=='a'||choose=='A')
	{
		cout<<"������"<<n<<"����Ȼ��\n";
		for(i=0;i<n;i++)
		cin>>array[i];
	}
	else 
		for(i=0;i<n;i++)
			array[i]=rand()%1000;

	sort(array,0,n-1);
		display(array,0,n-1);
	cout<<"������Ҫ���ҵ����ݣ�";
	cin>>num;


	search(array,num,0,n-1);
	cout<<"Ҫ�쿴���ݿ���ȷ����[Y/y]\n";
	cin>>choice;
	if(choice=='y'||choice=='Y')
		display(array,0,n-1);
	delete[] array;
}


void search(intArray array,int num,int first,int last)
{
	int key=(first+last)/2;
	if(last>first && num!=*(array+key))
	{
		cout<<"--------------��ǰ���----------------\n";
		display(array,first,last);
		cout<<"count:"<<count<<"\tkey:"<<key
			<<"\tarray[key]:"<<array[key]<<"\tnum:"<<num
			<<"\tfirst:"<<first<<"\tlast:"<<last<<endl;
		count++;
		position=key;
		if(num<*(array+key))
			search(array,num,first,key);
		else
           	search(array,num,key+1,last);
	}
	else if(num== *(array+key))
	{
		cout<<"\n�����Ѳ鵽�����ڵ�"<<position<<"����\nһ����ѯ��"<<count<<"�Σ�\n";
		return;
	}
	else if(last<first || (last==first && num!=*(array+key)))
	{
		count=-1;
		cout<<"����δ�鵽�����������ݿ��У�\a\n";
		return;
	}
}

void sort(intArray array,int first,int last)
{
	for(int i=1;i<=last;i++)
		for(int j=first;j<=last-i;j++)
			if(array[j]>array[j+1])
			{
				int t=array[j];
				array[j]=array[j+1];
				array[j+1]=t;
			}
}

void display(intArray array,int first,int last)
{
	for(int i=first;i<=last;i++)
	{
		cout<<array[i]<<"\t";
		if((i-first)%5==4)
			cout<<endl;
		if(i==last)
			cout<<endl;
	}
}