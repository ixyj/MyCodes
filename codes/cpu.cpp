#include <iostream>
#include <iomanip>
#include <string>
#include <vector>
#include <stack>
#include <exception>
#include <algorithm>

#include <cmath>
#include <cstdio>

using namespace std;

const size_t nFunction = 15;
const string Function[nFunction] = {"sin", "cos", "tan", "asin", "acos", "atan", "sinh", "cosh", "tanh", "lg", "ln", "abs", "floor", "ceil", "round"};
const string operate = "+-*/^%()";//优先级:(),^,*/,+-%
string pxyz[3] = {"NULL", "NULL", "NULL"};
/////////////////////////////////////////////////
//math function
double lg(double);
double ln(double);
double round(double);

double (*FunArray[])(double) = {sin, cos, tan, asin, acos, atan, sinh, cosh, tanh, lg, ln, abs, floor, ceil, round};
double Calc(double first, double second, char op);//op="=-*/^%"
//////////////////////////////////////////////////

void ParseExpress(vector<string>& parse, string& express);//根据表达式express分析操作和数据
double Compute(string& express);	//根据表达式express计算出结果
double Compute(vector<string>& parse);		//计算细节
double Compute(vector<string>::iterator itBegin, vector<string>::iterator itEnd);//计算细节不包括"()"
double ParseFromStrToValue(string& value);//返回字符串的对应的值
void ValidValue(const string& value);	//验证浮点数是否合法
void PreProcess(vector<string>& parse); //处理符号
bool Compare(const string& op1, const string& op2);	//运算符优先级比较

void GetExpress(string& express, const string& Fxyz, const string& xyz);

int main(int argc, char** argv)
{
	string strInput("");
	string F_x_y_z(""), x_y_z("");//函数参数
	getline(cin, strInput);
	
	while (true)
	{
		if (strInput == "q" || strInput == "Q")
			break;

		bool bCalcFun = false;
		if (strInput.empty() || find(strInput.begin(), strInput.end(), '=') != strInput.end())
		{
			cout << "F(x,y,z) = " << F_x_y_z;
			if (!strInput.empty())
				x_y_z = strInput;
			bCalcFun = true;
		}
		else if (strInput[0] == ':')
		{			
			F_x_y_z = strInput.substr(1);

			cout << endl;
			getline(cin, strInput);
			continue;
		}

		try
		{
			if (bCalcFun)
			{
				GetExpress(strInput, F_x_y_z, x_y_z);
				cout <<";\t(" << pxyz[0] << "," << pxyz[1] << "," <<  pxyz[2] << ")\n";
			}

			cout << "=" << setprecision(16) << Compute(strInput) << "\n\n";
		}
		
		catch (char* error)
		{
			cout << error << endl;
			cout <<"请重新输入：" << endl;
		}
		catch (string& error)
		{
			cout << error << endl;
			cout <<"请重新输入：" << endl;
		}
		catch (...)
		{
			cout <<"输入错误，请重新输入：" << endl;
		}
	

		getline(cin, strInput);
	}
	
	return 0;
}

double Compute(string& express)
{
	vector<string> parse;
	
	ParseExpress(parse, express);
	
	PreProcess(parse);
		
	return Compute(parse);
}

double Compute(vector<string>& parse)
{
	if (parse.empty())
		throw "算式不可为空！";
		
	vector<string> op;
	stack<vector<string> >sop;
		
	vector<string>::iterator it = parse.begin();
	while (it != parse.end())
	{
		if (*it == "(")
		{
			sop.push(op);
			op.clear();
			++it;
		}
		else if (*it == ")")
		{
			 double ret = Compute(op.begin(), op.end());
			 op.assign(sop.top().begin(), sop.top().end());
			 sop.pop();  
			 	
			 char tmp[100];
			 sprintf(tmp, "%16.8f", ret);
			 string v(tmp);
			 while (v[0] == ' ')
				 v.erase(v.begin());

			 if (!op.empty() && find(Function, Function+nFunction, *op.rbegin()) != Function+nFunction)
				 op[op.size()-1] += v;
else
				 op.push_back(v);

			 ++it;
		}

		while (it != parse.end() && *it != "(" && *it != ")")
		{
			op.push_back(*it);
			++it;
		}
	}
	
	if (op.empty())
		throw "计算错误！";
		
	return Compute(op.begin(), op.end());
}

