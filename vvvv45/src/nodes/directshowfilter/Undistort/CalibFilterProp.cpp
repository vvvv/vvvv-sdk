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
//M*/

// This class implements the property page for the CCalibFilter filter

#pragma warning( disable: 4201 4710 )

#include <windows.h>
#include <cvstreams.h>
#include <commctrl.h>
#include <olectl.h>
#include "resource.h"
#include "iCalibFilter.h"
#include "CalibFilterprop.h"
#include "CalibFilter.h"
#include "CalibFilteruids.h"
#include <assert.h>
#include "math.h"
#include <stdio.h>
#include "Calib3DWindow.h"

//
// CreateInstance
//
// This goes in the factory template table to create new filter instances
//
const signed char MaxLevel = 127;
const signed char MinLevel = -128;

CUnknown * WINAPI CCalibFilterProperties::CreateInstance(LPUNKNOWN lpunk, HRESULT *phr)
{
    CUnknown *punk = new CCalibFilterProperties(lpunk, phr);
    if (punk == NULL) {
    *phr = E_OUTOFMEMORY;
    }
    return punk;

} // CreateInstance


//
// Constructor
//
CCalibFilterProperties::CCalibFilterProperties(LPUNKNOWN pUnk, HRESULT *phr) :
    CBasePropertyPage(NAME("CalibFilter Property Page"),pUnk,
                      IDD_CCalibFilterPROP,
                      IDS_TITLE),
    m_pCalibFilter(NULL)
{
    InitCommonControls();
} // (Constructor)


//
// SetDirty
//
// Sets m_bDirty and notifies the property page site of the change
//
void CCalibFilterProperties::SetDirty()
{
    m_bDirty = TRUE;
    if (m_pPageSite) m_pPageSite->OnStatusChange(PROPPAGESTATUS_DIRTY);
} // SetDirty


void CCalibFilterProperties::SetControls( HWND  hwnd )
{
    char buffer[100];
    SetDlgItemInt( hwnd, IDC_WIDTH,  (int)m_params.etalon_params[0], 0 );
    SetDlgItemInt( hwnd, IDC_HEIGHT, (int)m_params.etalon_params[1], 0 );
    sprintf(buffer,"%.2f", m_params.etalon_params[2]);
    SetDlgItemText( hwnd, IDC_SQUARE_SIZE, buffer );
    SetDlgItemInt( hwnd, IDC_INTERVAL, m_params.frame_interval, 0 );
    SetDlgItemInt( hwnd, IDC_MAX_FRAMES, m_params.frames_to_collect, 0 );
////////////////////
    Button_SetCheck( GetDlgItem( hwnd, IDC_UNDISTORTION), m_params.enable_undistortion != 0 );
	Button_SetCheck( GetDlgItem( hwnd, IDC_SHOW3D), m_params.show_3d_window != 0);
}


void CCalibFilterProperties::GetControls( HWND  hwnd )
{
    char buffer[100];
    BOOL ok = 0;
    float fval;
    
    unsigned val = GetDlgItemInt( hwnd, IDC_WIDTH, &ok, 0 );
    if( ok ) m_params.etalon_params[0] = (float)val;
    
    val = GetDlgItemInt( hwnd, IDC_HEIGHT, &ok, 0 );
    if( ok ) m_params.etalon_params[1] = (float)val;
    
    val = GetDlgItemInt( hwnd, IDC_INTERVAL, &ok, 0 );
    if( ok ) m_params.frame_interval = val;

    val = GetDlgItemInt( hwnd, IDC_MAX_FRAMES, &ok, 0 );
    if( ok ) m_params.frames_to_collect = val;

    GetDlgItemText( hwnd, IDC_SQUARE_SIZE, buffer, sizeof(buffer));
    if( sscanf(buffer,"%f", &fval ) == 1 )
    {
        m_params.etalon_params[2] = fval;
    }

/////////////////////////////////
    
    val = Button_GetCheck( GetDlgItem( hwnd, IDC_UNDISTORTION) );
    m_params.enable_undistortion = val;

    val = Button_GetCheck( GetDlgItem( hwnd, IDC_SHOW3D) );
    m_params.show_3d_window = val;
}


void CCalibFilterProperties::SetParameters()
{
    if( m_pCalibFilter )
    {
        m_pCalibFilter->set_EtalonParams(
              m_params.etalon_type,
              m_params.etalon_params, 3 );
    
        m_pCalibFilter->set_FrameInterval( m_params.frame_interval );

        m_pCalibFilter->set_FramesToCollect( m_params.frames_to_collect );
///////////
        m_pCalibFilter->set_EnableUndistortion( m_params.enable_undistortion );
        m_pCalibFilter->set_Show3DWindow( m_params.show_3d_window );
//        m_pCalibFilter->set_Show3DEtalon( m_params.show_3d_etalon );

    }
}


void CCalibFilterProperties::GetParameters()
{
    if( m_pCalibFilter )
    {
        long count = sizeof(m_params.etalon_params)/sizeof(float);

        m_pCalibFilter->get_EtalonParams(
             &m_params.etalon_type,
             m_params.etalon_params,
             &count );
        m_params.etalon_params_count = count;
    
        m_pCalibFilter->get_FrameInterval( &m_params.frame_interval );

        m_pCalibFilter->get_FramesToCollect( &m_params.frames_to_collect );
////////        
        m_pCalibFilter->get_EnableUndistortion( &m_params.enable_undistortion );
        m_pCalibFilter->get_Show3DWindow( &m_params.show_3d_window );
    }
}


