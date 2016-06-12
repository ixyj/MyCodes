import java.net.*;
import java.io.*;

public class UDPserver{
public static void main(String args[])throws Exception
{
   String received;
   String sent;
byte[] buf=new byte[256];

   DatagramSocket socket=new DatagramSocket(6666);
while(true)
{
    DatagramPacket packet=new DatagramPacket(buf,buf.length);
    socket.receive(packet);
    System.out.println("=====已收到=====");
    received=new String(packet.getData(),0,packet.getLength());
    System.out.println(received);

          System.out.println("=====请输入====="); 
          int num=System.in.read(buf);
          InetAddress address=packet.getAddress();
          int port=packet.getPort(); 
          packet=new DatagramPacket(buf,num,address,port);
          socket.send(packet);
   }
 }
}