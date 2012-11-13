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

#pragma warning( disable: 4201 4710 )

#define MIRROR_POINTS

#include <windows.h>
#include <cvstreams.h>
#include <initguid.h>
#include <olectl.h>
#if (1100 > _MSC_VER)
#include <olectlid.h>
#endif
#include "iCalibFilter.h"
#include "CalibFilterprop.h"
#include "CalibFilter.h"
#include "CalibFilteruids.h"
#include <assert.h>
#include "math.h"
#include <stdio.h>
#include "Calib3DWindow.h"
#include "CV.h"

// setup data
const AMOVIESETUP_MEDIATYPE sudPinTypes =
{
    &MEDIATYPE_Video,       // Major type
    &MEDIASUBTYPE_NULL      // Minor type
};

const AMOVIESETUP_PIN psudPins[] =
{
    {
        L"Input",           // String pin name
        FALSE,              // Is it rendered
        FALSE,              // Is it an output
        FALSE,              // Allowed none
        FALSE,              // Allowed many
        &CLSID_NULL,        // Connects to filter
        L"Output",          // Connects to pin
        1,                  // Number of types
        &sudPinTypes },     // The pin details
      { L"Output",          // String pin name
        FALSE,              // Is it rendered
        TRUE,               // Is it an output
        FALSE,              // Allowed none
        FALSE,              // Allowed many
        &CLSID_NULL,        // Connects to filter
        L"Input",           // Connects to pin
        1,                  // Number of types
        &sudPinTypes        // The pin details
    }
};


const AMOVIESETUP_FILTER sudCCalibFilter =
{
    &CLSID_CCalibFilter,        // Filter CLSID
    L"CalibFilter",                    // Filter name
    MERIT_DO_NOT_USE,               // Its merit
    2,                              // Number of pins
    psudPins                        // Pin details
};


// List of class IDs and creator functions for the class factory. This
// provides the link between the OLE entry point in the DLL and an object
// being created. The class factory will call the static CreateInstance

CFactoryTemplate g_Templates[2] = {

    { L"CalibFilter"
    , &CLSID_CCalibFilter
    , CCalibFilter::CreateInstance
    , NULL
    , &sudCCalibFilter }
  ,
    { L"CalibFilter Property Page"
    , &CLSID_CCalibFilterPropertyPage
    , CCalibFilterProperties::CreateInstance }
};
int g_cTemplates = sizeof(g_Templates) / sizeof(g_Templates[0]);

static DWORD WINAPI _3DWindowThreadProc( void* window );

/* Constructor */
CCalibFilter::CCalibFilter(TCHAR *tszName,LPUNKNOWN punk,HRESULT *phr) :
        CTransInPlaceFilter(tszName, punk, CLSID_CCalibFilter,phr),
        CPersistStream(punk, phr)
{
    m_initial_params.etalon_type = CalibEtalon_ChessBoard;
    
    m_initial_params.etalon_params[0] = 6;
    m_initial_params.etalon_params[1] = 8;
    m_initial_params.etalon_params[2] = 3;
    m_initial_params.etalon_params_count = 3;
    
    m_initial_params.show_feature_points_flag = 1;
    m_initial_params.frame_interval = 1000;
    m_initial_params.frames_to_collect = 10;
    m_initial_params.frames_collected = 0;
    m_initial_params.frames_passed = 0;
    m_initial_params.calib_state = CalibState_Initial;
    m_initial_params.last_frame_time = -1e6f;

    m_initial_params.enable_undistortion    = 0;
    m_initial_params.show_3d_window         = 0;

    m_params = m_initial_params;

    m_max_points = 0;
    m_imagePoints = 0;
    m_objectPoints = 0;
    m_transVects = 0;
    m_rotMatrs = 0;

    m_gray_img   = cvCreateImage( cvSize(1,1), IPL_DEPTH_8U, 1 );
    m_thresh_img = cvCreateImage( cvSize(1,1), IPL_DEPTH_8U, 1 );
    m_rgb_img    = cvCreateImage( cvSize(1,1), IPL_DEPTH_8U, 3 );
    m_undist_img = cvCreateImage( cvSize(1,1), IPL_DEPTH_8U, 3 );

    memset( &m_undistort_params, 0, sizeof(m_undistort_params));
    m_undistort_data = 0;

    m_window3D = 0;

    DWORD threadId;
    m_thread = CreateThread( 0, 0, _3DWindowThreadProc, &m_window3D, 0, &threadId );

	m_ShowPopupErrors = true;
	m_ShowEtalon = true;
}

