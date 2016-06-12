import java.io.FileInputStream;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.ObjectInputStream;
import java.io.ObjectOutputStream;
import java.io.Serializable;

public class serializeFile
{
	public static void main(String[] args) throws IOException, ClassNotFoundException
	{
		MyFile my = new MyFile(1, 3.14d, "hello,world");
		FileOutputStream file = new FileOutputStream("dat.txt");
		ObjectOutputStream os = new ObjectOutputStream(file);
		os.writeObject(my);
		os.close();
		
		MyFile dat = new MyFile(0, 0, "null");
		dat.Print();
		file.close();
		
		FileInputStream fin = new FileInputStream("dat.txt");
		ObjectInputStream is = new ObjectInputStream(fin);
		dat = (MyFile) is.readObject();
		is.close();
		fin.close();
		dat.Print();
///////////////////////////////////////////////////////////////////		
		os = new ObjectOutputStream(new FileOutputStream("data.txt"));
		os.writeUTF("string write to file!");
		os.close();
		
		is = new ObjectInputStream(new FileInputStream("data.txt"));
		System.out.println("read from data.txt:\t"+is.readUTF());
		is.close();
		
	}
	
	
	
}

class MyFile implements Serializable
{
	private static final long serialVersionUID = 123456L;
	private int m_i;
	private double m_d;
	private String m_s;
	
	public MyFile(int i, double d, String s)
	{
		m_i = i;
		m_d = d;
		m_s = s;
	}
	
	public void Print()
	{
		System.out.println("m_i="+m_i+"\tm_d="+m_d+"\tm_s="+m_s);
	}
}