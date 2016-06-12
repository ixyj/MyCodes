#ifndef GRAPHIC_H
#define GRAPHIC_H

class graphic
{
protected:
	double area;

public:
	graphic();
	virtual ~graphic();
public:
	virtual double GetArea();
	virtual void   Display()=0;
};

class point:public graphic
{
public:
	point();
	virtual ~point();
	virtual void   Display();
};

class rectangle:public graphic
{
private:
	float height;
	float width;

public:
	rectangle();
	rectangle(float heigth,float width);
	virtual ~rectangle();
	float GetHeight();
	float GetWidth();
	virtual double GetArea();
	virtual void   Display();
};

class circle:protected graphic
{
private:
	float radius;
public:
	const double PI;

public:
	circle();
	circle(float radius);
	virtual ~circle();
	float GetRadius();
	virtual double GetArea();
	virtual void   Display();
};

#endif