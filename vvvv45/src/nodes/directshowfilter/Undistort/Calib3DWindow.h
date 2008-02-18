/*M///////////////////////////////////////////////////////////////////////////////////////
//
//  IMPORTANT: READ BEFORE DOWNLOADING, COPYING, INSTALLING OR USING.
//
//  By downloading, copying, installing or using the software you agree to this license.
//  If you do not agree to this license, do not download, install,
//  copy or use the software.
//
//
//                        Intel License Agreement
//                For Open Source Computer Vision Library
//
// Copyright (C) 2000, Intel Corporation, all rights reserved.
// Third party copyrights are property of their respective owners.
//
// Redistribution and use in source and binary forms, with or without modification,
// are permitted provided that the following conditions are met:
//
//   * Redistribution's of source code must retain the above copyright notice,
//     this list of conditions and the following disclaimer.
//
//   * Redistribution's in binary form must reproduce the above copyright notice,
//     this list of conditions and the following disclaimer in the documentation
//     and/or other materials provided with the distribution.
//
//   * The name of Intel Corporation may not be used to endorse or promote products
//     derived from this software without specific prior written permission.
//
// This software is provided by the copyright holders and contributors "as is" and
// any express or implied warranties, including, but not limited to, the implied
// warranties of merchantability and fitness for a particular purpose are disclaimed.
// In no event shall the Intel Corporation or contributors be liable for any direct,
// indirect, incidental, special, exemplary, or consequential damages
// (including, but not limited to, procurement of substitute goods or services;
// loss of use, data, or profits; or business interruption) however caused
// and on any theory of liability, whether in contract, strict liability,
// or tort (including negligence or otherwise) arising in any way out of
// the use of this software, even if advised of the possibility of such damage.
//
//M*/// Calib3DWindow.h: interface for the CCalib3DWindow class.
//
//////////////////////////////////////////////////////////////////////

#if !defined(AFX_CALIB3DWINDOW_H__A1A51417_DFE4_11D3_B5AD_005056CBF694__INCLUDED_)
#define AFX_CALIB3DWINDOW_H__A1A51417_DFE4_11D3_B5AD_005056CBF694__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000
#include <windows.h>

#include <gl\gl.h>
#include <gl\glu.h>
#include <gl\glaux.h>

#include "iCalibFilter.h"

class CCalib3DWindow  
{
public:
	void Resize();
	void Show( bool show );
	void OnPaint();
    
    void OnLButtonDown ( CvPoint pt, int fwKeys);
    void OnLButtonUp   ( CvPoint pt, int fwKeys);
    void OnRButtonDown ( CvPoint pt, int fwKeys);
    void OnRButtonUp   ( CvPoint pt, int fwKeys);
    void OnMouseMove   ( CvPoint pt, int fwKeys);

    int m_holdLMouse;
    int m_holdRMouse;

    CvPoint m_oldLPoint, m_oldRPoint;

    float m_baseScale, m_scale;
    
	void CCalib3DWindow::SetParams( CvCameraParams* camera,
                                    CvSize etalonSize,
                                    float squareSize );

	CCalib3DWindow();
	~CCalib3DWindow();
    
    LRESULT CALLBACK WindowProc( HWND hwnd,// handle to window
                                 UINT uMsg,// message identifier
                                 WPARAM wParam,  // first message parameter
                                 LPARAM lParam); // second message parameter
    HGLRC  m_hglrc;
    HDC    m_hdc;

    CvCameraParams  m_camera;
    CvSize m_etalonSize;
    float  m_squareSize;

    bool   m_haveParams;
  	HWND   m_hwnd;
    
    /* OpenGL View parameters */
    float  m_alpha;
    float  m_beta;
    float  m_baseAlpha;
    float  m_baseBeta;
    float  m_setScale;
};

#endif // !defined(AFX_CALIB3DWINDOW_H__A1A51417_DFE4_11D3_B5AD_005056CBF694__INCLUDED_)
