#include<iostream.h>
#include<fstream.h>
#include<stdlib.h>

void creat(fstream &out,char *name,char ch)
{
char p[100];
if(ch=='1')
out.open(name,ios::out);
else if(ch=='3')
out.open(name,ios::app);
if(out.fail())
{
cerr<<name<<"文件打开失败！\n";
exit(-1);
}
cout<<"请输入内容：";
cin.getline(p,99,'\\');
out<<p;
out.close();
}

void read(fstream &out,char *name)
{
	char data;
	out.open(name,ios::in);
	if(out.fail())
{
cerr<<name<<"文件打开失败！\n";
exit(-1);
}
	while(!out.eof())
	{
	out.get(data);
	cout<<data;
	}
	out.close();
	cout<<endl;
}

void copy(fstream &in,fstream &out,char *inf,char *outf)
{
	char data;
	
	in.open(inf,ios::in);
if(in.fail())
{
cerr<<inf<<"文件打开失败！\n";
exit(-1);
}
out.open(outf,ios::out);
if(out.fail())
{
cerr<<outf<<"文件打开失败！\n";
exit(-1);
}
while(in.get(data))
out.put(data);
in.close();
out.close();
}

void main()
{
fstream data,in,out;
char name[20],*file=name,ch,outfile[20]="source.txt",*outf=outfile;
cout<<"请输入文件名！（注：输入结束请按\\键！）\n";
	cin.getline(file,19,'\\');
begin:
cout<<"************菜单***************\n"
    <<"    -=1=- 新建文件\n"
	<<"    -=2=- 读出文件\n"
	<<"    -=3=- 编辑文件\n"
	<<"    -=4=- 复制文件\n"
	<<"    -=5=- 退出程序\n"
	<<"*******************************\n";
cout<<"请选择！";
cin>>ch;
if(ch=='5')goto end;

if(ch=='1'||ch=='3')	
creat(data,file,ch);

else if(ch=='2')
read(data,file);

else if(ch=='4')
{
copy(in,out,file,outf);
}

else
cout<<"输入错误！\n";
	goto begin;

end:cout<<"谢谢使用！\n";
}