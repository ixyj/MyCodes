import java.net.*;
import java.io.*;

public class Client{
public static void main(String args[])throws Exception
{
   String received;
   String sent;
      
System.out.println("请输入服务器IP地址: ");
InputStreamReader ir=new InputStreamReader(System.in);
BufferedReader in =new BufferedReader(ir);
String ip=in.readLine();
System.out.println("请注意：如果IP地址错误，程序将异常，此时请重新启动本程序!");
System.out.println("请输入你的用户名: ");
ir=new InputStreamReader(System.in);
in =new BufferedReader(ir);
String name=in.readLine();
do{
Socket clientsocket=new Socket(ip,6666);
BufferedReader inFromUser=new BufferedReader(new InputStreamReader(System.in));
DataOutputStream out=new DataOutputStream(clientsocket.getOutputStream());
BufferedReader inFromServer=new BufferedReader(new InputStreamReader(clientsocket.getInputStream()));
System.out.println("=====请输入=====                                **若退出请输入Exit**");
sent=inFromUser.readLine();
out.writeBytes(name+':'+sent+'\n'); 
received=inFromServer.readLine();
if(!sent.equals("Exit"))
{
System.out.println("=====已收到=====");
System.out.println(received);
}
clientsocket.close(); 
}while(!sent.equals("Exit"));
 }
}