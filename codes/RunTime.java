import java.io.IOException;

public class RunTime
{
	public static void main(String[] args) 
	{
		Runtime rt = Runtime.getRuntime();
		System.out.println("��Ч��������"+rt.availableProcessors());
		System.out.println("��Ч�ڴ棺"+rt.freeMemory());
		System.out.println("ȫ���ڴ棺"+rt.totalMemory());
		System.out.println("������������");
		rt.gc();
		System.gc();
		
		try
		{
			System.out.println("��word����");
			rt.exec("cmd /c start winword");//ǰ̨����
			rt.exec("systeminfo");//��̨����
			rt.exec("cmd /c start D:/theworld_3.0.9.5/TheWorld");
			
		}
		catch (IOException e)
		{
			System.out.println(e);
		}
	}
	
}
