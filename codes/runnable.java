import java.util.*;
public class t
{
	public static void main(String[] args) throws Exception
	{
		
		Thread t1 = new Thread(new Runner("first"));
		
		Thread t2 =  new Thread(new Runner("second"));
		
		Thread t3 =  new Thread(new Runner("third"));
		

t1.start();

t2.start();

t3.start();


	}
}

class Runner implements Runnable
{ 
    String s;
public Runner(String a)
{
s=a;
}
	public void run()
	{
for(int i=0;i<50;i++){
try{
		Date now = new Date();
		System.out.println(s+":\tThe thread time is- " + now.getHours() + " : " + now.getMinutes() + " : " +                 now.getSeconds());
	        Thread.sleep((int)(Math.random()*1000));
}catch (InterruptedException e) {
     Thread.interrupted();
    }
}
}

}

