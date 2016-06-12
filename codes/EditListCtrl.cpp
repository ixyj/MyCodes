#pragma once

#include "StdAfx.h"
#include "EditListctrl.h"

CEditListCtrl::CEditListCtrl(void)
: m_col(-1)
, m_row(-1)
, m_pEditItem(NULL)
{
}

CEditListCtrl::~CEditListCtrl(void)
{
	if (m_pEditItem != NULL && m_pEditItem->m_hWnd != NULL)
		m_pEditItem->DestroyWindow();

	delete m_pEditItem;
}

BEGIN_MESSAGE_MAP(CEditListCtrl, CListCtrl)
	ON_WM_LBUTTONDBLCLK()
	ON_WM_LBUTTONDOWN()
END_MESSAGE_MAP()

void CEditListCtrl::InitCtrl(void)
{
	SetExtendedStyle(GetExtendedStyle() | LVS_EX_GRIDLINES | LVS_EX_FULLROWSELECT | LVS_EX_HEADERDRAGDROP | LVS_EX_TWOCLICKACTIVATE);
	
	if (m_pEditItem == NULL)
		VERIFY(m_pEditItem = new CEditCtrl(this));

	if (m_pEditItem->GetSafeHwnd() == NULL)
	{
		CRect rect(0, 0, 100, 20);
		m_pEditItem->Create(WS_CHILD | ES_LEFT | WS_BORDER | ES_AUTOHSCROLL | ES_WANTRETURN | ES_MULTILINE, rect, this, NULL);
		m_pEditItem->SetFont(this->GetFont(), FALSE);
		m_pEditItem->ShowWindow(SW_HIDE);
	}
}

int CEditListCtrl::GetColumnCount(void) const
{
	return GetHeaderCtrl()->GetItemCount();    
}

void CEditListCtrl::OnLButtonDblClk(UINT nFlags, CPoint point)
{
	// TODO: 在此添加消息处理程序代码和/或调用默认值
	LVHITTESTINFO hi;
	hi.pt = point;

	if (SubItemHitTest(&hi) == -1)
	{
		int nLineCount = GetItemCount();     //总记录条数   
		int nColCount = GetColumnCount();    //列数 
		int pos = point.x ;
		m_col = 0;

		for (int space = 0; m_col < nColCount; m_col++)
		{
			space += GetColumnWidth(m_col);
			if (pos <= space)
				break;
		}

		if (m_col < nColCount)
		{
			m_row = GetItemCount();
			InsertItem(m_row, L"NULL");
			for (int i = 0; i < nColCount; i++)
				SetItemText(m_row, i, L"NULL");
		}
	}
	else
	{
		m_row = hi.iItem;
		m_col = hi.iSubItem;
	}
	
	static CRect rect;
	GetSubItemRect(m_row, m_col, LVIR_BOUNDS, rect);
	m_pEditItem->SetWindowText(this->GetItemText(hi.iItem, hi.iSubItem));
	m_pEditItem->MoveWindow(&rect, TRUE);
	m_pEditItem->ShowWindow(SW_NORMAL);
	m_pEditItem->SetLoc(m_row, m_col);
	m_pEditItem->SetFocus();
	m_pEditItem->SetSel(0, -1);
	
	CListCtrl::OnLButtonDblClk(nFlags, point);
}

void CEditListCtrl::OnLButtonDown(UINT nFlags, CPoint point)
{
	// TODO: 在此添加消息处理程序代码和/或调用默认值
	ASSERT(m_pEditItem != NULL);

	m_pEditItem->ShowWindow(SW_HIDE);
	if (m_row != -1)
	{
		CString ItemText;
		m_pEditItem->GetWindowText(ItemText);
		this->SetItemText(m_row, m_col, ItemText);
		GetParent()->PostMessage(MLSM_ITEMCHANGED, (WPARAM)MAKELONG(m_row, m_col), (LPARAM)this->m_hWnd);
	}
	
	m_col = -1;
	m_row = -1;

	CListCtrl::OnLButtonDown(nFlags, point);
}

BOOL CEditListCtrl::PreTranslateMessage(MSG* pMsg)
{
	// TODO: 在此添加专用代码和/或调用基类
	if (GetFocus() == this && pMsg->message == WM_RBUTTONDOWN)
	{
		SendMessage(WM_LBUTTONDBLCLK, pMsg->wParam, pMsg->lParam);
		return TRUE;
	}
	else if (pMsg->hwnd == m_pEditItem->GetSafeHwnd() && pMsg->message == WM_KEYDOWN && pMsg->wParam == VK_RETURN)
	{
		if(m_row != -1)
		{
			CString ItemText;
			m_pEditItem->GetWindowText(ItemText);
			this->SetItemText(m_row, m_col, ItemText);
			m_pEditItem->ShowWindow(SW_HIDE);
			GetParent()->PostMessage(MLSM_ITEMCHANGED, (WPARAM)MAKELONG(m_row, m_col), (LPARAM)this->m_hWnd);
			
			return TRUE;
		}	
	}
	else if (pMsg->hwnd != m_pEditItem->m_hWnd  && pMsg->message == WM_KEYDOWN)
	{
		if (pMsg->wParam == VK_DELETE || pMsg->wParam == VK_BACK)
		{
			int delRow = GetNextItem(-1, LVNI_SELECTED);
			if (delRow != -1)
				DeleteItem(delRow);
		}

		return TRUE;
	}
		
	return CListCtrl::PreTranslateMessage(pMsg);
}



/////////////////////////////////////////////////////
//CEditCtrl class
CEditCtrl::CEditCtrl(CEditListCtrl* pWnd)
: CEdit()
, pParentWnd(pWnd)
, m_row(-1)
, m_col(-1)
{
}

CEditCtrl::~CEditCtrl(void)
{
}

BEGIN_MESSAGE_MAP(CEditCtrl, CEdit)
	ON_WM_KILLFOCUS()
END_MESSAGE_MAP()

void CEditCtrl::OnKillFocus(CWnd* pNewWnd)
{
	CEdit::OnKillFocus(pNewWnd);

	ShowWindow(SW_HIDE);
	if(m_row != -1)
	{
		CString ItemText;
		GetWindowText(ItemText);
		pParentWnd->SetItemText(m_row, m_col, ItemText);
		pParentWnd->GetParent()->PostMessage(MLSM_ITEMCHANGED, (WPARAM)MAKELONG(m_row, m_col), (LPARAM)this->m_hWnd);
	}

	m_row = -1;
	m_col = -1;
}


