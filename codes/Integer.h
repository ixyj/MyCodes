#pragma once

#ifndef _MY_INTEGER_H_
#define _MY_INTEGER_H_

#include <string>
#include <vector>
#include <map>
#include <iterator>

#include <algorithm>

using namespace std;

/////////////////////////////////////////////////////////
//定义大整数类
class CInteger
{
public:
	//////////////////////////////////////////////////
	CInteger(long value = 0);
	CInteger(const char* value);
	CInteger(const CInteger& CInt);

	virtual ~CInteger(void);

public:
	CInteger operator = (const CInteger& CInt);
	CInteger operator = (const char* value);
	CInteger operator = (long value);
	char& operator [] (size_t loc); 	//返回其值而非其字符

	CInteger operator + (const CInteger& one);
	CInteger operator - (const CInteger& one);
	CInteger operator * (const CInteger& one);
	CInteger operator / (const CInteger& one);

	bool operator < (const CInteger& one);
	bool operator == (const CInteger& one);

	operator string ();
	operator long ();  // 可能溢出

protected:
	char Compare(const vector<char>& first, const vector<char>& second) const;

	void Add(vector<char>& result, const vector<char>& first, const vector<char>& second);
	void Sub(vector<char>& result, const vector<char>& first, const vector<char>& second);//|first|>=|second|
	void Mul(vector<char>& value, char n); // 0 <= n <= 9
	char Div(vector<char>& first, const map<char, vector<char> >& second);

	inline void TrimZero(void);

private:
	vector<char> _value;
	char _sign; //1,>=0    -1,<0
};


//////////////////////////////////////////////////////////////////
//constructor

CInteger::CInteger(long value)
: _sign(value < 0 ? -1 : 1)
{
	value *= _sign;
	do
	{
		_value.push_back(value % 10);
		value /= 10;
	}while (value > 0);

	reverse(_value.begin(), _value.end());
}

CInteger::CInteger(const char* value)
: _sign(1)
{
	const char* p = value;
	if (*p == '-')
		_sign = -1;
	if (*p == '+' || *p == '-')
		++p;
	while (*p != '\0' && *p >= '0' && *p <= '9')
	{
		_value.push_back(*p - '0');
		++p;
	}

	TrimZero();
}

CInteger::CInteger(const CInteger& CInt)
: _value(CInt._value.begin(), CInt._value.end())
, _sign(CInt._sign)
{
}

CInteger::~CInteger(void)
{
	_value.clear();
}


//////////////////////////////////////////////////////
//implement
CInteger CInteger::operator = (const CInteger& CInt)
{
	_sign = CInt._sign;
	_value.assign(CInt._value.begin(), CInt._value.end());

	return *this;
}

CInteger CInteger::operator = (const char* value)
{
	const char* p = value;
	if (*p == '-')
		_sign = -1;
	else
		_sign = 1;
	if (*p == '+' || *p == '-')
		++p;

	_value.clear();
	while (*p != '\0' && *p >= '0' && *p <= '9')
	{
		_value.push_back(*p - '0');
		++p;
	}

	TrimZero();
	return *this;
}

CInteger CInteger::operator = (long value)
{
	if (value >= 0)
		_sign = 1;
	else
		_sign = -1;

	value *= _sign;
	_value.clear();
	do
	{
		_value.push_back(value % 10);
		value /= 10;
	}while (value > 0);

	reverse(_value.begin(), _value.end());
	return *this;
}

char& CInteger::operator [] (size_t loc)
{
	return _value[loc];
}


//////////////////////////////////////////////////////////////////
CInteger CInteger::operator + (const CInteger& one)
{
	CInteger CInt;

	if (_sign * one._sign == 1)
	{
		CInt._sign = _sign;
		Add(CInt._value, _value, one._value);
	}
	else
	{
		if (Compare(_value, one._value) == 1)
		{
			CInt._sign = _sign;
			Sub(CInt._value, _value, one._value);
		}
		else
		{
			CInt._sign = one._sign;
			Sub(CInt._value, one._value, _value);
		}
	}

	return CInt;
}


CInteger CInteger::operator - (const CInteger& one)
{
	CInteger CInt;

	if (_sign * one._sign == -1)
	{
		CInt._sign = _sign;
		Add(CInt._value, _value, one._value);
	}
	else
	{
		if (Compare(_value, one._value) == 1)
		{
			CInt._sign = _sign;
			Sub(CInt._value, _value, one._value);
		}
		else
		{
			CInt._sign = -_sign;
			Sub(CInt._value, one._value, _value);
		}
	}

	TrimZero();
	return CInt;
}

CInteger CInteger::operator * (const CInteger& one)
{
	CInteger CInt;
	vector<char> tempInt, tempResult;
	int zeroNum = 0;

	for (vector<char>::const_reverse_iterator it = one._value.rbegin(); it != one._value.rend(); ++it, ++zeroNum)
	{
		tempInt.assign(_value.begin(), _value.end());
		Mul(tempInt, *it);
		tempInt.insert(tempInt.end(), zeroNum, 0);
		Add(tempResult, CInt._value, tempInt);
		CInt._value.assign(tempResult.begin(), tempResult.end());
	}

	CInt._sign = _sign * one._sign;
	TrimZero();
	
	return CInt;
}

