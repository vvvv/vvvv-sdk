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
//M*/// Calib3DWindow.cpp: implementation of the CCalib3DWindow class.
//
//////////////////////////////////////////////////////////////////////

//#include "CalibFilterprop.h"
//#include "calibfilterprop.h"
#include "CV.h"
#include "Calib3DWindow.h"
#include <stdio.h >

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

extern HINSTANCE g_hInst;

static LRESULT CALLBACK MyWindowProc( HWND m_hwnd, UINT uMsg, WPARAM wParam, LPARAM lParam )
{
    long data = ::GetWindowLong(m_hwnd,GWL_USERDATA);
    if( data == 0)
    {
        return DefWindowProc( m_hwnd, uMsg, wParam, lParam);
    }
    else
    {
        return ((CCalib3DWindow*)data)->WindowProc( m_hwnd, uMsg, wParam, lParam);
    }
}

//////////////////////////////////////////////////////////////////////
// Construction/Destruction
//////////////////////////////////////////////////////////////////////
static const char* _3DWindowClass = "3DWindow";

CCalib3DWindow::CCalib3DWindow()
{
    /* Create window and show it */
    m_haveParams = false;
    m_alpha = 0;
    m_beta = 0;
    
    m_baseAlpha = 0;
    m_baseBeta = 0;
    m_holdLMouse = 0;
    m_holdRMouse = 0;

    m_baseScale = 0.02f;
    m_scale = 0.0;

    /* Register window class */
    WNDCLASSEX wndclass;

    wndclass.cbSize         = sizeof(WNDCLASSEX);
    wndclass.style          = CS_DBLCLKS;
    wndclass.lpfnWndProc    = MyWindowProc; 
    wndclass.cbClsExtra     = 0;
    wndclass.cbWndExtra     = 0;
    wndclass.hInstance      = g_hInst; 
    wndclass.hIcon          = 0;
    wndclass.hCursor        = 0;
    wndclass.hbrBackground  = 0;
    wndclass.lpszMenuName   = 0;
    wndclass.lpszClassName  = _3DWindowClass; 
    wndclass.hIconSm        = 0; 

    m_hwnd = 0;
    m_hdc = 0;
    m_hglrc = 0;

    RegisterClassEx(&wndclass);

    DWORD dwStyle = WS_BORDER |
                    WS_CAPTION |
                    WS_MAXIMIZEBOX |
                    WS_MINIMIZEBOX |
                    //WS_VISIBLE |
                    WS_THICKFRAME |
                    WS_CLIPCHILDREN |
                    WS_CLIPSIBLINGS;

    m_hwnd = CreateWindowEx(  0,              // extended window style
                            _3DWindowClass, // pointer to registered class name
                            "3D Window",    // pointer to window name
                            dwStyle,        // window style
                            100,            // horizontal position of window
                            100,            // vertical position of window
                            200,            // window width
                            200,            // window height
                            0,              // handle to parent or owner window
                            0,              // handle to menu, or child-window identifier
                            g_hInst,        // handle to application instance
                            0 );            // pointer to window-creation data

    if( m_hwnd == 0 )
    {
        int error = GetLastError();
        char st[100];
        sprintf(st,"Can't create window.\nError = %d",error);
        MessageBox(0,st,"Error",MB_OK);
    }
    else
    {
    /*  Set OpenGL Formats */
        int iPixelType = PFD_TYPE_RGBA;
    
        DWORD dwFlag =  PFD_DOUBLEBUFFER |
                        PFD_SUPPORT_OPENGL |
                        PFD_DRAW_TO_WINDOW;

        PIXELFORMATDESCRIPTOR pfd;
        memset(&pfd,0,sizeof(PIXELFORMATDESCRIPTOR));

        pfd.nSize = sizeof(PIXELFORMATDESCRIPTOR);
        pfd.nVersion = 1;
        pfd.dwFlags = dwFlag;
        pfd.iPixelType = (uchar)iPixelType;

        pfd.cColorBits = 24;
        pfd.cAlphaBits = 32;
        pfd.cAccumBits = 32;
        pfd.cDepthBits = 32;
        pfd.cStencilBits = 32;
        pfd.iLayerType = PFD_MAIN_PLANE;

        m_hdc = GetDC( m_hwnd );

        int nPixelFormat = ChoosePixelFormat( m_hdc, &pfd );

        if (nPixelFormat == 0) {
            MessageBox(0,"Error Choose pixel formar","Info",MB_OK);
        }

        BOOL dResult = SetPixelFormat( m_hdc, nPixelFormat, &pfd );

        if ( !dResult) {
            MessageBox(0,"Error Set pixel formar","Info",MB_OK);
        }
    
        m_hglrc = wglCreateContext( m_hdc );

        if( m_hglrc == 0 ) {
            MessageBox(0,"Error Create wgl Context","Info",MB_OK);
        }

        BOOL err = wglMakeCurrent(m_hdc,m_hglrc);

        if (err==FALSE) {
            MessageBox(0,"Error Make wgl Context","Info",MB_OK);
        }
        /*---------------------*/

        ::SetWindowLong(m_hwnd,GWL_USERDATA,(long)this);
    }
}