CvSize  CCalibFilter::GetEtalonSize()
{
    return cvSize( cvRound(m_params.etalon_params[0]) - 1,
                      cvRound(m_params.etalon_params[1]) - 1 ); 
}

/* a whole life of 3D window in the single routine */
DWORD WINAPI _3DWindowThreadProc( void* window )
{
    CCalib3DWindow* window3D = new CCalib3DWindow;
    *((CCalib3DWindow**)window) = window3D;
    
    MSG msg;

    // Main message loop:
    while( GetMessage(&msg, NULL, 0, 0)) 
    {
        TranslateMessage(&msg);
        DispatchMessage(&msg);
    }

    *((CCalib3DWindow**)window) = 0;
    delete window3D;
    return 0;
}

CCalibFilter::~CCalibFilter()
{
	if (m_window3D != 0)
		SendMessage( m_window3D->m_hwnd, WM_CLOSE, 0, 0 );
    WaitForSingleObject( m_thread, 100 );
    CloseHandle( m_thread );
}


/* CreateInstance */
CUnknown * WINAPI CCalibFilter::CreateInstance(LPUNKNOWN punk, HRESULT *phr)
{
    CCalibFilter *pNewObject = new CCalibFilter(NAME("CalibFilter"), punk, phr);
    if( !pNewObject ) *phr = E_OUTOFMEMORY;
    return pNewObject;
}

/* NonDelegatingQueryInterface */
STDMETHODIMP CCalibFilter::NonDelegatingQueryInterface(REFIID riid, void **ppv)
{
    CheckPointer(ppv,E_POINTER);

    if( riid == IID_ICalibFilter )
        return GetInterface((ICalibFilter *) this, ppv);
	if( riid == IID_ICalibFilter2 )
        return GetInterface((ICalibFilter2 *) this, ppv);
    else if( riid == IID_ISpecifyPropertyPages )
        return GetInterface((ISpecifyPropertyPages *) this, ppv);
    else if( riid == IID_IPersistStream )
        return GetInterface((IPersistStream *) this, ppv);
    else
        return CTransInPlaceFilter::NonDelegatingQueryInterface(riid, ppv);
}


/* CheckReallocBuffers */
void  CCalibFilter::CheckReallocBuffers( IplImage* rgb_img )
{
    CvSize etalon_size = GetEtalonSize();
    int etalon_points = etalon_size.width*etalon_size.height;

    if( m_gray_img->imageData == 0 ||
        m_gray_img->width != rgb_img->width ||
        m_gray_img->height != rgb_img->height )
    {
        cvReleaseImageData( m_gray_img );
        cvInitImageHeader( m_gray_img, cvSize(rgb_img->width, rgb_img->height),
                            IPL_DEPTH_8U, 1, IPL_ORIGIN_TL, 4 );
        cvCreateImageData( m_gray_img ); 

        cvReleaseImageData( m_thresh_img );
        cvInitImageHeader( m_thresh_img, cvSize(rgb_img->width, rgb_img->height),
                            IPL_DEPTH_8U, 1, IPL_ORIGIN_TL, 4 );
        cvCreateImageData( m_thresh_img );

        cvReleaseImageData( m_undist_img );
        cvInitImageHeader( m_undist_img, cvSize(rgb_img->width, rgb_img->height),
                            IPL_DEPTH_8U, 3, IPL_ORIGIN_TL, 4 );
        
        cvCreateImageData( m_undist_img ); 
    }

    if( etalon_points * m_params.frames_to_collect > m_max_points )
    {
        int  new_max_points = etalon_points * (m_params.frames_to_collect + 1) + 128;
        CvPoint2D32f* imagePoints = (CvPoint2D32f*)malloc( new_max_points * 
                                                           sizeof(CvPoint2D32f));

        memcpy( imagePoints, m_imagePoints, m_max_points * sizeof(CvPoint2D32f));

        free( m_imagePoints );
        free( m_objectPoints );
        free( m_transVects );
        free( m_rotMatrs );

        m_imagePoints  = imagePoints;
        m_objectPoints  = (CvPoint3D32f*)malloc( new_max_points * sizeof(CvPoint3D32f) );
        m_transVects = (CvPoint3D32f*)malloc( new_max_points * sizeof(CvPoint2D32f) );
        m_rotMatrs = (float*)malloc( new_max_points * 9 * sizeof(float));
        m_numsPoints = (int*)calloc( m_params.frames_to_collect,sizeof(int));

        m_max_points = new_max_points;
    }
}


