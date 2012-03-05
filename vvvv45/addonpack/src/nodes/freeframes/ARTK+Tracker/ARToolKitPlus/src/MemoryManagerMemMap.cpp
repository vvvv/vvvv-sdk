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
 * $Id: MemoryManagerMemMap.cpp 162 2006-04-19 21:28:10Z grabner $
 * @file
 * ======================================================================== */


#include <ARToolKitPlus/MemoryManagerMemMap.h>
#include <assert.h>


#if defined(_MSC_VER) || defined(_WIN32_WCE)

namespace ARToolKitPlus
{


MemoryManagerMemMap::MemoryManagerMemMap()
{
	_didInit = false;

	lpvMem = NULL;
	hMapObject = NULL;

	fullSize = reservedSize = 0;
}


MemoryManagerMemMap::~MemoryManagerMemMap()
{
	deinit();
}


bool
MemoryManagerMemMap::init(size_t nNumInitialBytes, size_t nNumGrowBytes)
{
	if(_didInit)
		return false;

	fullSize = nNumInitialBytes;

	hMapObject = CreateFileMappingW(INVALID_HANDLE_VALUE,		// use paging file
									NULL,						// default security attributes
									PAGE_READWRITE,				// read/write access
									0,							// size: high 32-bits
									(DWORD)nNumInitialBytes,	// size: low 32-bits
									L"myMemMap");				// name of map object
	if (hMapObject == NULL) 
		return false;
 
	lpvMem = MapViewOfFile(hMapObject,     // object to map view of
						   FILE_MAP_WRITE, // read/write access
						   0,              // high offset:  map from
						   0,              // low offset:   beginning
						   0);             // default: map entire file


	_didInit = lpvMem != NULL;
	return _didInit;
}


bool
MemoryManagerMemMap::deinit()
{
	if(!_didInit)
		return false;

	if(!lpvMem || !hMapObject)
		return false;

	// Unmap shared memory from the process's address space.
	UnmapViewOfFile(lpvMem); 
	lpvMem = NULL;
 
	// Close the process's handle to the file-mapping object.
	CloseHandle(hMapObject); 
	hMapObject = NULL;

	_didInit = false;
	return true;
}


bool
MemoryManagerMemMap::didInit()
{
	return _didInit;
}


void*
MemoryManagerMemMap::getMemory(size_t nNumBytes)
{
	// check if enough memory is left
	//
	if(reservedSize+nNumBytes > fullSize)
	{
		assert(false && "out of memory in MemoryManagerMemMap");
		return NULL;
	}

	void* newBlock = reinterpret_cast<unsigned char*>(lpvMem) + reservedSize;

	reservedSize += nNumBytes;
	return newBlock;
}


void
MemoryManagerMemMap::releaseMemory(void* nMemoryBlock)
{
	if(!_didInit)
		return;

	assert(false && "MemoryManagerMemMap can not release memory...");
}

}  // namespace ARToolKitPlus

#endif  // defined(_MSC_VER) || defined(_WIN32_WCE)
