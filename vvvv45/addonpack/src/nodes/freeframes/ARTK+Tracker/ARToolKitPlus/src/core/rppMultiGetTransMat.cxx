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
 ** @author   Thomas Pintaric
 *
 * $Id: rppMultiGetTransMat.cxx 162 2006-04-19 21:28:10Z grabner $
 * @file
 * ======================================================================== */


#include <stdio.h>
#include <stdlib.h>
#include <math.h>
#include <map>
#include <deque>
#include <ARToolKitPlus/Tracker.h>
#include <ARToolKitPlus/matrix.h>
#include <ARToolKitPlus/extra/rpp.h>


namespace ARToolKitPlus {

AR_TEMPL_FUNC ARFloat
AR_TEMPL_TRACKER::rppMultiGetTransMat(ARMarkerInfo *marker_info, int marker_num, ARMultiMarkerInfoT *config)
{
	rpp_float err = 1e+20;
	rpp_mat R, R_init;
	rpp_vec t;

	std::map<int, int> marker_id_freq;
	for(int i=0; i<marker_num; i++)
	{
		const int m_patt_id = marker_info[i].id;
		if(m_patt_id >= 0)
		{
			std::map<int, int>::iterator iter = marker_id_freq.find(m_patt_id);
			if(iter == marker_id_freq.end()) marker_id_freq.insert(std::make_pair<int,int>(m_patt_id,1));
			else ((*iter).second)++;
		}
	}

	std::deque<std::pair<int,int> > config_patt_id;
	for(int j=0; j<config->marker_num; j++)
		config_patt_id.push_back(std::make_pair<int,int>(j, config->marker[j].patt_id));

	std::map<int, int> m2c_idx;
	for(int m=0; m<marker_num; m++)
	{
		const int m_patt_id = marker_info[m].id;
		bool ignore_marker = (m_patt_id < 0);
		std::map<int, int>::iterator m_iter = marker_id_freq.find(m_patt_id);
		if(m_iter != marker_id_freq.end()) ignore_marker |= ((*m_iter).second > 1);
		if(!ignore_marker)
		{
			std::deque<std::pair<int,int> >::iterator c_iter = config_patt_id.begin();
			if(c_iter != config_patt_id.end()) do
			{
				const int patt_id = (*c_iter).second;
				if(marker_info[m].id == patt_id)
				{
					m2c_idx.insert(std::make_pair<int,int>(m,(*c_iter).first));
					config_patt_id.erase(c_iter);
					c_iter = config_patt_id.end();
					continue;
				}
				else
				{
					c_iter++;
				}
			}
			while(c_iter != config_patt_id.end());
		}
	}

	// ----------------------------------------------------------------------
	const unsigned int n_markers = (unsigned int) m2c_idx.size();
	const unsigned int n_pts = 4*n_markers;

	if(n_markers == 0) return(-1);

	rpp_vec *ppos2d = NULL, *ppos3d = NULL;
	arMalloc( ppos2d, rpp_vec, n_pts);
	arMalloc( ppos3d, rpp_vec, n_pts);
	memset(ppos2d,0,sizeof(rpp_vec)*n_pts);
	memset(ppos3d,0,sizeof(rpp_vec)*n_pts);

	const rpp_float iprts_z =  1;

	int p=0;
	for(std::map<int, int>::iterator iter = m2c_idx.begin();
		iter != m2c_idx.end(); iter++)
	{
		const int m = (*iter).first;
		const int c = (*iter).second;

		const int dir = marker_info[m].dir;
		const int v_idx[4] = {(4-dir)%4, (5-dir)%4, (6-dir)%4, (7-dir)%4};

		for(int i=0; i<4; i++)
			for(int j=0; j<3; j++)
			{
				ppos2d[p+i][j] = (rpp_float) (j==2 ? iprts_z : marker_info[m].vertex[v_idx[i]][j]);
				ppos3d[p+i][j] = (rpp_float) config->marker[c].pos3d[i][j];
			}

		p += 4;
	}

	const rpp_float cc[2] = {arCamera->mat[0][2],arCamera->mat[1][2]};
	const rpp_float fc[2] = {arCamera->mat[0][0],arCamera->mat[1][1]};

	robustPlanarPose(err,R,t,cc,fc,ppos3d,ppos2d,n_pts,R_init,true,0,0,0);

	for(int k=0; k<3; k++)
	{
		config->trans[k][3] = (ARFloat)t[k];
		for(int j=0; j<3; j++)
			config->trans[k][j] = (ARFloat)R[k][j];
	}

	if(ppos2d != NULL) free(ppos2d);
	if(ppos3d != NULL) free(ppos3d);

	if(err > 1e+10) return(-1); // an actual error has occurred in robustPlanarPose()
	return(ARFloat(err)); // NOTE: err is a real number from the interval [0,1e+10]
}

}  // namespace ARToolKitPlus
