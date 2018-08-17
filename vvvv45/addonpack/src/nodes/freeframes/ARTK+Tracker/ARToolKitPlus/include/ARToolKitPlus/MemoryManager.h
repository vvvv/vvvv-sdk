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
* $Id: MemoryManager.h 164 2006-05-02 11:29:10Z daniel $
* @file
* ======================================================================== */


#ifndef __ARTOOLKITPLUS_MEMORYMANAGER_HEADERFILE__
#define __ARTOOLKITPLUS_MEMORYMANAGER_HEADERFILE__


#include "ARToolKitPlus.h"


namespace ARToolKitPlus
{


/// MemoryManager defines a basic interface for a custom memory manager to be uses by ARToolKitPlus
/** 
 *  In various situations it makes sense to have a custom memory manager rather than
 *  using the default new/delete functions provided by the c-runtime.
 *  To use a custom memory manager it has to be set using ARToolKitPlus::setMemoryManager()
 *  before ARToolKitPlus is instantiated. If no memory manager is set, ARToolKitPlus
 *  will use the default new/delete functions.
 */
class ARTOOLKITPLUS_API MemoryManager
{
public:
	virtual ~MemoryManager() {}

	/// The MemoryManager will allocate an initial amount of memory
	/**
	 *  Each time the MemoryManager does not have enough memory left for
	 *  a getMemory() request it will allocate another block of nNumGrowBytes bytes.
	 */
	virtual bool init(size_t nNumInitialBytes, size_t nNumGrowBytes=0) = 0;

	/// Releases all memory alloced previously
	/**
	 *  After deinit() has been called getMemory() will not be able to reserve memory anymore.
	 *  deinit( ) releases all memory allocated previously. It is not required to call releaseMemory() after deinit().
	 */
	virtual bool deinit() = 0;

	/// Returns true if the MemoryManager was already initialized
	virtual bool didInit() = 0;

	/// Reserves nNumBytes from previously allocated memory
	/**
	 *  init() hast to be called before getMemory can be invoked.
	 */
	virtual void* getMemory(size_t nNumBytes) = 0;

	/// Releases a memory block that was allocated via getMemory()
	virtual void releaseMemory(void* nMemoryBlock) = 0;

	/// Returns the number of bytes allocated alltogether
	virtual unsigned int getBytesAllocated() = 0;
};


}  // namespace ARToolKitPlus


#endif //__ARTOOLKITPLUS_MEMORYMANAGER_HEADERFILE__
