// ResizeControl.h: interface for the CResizeControl class.
//
//////////////////////////////////////////////////////////////////////
//Usage:
//1. Add #include "ResizeControl.h";
//2. add a variable such as "CResizeControl m_Resize";
//3. add a statement into OnInitialUpdate() or OnInitialDlg():
//	m_Resize.SetOwner(this);
//	and then set the control: m_Resize.SetResize(IDC_BUTTON, k_NO_TYPE, k_NO_TYPE, k_NO_TYPE, k_NO_TYPE);
//4.overload the OnSize message, for example,
//	CFormView::OnSize(nType, cx, cy);
//    m_Resize.OnSize(cx,cy);
//////////////////////////////////////////////////////////////////////

#ifndef _RESIZECONTROL_H_INCLUDED_
#define _RESIZECONTROL_H_INCLUDED_


//#include "afxtempl.h" 

#include <vector>

typedef enum _kResizeType
{
	k_NO_TYPE = 0,			//keep still
	k_MOVE_TYPE,			
	k_RATIO_MOVE_TYPE
} kResizeType;

typedef struct _tagResize
{
	_tagResize(UINT id = 0, kResizeType l = k_NO_TYPE, kResizeType r = k_NO_TYPE, kResizeType t = k_NO_TYPE, kResizeType b = k_NO_TYPE)
		: ID(id), typeLeft(l), typeRight(r), typeTop(t), typeBottom(b)
	{
	}

	UINT ID;							//ControlID
	CRect rect;							
	kResizeType typeLeft;				//left ResizeType
	kResizeType typeRight;			//right ResizeType
	kResizeType typeTop;				//top ResizeType
	kResizeType typeBottom;		//bottom ResizeType
} tagResize;



class CResizeControl  
{
//Construction/Destruction
public:
	CResizeControl(CWnd* pWnd = NULL);
	virtual ~CResizeControl(void);


//public methods
public:
	void SetOwner(CWnd* pWnd);
	void SetResize(UINT ID, kResizeType l, kResizeType r, kResizeType t, kResizeType b);	//add Control
	void OnSize(int cx, int cy);
	
//class members
private:
	CWnd* m_pWnd;
	CRect m_rtParent;
	std::vector<tagResize> m_arrResize;	
};

#endif //_RESIZECONTROL_H_INCLUDED_



