import java.net.*;
import java.io.*;

public class Client{
public static void main(String args[])throws Exception
{
   String received;
   String sent;
      
System.out.println("�����������IP��ַ: ");
InputStreamReader ir=new InputStreamReader(System.in);
BufferedReader in =new BufferedReader(ir);
String ip=in.readLine();
System.out.println("��ע�⣺���IP��ַ���󣬳����쳣����ʱ����������������!");
System.out.println("����������û���: ");
ir=new InputStreamReader(System.in);
in =new BufferedReader(ir);
String name=in.readLine();
do{
Socket clientsocket=new Socket(ip,6666);
BufferedReader inFromUser=new BufferedReader(new InputStreamReader(System.in));
DataOutputStream out=new DataOutputStream(clientsocket.getOutputStream());
BufferedReader inFromServer=new BufferedReader(new InputStreamReader(clientsocket.getInputStream()));
System.out.println("=====������=====                                **���˳�������Exit**");
sent=inFromUser.readLine();
out.writeBytes(name+':'+sent+'\n'); 
received=inFromServer.readLine();
if(!sent.equals("Exit"))
{
System.out.println("=====���յ�=====");
System.out.println(received);
}
clientsocket.close(); 
}while(!sent.equals("Exit"));
 }
}