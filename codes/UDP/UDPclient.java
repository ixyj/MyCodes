import java.net.*;
import java.io.*;

public class UDPclient{
public static void main(String args[])throws Exception
{
   String received;
   String sent;
   byte[] buf=new byte[256];  

System.out.println("请输入服务器IP地址: ");
String ip=(new BufferedReader(new InputStreamReader(System.in))).readLine();
System.out.println("请注意：如果IP地址错误，程序将异常，此时请重新启动本程序!");
         InetAddress address=InetAddress.getByName(ip);
         DatagramSocket socket= new  DatagramSocket();

while(true){
System.out.println("=====请输入=====");
int num=System.in.read(buf);
received=new String(buf,0,num);
DatagramPacket packet=new DatagramPacket(buf,num,address,6666);
socket.send(packet);
  
      socket.receive(packet);
      System.out.println("=====已收到=====");
    received=new String(packet.getData(),0,packet.getLength());
    System.out.println(received);
   }
//socket.close(); 
 }
}