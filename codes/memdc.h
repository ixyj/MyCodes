#ifndef _MEMDC_H_   
#define _MEMDC_H_   
    
//////////////////////////////////////////////////   
// CMemDC - memory DC   
//   
// modified by Xu Yajun 2010.8.20
//   
//   
// This class implements a memory Device Context which allows   
// flicker free drawing.   
//
//Usage:
//一、修改MFC应用程序
//1.为WM_ERASEBKGND消息添加windows消息处理函数，并直接返回TRUE/FALSE；
//2.然后为OnDraw函数做出如下改动: 
//void CExampleView::OnDraw(CDC* dc)   
//{   
//    CMemDC pDC(dc);  
//		……
//}  
//二、修改MFC Active X空间
//在OnDraw函数中做如下改变：
//void CParticleTestCtlCtrl::OnDraw(CDC* pdc, const CRect& rcBounds, const CRect& rcInvalid)   
//{   
//    CMemDC pDC(pdc, &rcBounds);   
//    …… 
//}  
//

    
class CMemDC : public CDC
{
	//constructor
public:
	CMemDC(CDC* pDC, const CRect* pRect = NULL);
	virtual ~CMemDC(void);

	//operator
public:
	CMemDC* operator->(void);
	operator CMemDC*(void);

private:          
	CBitmap m_bitmap;			// Offscreen bitmap
	CBitmap* m_oldBitmap;  // bitmap originally found in CMemDC
    CDC* m_pDC;					// Saves CDC passed in constructor
    CRect m_rect;					// Rectangle of drawing area
    BOOL m_bMemDC;        // TRUE if CDC really is a Memory DC
};

CMemDC::CMemDC(CDC* pDC, const CRect* pRect)
: CDC()
, m_pDC(pDC)
, m_oldBitmap(NULL)
{
	ASSERT(pDC != NULL);   

    //Some initialization   
	m_bMemDC = !pDC->IsPrinting();   

    //Get the rectangle to draw   
	if (pRect == NULL)
	{   
		pDC->GetClipBox(&m_rect);   
    }
	else
	{   
		m_rect = *pRect;   
    }   

    if (m_bMemDC)
	{   
         //Create a Memory DC   
		CreateCompatibleDC(pDC);   
        pDC->LPtoDP(&m_rect);   

		m_bitmap.CreateCompatibleBitmap(pDC, m_rect.Width(), m_rect.Height());   
		m_oldBitmap = SelectObject(&m_bitmap);   

		SetMapMode(pDC->GetMapMode());   

		SetWindowExt(pDC->GetWindowExt());   
		SetViewportExt(pDC->GetViewportExt());   

		pDC->DPtoLP(&m_rect);   
		SetWindowOrg(m_rect.left, m_rect.top);   
	}
	else
	{   
		//Make a copy of the relevent parts of the current DC for printing   
		m_bPrinting = pDC->m_bPrinting;
		m_hDC = pDC->m_hDC;
		m_hAttribDC = pDC->m_hAttribDC;
    }   

    //Fill background    
	FillSolidRect(m_rect, pDC->GetBkColor());   
}   
       
CMemDC::~CMemDC(void)         
{             
	if (m_bMemDC)
	{   
         // Copy the offscreen bitmap onto the screen.   
		m_pDC->BitBlt(m_rect.left, m_rect.top, m_rect.Width(), m_rect.Height(),
			this, m_rect.left, m_rect.top, SRCCOPY); 

         //Swap back the original bitmap.   
		SelectObject(m_oldBitmap);

		 //delete DC & bitmap
		DeleteDC();
		m_bitmap.DeleteObject();	
    }
	else
	{   
         // All we need to do is replace the DC with an illegal value, this keeps us from accidentally deleting the
         // handles associated with the CDC that was passed to the constructor.                 
		m_hDC = NULL;
		m_hAttribDC = NULL;   
    }          
}
       
 //Allow usage as a pointer       
CMemDC* CMemDC::operator->(void)    
{   
	return this;   
}       
    
//Allow usage as a pointer       
CMemDC::operator CMemDC*(void)    
{   
	return this;   
}
    
#endif  // _MEMDC_H_  
