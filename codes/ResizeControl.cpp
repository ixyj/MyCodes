// ResizeControl.cpp: implementation of the CResizeControl class.
//
//////////////////////////////////////////////////////////////////////

#pragma once


#include "stdafx.h"
#include "ResizeControl.h"

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[]=__FILE__;
#define new DEBUG_NEW
#endif


//
//////////////////////////////////////////////////////////////////////
CResizeControl::CResizeControl(CWnd* pWnd)
{
	if (pWnd != NULL)
	{
		m_pWnd = pWnd;
		pWnd->GetClientRect(&m_rtParent);
	}
}

CResizeControl::~CResizeControl(void)
{
}

void CResizeControl::SetOwner(CWnd* pWnd)
{
	ASSERT(pWnd != NULL);

	m_pWnd = pWnd;
	pWnd->GetClientRect(&m_rtParent);
}

void CResizeControl::OnSize(int cx, int cy)
{	
	int offsetX = cx - m_rtParent.right - m_rtParent.left;
	int offsetY = cy - m_rtParent.bottom - m_rtParent.top;

	CRect rt;
	for (std::vector<tagResize>::iterator it = m_arrResize.begin(); it != m_arrResize.end(); ++it)
	{
		if (m_pWnd->GetDlgItem((*it).ID)->GetSafeHwnd() == NULL)
			continue;

		rt = (*it).rect;

		switch ((*it).typeLeft)
		{
		case k_MOVE_TYPE:
			rt.left += offsetX;
			break;

		case k_RATIO_MOVE_TYPE:
			rt.left = static_cast<LONG>(((*it).rect.left - m_rtParent.left) * cx / static_cast<double>(m_rtParent.Width()));
			break;

		default:
			break;
		}

		switch ((*it).typeRight)
		{
		case k_MOVE_TYPE:
			rt.right += offsetX;
			break;

		case k_RATIO_MOVE_TYPE:
			rt.right = static_cast<LONG>(((*it).rect.right - m_rtParent.left) * cx / static_cast<double>(m_rtParent.Width()));
			break;

		default:
			break;
		}

		switch ((*it).typeTop)
		{
		case k_MOVE_TYPE:
			rt.top += offsetY;
			break;

		case k_RATIO_MOVE_TYPE:
			rt.top = static_cast<LONG>(((*it).rect.top - m_rtParent.top) * cy / static_cast<double>(m_rtParent.Height()));
			break;

		default:
			break;
		}

		switch ((*it).typeBottom)
		{
		case k_MOVE_TYPE:
			rt.bottom += offsetY;
			break;

		case k_RATIO_MOVE_TYPE:
			rt.bottom = static_cast<LONG>(((*it).rect.bottom - m_rtParent.top) * cy / static_cast<double>(m_rtParent.Height()));
			break;

		default:
			break;
		}


		m_pWnd->GetDlgItem((*it).ID)->MoveWindow(rt);
	}
}

void CResizeControl::SetResize(UINT ID, kResizeType l, kResizeType r, kResizeType t, kResizeType b)	
{
	tagResize resizeControl(ID, l, r, t, b);
	m_pWnd->GetDlgItem(ID)->GetWindowRect(&resizeControl.rect);
	m_pWnd->ScreenToClient(resizeControl.rect);

	m_arrResize.push_back(resizeControl);
}