void  CCalibFilter::DrawEtalon( IplImage* rgb_img, CvPoint2D32f* corners,
                                int corner_count, CvSize etalon_size,
                                int draw_ordered )
{
    const int r = 4;
    int i;

    if( corner_count == etalon_size.width * etalon_size.height && draw_ordered )
    {
        int x, y;
        CvPoint prev_pt = { 0, 0};
        const int line_max = 7;
        CvScalar line_colors[line_max];

        line_colors[0] = CV_RGB(255,0,0);
        line_colors[1] = CV_RGB(255,128,0);
        line_colors[2] = CV_RGB(200,200,0);
        line_colors[3] = CV_RGB(0,255,0);
        line_colors[4] = CV_RGB(0,200,200);
        line_colors[5] = CV_RGB(0,0,255);
        line_colors[6] = CV_RGB(255,0,255);

        for( y = 0, i = 0; y < etalon_size.height; y++ )
        {
            CvScalar color = line_colors[y % line_max];
            for( x = 0; x < etalon_size.width; x++, i++ )
            {
                CvPoint pt;
                pt.x = cvRound(corners[i].x);
                pt.y = cvRound(corners[i].y);

                if( i != 0 )
                    cvLine( rgb_img, prev_pt, pt, color, 1, CV_AA, 0 );

                cvLine( rgb_img, cvPoint( pt.x - r, pt.y - r ),
                        cvPoint( pt.x + r, pt.y + r ), color, 1, CV_AA, 0 );
                cvLine( rgb_img, cvPoint( pt.x - r, pt.y + r),
                        cvPoint( pt.x + r, pt.y - r), color, 1, CV_AA, 0 );
                cvCircle( rgb_img, pt, r+1, color, 1, CV_AA, 0 );
                prev_pt = pt;
            }
        }
    }
    else
    {
        CvScalar color = CV_RGB(255,0,0);
        for( i = 0; i < corner_count; i++ )
        {
            CvPoint pt;
            pt.x = cvRound(corners[i].x);
            pt.y = cvRound(corners[i].y);
            cvLine( rgb_img, cvPoint( pt.x - r, pt.y - r ),
                    cvPoint( pt.x + r, pt.y + r ), color, 1, CV_AA, 0 );
            cvLine( rgb_img, cvPoint( pt.x - r, pt.y + r),
                    cvPoint( pt.x + r, pt.y - r), color, 1, CV_AA, 0 );
            cvCircle( rgb_img, pt, r+1, color, 1, CV_AA, 0 );
        }
    }
}


void  CCalibFilter::FillEtalonObjPoints( CvPoint3D32f* obj_points,
                                         CvSize etalon_size,
                                         float square_size )
{
    int x, y, i;

    for( y = 0, i = 0; y < etalon_size.height; y++ )
    {
        for( x = 0; x < etalon_size.width; x++, i++ )
        {
            obj_points[i].x = square_size * x;
            obj_points[i].y = square_size * y;
            obj_points[i].z = 0;
        }
    }
}


#ifdef MIRROR_POINTS
void MirrorPoints( CvPoint2D32f* points, int frames, CvSize etalon_size, CvSize imgSize )
{
    int i, j, k;
    for( i = 0; i < frames; i++ )
    {
        int start = i*etalon_size.width*etalon_size.height;

        for( j = 0; j < etalon_size.height; j++ )
        {
            for( k = 0; k < etalon_size.width/2; k++ )
            {
                CvPoint2D32f temp;
                CV_SWAP( points[start + j*etalon_size.width + k],
                         points[start + (j+1)*etalon_size.width - k - 1], temp );
            }

            for( k = 0; k < etalon_size.width; k++ )
            {
                points[start + j*etalon_size.width + k].y = imgSize.height - 
                    points[start + j*etalon_size.width + k].y;
            }
        }
    }
}
#else
void MirrorPoints( CvPoint2D32f*, int, CvSize )
{
}
#endif

