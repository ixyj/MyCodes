import java.awt.*;
import java.awt.event.*;

public class GUIExample 
{
	public static void main(String[] args)
	{
		try
		{
			Calc calc_obj = new Calc();
		}
		catch(HeadlessException e)
		{
			System.out.println("throw:"+e);
		}
		catch(IllegalArgumentException e)
		{
			System.out.println("throw:"+e);
		}
	}

}

class Calc implements WindowListener,ActionListener,ItemListener,MouseMotionListener
{
	private Frame frm;
	private Panel pnl;
	
	private TextField txtFirst;
	private TextField txtSecond;
	private TextField txtResult;
	private Choice chc;
	private Button btn_calc;
	private Dialog errDlg;
	private Button dlg_btn;
	private Button btn_viewCircle;
	private TextField dlg_txt_err;
	private TextField mousePos;
	
	public Calc()
	{
		frm = new Frame("简单计算器");
		pnl = new Panel();
		txtFirst = new TextField("0",8);
		txtSecond = new TextField("0",8);
		txtResult = new TextField("0",8);
		chc = new Choice();
		btn_calc = new Button("=");
		errDlg = new Dialog(frm, "错误");
		dlg_btn = new Button("确定");
		btn_viewCircle= new Button("移动圆形");
		dlg_txt_err = new TextField("",8);
		mousePos = new TextField("",20);
		
		frm.setLayout(null);
		frm.setBounds(40,30,250,210);
		frm.setResizable(false);
		
		errDlg.setBackground(new Color(255,0,0));
		errDlg.setBounds(40,50,250,150);
		dlg_txt_err.setBounds(50,60,80,20);
		dlg_btn.setBounds(100,100,40,20);
		
		pnl.setLayout(new FlowLayout(20,15,java.awt.FlowLayout.LEFT));
		pnl.setBounds(25,45,200,150);
		pnl.setBackground(new Color(222,22,222));
		
		chc.add("＋");
		chc.add("－");
		chc.add("×");
		chc.add("÷");
		chc.select(0);
		
		errDlg.add(dlg_btn);
		errDlg.add(dlg_txt_err);
		
		txtResult.setEditable(false);
		dlg_txt_err.setEditable(false);
		mousePos.setEditable(false);
		
		frm.setVisible(true);
		
		frm.add(pnl);
		
		pnl.add(txtFirst);
		pnl.add(chc);
		pnl.add(txtSecond);
		pnl.add(btn_calc);
		pnl.add(txtResult);
		pnl.add(mousePos);
		pnl.add(btn_viewCircle);
		
		frm.addWindowListener(this);
		chc.addItemListener(this);
		btn_calc.addActionListener(this);
		errDlg.addWindowListener(this);
		dlg_btn.addActionListener(this);
		btn_viewCircle.addActionListener(this);
		frm.addMouseMotionListener(this);
	}
	
	
	
	public void windowActivated(WindowEvent e)
	{
	}
	public void windowClosed(WindowEvent e)
	{
	}
	public void windowClosing(WindowEvent e)
	{
		System.exit(0);
	}
	public void windowDeactivated(WindowEvent e)
	{
	}
	public void windowDeiconified(WindowEvent e)
	{
	}
	public void windowIconified(WindowEvent e)
	{
	}
	public void windowOpened(WindowEvent e)
	{
	}
	
	public void mouseMoved(MouseEvent e)
	{
		mousePos.setText("当前鼠标位置(X:"+e.getX()+",Y:"+e.getY()+")");
	}
	public void mouseDragged(MouseEvent e)
	{
		mousePos.setText("当前鼠标位置(X:"+e.getX()+",Y:"+e.getY()+")  拖拽中……");
	}
	
	public void actionPerformed(ActionEvent e)
	{
		Button btn = (Button) e.getSource();
		if (btn == btn_calc)
			PanelEqual();
		else if (btn == dlg_btn)
			errDlg.dispose();
		else if (btn == btn_viewCircle);
		MoveCircle mc = new MoveCircle();
	}

	public void itemStateChanged(ItemEvent e)
	{
	}
	
	
	private void PanelEqual()
	{
		double first = Double.parseDouble(txtFirst.getText());
		double second = Double.parseDouble(txtSecond.getText());
		double result = 0;
		
		switch (chc.getSelectedIndex())
		{
		case 0: result = first + second; break;
		case 1: result = first - second; break;
		case 2: result = first * second; break;
		case 3:
			{
				if (second < 1e-4 && second > -1e-4)
					DlgBtn();
				result = first / second;
			}
			 break;
		default: System.out.println("sign error!"); break;
		}
		
		txtResult.setText(Double.toString(result));
	}
	
