#include<iostream.h>
int add2(int a,int b)
{
	int c;
	__asm{
		mov eax,a
		add eax,b
		mov c,eax
	}
	return c;
}

int sub2(int a,int b)
{
return (a-b);
}

void main()
{
int x,y,z,c;
cout<<"0.add\n1.sub\n";
cout<<"please choose:";
cin>>c;
cout<<"\nplease input a,b:";
cin>>x>>y;

if(c==0)
{
z=add2(x,y);
goto print;
}
if(c==1)
__asm{
push eax
jian:mov eax,y
push eax
mov eax,x
push eax
call sub2
mov z,eax
pop eax
pop eax
pop eax
}
print:cout<<"\nThe result is:"<<z<<endl;
end:
cout<<"Thanks for using!\n";
}