CCalib3DWindow::~CCalib3DWindow()
{
    int result, error;
    result = wglMakeCurrent( 0, 0 );
    error = GetLastError();
    result = wglDeleteContext( m_hglrc );
    error = GetLastError();
    result = ReleaseDC( m_hwnd, m_hdc );
    error = GetLastError();
    result = ShowWindow( m_hwnd, SW_HIDE );
    error = GetLastError();
    result = DestroyWindow( m_hwnd );
    error = GetLastError();
    if( !result )
    {
        char st[100];
        sprintf(st,"Can't register class.\nError = %d",error);
        MessageBox(0,st,"Error",MB_OK);
    }
    m_hglrc = 0;
    m_hdc = 0;
    m_hwnd = 0;
}

LRESULT CALLBACK CCalib3DWindow::WindowProc( HWND /*hwnd*/, UINT uMsg,
                                             WPARAM wParam, LPARAM lParam )
{
    int fwKeys = wParam; /* key flags */
    CvPoint pt = cvPoint( LOWORD(lParam), HIWORD(lParam)); /* cursor position */
	//int width  = LOWORD(lParam);
	//int height = HIWORD(lParam);

    switch( uMsg )
    {        
    case WM_PAINT:
        {
            OnPaint();
            return 0;
        }

    case WM_SIZE:
        {
			Resize();
            ///OnPaint();
            return 0;
        }

    case WM_LBUTTONDOWN:
        {
            OnLButtonDown( pt, fwKeys );
            return 0;
        }
    case WM_LBUTTONUP:
        {
            OnLButtonUp( pt, fwKeys );
            return 0;
        }
    case WM_RBUTTONDOWN:
        {
            OnRButtonDown( pt, fwKeys );
            return 0;
        }
    case WM_RBUTTONUP:
        {
            OnRButtonUp( pt, fwKeys );
            return 0;
        }
    case WM_MOUSEMOVE:
        {
            OnMouseMove( pt, fwKeys );
            return 0;
        }
    case WM_CLOSE:
        {
            CloseWindow( m_hwnd );
            PostQuitMessage(0);
            return 0;
        }
    }

    return DefWindowProc( m_hwnd, uMsg, wParam, lParam);
}


void CCalib3DWindow::SetParams( CvCameraParams* camera,
                                CvSize etalonSize, float squareSize )
{
    if( camera )
    {
        m_camera = *camera;
        m_etalonSize = etalonSize;
        m_squareSize = squareSize;
    }
    m_haveParams = camera != 0;
    
    InvalidateRect( m_hwnd, 0, FALSE );
    UpdateWindow( m_hwnd );
}

