// -> cvcamshift_mod.h
#include <opencv/cv.h>

int cvMeanShift_mod( const void* imgProb,
                     CvRect windowIn,
                     CvTermCriteria criteria, CvConnectedComp* comp );

int cvCamShift_mod( const void* imgProb,
                    CvRect windowIn,
                    CvTermCriteria criteria,
                    CvConnectedComp* _comp,
                    CvBox2D* box,
                    int iwidth,
                    int iheight,
                    int * first_round,
                    float* angledamp,
                    float* lastangle,
                    float* angleoffset,
                    float* area,
                    float is_scaled );

