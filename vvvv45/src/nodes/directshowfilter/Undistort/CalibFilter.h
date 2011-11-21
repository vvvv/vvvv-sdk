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

#pragma once

#include "CV.h"
#include "Calib3DWindow.h"

class CCalibFilter : public CTransInPlaceFilter,
                     public ICalibFilter,
					 public ICalibFilter2,
                     public ISpecifyPropertyPages,
                     public CPersistStream
{

public:
    
    // Constructor
    CCalibFilter( TCHAR *tszName, LPUNKNOWN punk, HRESULT *phr);

    // Destructor
    ~CCalibFilter();

    static CUnknown * WINAPI CreateInstance(LPUNKNOWN punk, HRESULT *phr);

    // Reveals CCalibFilter & ISpecifyPropertyPages
    STDMETHODIMP NonDelegatingQueryInterface(REFIID riid, void ** ppv);

    DECLARE_IUNKNOWN;

    HRESULT Transform(IMediaSample *pSample);
    HRESULT CheckInputType(const CMediaType *mtIn);
    HRESULT GetMediaType(int iPosition, CMediaType *pMediaType);
    HRESULT DecideBufferSize(IMemAllocator *pAlloc,ALLOCATOR_PROPERTIES *pProperties);
    HRESULT CheckTransform(const CMediaType *mtIn,const CMediaType *mtOut);

	// ICCalibFilter2 methods
    STDMETHODIMP LoadFile(
        char*   filename);
	STDMETHODIMP set_ShowPopupErrors(
		bool show);
	STDMETHODIMP set_ShowEtalon(
		bool show);


    // ICCalibFilter methods
    STDMETHODIMP get_EtalonParams(
        CalibEtalonType*   etalon_type,
        float*  etalon_params,
        long*   etalon_param_count);

    STDMETHODIMP set_EtalonParams(
        CalibEtalonType    etalon_type,
        float*  etalon_params,
        long    etalon_param_count);
    
    STDMETHODIMP get_FrameInterval(
        long*   count);

    STDMETHODIMP set_FrameInterval(
        long    count);

    STDMETHODIMP get_FramesToCollect(
        long*   frames);

    STDMETHODIMP set_FramesToCollect(
        long    frames);

    STDMETHODIMP StartCalibrate();

////////////////////////////////

    STDMETHODIMP get_EnableFilter(
        long*   enable);

    STDMETHODIMP set_EnableFilter(
        long    enable);

    STDMETHODIMP get_EnableUndistortion(
        long*   enable);

    STDMETHODIMP set_EnableUndistortion(
        long    enable);

    STDMETHODIMP get_Show3DWindow(
        long*   enable);

    STDMETHODIMP set_Show3DWindow(
        long    enable);

    STDMETHODIMP get_TrackEtalon(
        long*   enable);

    STDMETHODIMP set_TrackEtalon(
        long    enable);

    STDMETHODIMP Update3DWindow();

    STDMETHODIMP SaveCameraParams();

    STDMETHODIMP LoadCameraParams();

////////////////////////////////

    STDMETHODIMP GetCameraParams( CvCameraParams* camera );

    STDMETHODIMP GetState(
       CalibState*   calib_state,
       long*   frames_collected,
       long*   frames_passed,
       double* last_frame_time);

    // ISpecifyPropertyPages method
    STDMETHODIMP GetPages(CAUUID *pPages);

    // IPersistStream implementation
    STDMETHODIMP GetClassID( CLSID *pClsID );
    int          SizeMax();
    DWORD        GetSoftwareVersion();
    //STDMETHODIMP Save(LPSTREAM pStm, BOOL fClearDirty);

    HRESULT      ReadFromStream( IStream* stream );
    HRESULT      WriteToStream( IStream* stream );

private:

    void   CheckReallocBuffers( IplImage* rgb_img );
    void   ProcessFrame( IplImage* rgb_img, double frame_time );
    void   CalculateCameraParams( CvSize img_size );
    double GetFrameTime( IMediaSample* sample );
    void   DrawEtalon( IplImage* rgb_img, CvPoint2D32f* corners,
                       int corner_count, CvSize etalon_size,
                       int draw_ordered );
    void   FillEtalonObjPoints( CvPoint3D32f* obj_points,
                                CvSize etalon_size,
                                float square_size );
	HRESULT OpenFile(char filename[MAX_PATH]);
	
	
	bool  m_ShowPopupErrors;
	bool  m_ShowEtalon;
    CvSize GetEtalonSize();
    
    // Non interface locking critical section
    CCritSec  m_CCalibFilterLock;
    CalibFilterParams  m_params, m_initial_params;

    /* temporary images, used in processing */
    IplImage*         m_gray_img;
    IplImage*         m_thresh_img;
    IplImage*         m_undist_img;

    /* processed frame */
    IplImage*         m_rgb_img;
    
    /* calibration buffers */
    int                m_max_points;
    CvPoint2D32f*      m_imagePoints;
    CvPoint3D32f*      m_objectPoints;
    CvPoint3D32f*      m_transVects;
    float*             m_rotMatrs;
    int*               m_numsPoints;
    HANDLE             m_thread;
    
    /* camera parameters */
    CvCameraParams   m_camera;

    /* 3D OpenGL window */
    CCalib3DWindow*    m_window3D;

    /* undistortion data */
    CvCameraParams   m_undistort_params;
    IplImage*        m_undistort_data;

}; /* CCalibFilter */

/* End of file. */