/* ProcessFrame */
void  CCalibFilter::ProcessFrame( IplImage* rgb_img, double frame_time )
{
    bool find_corners = m_initial_params.show_3d_window || m_ShowEtalon || m_params.calib_state == CalibState_CalibrationProcess;
    bool chess_found = false; 
    CvSize etalon_size = GetEtalonSize();
    int   etalon_points = etalon_size.width * etalon_size.height;
    CvSize size;
    cvGetImageRawData(rgb_img, 0, 0, &size);
        CheckReallocBuffers( rgb_img );

    CvPoint2D32f* pt_ptr = m_imagePoints + m_params.frames_collected * etalon_points;

    if( find_corners )
    {  
        /* Begin of find etalon points */
        int count = etalon_points;// + 10;

        cvCvtColor( rgb_img, m_gray_img, CV_BGR2GRAY );

        /*********************************************************/    
        //////////////   FIND CHECKERBOARD CORNERS   //////////////    
        ///////////////////////////////////////////////////////////    

        chess_found = cvFindChessBoardCornerGuesses( m_gray_img, m_thresh_img, 0,
                                                     etalon_size, pt_ptr, &count ) != 0;
        if( count != 0 )
        {
            cvFindCornerSubPix( 
                m_gray_img, pt_ptr, count, cvSize(5,5), cvSize(-1,-1),
                cvTermCriteria( CV_TERMCRIT_ITER|CV_TERMCRIT_EPS, 10, 0.01f ));
        }
		
		if (m_ShowEtalon)
			DrawEtalon( rgb_img, pt_ptr, count, etalon_size, chess_found );
    }

    if( m_params.calib_state == CalibState_Calibrated )
    {
        /* Calibration finished */
        if( m_initial_params.show_3d_window && chess_found )
        {/* We must show 3D etalon and compute extrinsic parameters */
            float  rotVect[3];
            //float  Jacobian[27];

            /* Collect object points */
            FillEtalonObjPoints( m_objectPoints, etalon_size,
                                 m_params.etalon_params[2] );
    
            MirrorPoints( pt_ptr, 1, etalon_size, size );

            cvFindExtrinsicCameraParams( etalon_points,
                                         size,
                                         pt_ptr,
                                         m_objectPoints,
                                         m_camera.focalLength,
                                         (CvPoint2D32f&)m_camera.principalPoint,
                                         m_camera.distortion,
                                         rotVect,
                                         m_camera.transVect );
    
            {
                CvMat rmat = cvMat( 3, 3, CV_32FC1, m_camera.rotMatr );
                CvMat rvec = cvMat( 3, 1, CV_32FC1, rotVect );
                //CvMat jacob = cvMat( 3, 9, CV_32FC1, Jacobian );

                /* Calc rotation matrix by via Rodrigues Transform */
                cvRodrigues( &rmat, &rvec, 0, CV_RODRIGUES_V2M );
            }
            Update3DWindow();
        }

        if( m_initial_params.enable_undistortion )
        {
            /* Apply undistortion */
            if( memcmp( m_camera.matrix, m_undistort_params.matrix, sizeof(m_camera.matrix)) != 0 ||
                memcmp( m_camera.distortion, m_undistort_params.distortion, sizeof(m_camera.distortion)) != 0 )
            {
                memcpy( &m_undistort_params, &m_camera, sizeof(m_camera));
                
                if( !m_undistort_data || m_undistort_data->width != rgb_img->width ||
                    m_undistort_data->height != rgb_img->height )
                {
                    cvReleaseImage( &m_undistort_data );
                }
                m_undistort_data = cvCreateImage( cvSize( rgb_img->width, rgb_img->height ),
                                                  IPL_DEPTH_32S, 3 );
            }

            {
            CvMat a = cvMat( 3, 3, CV_32F, m_undistort_params.matrix );
            CvMat dist = cvMat( 1, 4, CV_32F, m_undistort_params.distortion );
            cvUndistort2( rgb_img, m_undist_img, &a, &dist );
            }
            cvCopyImage(m_undist_img, rgb_img);
        }
    } /* Check if Calibration not finished and the etalon is recognized */

    if( m_params.calib_state == CalibState_CalibrationProcess && chess_found &&
        frame_time >= m_params.last_frame_time + m_params.frame_interval )
    {
        m_params.last_frame_time = frame_time;
        m_params.frames_collected++;

        cvXorS( rgb_img, cvScalarAll( 255 ), rgb_img );

        if( m_params.frames_collected == m_params.frames_to_collect )
        {
            /* all frames are collected. Now will calibrate */
            CalculateCameraParams( size );
            m_params.calib_state = CalibState_Calibrated;

            SetDirty(TRUE);

        }/* End calibration */
    } /* Else point accumulation */
}



void  CCalibFilter::CalculateCameraParams( CvSize size )
{
    int frame;
    CvSize etalon_size = GetEtalonSize();
    int   etalon_points = etalon_size.width * etalon_size.height;
    
    FillEtalonObjPoints( m_objectPoints, etalon_size,
                         m_params.etalon_params[2] );
    
    for( frame = 1; frame < m_params.frames_collected; frame++ )
    {
        memcpy( m_objectPoints + etalon_points*frame, m_objectPoints,
                etalon_points * sizeof(m_objectPoints[0]));
    }

    /* Set etalon points counters */
    for( frame = 0; frame < m_params.frames_collected; frame++ )
    {
        m_numsPoints[frame] = etalon_points;
    }

    MirrorPoints( m_imagePoints, m_params.frames_collected, etalon_size, size );

    /* Calirate camera */
    cvCalibrateCamera( m_params.frames_collected, m_numsPoints,
                       size, m_imagePoints, m_objectPoints,
                       m_camera.distortion, m_camera.matrix,
                       (float*)m_transVects, m_rotMatrs, 0 );

    /* Copy some camera parameters */
    m_camera.focalLength[0] = m_camera.matrix[0];
    m_camera.focalLength[1] = m_camera.matrix[4];

    m_camera.principalPoint[0] = m_camera.matrix[2];
    m_camera.principalPoint[1] = m_camera.matrix[5];
}


