#include<iostream.h>
#include<string.h>

class A
{
public:
A(int,int,char*);
A(A&);
~A();
friend class change;
void dis();
void set(int,int,char*);
private:
static int count;
int a;
char *b;
const int c;
};

class change
{
public:
	change(int z1=0,int z=0,char *z2="")
	{r=z1;
	t=z;
	s=new char[strlen(z2)+1];
    strcpy(s,z2);
	}
	~change(){delete s;}
	void setA(A &);
void dis();
private:
	int r,t;
	char *s;
};

void change::setA(A &z)
{
	z.a=r;
delete z.b;
z.b=new char[strlen(s)+1];
strcpy(z.b,s);
}
void change::dis()
{
	cout<<"chane\t"<<r<<ends<<s<<endl;
}

int A::count=0;

A::A(int x=0,int z=0,char *p=""):c(z)
{
	a=x;
b=new char[strlen(p)+1];
strcpy(b,p);
count++;
cout<<"count:"<<count<<endl;
}
A::A(A &z):c(z.a)
{
this->a=z.a;
this->b=new char[strlen(z.b)+1];
strcpy(this->b,z.b);
count++;
}

A::~A(){
delete b;
cout<<"count:"<<count<<endl;
count--;
}

void A::dis()
{
cout<<a<<ends<<b<<"\tconst:"<<c<<endl;
}
void A::set(int x=0,int y=0,char *p="")
{
	a=x;
	delete b;
b=new char[strlen(p)+1];
strcpy(b,p);
}
void main()
{
A a1(10,11,"xyj");
change C(1000,1100,"change");
char *q="good!";
a1.dis();
a1.set(100,110,q);
a1.dis();
A a2=a1;
a2.dis();
a1.set(20,21,"perfect");
a1.dis();
A a3(a1);
a3.dis();
C.dis();
C.setA(a3);
a3.dis();
}