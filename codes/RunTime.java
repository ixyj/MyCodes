import java.io.IOException;

public class RunTime
{
	public static void main(String[] args) 
	{
		Runtime rt = Runtime.getRuntime();
		System.out.println("有效处理器："+rt.availableProcessors());
		System.out.println("有效内存："+rt.freeMemory());
		System.out.println("全部内存："+rt.totalMemory());
		System.out.println("回收垃圾……");
		rt.gc();
		System.gc();
		
		try
		{
			System.out.println("打开word……");
			rt.exec("cmd /c start winword");//前台运行
			rt.exec("systeminfo");//后台运行
			rt.exec("cmd /c start D:/theworld_3.0.9.5/TheWorld");
			
		}
		catch (IOException e)
		{
			System.out.println(e);
		}
	}
	
}
