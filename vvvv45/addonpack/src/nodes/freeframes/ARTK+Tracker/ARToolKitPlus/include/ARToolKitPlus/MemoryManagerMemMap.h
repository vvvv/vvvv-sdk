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
* $Id: MemoryManagerMemMap.h 162 2006-04-19 21:28:10Z grabner $
* @file
* ======================================================================== */

#ifndef __ARTOOLKITPLUS_MEMORYMANAGERMEMMAP_HEADERFILE__
#define __ARTOOLKITPLUS_MEMORYMANAGERMEMMAP_HEADERFILE__

#if defined(_MSC_VER) || defined(_WIN32_WCE)

#include "MemoryManager.h"
#include <windows.h>


namespace ARToolKitPlus
{


/// A MemoryManager that uses a memory mapped file (without file backup) for basic allocation
/**
 *  Currently this memory manager is *very* simple. only an initial block is allocated.
 *  No more blocks are allocated, which means that the value passed to init() must be large
 *  enough to hold all blocks every reserved with getMemory(). Furthermore, memory is not
 *  released when calling releaseMemory().
 *  To sum it up, this memmory manager is only useful if the full amount of data required is
 *  known beforehand and has not to be freed before the full object is deleted (using deinit()).
 */
class ARTOOLKITPLUS_API MemoryManagerMemMap : public MemoryManager
{
public:
	MemoryManagerMemMap();
	~MemoryManagerMemMap();

	bool init(size_t nNumInitialBytes, size_t nNumGrowBytes=0);
	bool deinit();
	bool didInit();

	unsigned int getBytesAllocated()  {  return (int)fullSize;  }

	void* getMemory(size_t nNumBytes);
	void releaseMemory(void* nMemoryBlock);

protected:
	bool _didInit;

	LPVOID lpvMem;      // pointer to shared memory
	HANDLE hMapObject;  // handle to file mapping

	size_t	fullSize,
			reservedSize;
};


}  // namespace ARToolKitPlus


#endif

#endif //__ARTOOLKITPLUS_MEMORYMANAGERMEMMAP_HEADERFILE__
