
#include"derive.h"
#include<iostream>

void main()
{
	using namespace std;
	graphic *r=(graphic*)new rectangle(12,16);
	r->Display();
	cout<<"hello!,the area of last rectangle is:"<<r->GetArea<<endl;
	graphic *c=(graphic*)new circle(12);
	c->Display();
	graphic *p=(graphic*)new point();
	r->Display();
}