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
cerr<<name<<"�ļ���ʧ�ܣ�\n";
exit(-1);
}
cout<<"���������ݣ�";
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
cerr<<name<<"�ļ���ʧ�ܣ�\n";
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
cerr<<inf<<"�ļ���ʧ�ܣ�\n";
exit(-1);
}
out.open(outf,ios::out);
if(out.fail())
{
cerr<<outf<<"�ļ���ʧ�ܣ�\n";
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
cout<<"�������ļ�������ע����������밴\\������\n";
	cin.getline(file,19,'\\');
begin:
cout<<"************�˵�***************\n"
    <<"    -=1=- �½��ļ�\n"
	<<"    -=2=- �����ļ�\n"
	<<"    -=3=- �༭�ļ�\n"
	<<"    -=4=- �����ļ�\n"
	<<"    -=5=- �˳�����\n"
	<<"*******************************\n";
cout<<"��ѡ��";
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
cout<<"�������\n";
	goto begin;

end:cout<<"ллʹ�ã�\n";
}