void CCalib3DWindow::OnPaint()
{
    PAINTSTRUCT ps;

    BeginPaint( m_hwnd, &ps );

    /* 115 180 251*/
    glClearColor(0.45f,0.705f,0.984f,1.0f);
    glEnable(GL_DEPTH_TEST);
    glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);

    /* ========= Begin draw the etalon position ========= */
    if( m_hwnd && m_haveParams && IsWindowVisible( m_hwnd ))
    {
        int   numX = m_etalonSize.width - 1;
        int   numY = m_etalonSize.height - 1;
        
        //float minX,minY,minZ,maxZ;
        float floorHeight = 6;

/*        minX = -20;
        minY = -20;
        minZ = 0;

        maxX = 20;
        maxY = 20;
        maxZ = 80;*/

        float x1,y1,z1;
        float x2,y2,z2;
        float x3,y3,z3;
        float x4,y4,z4;
        float x5,y5,z5;
        float x6,y6,z6;
        float x7,y7,z7;
        float x8,y8,z8;
        float yfloor;

        float Box_W;
        float Box_H;
        float Box_L;

        CvMat  Points = cvMat( numX * numY, 3, CV_MAT32F, 0 );
        CvMat  objPoint = cvMat( 3, 1, CV_MAT32F, 0 );
        CvMat  objPoints = cvMat( numX * numY, 3, CV_MAT32F, 0 );
        CvMat  tmp3 = cvMat( 3, 1, CV_MAT32F, 0 );
        CvMat  Point = cvMat( 3, 1, CV_MAT32F, 0 );
        CvMat  cameraMatr = cvMat( 3, 3, CV_MAT32F, m_camera.rotMatr );
        CvMat  transVect = cvMat( 3, 1, CV_MAT32F, m_camera.transVect );

        int   t,k,index;
        int   etalonNum;

        cvmAlloc( &objPoints );
        cvmAlloc( &Points );
        cvmAlloc( &objPoint );
        cvmAlloc( &tmp3 );
        cvmAlloc( &Point );

        etalonNum = numX * numY;
        /* Fill etalon coordinates */
        for( t = 0; t < numY; t++ )
        {
            for ( k = 0; k < numX; k++)
            {
                index = t * numX + k;
                objPoints.data.fl[index * 3 + 0] = k * m_squareSize;
                objPoints.data.fl[index * 3 + 1] = t * m_squareSize;
                objPoints.data.fl[index * 3 + 2] = 0;
            }
        }
        
        Box_W = 40;
        Box_H = 40;
        Box_L = 80;
        
        x1 =  Box_W / 2;
        y1 = -Box_H / 2;
        z1 =  0;

        x2 = -Box_W / 2;
        y2 = -Box_H / 2;
        z2 =  0;

        x3 =  Box_W / 2;
        y3 =  Box_H / 2;
        z3 =  0;

        x4 = -Box_W / 2;
        y4 =  Box_H / 2;
        z4 =  0;
        
        x5 =  Box_W / 2;
        y5 = -Box_H / 2;
        z5 =  Box_L;
        
        x6 = -Box_W / 2;
        y6 = -Box_H / 2;
        z6 =  Box_L;

        x7 =  Box_W / 2;
        y7 =  Box_H / 2;
        z7 =  Box_L;
        
        x8 = -Box_W / 2;
        y8 =  Box_H / 2;
        z8 =  Box_L;

        floorHeight = Box_H / 6;

        yfloor = - Box_H /2 - floorHeight;

        glMatrixMode(GL_PROJECTION);

        glLoadIdentity();
        glDepthRange(0,1.0);

        glOrtho(    -1,1,
                    -1,1,
                    -10,10);

        glRotatef(m_baseAlpha + m_alpha,0,1,0);
        glRotatef(m_baseBeta + m_beta,1,0,0);
        
        float setScale;
        setScale = m_baseScale + m_scale;
        
        glScalef(   setScale,
                    setScale,
                    setScale);

        glTranslatef(0, 0,- Box_L / 2.0f);

        /* Draw the room */
        
        glBegin(GL_LINE_STRIP);
        glColor3f(1,0,0);
        glVertex3f(x1,y1,z1);
        glVertex3f(x3,y3,z3);
        glVertex3f(x4,y4,z4);
        glVertex3f(x2,y2,z2);
        glVertex3f(x1,y1,z1);
        glVertex3f(x5,y5,z5);
        glVertex3f(x6,y6,z6);
        glVertex3f(x8,y8,z8);
        glVertex3f(x7,y7,z7);
        glVertex3f(x5,y5,z5);
        
        glEnd();

        glBegin(GL_LINES);

        glVertex3f(x3,y3,z3);
        glVertex3f(x7,y7,z7);
        
        glVertex3f(x4,y4,z4);
        glVertex3f(x8,y8,z8);

        glVertex3f(x2,y2,z2);
        glVertex3f(x6,y6,z6);

        glEnd();

        /* Draw floor for the room */

        /*  First draw the lines */
        glBegin(GL_LINES);
        glColor3f(0.0,1.0,0.0);
        glVertex3f(x1,y1,z1);
        glVertex3f(x5,y5,z5);

        glVertex3f(x5,y5,z5);
        glVertex3f(x6,y6,z6);

        glVertex3f(x6,y6,z6);
        glVertex3f(x2,y2,z2);

        glVertex3f(x2,y2,z2);
        glVertex3f(x1,y1,z1);

        glVertex3f(x1,y1,z1);
        glVertex3f(x1,yfloor,z1);

        glVertex3f(x1,yfloor,z1);
        glVertex3f(x5,yfloor,z5);

        glVertex3f(x5,yfloor,z5);
        glVertex3f(x6,yfloor,z6);

        glVertex3f(x6,yfloor,z6);
        glVertex3f(x2,yfloor,z2);

        glVertex3f(x2,yfloor,z2);
        glVertex3f(x1,yfloor,z1);
        glVertex3f(x2,y2,z2);
        glVertex3f(x2,yfloor,z2);
        glVertex3f(x6,y6,z6);
        glVertex3f(x6,yfloor,z6);
        glVertex3f(x5,y5,z5);
        glVertex3f(x5,yfloor,z5);
        glEnd();

        /* Draw floor under floor */
        glBegin(GL_QUADS);
        glColor3f(0.0f, 0.6f, 1.1f);

        glVertex3f(x1,yfloor,z1);
        glVertex3f(x5,yfloor,z5);
        glVertex3f(x6,yfloor,z6);
        glVertex3f(x2,yfloor,z2);
        glEnd();

        /* Draw walls under floor */
        glBegin(GL_QUADS);
        glColor3f(0.0f,0.3f,0.5f);

        glVertex3f(x1,y1,z1);
        glVertex3f(x5,y5,z5);
        glVertex3f(x5,yfloor,z5);
        glVertex3f(x1,yfloor,z1);

        glVertex3f(x5,y5,z5);
        glVertex3f(x6,y6,z6);
        glVertex3f(x6,yfloor,z6);
        glVertex3f(x5,yfloor,z5);

        glVertex3f(x2,y2,z2);
        glVertex3f(x6,y6,z6);
        glVertex3f(x6,yfloor,z6);
        glVertex3f(x2,yfloor,z2);

        glVertex3f(x1,y1,z1);
        glVertex3f(x2,y2,z2);
        glVertex3f(x2,yfloor,z2);
        glVertex3f(x1,yfloor,z1);

        glEnd();


        /* Draw the camera */
        float camX0,camX1,camX2,camX3,camX4;
        float camY0,camY1,camY2,camY3,camY4;
        float camZ0,camZ1,camZ2,camZ3,camZ4;

        camX0 =  0.0; camY0 =  0.0; camZ0 = 0.0;
        camX1 = -1.0; camY1 =  1.0; camZ1 = 4.0;
        camX2 =  1.0; camY2 =  1.0; camZ2 = 4.0;
        camX3 =  1.0; camY3 = -1.0; camZ3 = 4.0;
        camX4 = -1.0; camY4 = -1.0; camZ4 = 4.0;
/*
        glBegin(GL_TRIANGLE_FAN);
        glVertex3f(camX0,camY0,camZ0);
        glVertex3f(camX1,camY1,camZ1);
        glVertex3f(camX2,camY2,camZ2);
        glVertex3f(camX3,camY3,camZ3);
        glVertex3f(camX4,camY4,camZ4);
        glEnd();
*/        
        glBegin(GL_LINE_STRIP);
        glColor3f(1.0,0.0,0.0);
        glVertex3f(camX0,camY0,camZ0);
        glVertex3f(camX1,camY1,camZ1);
        glVertex3f(camX2,camY2,camZ2);
        glVertex3f(camX3,camY3,camZ3);
        glVertex3f(camX4,camY4,camZ4);
        glVertex3f(camX1,camY1,camZ1);
        glEnd();
        
        glBegin(GL_LINES);
        glVertex3f(camX0,camY0,camZ0);
        glVertex3f(camX2,camY2,camZ2);
        glVertex3f(camX0,camY0,camZ0);
        glVertex3f(camX3,camY3,camZ3);
        glVertex3f(camX0,camY0,camZ0);
        glVertex3f(camX4,camY4,camZ4);
        glEnd();

        /* draw direct to ground */

        glBegin(GL_LINES);
        glVertex3f(0,0,0);
        glVertex3f(0,-Box_H/2,0);
        glEnd();

        /* Draw flat ground */

        glBegin(GL_QUADS);
        glColor3f(1.0f, 0.6f, 0.1f);

        //glShadeModel(GL_FLAT);

        glVertex3f(x1,y1,z1);
        glVertex3f(x5,y5,z5);
        glVertex3f(x6,y6,z6);
        glVertex3f(x2,y2,z2);
        glEnd();
        
        /* Convert Points */

        for( t = 0; t < etalonNum; t++) {
        
            objPoint.data.fl[0] = objPoints.data.fl[t * 3 + 0];
            objPoint.data.fl[1] = objPoints.data.fl[t * 3 + 1];
            objPoint.data.fl[2] = objPoints.data.fl[t * 3 + 2];

            cvmMul( &cameraMatr, &objPoint, &tmp3 );
            
            cvmAdd( &tmp3, &transVect, &Point );

            // MirrorPoints !!!
            Points.data.fl[t*3+0] = -Point.data.fl[0];
            Points.data.fl[t*3+1] = -Point.data.fl[1];
            Points.data.fl[t*3+2] =  Point.data.fl[2];
        }

        
        /* Draw Object */
        glBegin(GL_QUADS);

        /* Draw black and white quads */
        float   xq[4];//1,xq2,xq3,xq4;
        float   yq[4];//1,yq2,yq3,yq4;
        float   zq[4];//,zq2,zq3,zq4;

        int     inds[4];//1,ind2,ind3,ind4;
        int     curr;

        for(t = 0; t < numY - 1; t++)
        {
            for(k = 0; k < numX - 1; k++)
            {
                inds[0] = t * numX + k;
                inds[1] = inds[0] + 1;
                inds[2] = inds[0] + numX + 1;
                inds[3] = inds[0] + numX;

                for( curr = 0; curr < 4; curr++ )
                {
                    xq[curr] = Points.data.fl[inds[curr] * 3 + 0];
                    yq[curr] = Points.data.fl[inds[curr] * 3 + 1];
                    zq[curr] = Points.data.fl[inds[curr] * 3 + 2];
                }

                if( (t + k & 1) == 1 )
                {
                    glColor3f(0.1f, 0.1f, 0.1f);
                }
                else
                {
                    glColor3f(1.0f, 1.0f, 1.0f);
                }

                for( curr = 0; curr < 4; curr++ )
                {
                    glVertex3f(xq[curr],yq[curr],zq[curr]);
                }
            }
        }

        glEnd();

        cvmFree(&objPoints);

        cvmFree(&Points   );
        cvmFree(&objPoint );
        cvmFree(&tmp3     );
        cvmFree(&Point    );
    }

    glFinish();
    glFlush();
    
    SwapBuffers(m_hdc);

    EndPaint( m_hwnd, &ps );

}/* OnPaint */


