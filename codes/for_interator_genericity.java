import java.util.Iterator;
import java.util.Vector;

public class for_interator_genericity
{
	public static void main(String[] args)
	{
		genCls[] arr = new genCls[3];
		for (int i = 0; i < 3; i++)
			arr[i] = new genCls<Integer>(i*2);
		
		for (genCls g : arr)
			g.Print(g.t);
		
		Vector<String> vs = new Vector<String>();
		vs.add("vector1");
		vs.add("vector2");
		vs.add("vector3");
		
		for (Iterator<String> it = vs.iterator(); it.hasNext();)
		{
			String str;
			str = it.next();
			System.out.println("Vector:"+str);
		}
	}
	
	
}
interface genInf <T extends Number>
{
	public void Print(T t);	
}

class genCls <T extends Number> implements genInf<T>
{
	T t;
	public genCls(T tt)
	{
		t=tt;
	}
	public void Print(T t)
	{
		System.out.println("interface genInf <T extends Number> t="+t);
	}
}