double Compute(vector<string>::iterator itBegin, vector<string>::iterator itEnd)
{	
	if (itBegin == itEnd)
		throw("括号内表达式不能为空！");
		
	vector<string>::iterator it = itBegin;
	
	stack<double> dat;
	stack<string> op;
	
	dat.push(ParseFromStrToValue(*it));
	
	if (++it != itEnd)
		op.push(*it);
	else
		return dat.top(); 
	
	double v1 = 0.0f, v2 = 0.0f;
	while (++it != itEnd)
	{
		v2 = ParseFromStrToValue(*it);
		
		if (++it != itEnd)
		{
			while (!op.empty() &&  Compare(op.top(), *it))
			{
				v2 = Calc(dat.top(), v2, op.top()[0]);
				dat.pop();
				op.pop();
			} 
			
			op.push(*it);
			dat.push(v2);
		}
		else
		{
			dat.push(v2);
			while (!op.empty())
			{
				v2 = dat.top();
				dat.pop();
				v1 = dat.top();
				dat.pop();
				
				dat.push(Calc(v1, v2, op.top()[0]));
				op.pop();
			} 
			
			if (dat.size() != 1)
			{
				throw "运算符和数值数目不匹配！";
			}
		}

		if (it == itEnd)
			break;
	}
	
	return dat.top();
}

void ParseExpress(vector<string>& parse, string& express)
{
	if (express.empty())
		throw "表达式不可为空！";
	
	//添加省略的*号
	for (string::iterator it = express.begin() + 1; it != express.end(); ++it)
	{
		if ((*it == '(' || islower(*it)) && *it != 'e' && (isdigit(*(it-1)) || isupper(*(it-1)) || *(it-1)==')'))
			it = express.insert(it, '*');
	}

	parse.clear();
	
	string::iterator itBegin = express.begin(), it = itBegin;
	if (*it == '(')
	{
		parse.push_back(string(it, it+1));
		++it;
		itBegin = it;
	}
	else if (*it == '+' || *it == '-')//+-为正负号
	{
		itBegin = it = express.insert(it, '0');
	}
	
	while (it != express.end())
	{
		while (it+1 != express.end() && find(operate.begin(), operate.end(), *it) == operate.end())
			++it;

		if (*it == '+' || *it == '-')
		{
			if (it + 1 == express.end())
				throw "表达式最后不允许为+/-号！";

			if (*(it-1) == '(')	//+-为正负号
			{
				itBegin = it = express.insert(it, '0');
				continue;
			}
			else if (it - itBegin >= 2 && (*(it-1) == 'e' || *(it-1) == 'E') && isdigit(*(it-2)))
			{
				++it;
				continue;
			}
			else if ((*(it-1) == 'e' || *(it-1) == 'E') && (it == itBegin+1 || !isdigit(*(it-2))))
				throw "科学计数法错误！";
		} 
		
		if (*it == '(')
		{
			if (it != itBegin)
				parse.push_back(string(itBegin, it));	
			parse.push_back(string(it, it+1));	

			itBegin = ++it;	
			continue;
		}
			
		if (isdigit(*it) || isupper(*it))
		{
			parse.push_back(string(itBegin, it+1));
		}
		else
		{	
			if (it != itBegin)
				parse.push_back(string(itBegin, it));						
			parse.push_back(string(it, it + 1));	
		}

		itBegin = ++it;
	}	
}


double ParseFromStrToValue(string& value)
{
	if (value.empty())
		throw "数值不能为空！";
	
	size_t pos = 0, start = 0, length = value.size();
	int sign = (value[0] == '-' ? -1 : 1);

	if (value[0] == '-' || value[0] == '+' )
		pos = start = 1;

	while (pos < length && islower(value[pos]))
		++pos;
		
	string fun(value.begin() + start, value.begin() + pos);		
	
	string v(value.begin()+pos, value.end());
	
	if (v == "E")
		v = "2.718281828459";
	else if (v == "PI")
		v = "3.1415926535898";
	
	ValidValue(v);
		
	double ret = atof(v.c_str());
	
	if (!fun.empty())
	{
		size_t funPos = find(Function, Function + nFunction, fun) - Function;
		if (funPos == nFunction)
			throw string("找不到数学函数“" + fun + "”！");	
		
		ret = FunArray[funPos](ret);
	}
	
	return sign * ret;
}

void ValidValue(const string& value)
{
	bool bDot = false;
	
	string::const_iterator itE = value.begin();
	while (itE != value.end() && *itE != 'e' && *itE != 'E')
		++itE;
		
	string v(value.begin(), itE);
	
	if (v.empty() && itE != value.end())
		throw "科学计数法错误！";
	else if (v.empty() && itE == value.end())
		throw "转换成数值的字符值不能为空！";
	
	while (!v.empty())
	{	
		string::iterator it = v.begin();
		if (*it == '+' || *it == '-')
			++it;
		for (; it != v.end(); ++it)
		{
			if (*it == '.')
			{
				if (bDot)
					throw "'.'符号太多！";
				if (it == v.begin() || it == v.begin() + (v.size()-1) || (it == v.begin()+1 && (v[0] == '-' || v[0] == '+')))
					throw "'.'位置错误！";
				bDot = true;
			}
			else if (!isdigit(*it))
				throw "数值中出现非数字符号！";
		}
		
		if (itE != value.end())
			++itE;
		while (itE != value.end() && *itE != 'e' && *itE != 'E')
			++itE;
		if (itE != value.end())
			throw "'E/e'有误！";
	
		v.assign(itE, value.end());
		bDot = false;
	}
}

