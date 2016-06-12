import java.net.*;
import java.io.*;
import java.lang.String.*; 

public class Server{
public static void main(String args[])throws Exception
{
   String received;
   String sent;
ServerSocket receive=new ServerSocket(6666);
while(true)
{
Socket connect=receive.accept();
BufferedReader in=new BufferedReader(new InputStreamReader(connect.getInputStream()));
DataOutputStream out=new DataOutputStream(connect.getOutputStream());
received=in.readLine();
System.out.println("=====已收到=====");
System.out.println(received);
if(!received.substring(received.length()-4,received.length()).equals("Exit"))
{
System.out.println("=====请输入====="); 
InputStreamReader ir;
ir=new InputStreamReader(System.in);
in =new BufferedReader(ir);
sent=in.readLine();  
}
else
{
System.out.println("=====客户机已关闭=====");  
sent="";
}
out.writeBytes(sent+'\n');
   }
 }
}