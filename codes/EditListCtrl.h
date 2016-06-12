#pragma once

#include "afxwin.h"

#define MLSM_ITEMCHANGED (WM_USER + 200)

class CEditCtrl;

//Sources:
//Alignment:Top
//View:Report

class CEditListCtrl : public CListCtrl
{
public:
	CEditListCtrl(void);
	virtual ~CEditListCtrl(void);

public:
	void InitCtrl(void);   //before use Ctrl, this must be inited
	inline int GetColumnCount(void) const;

	using CListCtrl::InsertColumn;

	//messages
protected:
	DECLARE_MESSAGE_MAP()
	afx_msg void OnLButtonDblClk(UINT nFlags, CPoint point);
	afx_msg void OnLButtonDown(UINT nFlags, CPoint point);
	virtual BOOL PreTranslateMessage(MSG* pMsg);

private:
	CEditCtrl* m_pEditItem;
	int m_row;
	int m_col;
};


//CEditCtrl
class CEditCtrl : public CEdit
{
public:
	CEditCtrl(CEditListCtrl* pWnd);
	virtual ~CEditCtrl(void);

	inline void SetLoc(int row, int col)
	{
		m_row = row;
		m_col = col;
	}

	//messages
protected:
	DECLARE_MESSAGE_MAP()
	afx_msg void OnKillFocus(CWnd* pNewWnd);

private:
	CEditListCtrl* pParentWnd;
	int m_row;
	int m_col;
};