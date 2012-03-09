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

typedef enum _CalibState
{
    CalibState_Initial            = 0,
    CalibState_NotCalibrated      = 1,
    CalibState_Calibrated         = 2,
    CalibState_CalibrationProcess = 3
}
CalibState;

typedef enum _CalibEtalonType
{
    CalibEtalon_ChessBoard = 0
}
CalibEtalonType;

typedef struct _CvCameraParams
{
    float   focalLength[2];
    float   distortion[4];
    float   principalPoint[2];
    float   matrix[9];
    float   rotMatr[9];
    float   transVect[3];
}
CvCameraParams;

typedef struct _CalibFilterParams
{
    CalibEtalonType  etalon_type;
    long        etalon_params_count;
    float       etalon_params[7];
    long        etalon_points;
    long        show_feature_points_flag;
    long        frame_interval;
    long        frames_to_collect;
    long        frames_collected;
    long        frames_passed;
	double      last_frame_time;
    CalibState  calib_state;
    long        enable_undistortion;
    long        show_3d_window;
}
CalibFilterParams;

#ifdef __cplusplus
extern "C" {
#endif
	
	// {41149574-6C26-47ad-9068-E5540655DC3F}
	DEFINE_GUID(IID_ICalibFilter2, 0x41149574, 0x6c26, 0x47ad, 0x90, 0x68, 0xe5, 0x54, 0x6, 0x55, 0xdc, 0x3f);
	DECLARE_INTERFACE_(ICalibFilter2, IUnknown)
	{
		STDMETHOD(LoadFile) (THIS_
            char* filename
        ) PURE;

		STDMETHOD(set_ShowPopupErrors) (THIS_
            bool show
        ) PURE;

		STDMETHOD(set_ShowEtalon) (THIS_
            bool show
        ) PURE;
	};

    // 0x1010045, 0x1d0, 0x6, 0x1, 0x0, 0x0, 0x0, 0x0, 0xe5, 0x12, 0x0  
    DEFINE_GUID(IID_ICalibFilter, 0x1010045, 0x1d0, 0x6, 0x1, 0x0, 0x0, 0x0, 0x0, 0xe5, 0x12, 0x0 );     
    DECLARE_INTERFACE_(ICalibFilter, IUnknown)
    {
        STDMETHOD(get_EtalonParams ) (THIS_
            CalibEtalonType*   etalon_type,
            float*  etalon_params,
            long*   etalon_param_count
        ) PURE;

        STDMETHOD(set_EtalonParams ) (THIS_
            CalibEtalonType    etalon_type,
            float*  etalon_params,
            long    etalon_param_count
        ) PURE;
        
        STDMETHOD(get_FrameInterval) (THIS_
            long*   interval
        ) PURE;

        STDMETHOD(set_FrameInterval) (THIS_
            long    interval
        ) PURE;

        STDMETHOD(get_FramesToCollect) (THIS_
            long*   frames
        ) PURE;

        STDMETHOD(set_FramesToCollect) (THIS_
            long    frames
        ) PURE;

        STDMETHOD(get_EnableUndistortion) (THIS_
            long*   enable
        ) PURE;

        STDMETHOD(set_EnableUndistortion) (THIS_
            long    enable
        ) PURE;

        STDMETHOD(get_Show3DWindow) (THIS_
            long*   enable
        ) PURE;

        STDMETHOD(set_Show3DWindow) (THIS_
            long    enable
        ) PURE;

        STDMETHOD(StartCalibrate)  (THIS
        ) PURE;

        STDMETHOD(SaveCameraParams)  (THIS
        ) PURE;
        
        STDMETHOD(LoadCameraParams)  (THIS
        ) PURE;

        STDMETHOD(Update3DWindow)  (THIS
        ) PURE;

        STDMETHOD(GetCameraParams)  (THIS_
           CvCameraParams*  params
        ) PURE;

        STDMETHOD(GetState) (THIS_
           CalibState*   calib_state,
           long*   frames_collected,
           long*   frames_passed,
           double* last_frame_time
        ) PURE;
    };

#ifdef __cplusplus
}
#endif