void CCalib3DWindow::OnLButtonDown( CvPoint pt, int /*fwKeys*/)
{
    m_oldLPoint = pt;
    SetCapture(m_hwnd);
    
    m_alpha = 0.0;
    m_beta = 0.0;

    m_holdLMouse = true;
    InvalidateRect( m_hwnd, 0, FALSE );
    UpdateWindow( m_hwnd );
}

void CCalib3DWindow::OnLButtonUp( CvPoint /*pt*/, int /*fwKeys*/)
{
    ReleaseCapture();
    m_holdLMouse = false;
    m_baseAlpha += m_alpha;
    m_baseBeta += m_beta;
    m_alpha = 0.0;
    m_beta = 0.0;
    InvalidateRect( m_hwnd, 0, FALSE );
    UpdateWindow( m_hwnd );
}

void CCalib3DWindow::OnRButtonDown( CvPoint pt, int /*fwKeys*/)
{
    m_oldRPoint = pt;
    SetCapture(m_hwnd);

    m_holdRMouse = true;

    m_scale = 0.0;
    InvalidateRect( m_hwnd, 0, FALSE );
    UpdateWindow( m_hwnd );
}

void CCalib3DWindow::OnRButtonUp( CvPoint /*pt*/, int /*fwKeys*/)
{
    ReleaseCapture();
    m_holdRMouse = false;
    
    m_baseScale += m_scale;
    if( m_baseScale <= 0.0) m_baseScale = 0.0;
    m_scale = 0.0;
    InvalidateRect( m_hwnd, 0, FALSE );
    UpdateWindow( m_hwnd );
}