void PreProcess(vector<string>& parse)
{
	int nBrackets = 0;
	bool bDigit = false;

	vector<string>::iterator it = parse.begin();
	if (it == parse.end())
		throw "表达式空！";
		
	if (*it == "*" || *it == "/"  || *it == "%"  || *it == "^")
		throw "表达式不能以运算符开始!";

	while (it != parse.end())
	{
		if ((*it).size() == 1 && find(operate.begin(), operate.end(), (*it)[0]) != operate.end())
		{			
			if (!bDigit && it != parse.begin())
			{
				if (*(it-1) == "(" && *it == ")")
					throw "()内不允许空！";
			} 
			
			if (*it == "(")
				++nBrackets;
			else if (*it == ")")
			{
				--nBrackets;
				if (nBrackets < 0)
					throw "'('和')'不匹配！";
			}
				
			bDigit = false;
		}
		else
		{
			if (bDigit)
				throw "'数值连不可续出现！";
				
			bDigit = true;
		}

		++it;
	}	

	if (nBrackets != 0)
		throw "'('和')'不匹配！";
}

void GetExpress(string& express, const string& Fxyz, const string& xyz)
{
	if (Fxyz.empty())
		throw "函数为空！";

	express.assign(Fxyz.begin(), Fxyz.end());

	for (string::iterator it = express.begin() + 1; it != express.end(); ++it)
	{
		if ((*(it-1) == 'x' || *(it-1) == 'y' || *(it-1) == 'z') && (*it == '(' || islower(*it)))
			it = express.insert(it, '*');
		else if ((isdigit(*(it-1)) || isupper(*(it-1))) && (*it == 'x' || *it == 'y' || *it == 'z'))
			it = express.insert(it, '*');
	}

	string::const_iterator itBegin = xyz.begin(), it = itBegin;
	while (it != xyz.end())
	{
		while (*it != ' ' && *it != '\t' && it+1 != xyz.end())
			++it;

		if (it - itBegin < 2 || (*itBegin != 'x' && *itBegin != 'y' && *itBegin != 'z' && *(itBegin+1) != '='))
			throw "参数赋值格式必须为'x=1'";

		if (it + 1 != xyz.end())
			pxyz[*itBegin - 'x'].assign(itBegin, it);
		else
		{
			++it;
			pxyz[*itBegin - 'x'].assign(itBegin, it);
		}
		
		while (it != xyz.end() && (*it == ' ' || *it == '\t'))
			++it;
		itBegin = it;
	}

	for (int i = 0; i < 3; i++)
	{
		if (pxyz[i]!= "NULL" && find(express.begin(), express.end(), pxyz[i].at(0)) != express.end())
		{
			string value = pxyz[i].substr(2);
			for (string::iterator it = express.begin(); it != express.end(); ++it)
			{
				if (*it == pxyz[i].at(0))
				{
					*it = ')';
					it = express.insert(it, '(');
					size_t pos = it - express.begin();
					express.insert(pos+1, value);
					it = express.begin() + pos;
				}
			}
		}
	}
}

double Calc(double first, double second, char op)
{
	double ret = 0.0f;
	switch (op)
	{
		case '+':
			ret = first + second;
			break;
		case '-':
			ret = first - second;
			break;
		case '*':
			ret = first * second;
			break;
		case '/':
			if (fabs(second) < 1e-8)
				throw "除数不可为零！";
			ret = first / second;
			break;
		case '%':
			ret = first - second * floor(first / second);
			break;
		case '^':
			ret = pow(first, second);
			break;
			
		default:
			throw "不可识别的操作符！";
	}

	return ret;
}

bool Compare(const string& op1, const string& op2)
{
	if (op1 == "^")
		return true;
	else if ((op1 == "*" || op1 == "/")&& (op2 != "^"))
		return true;
	else if ((op1 == "+" || op1 == "-") && (op2 == "+" || op2 == "-"))
		return true;
	else
		return false;
}

double lg(double arg)
{
	return log10(arg);
}

double ln(double arg)
{
	return log(arg);
}

double round(double arg)
{
	return static_cast<double>(static_cast<int>(arg + 0.5f));
}

