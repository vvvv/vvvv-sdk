//moments_mod.h
#include <opencv/cv.h>

static void icvContourMoments_mod( CvSeq* contour, CvMoments* moments, float width, float height );

CV_IMPL void cvMoments_mod( const void* array, CvMoments* moments, float width, float height, int binary );
