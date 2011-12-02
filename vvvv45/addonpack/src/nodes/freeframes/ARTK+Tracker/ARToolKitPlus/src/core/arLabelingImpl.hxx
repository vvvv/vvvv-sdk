/* ========================================================================
 * PROJECT: ARToolKitPlus
 * ========================================================================
 * This work is based on the original ARToolKit developed by
 *   Hirokazu Kato
 *   Mark Billinghurst
 *   HITLab, University of Washington, Seattle
 * http://www.hitl.washington.edu/artoolkit/
 *
 * Copyright of the derived and new portions of this work
 *     (C) 2006 Graz University of Technology
 *
 * This framework is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This framework is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this framework; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 *
 * For further information please contact 
 *   Dieter Schmalstieg
 *   <schmalstieg@icg.tu-graz.ac.at>
 *   Graz University of Technology, 
 *   Institut for Computer Graphics and Vision,
 *   Inffeldgasse 16a, 8010 Graz, Austria.
 * ========================================================================
 ** @author   Daniel Wagner
 *
 * $Id$
 * @file
 * ======================================================================== */


AR_TEMPL_FUNC ARInt16*
AR_TEMPL_TRACKER::LABEL_FUNC_NAME(ARUint8 *image, int thresh, int *label_num, int **area,
								  ARFloat **pos, int **clip, int **label_ref)
{
    ARUint8   *pnt;                     /*  image pointer       */
    ARInt16   *pnt1, *pnt2;             /*  image pointer       */
    int       *wk;                      /*  pointer for work    */
    int       wk_max;                   /*  work                */
    int       m,n;                      /*  work                */
    int       i,j,k;                    /*  for loop            */
    int       lxsize, lysize;
    int       poff;
    ARInt16   *l_image;
    int       *work, *work2;
    int       *wlabel_num;
    int       *warea;
    int       *wclip;
    ARFloat    *wpos;
#ifndef _DISABLE_TP_OPTIMIZATIONS_
	int		  pnt2_index, wmax_idx;   // [t.pintaric]
#else
	#pragma message(">> Performance Warning: arlabeling() optimizations disabled.")
#endif //_!DISABLE_TP_OPTIMIZATIONS_

	if(pixelFormat==PIXEL_FORMAT_RGB565)
		checkRGB565LUT();


	assert(l_imageL && "checkImageBuffer() must be called before labeling2(). this should happen automatically in arDetectMarker() & arDetectMarkerLite()");

    l_image = &l_imageL[0];
    work    = &workL[0];
    work2   = &work2L[0];
    wlabel_num = &wlabel_numL;
    warea   = &wareaL[0];
    wclip   = &wclipL[0];
    wpos    = &wposL[0];


	if(pixelFormat!=PIXEL_FORMAT_RGB565 && pixelFormat!=PIXEL_FORMAT_LUM)
		thresh *= 3;

    if( arImageProcMode == AR_IMAGE_PROC_IN_HALF ) {
        lxsize = arImXsize / 2;
        lysize = arImYsize / 2;
    }
    else {
        lxsize = arImXsize;
        lysize = arImYsize;
    }

    pnt1 = &l_image[0];
    pnt2 = &l_image[(lysize-1)*lxsize];
    for(i = 0; i < lxsize; i++) {
        *(pnt1++) = *(pnt2++) = 0;
    }

    pnt1 = &l_image[0];
    pnt2 = &l_image[lxsize-1];
    for(i = 0; i < lysize; i++) {
        *pnt1 = *pnt2 = 0;
        pnt1 += lxsize;
        pnt2 += lxsize;
    }

    wk_max = 0;
    pnt2 = &(l_image[lxsize+1]);
    if( arImageProcMode == AR_IMAGE_PROC_IN_HALF ) {
        pnt = &(image[(arImXsize*2+2)*pixelSize]);
        poff = pixelSize*2;
    }
    else {
        pnt = &(image[(arImXsize+1)*pixelSize]);
        poff = pixelSize;
    }


//	int diffCorners = -60,
//		diffLeftRight = -40,
//		difftopBottom = -20;

	const int shiftBits = 10;
	int iHalf=lxsize/2, jHalf=lysize/2;

	int threshFact = (pixelFormat!=PIXEL_FORMAT_RGB565 && pixelFormat!=PIXEL_FORMAT_LUM) ? 3 : 1;

	int corrLeftY = (vignetting.corners*threshFact)<<shiftBits,
		dCorrLeftY = ((vignetting.leftright-vignetting.corners*threshFact)<<shiftBits)/jHalf,
		corrCenterY = (vignetting.bottomtop*threshFact)<<shiftBits,
		dCorrCenterY = -corrCenterY/jHalf,
		corrX = 0, dCorrX = 0,
		corrThresh;


	for(j = 1; j < lysize-1; j++, pnt+=poff*2, pnt2+=2)
	{
		if(vignetting.enabled)
		{
			corrX = corrLeftY;
			dCorrX = (corrCenterY-corrLeftY)/iHalf;

			if(j==jHalf)
			{
				dCorrLeftY = -dCorrLeftY;
				dCorrCenterY = -dCorrCenterY;
			
			}

			corrLeftY += dCorrLeftY;
			corrCenterY += dCorrCenterY;
		}

		for(i = 1; i < lxsize-1; i++, pnt+=poff, pnt2++)
		{
			if(vignetting.enabled)
			{
				if(i==iHalf)
					dCorrX = -dCorrX;
				corrX += dCorrX;

				corrThresh = thresh + (corrX>>shiftBits);
			}
			else
				corrThresh = thresh;

			bool isBlack = false;


#ifdef _DEF_PIXEL_FORMAT_ABGR
	            isBlack = ( *(pnt+1) + *(pnt+2) + *(pnt+3) <= corrThresh );
#endif
#ifdef _DEF_PIXEL_FORMAT_BGR
				isBlack = ( *(pnt+0) + *(pnt+1) + *(pnt+2) <= corrThresh );
#endif
#ifdef _DEF_PIXEL_FORMAT_RGB
				isBlack = ( *(pnt+0) + *(pnt+1) + *(pnt+2) <= corrThresh );
#endif
#ifdef _DEF_PIXEL_FORMAT_RGB565
				isBlack = (getLUM8_from_RGB565(pnt) <= corrThresh );
#endif
#ifdef _DEF_PIXEL_FORMAT_LUM
				isBlack = ( *pnt <= corrThresh );
#endif

			if(isBlack) {
				pnt1 = &(pnt2[-lxsize]);
                if( *pnt1 > 0 ) {
                    *pnt2 = *pnt1;

#ifdef _DISABLE_TP_OPTIMIZATIONS_
					// ORIGINAL CODE
					work2[((*pnt2)-1)*7+0] ++;
                    work2[((*pnt2)-1)*7+1] += i;
                    work2[((*pnt2)-1)*7+2] += j;
                    work2[((*pnt2)-1)*7+6] = j;
#else
					// OPTIMIZED CODE [tp]
					// ((*pnt2)-1)*7 should be treated as constant, since
					//  work2[n] (n=0..xsize*ysize) cannot overwrite (*pnt2)
					pnt2_index = ((*pnt2)-1) * 7;
                    work2[pnt2_index+0]++;
                    work2[pnt2_index+1]+= i;
                    work2[pnt2_index+2]+= j;
                    work2[pnt2_index+6] = j;
					// --------------------------------
#endif //!_DISABLE_TP_OPTIMIZATIONS_

                }
                else if( *(pnt1+1) > 0 ) {
                    if( *(pnt1-1) > 0 ) {
                        m = work[*(pnt1+1)-1];
                        n = work[*(pnt1-1)-1];
                        if( m > n ) {
                            *pnt2 = n;
                            wk = &(work[0]);
                            for(k = 0; k < wk_max; k++) {
                                if( *wk == m ) *wk = n;
                                wk++;
                            }
                        }
                        else if( m < n ) {
                            *pnt2 = m;
                            wk = &(work[0]);
                            for(k = 0; k < wk_max; k++) {
                                if( *wk == n ) *wk = m;
                                wk++;
                            }
                        }
                        else *pnt2 = m;

#ifdef _DISABLE_TP_OPTIMIZATIONS_
						// ORIGINAL CODE
						work2[((*pnt2)-1)*7+0] ++;
                        work2[((*pnt2)-1)*7+1] += i;
                        work2[((*pnt2)-1)*7+2] += j;
                        work2[((*pnt2)-1)*7+6] = j;
#else
						// PERFORMANCE OPTIMIZATION:
						pnt2_index = ((*pnt2)-1) * 7;
						work2[pnt2_index+0]++;
						work2[pnt2_index+1]+= i;
						work2[pnt2_index+2]+= j;
						work2[pnt2_index+6] = j;
#endif //!_DISABLE_TP_OPTIMIZATIONS_

                    }
                    else if( *(pnt2-1) > 0 ) {
                        m = work[*(pnt1+1)-1];
                        n = work[*(pnt2-1)-1];
                        if( m > n ) {
                            *pnt2 = n;
                            wk = &(work[0]);
                            for(k = 0; k < wk_max; k++) {
                                if( *wk == m ) *wk = n;
                                wk++;
                            }
                        }
                        else if( m < n ) {
                            *pnt2 = m;
                            wk = &(work[0]);
                            for(k = 0; k < wk_max; k++) {
                                if( *wk == n ) *wk = m;
                                wk++;
                            }
                        }
                        else *pnt2 = m;

#ifdef _DISABLE_TP_OPTIMIZATIONS_
						// ORIGINAL CODE
                        work2[((*pnt2)-1)*7+0] ++;
                        work2[((*pnt2)-1)*7+1] += i;
                        work2[((*pnt2)-1)*7+2] += j;
#else
						// PERFORMANCE OPTIMIZATION:
						pnt2_index = ((*pnt2)-1) * 7;
						work2[pnt2_index+0]++;
						work2[pnt2_index+1]+= i;
						work2[pnt2_index+2]+= j;
#endif //!_DISABLE_TP_OPTIMIZATIONS_

                    }
                    else {
                        *pnt2 = *(pnt1+1);

#ifdef _DISABLE_TP_OPTIMIZATIONS_
						// ORIGINAL CODE
                        work2[((*pnt2)-1)*7+0] ++;
                        work2[((*pnt2)-1)*7+1] += i;
                        work2[((*pnt2)-1)*7+2] += j;
                        if( work2[((*pnt2)-1)*7+3] > i ) work2[((*pnt2)-1)*7+3] = i;
                        work2[((*pnt2)-1)*7+6] = j;
#else
						// PERFORMANCE OPTIMIZATION:
						pnt2_index = ((*pnt2)-1) * 7;
						work2[pnt2_index+0]++;
						work2[pnt2_index+1]+= i;
						work2[pnt2_index+2]+= j;
                        if( work2[pnt2_index+3] > i ) work2[pnt2_index+3] = i;
						work2[pnt2_index+6] = j;
#endif //!_DISABLE_TP_OPTIMIZATIONS_

                    }
                }
                else if( *(pnt1-1) > 0 ) {
                    *pnt2 = *(pnt1-1);

#ifdef _DISABLE_TP_OPTIMIZATIONS_
						// ORIGINAL CODE
                    work2[((*pnt2)-1)*7+0] ++;
                    work2[((*pnt2)-1)*7+1] += i;
                    work2[((*pnt2)-1)*7+2] += j;
                    if( work2[((*pnt2)-1)*7+4] < i ) work2[((*pnt2)-1)*7+4] = i;
                    work2[((*pnt2)-1)*7+6] = j;
#else
					// PERFORMANCE OPTIMIZATION:
					pnt2_index = ((*pnt2)-1) * 7;
					work2[pnt2_index+0]++;
					work2[pnt2_index+1]+= i;
					work2[pnt2_index+2]+= j;
                    if( work2[pnt2_index+4] < i ) work2[pnt2_index+4] = i;
					work2[pnt2_index+6] = j;
#endif //!_DISABLE_TP_OPTIMIZATIONS_

                }
                else if( *(pnt2-1) > 0) {
                    *pnt2 = *(pnt2-1);

#ifdef _DISABLE_TP_OPTIMIZATIONS_
						// ORIGINAL CODE
                    work2[((*pnt2)-1)*7+0] ++;
                    work2[((*pnt2)-1)*7+1] += i;
                    work2[((*pnt2)-1)*7+2] += j;
                    if( work2[((*pnt2)-1)*7+4] < i ) work2[((*pnt2)-1)*7+4] = i;
#else
					// PERFORMANCE OPTIMIZATION:
					pnt2_index = ((*pnt2)-1) * 7;
					work2[pnt2_index+0]++;
					work2[pnt2_index+1]+= i;
					work2[pnt2_index+2]+= j;
                    if( work2[pnt2_index+4] < i ) work2[pnt2_index+4] = i;
#endif //!_DISABLE_TP_OPTIMIZATIONS_

                }
                else {
                    wk_max++;
                    if( wk_max > WORK_SIZE ) {
                        return(0);
                    }
                    work[wk_max-1] = *pnt2 = wk_max;
#ifdef _DISABLE_TP_OPTIMIZATIONS_
                    work2[(wk_max-1)*7+0] = 1;
                    work2[(wk_max-1)*7+1] = i;
                    work2[(wk_max-1)*7+2] = j;
                    work2[(wk_max-1)*7+3] = i;
                    work2[(wk_max-1)*7+4] = i;
                    work2[(wk_max-1)*7+5] = j;
                    work2[(wk_max-1)*7+6] = j;
#else
					wmax_idx = (wk_max-1)*7;
                    work2[wmax_idx+0] = 1;
                    work2[wmax_idx+1] = i;
                    work2[wmax_idx+2] = j;
                    work2[wmax_idx+3] = i;
                    work2[wmax_idx+4] = i;
                    work2[wmax_idx+5] = j;
                    work2[wmax_idx+6] = j;
#endif //!_DISABLE_TP_OPTIMIZATIONS_
                }
            }
            else {
                *pnt2 = 0;
            }

		}	// end for x
		
		if( arImageProcMode == AR_IMAGE_PROC_IN_HALF ) pnt += arImXsize*pixelSize;

	}	// end for y


    j = 1;
    wk = &(work[0]);
    for(i = 1; i <= wk_max; i++, wk++) {
        *wk = (*wk==i)? j++: work[(*wk)-1];
    }
    *label_num = *wlabel_num = j - 1;
    if( *label_num == 0 ) {
        return( l_image );
    }

    put_zero( (ARUint8 *)warea, *label_num *     sizeof(int) );
    put_zero( (ARUint8 *)wpos,  *label_num * 2 * sizeof(ARFloat) );

#ifdef _DISABLE_TP_OPTIMIZATIONS_
    for(i = 0; i < *label_num; i++) {
        wclip[i*4+0] = lxsize;
        wclip[i*4+1] = 0;
        wclip[i*4+2] = lysize;
        wclip[i*4+3] = 0;
    }
    for(i = 0; i < wk_max; i++) {
        j = work[i] - 1;
        warea[j]    += work2[i*7+0];
        wpos[j*2+0] += work2[i*7+1];
        wpos[j*2+1] += work2[i*7+2];
        if( wclip[j*4+0] > work2[i*7+3] ) wclip[j*4+0] = work2[i*7+3];
        if( wclip[j*4+1] < work2[i*7+4] ) wclip[j*4+1] = work2[i*7+4];
        if( wclip[j*4+2] > work2[i*7+5] ) wclip[j*4+2] = work2[i*7+5];
        if( wclip[j*4+3] < work2[i*7+6] ) wclip[j*4+3] = work2[i*7+6];
    }

    for( i = 0; i < *label_num; i++ ) {
        wpos[i*2+0] /= warea[i];
        wpos[i*2+1] /= warea[i];
    }
#else
	int *wclipRun, *work2Run;
	int iDown;

	wclipRun = wclip;
	iDown = *label_num+1;
	while(--iDown) {
		*wclipRun++ = lxsize;
		*wclipRun++ = 0;
		*wclipRun++ = lysize;
		*wclipRun++ = 0;
    }

	work2Run = work2;
    for(i = 0; i < wk_max; i++) {
        j = work[i] - 1;

        warea[j]    += *work2Run++;
        wpos[j*2+0] += *work2Run++;
        wpos[j*2+1] += *work2Run++;

		wclipRun = wclip+j*4;

		if(*wclipRun > *work2Run)
			*wclipRun = *work2Run;
		wclipRun++;
		work2Run++;

		if(*wclipRun < *work2Run)
			*wclipRun = *work2Run;
		wclipRun++;
		work2Run++;

		if(*wclipRun > *work2Run)
			*wclipRun = *work2Run;
		wclipRun++;
		work2Run++;

		if(*wclipRun < *work2Run)
			*wclipRun = *work2Run;
		wclipRun++;
		work2Run++;
    }


    for( i = 0; i < *label_num; i++ ) {
        wpos[i*2+0] /= warea[i];
        wpos[i*2+1] /= warea[i];
    }
#endif //!_DISABLE_TP_OPTIMIZATIONS_

    *label_ref = work;
    *area      = warea;
    *pos       = wpos;
    *clip      = wclip;
    return( l_image );
}


#undef LABEL_FUNC_NAME