	private void DlgBtn()
	{
		dlg_txt_err.setText("除数不能为零！");
		errDlg.setVisible(true);
	}
}

class MoveCircle extends Frame implements MouseMotionListener,MouseListener,WindowListener
{
	private int x,y,dx,dy;
	public MoveCircle()
	{
		setTitle("显示圆形");
		setBounds(100,60,450,350);
		setBackground(new Color(240,240,240));
		setLayout(null);
		
		x = 150;
		y = 120;
		dx=0;
		dy=0;
		
		addMouseListener(this);
		addMouseMotionListener(this);
		addWindowListener(this);
		
		setVisible(true);
	//	setResizable(false);
		
	}
	
	public void paint(Graphics g)
	{
		g.setColor(Color.pink);
		g.fillOval(x, y, 30, 30);
	}
	
	public void mouseMoved(MouseEvent e)
	{
	}
	public void mouseDragged(MouseEvent e)
	{
		if (Math.abs(dx)<50
			&& Math.abs(dy)<50)
		{
			x=e.getX()-dx;
			y=e.getY()-dy;
			update(getGraphics());
			setTitle("移动圆形  正在移动……");
		}
	}
	public void mousePressed(MouseEvent e)
	{
		dx=e.getX()-x;
		dy=e.getY()-y;
	}
	public void mouseClicked(MouseEvent e)
	{
	}
	public void mouseReleased(MouseEvent e)
	{
		setTitle("显示圆形");
	}
	public void mouseEntered(MouseEvent e)
	{
	}
	public void mouseExited(MouseEvent e)
	{
	}
	
	public void windowActivated(WindowEvent e)
	{
	}
	public void windowClosed(WindowEvent e)
	{
	}
	public void windowClosing(WindowEvent e)
	{
		myDialog md = new myDialog(this,"退出移动圆形","您确定退出吗？？？");
	}
	public void windowDeactivated(WindowEvent e)
	{
	}
	public void windowDeiconified(WindowEvent e)
	{
	}
	public void windowIconified(WindowEvent e)
	{
	}
	public void windowOpened(WindowEvent e)
	{
	}
}

 class myDialog extends Dialog implements WindowListener,ActionListener
{
	private TextField txt;
	private Button btn_OK;
	private Button btn_CANCEL;
	
	private String dlg_name;
	private String text;
	private Frame frm_parent;
	
	public myDialog()
	{
		this((Frame)null, "对话框", "消息区");
	}
	public myDialog(Frame frm_parent,String dlg_name, String text)
	{
		super(frm_parent,dlg_name);
		btn_OK = new Button("确定");
		btn_CANCEL = new Button("取消");
		txt = new TextField(text,text.length());
		this.frm_parent = frm_parent;
		this.dlg_name = dlg_name;
		this.text = text;
		
		setBounds(80,60,160,120);
		btn_OK.setBounds(20,80,50,25);
		btn_CANCEL.setBounds(85,80,50,25);
		txt.setBounds(10,30,140,30);
		txt.setEditable(false);
		
		setLayout(null);
		setResizable(false);
		add(btn_OK);
		add(btn_CANCEL);
		add(txt);
		
		setVisible(true);
		
		addWindowListener(this);
		btn_OK.addActionListener(this);
		btn_CANCEL.addActionListener(this);
	}
	
	public void SetFrameName(String name)
	{
		 dlg_name = name;
	}
	
	public void SetFrameText(String txt)
	{
		text = txt;
	}
	
	public void windowActivated(WindowEvent e)
	{
	}
	public void windowClosed(WindowEvent e)
	{
	}
	public void windowClosing(WindowEvent e)
	{
		dispose();
	}
	public void windowDeactivated(WindowEvent e)
	{
	}
	public void windowDeiconified(WindowEvent e)
	{
	}
	public void windowIconified(WindowEvent e)
	{
	}
	public void windowOpened(WindowEvent e)
	{
	}
	
	public void actionPerformed(ActionEvent e)
	{
		Button btn = (Button) e.getSource();
		
		if (btn == btn_OK && frm_parent != null)
		{
			frm_parent.dispose();
		}
		dispose();
	}
}
 