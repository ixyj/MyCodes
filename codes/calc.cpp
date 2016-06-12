
#include <iostream>
#include <fstream>
#include <stack>
#include <cmath>
#include <string>
#include <cstring>
#include <cctype>
#include <exception>

using namespace std;

void Scan(void);
bool Issign(char ch);
bool IsPE(char ch);
char Compare(char first, char second);
double Parse(void);
double Operate(double first, char sign, double second);


char* tmpFile="data.txt";
fstream file;
char ch = ' ';
string token = "";
bool isFirst = true;
stack<double> digit;
stack<char> sign;

void main()
{
	cout<<"==========请输入===========\n";
	try
	{
		Scan();
		cout<<Parse()<<endl;
	}
	catch(char* e)
	{
		cout<<e<<endl<<"程序发生异常退出！\n";
		file.close();
	}
	cin >>ch;
}


void Scan(void)
{
	string input="";
	char signDigit = ' ';
	size_t signCount = 1;
	bool firstDigit = true;
	size_t bracket = 0;
	unsigned int count = 0;
	cin >> input;
	file.open(tmpFile, ios::out);
	if (file.fail())
	{
		throw "创建文件失败！";
	}
	ch = input.at(count++);
	do
	{
//======================================================
		if (isdigit(ch) || ch == '.')
		{
			if (firstDigit)
			{
				firstDigit = false;
				if ((count == 1 || (count >1 && Issign(input.at(count -2))))
					&& ch == '.')
						token += '0';                                                                             //是否省略0.x中的0
					if (count >1 && input.at(count -2) == ')')
						file << "*\t*\n";
			if (signCount == 2)                                                //判断正负号
				{
					if (count == 1 && ( input.at(0) == '+' ||  input.at(0) == '-'))
						signDigit = input.at(0);
					else if ( count > 1 && (input.at(count - 2) == '+' || input.at(count - 2) == '-'))
						signDigit = input.at(count - 2);
				}
			}
			token += ch;
			signCount = 0;
		}
//====================================================
		else if (Issign(ch))
		{
			if (token != "" )
			{
				if (token == "0.")
					throw "数据不完整！";
				file << "NUM" << '\t' << signDigit << token <<endl;
				 if (signCount > 2)
					throw "运算符错误！";
				signDigit = ' ';
			}
			token = "";
			firstDigit = true;
			if (ch == '(')
			{
				bracket +=1;
				signCount =1;
				if (count > 1 && isdigit(input.at(count - 2)))
					file << "*\t*\n";
			}
			else if (ch == ')')
				bracket -= 1;
			else
			{
				signCount++;
			}
			if (signCount ==1 || ch == ')')
				file << ch << '\t' <<ch <<endl;
		}
//============================================================
		else if (IsPE(ch))
		{
			bool addSign = false;
			if (count > 1)
			{
				if ( isdigit(input.at(count -2)) || IsPE(input.at(count -2)) || input.at(count -2) == ')')
				{
					if (isdigit(input.at(count -2)))
						file << "NUM\t" << signDigit << token <<endl;
					file << "*\t*\n";
					token = "";
				}
				else if (input.at(count - 2) == '.')
					throw "数据错误！";
			}
			if (count < input.size())
			{
				if (input.at(count) == '(' || isdigit(input.at(count)))
					addSign = true;
				else if (input.at(count) == '.')
					throw "数据错误！";
			}
			if (signCount == 2)                                                //判断正负号
				{
					if (count == 1 && ( input.at(0) == '+' ||  input.at(0) == '-'))
						signDigit = input.at(0);
					else if ( count > 1 && (input.at(count - 2) == '+' || input.at(count - 2) == '-'))
						signDigit = input.at(count - 2);
				}
			if (ch == 'p' || ch == 'P')
				file << "NUM\t" << signDigit << "3.141593\n";
			else
				file << "NUM\t" << signDigit << "2.718282\n";
			if (addSign)
				file << "*\t*\n";
			firstDigit = true;
			signCount = 0;
			signDigit = ' ';
		}
//=============================================================
		else if ( ch == 13)       //ch == '\n'
		{
			break;
		}
//=============================================================
		else
		{
			throw "出现非法符号！";
		}
//==============================================================
		if (count <= input.size())
			do{
				if (count < input.size())
						ch = input.at(count);
					count++;
			}while (ch == ' ');
	}while (count <= input.size());

	if (token !="")
		file << "NUM" << '\t' << signDigit << token <<endl;
	file << "#\t#";
	if (bracket != 0)
		throw "括号不匹配！";
	file.close();
}

bool IsPE(char ch)
{
	return (ch == 'p' || ch == 'P' || ch == 'e' || ch == 'E');
}

bool Issign(char ch)
{
	return (ch == '+' || ch == '-' || ch == '*' || ch == '/'
		|| ch == '%' || ch == '^' || ch == '(' || ch == ')');
}

char Compare(char first, char second)
{
	if (first == '#')
	{
		return '<';
	}
	if (first == '(')
	{
		if (second == ')')
			return '=';
		else
		return '<';
	}
	else if ( first == '^' && second != '(')
	{
		if (second == '^')
			return '<';
		else
			return '>';
	}
	else if ( (first == '*' || first == '/' || first == '%')
		&& (second != '(' && second != '^') )
	{
		return '>';
	}
	else if ( (first == '+' || first == '-')
		&& (second == '+' || second == '-' || second == '#'))
	{
		return '>';
	}
	else if (first != '(' && second == ')')
	{
		return '>';
	}
	else
	{
		return '<';
	}
}

double Parse(void)
{
	string token1 = "";
	double first = 0, second = 0;
	char calc =' ';
	file.open(tmpFile, ios::in);
	if (file.fail())
	{
		throw "打开文件失败！";
	}
	sign.push('#');
	file >> token1 >> token;
	
	while (!file.eof() || sign.top() != '#')
	{
		if (token1 == "NUM")
		{
			digit.push(atof(token.c_str()));
			file >> token1 >> token;
		}
		else
		{
			if ( Compare(sign.top(), token.at(0))== '>')
			{
				calc = sign.top();
				sign.pop();
				second = digit.top();
				digit.pop();
				first = digit.top();
				digit.pop();
				digit.push(Operate(first, calc, second));
			}
			else if ( Compare(sign.top(), token.at(0))== '=')
			{
				sign.pop();
				file >> token1 >> token;
			}
			else
			{
				sign.push(token.at(0));
				file >> token1 >> token;
			}
		}
	}
	file.close();
	return digit.top();
}



double Operate(double first, char sign, double second)
{
	double result = 0;
	switch (sign)
	{
		case '+': result = first + second; break;
		case '-':  result = first - second; break;
		case '*': result = first * second; break;
		case '/': 
			{
				if (fabs(second) < 1e-6)
					throw "除数不能是零！";
				result = first / second; break;
			}
		case '%': result = static_cast<double>(static_cast<long>(first) % static_cast<long>(second)); 
			break;
		case '^':	result = pow(first, second);	break;
		default:	 throw "非法运算符，程序异常退出！";	break;
	}
	return result;
}