void CCalibFilterProperties::StartCalibration()
{
    if( m_pCalibFilter )
    {
        if( m_bDirty ) Apply();
        m_pCalibFilter->StartCalibrate();
    }
}
void CCalibFilterProperties::SaveCameraParams()
{
    if( m_pCalibFilter )
    {
        m_pCalibFilter->SaveCameraParams();
    }
}
void CCalibFilterProperties::LoadCameraParams()
{
    if( m_pCalibFilter )
    {
        m_pCalibFilter->LoadCameraParams();
    }
}


void CCalibFilterProperties::PrintStatus()
{
    if( m_pCalibFilter && m_hwnd &&
        m_pCalibFilter->GetState(
             &m_params.calib_state,   &m_params.frames_collected,
             &m_params.frames_passed, &m_params.last_frame_time ) == NOERROR )
    {
        char buffer[1000] = "";

        /* My print status */
        switch( m_params.calib_state )
        {
        case  CalibState_Initial:
        case  CalibState_NotCalibrated:
            sprintf(buffer,"Camera is not calibrated");
            break;
        case  CalibState_Calibrated:
            {
                CvCameraParams camera;
                m_pCalibFilter->GetCameraParams( &camera );

                sprintf(buffer,"Camera is Calibrated\r\n"
                               "principal point: (%5.1f, %5.1f)\r\n"
                               "focal length: (%7.3f x %7.3f)\r\n"
                               "distortion: k1 = %6.3f, k2 = %6.3f\r\n"
                               "\tp1 = %6.3f, p2 = %6.3f",
                               camera.principalPoint[0], camera.principalPoint[1],
                               camera.focalLength[0], camera.focalLength[1],
                               camera.distortion[0], camera.distortion[1],
                               camera.distortion[2], camera.distortion[3] );
            }
            break;
        case  CalibState_CalibrationProcess:
            sprintf(buffer,"Processing... Collected %d frames",m_params.frames_collected);
            break;
        default:
            break;
        }
        SetDlgItemText( m_hwnd, IDC_STATUS, buffer );
    }
}

//
// OnReceiveMessage
//
// Virtual method called by base class with Window messages
//
BOOL CCalibFilterProperties::OnReceiveMessage(HWND hwnd,
                                              UINT uMsg,
                                              WPARAM wParam,
                                              LPARAM lParam)
{
    switch (uMsg)
    {        
        case WM_INITDIALOG:
        {
        m_hwnd = hwnd;
        SetControls( hwnd );
        SetTimer( hwnd, 0, 50, 0 );
        return 1L;
        }

        case WM_VSCROLL:
        {
        return 1L;
        }

        case WM_COMMAND:
        if( HIWORD(wParam) == BN_CLICKED && LOWORD(wParam) == IDC_START_CALIB )
        {
            StartCalibration();
        }
        if( HIWORD(wParam) == BN_CLICKED && LOWORD(wParam) == IDC_SAVECALIBRATION )
        {
            SaveCameraParams();
        }
        if( HIWORD(wParam) == BN_CLICKED && LOWORD(wParam) == IDC_LOADCALIBRATION )
        {
            LoadCameraParams();
        }
        else if( HIWORD(wParam) == BN_CLICKED && LOWORD(wParam) == IDC_STOP )
        {
            //StopCalibration();
        }
        else if( HIWORD(wParam) == EN_CHANGE || HIWORD(wParam) == BN_CLICKED )
        {
            SetDirty();
        }
        return 1L;

        case WM_TIMER:
        {
            PrintStatus();
        }
        return 1L;

        case WM_DESTROY:
        {
            KillTimer( hwnd, 0 );
        }
        return 1L;
    }
    return CBasePropertyPage::OnReceiveMessage(hwnd,uMsg,wParam,lParam);

} // OnReceiveMessage


//
// OnConnect
//
// Called when the property page connects to a filter
//
HRESULT CCalibFilterProperties::OnConnect(IUnknown *pUnknown)
{
    ASSERT(m_pCalibFilter == NULL);

    HRESULT hr = pUnknown->QueryInterface(IID_ICalibFilter, (void **) &m_pCalibFilter);
    if( FAILED(hr) ) return E_NOINTERFACE;

    ASSERT(m_pCalibFilter);

    GetParameters();
    m_bDirty = FALSE;

    return NOERROR;
} // OnConnect


//
// OnDisconnect
//
// Called when we're disconnected from a filter
//
HRESULT CCalibFilterProperties::OnDisconnect()
{
    // Release of Interface after setting the parameters
    if( !m_pCalibFilter ) return E_UNEXPECTED;
    m_pCalibFilter->Release();
    m_pCalibFilter = NULL;
    return NOERROR;
} // OnDisconnect


//
// OnDeactivate
//
// We are being deactivated
//
HRESULT CCalibFilterProperties::OnDeactivate(void)
{
    //MessageBox(0,"OnDeactivate","Info",MB_OK);
    return NOERROR;
} // OnDeactivate


//
// OnApplyChanges
//
// Changes made should be kept. Change the  variable
//
HRESULT CCalibFilterProperties::OnApplyChanges()
{
    GetControls(m_hwnd);
    SetParameters();
    m_bDirty = FALSE;
    return  NOERROR;
} // OnApplyChanges

/* End of file. */

