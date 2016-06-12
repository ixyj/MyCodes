import java.util.*;
public class t
{
	public static void main(String[] args) throws Exception
	{
		Runner r1 = new Runner("first");
                Runner r2 = new Runner("second");
                Runner r3 = new Runner("third");
r1.start();
r2.start();
r3.start();
	}
}

class Runner extends Thread
{ String s;
public Runner(String a)
{
s=a;
}
	public void run()
	{
try{
for(int i=0;i<50;i++){
		Date now = new Date();
		System.out.println(s+":\tThe thread time is- " + now.getHours() + " : " + now.getMinutes() + " : " + now.getSeconds());
	sleep((int)(Math.random()*1000));
}
}
catch(InterruptedException e){}
}
}
