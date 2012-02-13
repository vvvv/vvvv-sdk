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
 * $Id: arMultiActivate.cxx 162 2006-04-19 21:28:10Z grabner $
 * @file
 * ======================================================================== */


#include <stdio.h>
#include <stdlib.h>
#include <math.h>
#include <ARToolKitPlus/Tracker.h>
#include <ARToolKitPlus/matrix.h>
#include <ARToolKitPlus/arMulti.h>


namespace ARToolKitPlus {


AR_TEMPL_FUNC int
AR_TEMPL_TRACKER::arMultiActivate( ARMultiMarkerInfoT *config )
{
    int    i;

    config->prevF = 0;

    for( i = 0; i < config->marker_num; i++ ) {
        arActivatePatt( config->marker[i].patt_id );
    }

    return 0;
}

AR_TEMPL_FUNC int
AR_TEMPL_TRACKER::arMultiDeactivate( ARMultiMarkerInfoT *config )
{
    int    i;

    config->prevF = 0;

    for( i = 0; i < config->marker_num; i++ ) {
        arDeactivatePatt( config->marker[i].patt_id );
    }

    return 0;
}

AR_TEMPL_FUNC int
AR_TEMPL_TRACKER::arMultiFreeConfig( ARMultiMarkerInfoT *config )
{
    int    i;

    for( i = 0; i < config->marker_num; i++ ) {
        arFreePatt( config->marker[i].patt_id );
    }
    free( config->marker );
    free( config );
    config = NULL;

    return 0;
}


}  // namespace ARToolKitPlus
