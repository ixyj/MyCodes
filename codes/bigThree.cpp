#include <iostream>
#include <fstream>
#include <cstring>

using namespace std;
typedef char* String;

class bigThree
{
public:
	bigThree();
	bigThree(float f,String c);
	~bigThree();
	void operator=(bigThree& other);
	void display(ostream out);

private:
	float f;
	String c;
};


bigThree::bigThree()
{
	f=0;
	c=new char[2];
	strcpy(c," ");
}

bigThree::bigThree(float f,String c)
{
	this->f=f;
	this->c=new char[strlen(c)+1];
	strcpy(this->c,c);
}

bigThree::~bigThree()
{
	delete c;
}

void bigThree::operator=(bigThree& other)
{
	/*
	this is a bug:
	c=other.c;
	*/
	f=other.f;
	delete c;
	c=new char[strlen(other.c)+1];
	strcpy(c,other.c);
}

void bigThree::display(ostream out)
{
	out<<"================================\n"
		<<"float f:"<<f<<"\tString c:"<<c<<endl;
}

void main()
{
	typedef bigThree* BT;
	bigThree bb(23.3,"example");
	int m=2,n=3;
	BT *bt=new BT[m+1];
	bt[0]=new bigThree[n];
	bt[0][0]=*(new bigThree(12.3,"hello\t"));
	bt[0][1]=*(new bigThree(21.4,"xyj\n"));
	for(m=0;m<2;m++)
		bt[0][m].display(cout);

	bt[1]=new bigThree[m];
	bt[1][0]=bt[0][0];
	bt[2]=new bigThree(100.01,"my computer");
	ofstream fout;
	fout.open("xyj.txt");
	bt[1][0].display(fout);
	bt[2]->display(fout);
	fout.close();
	
	delete[] bt;
}
