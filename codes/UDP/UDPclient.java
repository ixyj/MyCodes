import java.net.*;
import java.io.*;

public class UDPclient{
public static void main(String args[])throws Exception
{
   String received;
   String sent;
   byte[] buf=new byte[256];  

System.out.println("�����������IP��ַ: ");
String ip=(new BufferedReader(new InputStreamReader(System.in))).readLine();
System.out.println("��ע�⣺���IP��ַ���󣬳����쳣����ʱ����������������!");
         InetAddress address=InetAddress.getByName(ip);
         DatagramSocket socket= new  DatagramSocket();

while(true){
System.out.println("=====������=====");
int num=System.in.read(buf);
received=new String(buf,0,num);
DatagramPacket packet=new DatagramPacket(buf,num,address,6666);
socket.send(packet);
  
      socket.receive(packet);
      System.out.println("=====���յ�=====");
    received=new String(packet.getData(),0,packet.getLength());
    System.out.println(received);
   }
//socket.close(); 
 }
}