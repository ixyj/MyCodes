#ifndef _FINANCING_H_
#define _FINANCING_H_

#include <string>
#include <list>
#include <iostream>

typedef struct TimeType
{
	int year;
	int month;
	int date;

	TimeType()
	{
		this->year = 0;
		this->month = 1;
		this->date = 1;
	}

	TimeType(int year,int month,int date)
	{
		this->year = year;
		this->month = month;
		this->date = date;
	}

     static bool isValidate(int year,int month,int date)
	{
		bool isLeap = (((year % 4 == 0) && (year % 100 != 0))|| (year % 400 == 0));

		bool bRet = false;

		switch (month)
		{
			case 1:
			case 3:
			case 5:
			case 7:
			case 8:
			case 10:
			case 12:
				bRet = (date > 0 && date < 32);
				break;

			case 4:
			case 6:
			case 9:
			case 11:
				bRet = (date > 0 && date < 31);
				break;

			case 2:
				if (isLeap)
					bRet = (date > 0 && date < 30);
				else
					bRet = (date > 0 && date < 29);
				break;

			default:
				break;
		}

		return bRet;
	}

	friend bool operator<(const TimeType& time1,const TimeType& time2)
	{
	 	if (time1.year <time2.year)
			return true;
		else if(time1.year == time2.year && time1.month < time2.month)
			return true;
		else if(time1.year == time2.year && time1.month == time2.month && time1.date < time2.date)
			return true;
		else
			return false;
	}

	TimeType& operator=(const TimeType& time)
	{
		this->year = time.year;
		this->month = time.month;
		this->date = time.date;

		return *this;
	}

	bool operator==(const TimeType& time)
	{
		return
			year == time.year && month == time.month && date == time.date;
	}
} Time;


typedef struct 
{
	Time time;
	float money;
	std::string type;
}Financing;

class PesronalFinance
{
public:
	PesronalFinance(std::string name);
	PesronalFinance(std::string name,float money,Time time,std::string type,bool isIncome = true);
	PesronalFinance(const PesronalFinance& pf);
	virtual ~PesronalFinance();
	

public:
	virtual void Add(float money,Time time,std::string type,bool isIncome = true);
	virtual void Delete(Time time,bool isIncome = true);
	virtual void Print()const;
	virtual std::string GetName()const;

	//PesronalFinance& operator=(const PesronalFinance& pf);
	friend std::ostream& operator<<(std::ostream& out,const PesronalFinance& pf);
	friend std::istream& operator>>(std::istream& in,PesronalFinance& pf);


//private:
	std::string name;
	std::list<Financing> income;
	std::list<Financing> expense;
};
















#endif _FINANCING_H_