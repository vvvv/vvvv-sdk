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
 * $Id: CameraFactory.cxx 162 2006-04-19 21:28:10Z grabner $
 * @file
 * ======================================================================== */


#include <ARToolKitPlus/CameraFactory.h>
#include <ARToolKitPlus/CameraImpl.h>
#include <ARToolKitPlus/CameraAdvImpl.h>

namespace ARToolKitPlus {

Camera* CameraFactory::
createCamera(const char* filename)
{
	CameraAdvImpl *c_ptr = new CameraAdvImpl();
	if(c_ptr->loadFromFile(filename)) return((Camera*)c_ptr);
	else delete c_ptr;

	CameraImpl *c2_ptr = new CameraImpl();
	if(c2_ptr->loadFromFile(filename)) return((Camera*)c2_ptr);
	else delete c2_ptr;

	return(NULL);
}

}  // namespace ARToolKitPlus