double CCalibFilter::GetFrameTime( IMediaSample* pSample )
{
    double result = GetTickCount();
    return result;
}


/* Transform */
HRESULT CCalibFilter::Transform(IMediaSample *pSample)
{
    CalibState  state = m_params.calib_state;
                   
    switch( state )
    {
    case  CalibState_Initial:
        {
        
            CAutoLock cAutoLock(&m_CCalibFilterLock);
            m_params = m_initial_params;
            m_params.calib_state = CalibState_NotCalibrated;
        }
    case  CalibState_NotCalibrated:
    case  CalibState_Calibrated:
    case  CalibState_CalibrationProcess:
        {
            AM_MEDIA_TYPE* pType = &m_pInput->CurrentMediaType();
            VIDEOINFOHEADER *pvi = (VIDEOINFOHEADER *) pType->pbFormat;
            
            uchar*  rgb_data;
            CvSize size = cvSize( pvi->bmiHeader.biWidth, abs(pvi->bmiHeader.biHeight) );
            int  step = (size.width*3 + 3) & -4;
            pSample->GetPointer(&rgb_data);
            assert( pvi->bmiHeader.biBitCount == 24 );

            cvInitImageHeader(  m_rgb_img, size, IPL_DEPTH_8U, 3, IPL_ORIGIN_TL, 4 );
            cvSetImageData( m_rgb_img, rgb_data, step ); 

            ProcessFrame( m_rgb_img, GetFrameTime( pSample ));
            ++m_params.frames_passed;
        }
        break;
    default:
        assert(0);
    }

    return NOERROR;
}


/* CheckInputType */
HRESULT CCalibFilter::CheckInputType(const CMediaType *mtIn)
{
    // Check this is a VIDEOINFO type
    if( *mtIn->FormatType() != FORMAT_VideoInfo )
    {
        return E_INVALIDARG;
    }

    if( !IsEqualGUID(*mtIn->Subtype(), MEDIASUBTYPE_RGB24 ))
    {
        return E_FAIL;
    }
    return NOERROR;

}


/* CheckTransform */
HRESULT CCalibFilter::CheckTransform(const CMediaType *mtIn,const CMediaType *mtOut)
{
    HRESULT hr;
    if (FAILED(hr = CheckInputType(mtIn))) return hr;

    // format must be a VIDEOINFOHEADER
    if (*mtOut->FormatType() != FORMAT_VideoInfo) return E_INVALIDARG;
    
    // formats must be big enough 
    if (mtIn->FormatLength() < sizeof(VIDEOINFOHEADER) ||
    mtOut->FormatLength() < sizeof(VIDEOINFOHEADER))
    return E_INVALIDARG;
    
    VIDEOINFO *pInput = (VIDEOINFO *) mtIn->Format();
    VIDEOINFO *pOutput = (VIDEOINFO *) mtOut->Format();
    
    if( pInput->bmiHeader.biBitCount != 24 )
    {
        return E_FAIL;
    }

    if (memcmp(&pInput->bmiHeader,&pOutput->bmiHeader,sizeof(BITMAPINFOHEADER)) == 0)
        return NOERROR;

    return E_INVALIDARG;
}


/* DecideBufferSize */
HRESULT CCalibFilter::DecideBufferSize(IMemAllocator *pAlloc,ALLOCATOR_PROPERTIES *pProperties)
{
    if( !m_pInput->IsConnected()) return E_UNEXPECTED;

    ASSERT(pAlloc);
    ASSERT(pProperties);
    return NOERROR;
}


/* GetMediaType */
HRESULT CCalibFilter::GetMediaType(int iPosition, CMediaType *pMediaType)
{
    if( !m_pInput->IsConnected()) return E_UNEXPECTED;
    if( iPosition < 0 ) return E_INVALIDARG;

    /* Do we have more items to offer */
    if( iPosition > 0 ) return VFW_S_NO_MORE_ITEMS;

    *pMediaType = m_pInput->CurrentMediaType();
    return NOERROR;

}

/****************************************************************************************\
*                               Interface methods                                        *
\****************************************************************************************/

