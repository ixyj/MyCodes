import java.net.*;
import java.io.*;

public class clientnet{
public static void main(String args[])throws Exception
{
   String sent;
if(args.length!=2)
{
System.out.println("Input error!");
System.exit(0);
}
      int port=Integer.parseInt(args[1]);
Socket clientsocket=new Socket(args[0],port);
DataOutputStream out=new DataOutputStream(clientsocket.getOutputStream());
sent="Hello,My Java World!";
out.writeBytes(sent+'\n'); 
clientsocket.close(); 

 }
}