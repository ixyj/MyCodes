#include<iostream.h>
#include"derive.h"

graphic::graphic()
{
cout<<"graphic的构造函数！\n";
area=0;
}

graphic::~graphic()
{
cout<<"graphic的析构函数！\n";
}

double graphic::GetArea()
{
return area;
}

point::point():graphic()
{
cout<<"point的构造函数！\n";
}

point::~point()
{
cout<<"point的析构函数！\n";
}

void point::Display()
{
	cout<<"point的面积为零";
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
	cout<<"rectangle的析构函数！\n";
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
	cout<<"rectangle的面积："	<<GetArea()
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
	cout<<"circle的析构函数！\n";
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
	cout<<"circle的面积："<<GetArea()
		<<"\nradius:"<<GetRadius()
		<<endl;
}
