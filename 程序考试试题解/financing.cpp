#include "financing.h"
#include <cassert>

PesronalFinance::PesronalFinance(std::string name)
{
	this->name = name;
	income.clear();
	expense.clear();
}

PesronalFinance::PesronalFinance(std::string name, float money, Time time,std::string type, bool isIncome)
{
	this->name = name;

	assert(time.isValidate(time.year,time.month,time.date));
	Financing finance;
	
	finance.money = money;
	finance.time = time;
	finance.type = type;


	if (isIncome)
		income.push_front(finance);
	else
		expense.push_front(finance);
}

PesronalFinance::PesronalFinance(const PesronalFinance& pf)
{
	name = pf.name;
	income = pf.income;
	expense = pf.expense;
}

PesronalFinance::~PesronalFinance()
{
}

void PesronalFinance::Add(float money,Time time,std::string type,bool isIncome)
{
	Financing finance;
	finance.money = money;
	assert(time.isValidate(time.year,time.month,time.date));
	finance.time = time;
	finance.type = type;

	if (isIncome)
		income.push_front(finance);
	else
		expense.push_front(finance);

}

void PesronalFinance::Delete(Time time,bool isIncome)
{
	if (isIncome)
	{
		for (std::list<Financing>::iterator it = income.begin(); it != income.end(); ++it)
		{
			if((*it).time == time)
			{
				income.erase(it);
				break;
			}
		}
	}
	else
	{
		for (std::list<Financing>::iterator it = expense.begin(); it != expense.end(); ++it)
		{
			if((*it).time == time)
			{
				expense.erase(it);
				break;
			}
		}
	}
}
void PesronalFinance::Print()const
{
	std::cout<<"\n======name:"<<name<<"========\n";
	std::cout<<"-------income----------\n";
	for (std::list<Financing>::const_iterator it = income.begin(); it != income.end(); ++it) 
	{
		std::cout<<"time:"<<(*it).time.year<<"-"<<(*it).time.month<<"-"<<(*it).time.date<<"\tmoney:"<<(*it).money<<"\ttype:"<<(*it).type<<"\n";
	}
	std::cout<<"-------expense----------\n";
	for (std::list<Financing>::const_iterator it = expense.begin(); it != expense.end(); ++it) 
	{
		std::cout<<"time:"<<(*it).time.year<<"-"<<(*it).time.month<<"-"<<(*it).time.date<<"\tmoney:"<<(*it).money<<"\ttype:"<<(*it).type<<"\n";
	}
}

 std::string PesronalFinance::GetName()const
{
	return name;
}

 std::ostream& operator<<(std::ostream& out,const PesronalFinance& pf)
 {
	 out<<"#\n";
	 out<<pf.GetName()<<"\n";
	 for (std::list<Financing>::const_iterator it = pf.income.begin(); it != pf.income.end(); ++it) 
	{
		out<<(*it).time.year<<"\t"<<(*it).time.month<<"\t"<<(*it).time.date<<"\t"<<(*it).money<<"\t"<<(*it).type<<"\n";
	}
	for (std::list<Financing>::const_iterator it = pf.expense.begin(); it != pf.expense.end(); ++it) 
	{
		out<<(*it).time.year<<"\t"<<(*it).time.month<<"\t"<<(*it).time.date<<"\t"<<(*it).money<<"\t"<<(*it).type<<"\n";
	}
	
	return out;
 }

  std::istream& operator>>(std::istream& in,PesronalFinance& pf)
  {
	  char ch;
	  in >> ch;
	 while (ch == '#')
	 {
		in>>pf.name;
		for (std::list<Financing>::iterator it = pf.income.begin(); it != pf.income.end(); ++it) 
		{
			in>>(*it).time.year>>(*it).time.month>>(*it).time.date>>(*it).money>>(*it).type;
		}
		for (std::list<Financing>::iterator it = pf.expense.begin(); it != pf.expense.end(); ++it) 
		{
			in>>(*it).time.year>>(*it).time.month>>(*it).time.date>>(*it).money>>(*it).type;
		}
		ch = '0';
		in >> ch;
	 }

	return in;
  }
