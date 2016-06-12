#include <iostream>
#include <fstream>
#include <vector>
#include "financing.h"

using namespace std;

void Add(vector<PesronalFinance>& v);
void Del(vector<PesronalFinance>& v);
void View(const vector<PesronalFinance>& v);
void Count(vector<PesronalFinance>& v);

int main()
{
	char choose;
	vector<PesronalFinance> v;

	fstream fin;
	fin.open("file.dat",ios::in);
	while(!fin.eof())
	{
		PesronalFinance pf("");
		fin >> pf;
		v.push_back(pf);
	}
	fin.close();
		
	while(true)
	{
		cout<<"\n=============================\n"
			<<"[0]ADD. [1]DEL. [2]VIEW. [3]COUNT. [4]EXIT.\n"
			<<"Please choose:";
		cin >> choose;

		switch (choose)
		{
			case '0':
				Add(v);
				break;

			case '1':
				Del(v);
				break;

			case '2':
				View(v);
				break;

			case '3':
				Count(v);
				break;

			case '4':
				goto exitNow;

			default:
				cout<<"\ninput error!";
				break;
		
		}
	};

exitNow:
	fstream fout;
	fout.open("file.dat",ios::out);
	while (fout.fail())
	{
		fout.close();
		fout.open("file.dat");
	}
	for(vector<PesronalFinance>::iterator it = v.begin(); it != v.end(); ++it)
		fout << *it;
	fout.close();
	return 0;
}


void Add(vector<PesronalFinance>& v)
{
	char choose;
	string name,type;
	Time time;

	float money = 0;
	cout <<"\n----------------------\n"
		<<"[0] Add A Person. \n[1] Add a income record.\n[2] Add a expense record.\n Return.\nplease input:";
	cin >> choose;

	if (choose == '0')
	{
		cout << "\nplease input your name:";
		
		cin >> name;
		v.push_back(PesronalFinance(name)); 
	}
	else if (choose == '1' || choose == '2')
	{
		
		cout << "\nplease input name,money, type and time:";
		
		cin >>name >>money >> type>>time.year>>time.month>>time.date;
		for (vector<PesronalFinance>::iterator it = v.begin(); it != v.end(); ++it)
		{
			if (name == (*it).GetName())
			{
				bool bIncome = (choose == '1');
				(*it).Add(money,time,type,bIncome);
				cout <<"\nAdd success!";
				break;
			}
		}
	}
}
void Del(vector<PesronalFinance>& v)
{
	char choose;
	string name;
	Time time;
	float money = 0;

	cout <<"\n----------------------\n"
		<<"[0] Del A Person. \n[1] Del a income record.\n[2] Del a expense record.\n Return.\nplease input:";
	cin >> choose;

	if (choose == '0')
	{
		cout << "\nplease input your name:";
		
		cin >> name;
		for (vector<PesronalFinance>::iterator it = v.begin(); it != v.end(); ++it)
		{
			if ((*it).GetName() == name)
			{
				v.erase(it);
				break;
			}
		}
	}
	else if (choose == '1' || choose == '2')
	{
		
		cout << "\nplease input name and time:";
		
		cin >>name >>time.year>>time.month>>time.date;
		for (vector<PesronalFinance>::iterator it = v.begin(); it != v.end(); ++it)
		{
			if ((*it).GetName() == name)
			{
				bool bIncome = (choose == '1');
				(*it).Delete(time,bIncome);
				cout <<"\nDel success!";
				break;
			}
		}

	}
}
void View(const vector<PesronalFinance>& v)
{
	char choose;
	cout <<"\n----------------------\n"
		<<"[0] View all People. \n[1] View a People.\n Return.\nplease input:";
	cin >> choose;

	if (choose == '0')
	{
		for (vector<PesronalFinance>::const_iterator it = v.begin(); it != v.end(); ++it)
		{
			(*it).Print();
		}
	}
	else if (choose == '1')
	{
		string name;
		cout<<"\nplease input the name:";
		cin >> name;
		for (vector<PesronalFinance>::const_iterator it = v.begin(); it != v.end(); ++it)
		{
			if ((*it).GetName() == name)
			{
				(*it).Print();
				break;
			}
		}
	}
}

void CountAPerson(vector<PesronalFinance>& v)
{
	string name;
	float fincome = 0.0f;
	float fexpense = 0.0f;
	cout << "\nplease input name:";
	cin >> name;
	
	for (vector<PesronalFinance>::iterator it = v.begin(); it != v.end(); ++it)
	{
		if ((*it).GetName() == name)
		{
			(*it).Print();
			for (list<Financing>::iterator it_income = (*it).income.begin();it_income != (*it).income.end(); ++it_income)
				fincome += (*it_income).money;
			for (list<Financing>::iterator it_expense = (*it).expense.begin();it_expense != (*it).expense.end(); ++it_expense)
				fexpense += (*it_expense).money;

			cout <<"\ntotal income:"<<fincome<<"\ttotal expense:"<<fexpense;
			cout<<"\n all remains:"<<fincome - fexpense;
			break;
		}
	}
	
}

void CountAPerson(const Time& t1,const Time& t2,const PesronalFinance& v,float& fincome,float& fexpense)
{
	fincome = 0.0f;
	fexpense = 0.0f;
	
	for (list<Financing>::const_iterator it = v.income.begin(); it != v.income.end();++it)
	{
		if (t1<(*it).time && (*it).time < t2)
			fincome += (*it).money;
	}
	
	for (list<Financing>::const_iterator it = v.expense.begin(); it != v.expense.end();++it)
	{
		if (t1<(*it).time && (*it).time < t2)
			fexpense += (*it).money;
	}
}

void Count(vector<PesronalFinance>& v)
{
	float fin = 0.0f, fout = 0.0f;
	float f1 = 0.0f,f2=0.0f ;
	int choose;
	cout <<"[0]a person [1]all people. [2]all time Return\n";
	cin >> choose;
	if (choose == 1)
	{
		cout <<"please input yy-mm-dd to yy-mm-dd\n";
		Time time1,time2;
		cin>>time1.year>>time1.month>>time1.date;
		cin>>time2.year>>time2.month>>time2.date;
		
		for(vector<PesronalFinance>::const_iterator it = v.begin(); it != v.end(); ++it)
		{
			
			CountAPerson(time1,time2,*it,f1,f2);
			fin += f1;
			fout += f2;
		}
		cout <<"\n===result=====\nincome:"<<fin<<"\texpense:"<<fout<<"\ntotal:"<<fin-fout;
	}
	else if (choose == 0)
	{
		cout <<"please input the name and yy-mm-dd to yy-mm-dd\n";
		Time time1,time2;
		string name;
		cin >> name;
		cin>>time1.year>>time1.month>>time1.date;
		cin>>time2.year>>time2.month>>time2.date;
		float f1 = 0.0f,f2=0.0f ;
		for(vector<PesronalFinance>::const_iterator it = v.begin(); it != v.end(); ++it)
		{
			CountAPerson(time1,time2,*it,f1,f2);
			fin += f1;
			fout += f2;
		}
			cout <<"\n===result=====\nincome:"<<fin<<"\texpense:"<<fout<<"\ntotal:"<<fin-fout;
		
	}
	else if (choose ==2)
	{
		CountAPerson(v);
	}


}

