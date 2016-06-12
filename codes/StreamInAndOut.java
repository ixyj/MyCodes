import java.io.DataInputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;

public class StreamInAndOut
{
	public static void main(String[] args)
	{
		try
		{
			OutStreamOut(System.out, "Hello, world!");
			String str = "";
			InStreamIn(System.in);
			OutStreamOut(System.out, str);
			 GetNum(System.in);
		}
		catch (IOException e)
		{
			System.out.println(e.toString());
		}
	}
	
	public static void OutStreamOut(OutputStream out, String str)throws IOException
	{
		if (str == null)
			out.write((new String("OutputStream out")).getBytes());
		else
			out.write((new String(str+"\n")).getBytes());
	}
	public static void InStreamIn(InputStream in)throws IOException
	{
		System.out.print("please input a string:");
		byte[] b = new byte[128];
		in.read(b);
		String str = new String(b);
		OutStreamOut(System.out, str);
	}
	public static void GetNum(InputStream in)throws IOException
	{
		System.out.print("please input a int and float(incorrect):");
		DataInputStream ds = new DataInputStream(in);
		int a;
		float f;
		a = ds.read();
		f = ds.readFloat();
		String s = new String((new Integer(a)).toString());
		s += "\t"+f;
		OutStreamOut(System.out, s+"\n");
	}
}

