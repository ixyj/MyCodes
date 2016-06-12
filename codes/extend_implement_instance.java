public class extend_implement_instance
{
	public static void main(String[] args)
	{
		IM im = new IM();
		im.PrintInfo();
		
		System.out.println(im instanceof abs);
		System.out.println(im instanceof if1);
		System.out.println(im instanceof itf);
	}	
}

abstract class abs
{
	final float pi = 3.14f;
	public abstract void Printabs();
	public void PrintInfo()	//子类必须实现
	{
		System.out.println("Hello, this is abstract class!");
	}
}

interface if1
{
	public void Print();//不可以实现
}

interface if2
{
	public void Print();
}

interface itf extends if1,if2
{
	public void Print12();
}

class IM extends abs implements itf
{
	public void Print()
	{
	}
	public void Print12()
	{
	}
	public void PrintInfo()
	{
	}
	public void Printabs()
	{
	}
}