import java.net.*;
import java.io.*;

public class Client{
public static void main(String args[])throws Exception
{
   String received;
   String sent;
      
System.out.println("�����������IP��ַ: ");
String ip=(new BufferedReader(new InputStreamReader(System.in))).readLine();
System.out.println("��ע�⣺���IP��ַ���󣬳����쳣����ʱ����������������!");

do{
       Socket clientsocket=new Socket(ip,6666);
       BufferedReader inFromUser=new BufferedReader(new InputStreamReader(System.in));
       DataOutputStream out=new DataOutputStream(clientsocket.getOutputStream());
       BufferedReader inFromServer=new BufferedReader(new InputStreamReader(clientsocket.getInputStream()));

       System.out.println("=====������=====                     **���˳�������Exit**");
       sent=inFromUser.readLine();
       out.writeBytes(sent+'\n'); 
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