STDMETHODIMP CCalibFilter::get_EtalonParams(
    CalibEtalonType*   etalon_type,
    float*  etalon_params,
    long*   etalon_params_count)
{
    int params_to_copy = m_initial_params.etalon_params_count;
    *etalon_type = m_initial_params.etalon_type;
    
    if( etalon_params )
    {
        if( params_to_copy > *etalon_params_count )
            params_to_copy = *etalon_params_count;

        memcpy( etalon_params, m_initial_params.etalon_params,
                params_to_copy*sizeof(float) );
        *etalon_params_count = params_to_copy;
    }

    return NOERROR;
}


STDMETHODIMP CCalibFilter::set_EtalonParams(
    CalibEtalonType   etalon_type,
    float*  etalon_params,
    long    etalon_params_count)
{
    CAutoLock cAutoLock(&m_CCalibFilterLock);

    if( etalon_type != CalibEtalon_ChessBoard || 
        etalon_params_count != 3 ) return E_INVALIDARG;

    m_initial_params.etalon_type = etalon_type;
    m_initial_params.etalon_params_count = etalon_params_count;
    memcpy( m_initial_params.etalon_params, etalon_params,
            etalon_params_count*sizeof(float) );

    SetDirty(TRUE);

    return NOERROR;
}

STDMETHODIMP CCalibFilter::get_FrameInterval( long*  count )
{
    *count = m_params.frame_interval;
    return NOERROR;
}

STDMETHODIMP CCalibFilter::set_FrameInterval( long  count )
{
    CAutoLock cAutoLock(&m_CCalibFilterLock);
    if( count < 1 ) count = 1;
    m_initial_params.frame_interval = m_params.frame_interval = count;

    SetDirty(TRUE);

    return NOERROR;
}


STDMETHODIMP CCalibFilter::get_FramesToCollect(
    long*   frames)
{
    *frames = m_params.frames_to_collect;
    return  NOERROR;
}

/************************************/

STDMETHODIMP CCalibFilter::get_EnableUndistortion(
    long*   enable)
{
    *enable = m_params.enable_undistortion;
    return  NOERROR;
}

STDMETHODIMP CCalibFilter::set_EnableUndistortion(
    long    enable)
{
    CAutoLock cAutoLock(&m_CCalibFilterLock);

    m_initial_params.enable_undistortion = m_params.enable_undistortion = enable;

    SetDirty(TRUE);

    return  NOERROR;
}


STDMETHODIMP CCalibFilter::get_Show3DWindow(
    long*   enable)
{
    *enable = m_params.show_3d_window;
    return  NOERROR;
}

STDMETHODIMP CCalibFilter::set_Show3DWindow(
    long    enable)
{
    CAutoLock cAutoLock(&m_CCalibFilterLock);

    m_initial_params.show_3d_window = m_params.show_3d_window = enable;

    m_window3D->Show( enable != 0 );

    SetDirty(TRUE);
    
    return  NOERROR;
}

STDMETHODIMP CCalibFilter::SaveCameraParams()
{
    if( m_params.calib_state == CalibState_Calibrated)
    {
        OPENFILENAME lpofn;
        char fileName[MAX_PATH] = "";
        
        lpofn.lStructSize       = sizeof(OPENFILENAME);
        lpofn.hwndOwner         = 0;
        lpofn.hInstance         = 0;
        lpofn.lpstrFilter       = "Camera parameters (*.txt)\0*.txt\0";
        lpofn.lpstrCustomFilter = 0;
        lpofn.nMaxCustFilter    = 0;
        lpofn.nFilterIndex      = 1;
        lpofn.lpstrFile         = fileName;
        lpofn.nMaxFile          = MAX_PATH;
        lpofn.lpstrFileTitle    = 0;
        lpofn.nMaxFileTitle     = 0;
        lpofn.lpstrInitialDir   = 0;
        lpofn.lpstrTitle        = 0;
        lpofn.Flags             = OFN_OVERWRITEPROMPT;
        lpofn.nFileOffset       = 0;
        lpofn.nFileExtension    = 0;
        lpofn.lpstrDefExt       = "txt";
        lpofn.lCustData         = 0;
        lpofn.lpfnHook          = 0;
        lpofn.lpTemplateName    = 0;
        
        GetSaveFileName(&lpofn);
        if( strlen(fileName) != 0)
        {
            FILE *file = fopen(fileName,"wt");
            if( file != 0)
            {
                int i, j;
                fprintf(file,"Camera Matrix:\n");
                for( i = 0; i < 3; i++ )
                {
                    for( j = 0; j < 3; j++ )
                    {
                        fprintf( file,"M[%d.%d]=%20.7f", i, j, m_camera.matrix[i*3+j]);
                    }
                    fprintf(file,"\n");
                }

                fprintf(file,"\n\nDistortion:\n");
                for( i = 0; i < 4; i++ )
                    fprintf(file,"D[%d]=%f\n", i, m_camera.distortion[i]);
                
                fclose(file);
            }
            else
            {
				if (m_ShowPopupErrors)
					MessageBox(0,"Can't open file","Save camera parameters",MB_OK|MB_ICONERROR);
            }
        }
    }
    else
    {
		if (m_ShowPopupErrors)
			MessageBox(0,"Camera was not calibrated","Save camera parameters",MB_OK);
    }

    return  NOERROR;
}

