////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

// Additional functions of plugClass

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "ColorTracker.h"
#include <string.h>
#include <stdio.h>
#define max(a,b) ((a)>(b)?(a):(b))

////////////////////////////////////////////////////////////////////////////////////////////////////////

// -> maxNumObs function, gives back maximum spreadcount or 0 //
DWORD plugClass::maxNumObs()
{
    DWORD mNO=0;

    if (sc_reinit==0 || sc_colvals==0 || sc_tolvals==0 || sc_areathresh==0 || sc_filtersize==0) return 0;

    // -> if all spreadsizes are >0, determine maximum spreadcount
    mNO = max(sc_reinit, mNO);
    mNO = max(sc_colvals, mNO);
    mNO = max(sc_tolvals, mNO);
    mNO = max(sc_areathresh, mNO);
    mNO = max(sc_filtersize, mNO);

    return mNO;
}

////////////////////////////////////////////////////////////////////////////////////////////////////////

// -> Buffers realloc function (called when different Spreadsizes occur or Spreadsizes are altered) //
void plugClass::ReallocBuffers()
{
    register DWORD sl, obj, cval, tval; // slice no., obj no., color parameter no., col. tolerance parameter no.

    if (NumObs <= 0)
      return;

    float* temp = (float*) calloc(NumObs, sizeof(float)*NumObs);
    // -> if size of Color or ColorTolerance arrays differs from max spread count,
    //    loop individual tracking color parameter spreads to max spread count //

    // -> realloc reinit choice buffer if necessary
    if (sc_reinit!=NumObs)
    {
        sl=0;
        for (DWORD obj=0; obj<NumObs; obj++)
        {   // -> if end of spread is reached, loop //
            if (sl==sc_reinit)
                sl=0;

            temp[obj]=reinit[sl];
            sl++;
        }
        reinit = (float*) realloc(reinit, sizeof(float)*NumObs);
        for (DWORD obj=0; obj<NumObs; obj++)
            reinit[obj] = temp[obj];
    }

    // -> realloc colvals buffer if necessary
    if (sc_colvals!=NumObs)
       {
        for(cval=0; cval<3; cval++)
           {
            sl=0;
            for (obj=0; obj<NumObs; obj++)
                {// -> if end of spread is reached, loop //
                 if (sl==sc_colvals) sl=0;
                 temp[obj]=colvals[cval][sl];
                 sl++;
                }
            colvals[cval] = (float*) realloc(colvals[cval], sizeof(float)*NumObs);
            for (obj=0; obj<NumObs; obj++) colvals[cval][obj]= temp[obj];
           }
       }

    // -> realloc tolvals buffer if necessary
    if (sc_tolvals!=NumObs)
       {
        for(tval=0; tval<3; tval++)
           {
            sl=0;
            for (obj=0; obj<NumObs; obj++)
                {// -> if end of spread is reached, loop //
                 if (sl==sc_tolvals) sl=0;
                 temp[obj]=tolvals[tval][sl];
                 sl++;
                }
            tolvals[tval] = (float*) realloc(tolvals[tval], sizeof(float)*NumObs);
            for (obj=0; obj<NumObs; obj++) tolvals[tval][obj]= temp[obj];
           }
       }

    // -> realloc areathresh buffer if necessary
    if (sc_areathresh!=NumObs)
       {
        sl=0;
        for (obj=0; obj<NumObs; obj++)
            {// -> if end of spread is reached, loop //
             if (sl==sc_areathresh) sl=0;
             temp[obj]=areathresh[sl];
             sl++;
            }
        areathresh = (float*) realloc(areathresh, sizeof(float)*NumObs);
        for (obj=0; obj<NumObs; obj++) areathresh[obj]= temp[obj];
       }

    // -> realloc filtersize buffer if necessary
    if (sc_filtersize!=NumObs)
       {
        sl=0;
        for (obj=0; obj<NumObs; obj++)
            {// -> if end of spread is reached, loop //
             if (sl==sc_filtersize) sl=0;
             temp[obj]=filtersize[sl];
             sl++;
            }
        filtersize = (float*) realloc(filtersize, sizeof(float) * NumObs);
        for (obj=0; obj<NumObs; obj++) filtersize[obj]= temp[obj];
       }

    // -> Reallocating tracking buffers //
    track_window = (CvRect*) realloc(track_window, sizeof(CvRect) * NumObs);
    track_box    = (CvBox2D*) realloc(track_box, sizeof(CvBox2D) * NumObs);
    for (obj=0; obj<NumObs; obj++) track_window[obj] = selectall;
    track_comp   = (CvConnectedComp*) realloc(track_comp,   sizeof(CvConnectedComp)*NumObs);

    angledamp   = (float*) realloc(angledamp,   sizeof(float) * NumObs);
    lastangle   = (float*) realloc(lastangle,   sizeof(float) * NumObs);
    angleoffset = (float*) realloc(angleoffset, sizeof(float) * NumObs);
    area        = (float*) realloc(area,        sizeof(float) * NumObs);
    is_tracked  = (float*) realloc(is_tracked,  sizeof(float) * NumObs);

    NumObs_old=NumObs;
    dorealloc=0;

    free(temp);
}

