#include<iostream.h>
#include"derive.h"

graphic::graphic()
{
cout<<"graphic�Ĺ��캯����\n";
area=0;
}

graphic::~graphic()
{
cout<<"graphic������������\n";
}

double graphic::GetArea()
{
return area;
}

point::point():graphic()
{
cout<<"point�Ĺ��캯����\n";
}

point::~point()
{
cout<<"point������������\n";
}

void point::Display()
{
	cout<<"point�����Ϊ��";
}

rectangle::rectangle():graphic()
{
width=0;
height=0;
}

rectangle::rectangle(float heigth,float width):graphic()
{
this->height=heigth;
this->width=width;
}

rectangle::~rectangle()
{
	cout<<"rectangle������������\n";
}

float rectangle::GetHeight()
{
	return height;
}

float rectangle::GetWidth()
{
	return width;
}

double rectangle::GetArea()
{
	return width*height;
}

void rectangle::Display()
{
	cout<<"rectangle�������"	<<GetArea()
		<<"\nwidth:"			<<GetWidth()
		<<"\nheigth:"			<<GetHeight()
		<<endl;
}



circle::circle():PI(3.1416)
{
	radius=0;
}

circle::circle(float radius):PI(3.1416)
{
	this->radius=radius;
}

circle::~circle()
{
	cout<<"circle������������\n";
}

double circle::GetArea()
{
	return PI*radius*radius;
}

float circle::GetRadius()
{
	return radius;
}

void circle::Display()
{
	cout<<"circle�������"<<GetArea()
		<<"\nradius:"<<GetRadius()
		<<endl;
}