CInteger CInteger::operator / (const CInteger& one)
{
	CInteger CInt;
	CInt._sign = _sign * one._sign;

	if (Compare(_value, one._value) == -1)
		return CInt;

	vector<char> tmp(_value.begin(), _value.begin() + one._value.size() - 1);
	pair<char, vector<char> > pTmp(1, one._value);
	map<char, vector<char> > mTmp;

	do
	{
		mTmp.insert(mTmp.end(), pTmp);
		pTmp.first *= 2;
		Mul(pTmp.second, 2);
	} while (pTmp.first < 9);

	vector<char>::const_iterator it = _value.begin() + one._value.size() - 1;
	while (it != _value.end())
	{
		tmp.push_back(*it);
		CInt._value.push_back(CInt.Div(tmp, mTmp));
		++it;
	}

	CInt.TrimZero();
	return CInt;
}


///////////////////////////////////////////////////////////////////
bool CInteger::operator < (const CInteger& one)
{
	if (_sign == 1 && one._sign == -1)
		return false;
	else if (_sign == -1 && one._sign == 1)
		return true;

	return _sign * Compare(_value, one._value) < 0;
}

bool CInteger::operator == (const CInteger& one)
{
	if (_sign != one._sign || _value.size() != one._value.size())
		return false;

	for (vector<char>::const_iterator it1 = _value.begin(), it2 = one._value.begin(); it1 != _value.end(); ++it1, ++it2)
	{
		if (*it1 != *it2)
			return false;
	}
	return true;
}


///////////////////////////////////////////////////////////////////
CInteger::operator string ()
{
	string value;
	if (_sign == -1)
		value.push_back('-');
	for (vector<char>::const_iterator it = _value.begin(); it != _value.end(); ++it)
		value += *it + '0';

	return value;
}

CInteger::operator long ()  // 可能溢出
{
	long value = 0, tmp = 1;
	for (vector<char>::const_reverse_iterator it = _value.rbegin(); it != _value.rend(); ++it, tmp *= 10)
	{
		value += *it * tmp;
	}

	value *= _sign;

	return value;
}


///////////////////////////////////////////////////////////////////
char CInteger::Compare(const vector<char>& first, const vector<char>& second) const
{
	if (first.size() < second.size())
		return -1;
	else if (first.size() > second.size())
		return 1;

	for (vector<char>::const_iterator it1 = first.begin(), it2 = second.begin(); it1 != first.end(); ++it1, ++it2)
	{
		if (*it1 < *it2)
			return -1;
		else if (*it1 > *it2)
			return 1;
	}

	return 0;
}


void CInteger::Add(vector<char>& result, const vector<char>& first, const vector<char>& second)
{
	result.clear();
	char sum = 0;
	vector<char>::const_reverse_iterator it1 = first.rbegin(), it2 = second.rbegin();

	if (first.size() > second.size())
	{
		while (it2 != second.rend())
		{
			sum += *it1 + *it2;
			result.push_back(sum % 10);
			sum /= 10;
			++it1;
			++it2;
		}
		while (it1 != first.rend())
		{
			sum += *it1;
			result.push_back(sum % 10);
			sum /= 10;
			++it1;
		}
	}
	else
	{
		while (it1 != first.rend())
		{
			sum += *it1 + *it2;
			result.push_back(sum % 10);
			sum /= 10;
			++it1;
			++it2;
		}
		while (it2 != second.rend())
		{
			sum += *it2;
			result.push_back(sum % 10);
			sum /= 10;
			++it2;
		}
	}

	if (sum > 0)
		result.push_back(sum);

	reverse(result.begin(), result.end());

	while (result.size() > 1 && result.at(0) == 0)
		result.erase(result.begin());
}

void CInteger::Sub(vector<char>& result, const vector<char>& first, const vector<char>& second)
{
	result.clear();
	char sub = 0;
	vector<char>::const_reverse_iterator it1 = first.rbegin(), it2 = second.rbegin();

	while (it2 != second.rend())
	{
		sub = *it1 - *it2 - sub;
		if (sub >= 0)
		{
			result.push_back(sub);
			sub = 0;
		}
		else
		{
			result.push_back(sub + 10);
			sub = 1;
		}
		++it1;
		++it2;
	}
	while (it1 != first.rend())
	{
		sub = *it1 - sub;
		if (sub >= 0)
		{
			result.push_back(sub);
			sub = 0;
		}
		else
		{
			result.push_back(sub + 10);
			sub = 1;
		}
		++it1;
	}

	reverse(result.begin(), result.end());

	while (result.size() > 1 && result.at(0) == 0)
		result.erase(result.begin());
}

void CInteger::Mul(vector<char>& value, char n)
{
	if (value.empty())
		return;

	char ret = 0;
	for (vector<char>::reverse_iterator it = value.rbegin(); it != value.rend(); ++it)
	{
		ret += *it * n;
		*it = ret % 10;
		ret /= 10;
	}

	if (ret > 0)
		value.insert(value.begin(), ret);
}

char CInteger::Div(vector<char>& first, const map<char, vector<char> >& second)
{
	char result = 0, sign = 0;
	vector<char> tmp;

	for (map<char, vector<char> >::const_reverse_iterator it = second.rbegin(); it != second.rend(); ++it)
	{
		sign = Compare(first, (*it).second);
		if (sign > 0)
		{
			result += (*it).first;
			Sub(tmp, first, (*it).second);
			first.assign(tmp.begin(), tmp.end());
		}
		else if (sign == 0)
		{
			result += (*it).first;
			first.clear();
			return result;
		}
	}

	return result;
}

void CInteger::TrimZero(void)
{
	while (_value.size() > 1 && _value.at(0) == 0)
		_value.erase(_value.begin());

	if (_value.size() == 1 && _value.at(0) == 0)
	{
		_sign = 1;
	}
}



#endif //_MY_INTEGER_H_