void CCalib3DWindow::OnMouseMove( CvPoint pt, int /*fwKeys*/)
{
    if( m_holdLMouse )
    {
        RECT rect;
        GetClientRect( m_hwnd, &rect );
        float width  = (float)(rect.right  - rect.left);
        float height = (float)(rect.bottom - rect.top);

        float dx = (float)( pt.x - m_oldLPoint.x );
        float dy = (float)( pt.y - m_oldLPoint.y );
        m_alpha = (dx / width)  * 360;
        m_beta = (dy / height) * 360;
    }

    if( m_holdRMouse )
    {
        RECT rect;
        GetClientRect( m_hwnd, &rect );
        float height = (float)(rect.bottom - rect.top);
        float dy = (float)(pt.y - m_oldRPoint.y);
        m_scale = (float)((dy/height)*0.1);
    }
    
    InvalidateRect( m_hwnd, 0, FALSE );
    UpdateWindow( m_hwnd );
}

void CCalib3DWindow::Show( bool show )
{
    ShowWindow( m_hwnd, show ? SW_SHOWNORMAL : SW_HIDE );
}

/* End of file. */

void CCalib3DWindow::Resize()
{
    RECT rect;
    GetClientRect( m_hwnd, &rect );

	glViewport(0,0,rect.right - rect.left,rect.bottom - rect.top);
	OnPaint();
}