STDMETHODIMP CCalibFilter::set_ShowPopupErrors(bool show)
{
	m_ShowPopupErrors = show;
	return NOERROR;
}

STDMETHODIMP CCalibFilter::set_ShowEtalon(bool show)
{
	m_ShowEtalon = show;
	return NOERROR;
}

STDMETHODIMP CCalibFilter::LoadFile(char* filename)
{
	char file[MAX_PATH] = "";
	memcpy(&file, filename, MAX_PATH * sizeof(char));
	return OpenFile(file);
}

STDMETHODIMP CCalibFilter::LoadCameraParams()
{
    OPENFILENAME lpofn;
    char fileName[MAX_PATH] = "";
    
    lpofn.lStructSize       = sizeof(OPENFILENAME);
    lpofn.hwndOwner         = 0;
    lpofn.hInstance         = 0;
    lpofn.lpstrFilter       = "Camera parameters (*.txt)\0*.txt\0";
    lpofn.lpstrCustomFilter = 0;
    lpofn.nMaxCustFilter    = 0;
    lpofn.nFilterIndex      = 1;
    lpofn.lpstrFile         = fileName;
    lpofn.nMaxFile          = MAX_PATH;
    lpofn.lpstrFileTitle    = 0;
    lpofn.nMaxFileTitle     = 0;
    lpofn.lpstrInitialDir   = 0;
    lpofn.lpstrTitle        = 0;
    lpofn.Flags             = OFN_FILEMUSTEXIST;
    lpofn.nFileOffset       = 0;
    lpofn.nFileExtension    = 0;
    lpofn.lpstrDefExt       = "txt";
    lpofn.lCustData         = 0;
    lpofn.lpfnHook          = 0;
    lpofn.lpTemplateName    = 0;
    
    GetOpenFileName(&lpofn);

	return OpenFile(fileName);
}
	
HRESULT CCalibFilter::OpenFile(char fileName[MAX_PATH])
{
	#define BUF_SIZE 10000
    char buffer[BUF_SIZE + 100];

    if( strlen(fileName) != 0)
    {
        FILE *file = fopen(fileName,"rb");

        if( file != 0)
        {
            int i, j, k;
            float cameraMatrix[9];
            float distortion[4];

            int sz = fread( buffer, 1, BUF_SIZE, file );
            char* ptr = buffer;
            buffer[sz] = '\0';

            /* read matrix */
            for( k = 0; k < 9; k++ )
            {
                ptr = strstr( ptr, "M[" );
                if( ptr )
                {
                    int s = 0;
                    ptr += 2;
                    if( sscanf( ptr, "%d%*[.,]%d%n", &i, &j, &s ) == 2 && i == k/3 && j == k%3 )
                    {
                        ptr += s;
                        ptr = strstr( ptr, "=" );
                        if( ptr )
                        {
                            s = 0;
                            ptr++;
                            if( sscanf( ptr, "%f%n", cameraMatrix + k, &s ) == 1 )
                            {
                                ptr += s;
                                continue;
                            }
                        }
                    }
                }

                /* else report a bug */
				if (m_ShowPopupErrors)
					MessageBox(0,"Invalid file format","Load camera parameters",MB_OK|MB_ICONERROR);
                return E_FAIL;
            }

            /* read distortion */
            for( k = 0; k < 4; k++ )
            {
                ptr = strstr( ptr, "D[" );
                if( ptr )
                {
                    int s = 0;
                    ptr += 2;
                    if( sscanf( ptr, "%d%n", &i, &s ) == 1 && i == k )
                    {
                        ptr += s;
                        ptr = strstr( ptr, "=" );
                        if( ptr )
                        {
                            s = 0;
                            ptr++;
                            if( sscanf( ptr, "%f%n", distortion + k, &s ) == 1 )
                            {
                                ptr += s;
                                continue;
                            }
                        }
                    }
                }

                /* else report a bug */
				if (m_ShowPopupErrors)
					MessageBox(0,"Invalid file format","Load camera parameters",MB_OK|MB_ICONERROR);
                return E_FAIL;
            }

            memcpy( m_camera.matrix, cameraMatrix, sizeof( cameraMatrix ));
            memcpy( m_camera.distortion, distortion, sizeof( distortion ));

            m_camera.focalLength[0] = m_camera.matrix[0];
            m_camera.focalLength[1] = m_camera.matrix[4];

            m_camera.principalPoint[0] = m_camera.matrix[2];
            m_camera.principalPoint[1] = m_camera.matrix[5];

            m_params.calib_state = CalibState_Calibrated;

            fclose(file);
        }
        else
        {
			if (m_ShowPopupErrors)
              MessageBox(0,"Can't open file","Load camera parameters",MB_OK|MB_ICONERROR);
        }
    }

    SetDirty(TRUE);
}