////////////////////////////////////////////////////////////////////////////////////////////////////////

// -> Color Conversion Functions //

CvScalar plugClass::hsv2rgb( float hue )
{
    int rgb[3], p, sector;
    static const int sector_data[][3]=
        {{0,2,1}, {1,2,0}, {1,0,2}, {2,0,1}, {2,1,0}, {0,1,2}};
    hue *= 0.033333333333333333333333333333333f;
    sector = cvFloor(hue);
    p = cvRound(255*(hue - sector));
    p ^= sector & 1 ? 255 : 0;

    rgb[sector_data[sector][0]] = 255;
    rgb[sector_data[sector][1]] = 0;
    rgb[sector_data[sector][2]] = p;

    return cvScalar(rgb[2], rgb[1], rgb[0],0);
}


CvScalar plugClass::rgb2hsv(CvScalar rgb)
{
    float r = rgb.val[0]*255.0, g = rgb.val[1]*255.0, b = rgb.val[2]*255.0 ;

    if (r<0) r=0; if (r>255) r=255;
    if (g<0) g=0; if (g>255) g=255;
    if (b<0) b=0; if (b>255) b=255;

    float h, s, v;

    float vmin, diff;

    v = vmin = r;
    if( v < g ) v = g;
    if( v < b ) v = b;
    if( vmin > g ) vmin = g;
    if( vmin > b ) vmin = b;

    diff = v - vmin;
    s = diff/(float)(fabs(v) + FLT_EPSILON) * 255.0;
    diff = (float)(60./(diff + FLT_EPSILON));
    if( v == r )
        h = (g - b)*diff;
    else if( v == g )
        h = (b - r)*diff + 120.f;
    else
        h = (r - g)*diff + 240.f;

    if( h < 0 ) h += 360.f;

    h/=2.0;

    return cvScalar(h, s, v, 0);
}

////////////////////////////////////////////////////////////////////////////////////////////////////////

// -> Image cropping function //

void plugClass::SetImageROI(IplImage* image, int ROIX, int ROIY, int ROIwidth, int ROIheight)
{
    int start_x, end_x, start_y, end_y;
    int offset=0;

    start_y = ROIY-(ROIheight/2);
    if (start_y<0) start_y=0; else if (start_y>image->height) start_y=image->height;
    end_y   = ROIY+(ROIheight/2);
    if (end_y<0) end_y=0;     else if (end_y>image->height) end_y=image->height;

    start_x = (ROIX-(ROIwidth/2))*3;
    if (start_x<0) start_x=0; else if (start_x>image->widthStep) start_x=image->widthStep;
    end_x   = (ROIX+(ROIwidth/2))*3;
    if (end_x<0) end_x=0;     else if (end_x>image->widthStep)   end_x=image->widthStep;


// bottom

    for (register int y=0; y<start_y; y++)
        {
         for (register int x=0; x<image->widthStep; x++)
            {
             image->imageData[offset + x] =0;
            }
         offset+=image->widthStep;
        }

// top

    offset = (image->height) * image->widthStep;
    for (register int y=image->height; y>end_y; y--)
        {offset-=image->widthStep;
         for (register int x=0; x<image->widthStep; x++)
             {
              image->imageData[offset + x] =0;
             }
        }

// right and left

    offset=image->widthStep*start_y;
    for (register int y=start_y; y<end_y; y++)
        {
         for (register int x1=0; x1<start_x; x1++)
             {
              image->imageData[offset + x1] =0;
             }
         for (register int x2=image->widthStep; x2>end_x-1; x2--)
             {
              image->imageData[offset + x2] =0;
             }
         offset+=image->widthStep;
        }
}

////////////////////////////////////////////////////////////////////////////////
