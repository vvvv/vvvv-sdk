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
 * $Id: MemoryManager.cpp 164 2006-05-02 11:29:10Z daniel $
 * @file
 * ======================================================================== */


#include <ARToolKitPlus/MemoryManager.h>
#include <malloc.h>


namespace ARToolKitPlus
{


#ifndef _ARTKP_NO_MEMORYMANAGER_

MemoryManager* memManager = NULL;


ARTOOLKITPLUS_API void
setMemoryManager(MemoryManager* nManager)
{
	memManager = nManager;
}


ARTOOLKITPLUS_API MemoryManager*
getMemoryManager()
{
	return memManager;
}


#endif //_ARTKP_NO_MEMORYMANAGER_


void
artkp_Free(void* rawMemory)
{
	if(!rawMemory)
		return;

#ifndef _ARTKP_NO_MEMORYMANAGER_
	if(memManager)
		memManager->releaseMemory(rawMemory);
	else
#endif //_ARTKP_NO_MEMORYMANAGER_
		::free(rawMemory);

	rawMemory = NULL;
}


}  // namespace ARToolKitPlus


