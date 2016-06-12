#include <iostream>
#include <vector>
using namespace std;

void  PutBallIntoBasket(vector<int>& basket, vector<int>& ball, int num,bool& bRet);
bool CanPut(const vector<int>& basket,int ball, int place);
void Print(const vector<int>& basket);

int  main()
{
	vector<int> basket, ball;
	int num = 0;

	cout<<"please input the num of basketball:";
	cin >> num;
	cout<<endl;

	for (int i = 0; i < num; i++)
	{
		basket.push_back(-2);
		ball.push_back(i);
	}
	
	bool bRet = false;
	PutBallIntoBasket(basket, ball,  num, bRet);
	if (!bRet)
		cout<<"There is no ways!\n";

	system("pause");
	return 0;
}


void  PutBallIntoBasket(vector<int>& basket, vector<int>& ball, int num,bool& bRet)
{
	   int ballNum = static_cast<int>(ball.size());
	   int index = 0;

	   for(vector<int>::iterator it = ball.begin(); it != ball.end(); ++it, ++index)
       {
              if(CanPut(basket, *it, ballNum-1))
              {
                    vector<int> basketTemp = basket;
					vector<int> ballTemp  = ball;
					
					basketTemp.at(ballNum - 1) = *it;
					ballTemp.erase(ballTemp.begin()+index);

                    PutBallIntoBasket(basketTemp, ballTemp, num, bRet);
              }
       }

	   if (ballNum == 0)
	   {
		   Print(basket);
		   bRet = true;
	   }
}



bool CanPut(const vector<int>& basket,int ball, int place)
{
	bool bRet = false;

	if (ball == place)
		return false;

	int basketNum = static_cast<int>(basket.size());

	if (place == 0)
	{
		if (ball - basket.at(1) == 1
			|| ball - basket.at(1) == -1)
			return false;
	}
	else if (place == basketNum -1)
	{
		if ( ball - basket.at(place-1) == 1
			|| ball - basket.at(place-1) == -1)
			return false;
	}
	else
	{
		if (ball - basket.at(place-1) == 1
			||ball - basket.at(place-1) == -1)
			return false;
		if (ball - basket.at(place+1) == 1
			|| ball - basket.at(place+1) == -1)
			return false;
	}

	return true;
}

void Print(const vector<int>& basket)
{	
	int basketNum = static_cast<int>(basket.size());

	cout <<"Àº×ÓºÅ:";
	for (int i = 0; i < basketNum; i++)
	{
		cout <<i+1<<"\t";
	}

	cout <<"\nÀºÇòºÅ:";
	for (int i = 0; i < basketNum; i++)
	{
		cout <<basket.at(i)+1<<"\t";
	}

	cout<<"\n=================================================\n";
}