//////////////////////////////////////
//////////////////////////////////////
//////////////////////////////////////

STDMETHODIMP CCalibFilter::set_FramesToCollect(
    long    frames)
{
    CAutoLock cAutoLock(&m_CCalibFilterLock);

    m_initial_params.frames_to_collect = m_params.frames_to_collect = frames;
    SetDirty(TRUE);
    return  NOERROR;
}


STDMETHODIMP CCalibFilter::StartCalibrate()
{
    CAutoLock cAutoLock(&m_CCalibFilterLock);

    m_params = m_initial_params;
    m_params.calib_state = CalibState_CalibrationProcess;
    m_params.frames_collected = 0;

    return  NOERROR;
}

STDMETHODIMP CCalibFilter::GetCameraParams( CvCameraParams* camera )
{
    if( m_params.calib_state != CalibState_Calibrated ) return E_PENDING;
    
    *camera = m_camera;
    return  NOERROR;
}


STDMETHODIMP CCalibFilter::GetState(
   CalibState*  calib_state,
   long*   frames_collected,
   long*   frames_passed,
   double* last_frame_time)
{
    *calib_state = m_params.calib_state;
    *frames_collected = m_params.frames_collected;
    *frames_passed = m_params.frames_passed;
    *last_frame_time = m_params.last_frame_time;

    return NOERROR;
}


STDMETHODIMP CCalibFilter::GetPages(CAUUID *pPages)
{
    pPages->cElems = 1;
    pPages->pElems = (GUID *) CoTaskMemAlloc(sizeof(GUID));
    if( !pPages->pElems ) return E_OUTOFMEMORY;
    *(pPages->pElems) = CLSID_CCalibFilterPropertyPage;
    return NOERROR;
}

STDMETHODIMP CCalibFilter::Update3DWindow()
{
    CvSize etalon_size = cvSize( (int)m_params.etalon_params[0],
                                      (int)m_params.etalon_params[1] );
    float square_size = m_params.etalon_params[2];

    m_window3D->SetParams( &m_camera, etalon_size, square_size );
    return  NOERROR;
}


// IPersistStream implementation
STDMETHODIMP CCalibFilter::GetClassID( CLSID *pClsID )
{
    return CBaseFilter::GetClassID( pClsID );
}


DWORD CCalibFilter::GetSoftwareVersion()
{
    return 0x11;
}

int CCalibFilter::SizeMax()
{
    return 1024;
}


#define WRITEOUT(var)  hr = pStream->Write(&var, sizeof(var), NULL); \
               if (FAILED(hr)) return hr;

#define READIN(var)    hr = pStream->Read(&var, sizeof(var), NULL); \
               if (FAILED(hr)) return hr;

HRESULT CCalibFilter::ReadFromStream( IStream* pStream )
{
    HRESULT hr;
    READIN( m_initial_params );

    m_params = m_initial_params;

    m_params.calib_state = m_params.calib_state == CalibState_Calibrated ?
                           CalibState_Calibrated : CalibState_NotCalibrated;

    if( m_params.calib_state == CalibState_Calibrated )
    {
        READIN( m_camera );
    }
    return NOERROR;
}

HRESULT CCalibFilter::WriteToStream( IStream* pStream )
{
    HRESULT hr;
    m_initial_params.calib_state = m_params.calib_state;
    WRITEOUT( m_initial_params );

    if( m_params.calib_state == CalibState_Calibrated )
    {
        WRITEOUT( m_camera );
    }

    /*SetDirty(FALSE);*/

    return NOERROR;
}


STDAPI DllRegisterServer()
{
    return AMovieDllRegisterServer2( TRUE );
}

STDAPI DllUnregisterServer()
{
   return AMovieDllRegisterServer2( FALSE );
}

/* End of file. */

