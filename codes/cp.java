import java.io.*;
public class cp {
	public static void main(String[] args) throws IOException{
String in,out;
System.out.print("请输入源文件：");
BufferedReader br=new BufferedReader(new InputStreamReader(System.in));
in=new String(br.readLine());
System.out.print("请输入目标文件：");
out=new String(br.readLine());
copy(in,out);
	}
	static void copy(String inf,String outf)throws IOException{
		try{
			RandomAccessFile inFile=new RandomAccessFile(inf,"r");
			RandomAccessFile outFile=new RandomAccessFile(outf,"rw");
			byte[] temp=new byte[1024];
			int count=0;
				count=inFile.read(temp);
				while(count!=-1){
				outFile.write(temp,0,count);
				count=inFile.read(temp);
			}
		}
		catch(IOException e){
			System.out.println("发生异常！\n"+e);
		}
		}
}
