///////////////////////////////////////////////////////////////////////////////////
// FreeFrame.cpp
//
// FreeFrame Open Video Plugin Prototype
// C Version
//
// Implementation of a plugin interface for the FreeFrame API
//
// www.freeframe.org
// marcus@freeframe.org

/*

Copyright (c) 2002, Marcus Clements www.freeframe.org
All rights reserved.

FreeFrame 1.0 upgrade by Russell Blakeborough
email: boblists@brightonart.org

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

   * Redistributions of source code must retain the above copyright
     notice, this list of conditions and the following disclaimer.
   * Redistributions in binary form must reproduce the above copyright
     notice, this list of conditions and the following disclaimer in
     the documentation and/or other materials provided with the
     distribution.
   * Neither the name of FreeFrame nor the names of its
     contributors may be used to endorse or promote products derived
     from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

*/

/*
non standard additions to support output by joreg@gmx.at
*/

///////////////////////////////////////////////////////////////////////////////////////////
//
// includes 
//

#include "FreeFrame.h"
#include "DetectObject.h"  // replace this with your plugins header


///////////////////////////////////////////////////////////////////////////////////////////
// Windows DLL Entry point
//
// notes: we may want to capture hModule as the instance of the host...

//#ifdef WIN32
BOOL APIENTRY DllMain( HANDLE hModule, 
                       DWORD  ul_reason_for_call, 
                       LPVOID lpReserved
					 )
{
    return TRUE;
}
//#endif

///////////////////////////////////////////////////////////////////////////////////////
// plugMain - The one and only exposed function
// parameters: 
//	functionCode - tells the plugin which function is being called
//  pParam - 32-bit parameter or 32-bit pointer to parameter structure
//
// PLUGIN DEVELOPERS:  you shouldn't need to change this function
//
// All parameters are cast as 32-bit untyped pointers and cast to appropriate
// types here
// 
// All return values are cast to 32-bit untyped pointers here before return to 
// the host
//

#ifdef WIN32
extern "C"  __declspec(dllexport) __stdcall plugMainUnion plugMain(DWORD functionCode, LPVOID pParam, LPVOID instanceID)
#elif LINUX
extern "C" {
   plugMainUnion plugMain( DWORD functionCode, LPVOID pParam, LPVOID instanceID)
#endif	
{
	plugMainUnion retval;

	// declare pPlugObj - pointer to this instance
	plugClass *pPlugObj;

	// typecast LPVOID into pointer to a plugClass
	pPlugObj = (plugClass*) instanceID;

	switch(functionCode) {

	case FF_GETINFO:
		retval.PISvalue = getInfo();
		break;
	case FF_INITIALISE:
		retval.ivalue = initialise();
		break;
	case FF_DEINITIALISE:
		retval.ivalue = deInitialise();			// todo: pass on instance IDs etc
		break;
	case FF_GETNUMPARAMETERS:
		retval.ivalue = getNumParameters();
		break;
	case FF_GETPARAMETERNAME:
		retval.svalue =  getParameterName( (DWORD) pParam );
		break;
	case FF_GETPARAMETERDEFAULT:
		retval.fvalue =  getParameterDefault( (DWORD) pParam );
		break;
	case FF_GETPARAMETERDISPLAY:
		retval.svalue =  pPlugObj->getParameterDisplay( (DWORD) pParam );
		break;	
	// parameters are passed in here as a packed struct of two DWORDS:
	// index and value
	case FF_SETPARAMETER:
		retval.ivalue=  pPlugObj->setParameter( (SetParameterStruct*) pParam );
		break;
	case FF_PROCESSFRAME:
		retval.ivalue = pPlugObj->processFrame(pParam);
		break;
	case FF_GETPARAMETER:
		retval.fvalue =  pPlugObj->getParameter((DWORD) pParam);
		break;
	case FF_GETPLUGINCAPS:
		retval.ivalue = getPluginCaps( (DWORD) pParam);
		break;

// Russell - FF 1.0 upgrade in progress ...

	case FF_INSTANTIATE:
		retval.ivalue = (DWORD) instantiate( (VideoInfoStruct*) pParam);
		break;
	case FF_DEINSTANTIATE:
		retval.ivalue = deInstantiate(pPlugObj);
		break;
	case FF_GETEXTENDEDINFO: 
		retval.ivalue = (DWORD) getExtendedInfo();
		break;
	case FF_PROCESSFRAMECOPY:
		retval.ivalue = pPlugObj->processFrameCopy((ProcessFrameCopyStruct*)pParam);
		break;
	case FF_GETPARAMETERTYPE:		
		retval.ivalue = getParameterType( (DWORD) pParam );
		break;
		
// outputs
	case FF_GETNUMOUTPUTS:		
		retval.ivalue = getNumOutputs();
		break;
	case FF_GETOUTPUTNAME:
		retval.svalue = getOutputName((DWORD) pParam);
		break;
	case FF_GETOUTPUTTYPE:		
		retval.ivalue = getOutputType((DWORD) pParam);
		break;	
	case FF_GETOUTPUTSLICECOUNT:		
		retval.ivalue = pPlugObj->getOutputSliceCount((DWORD) pParam);
		break;
	case FF_GETOUTPUT:		
		retval.svalue = (char*)pPlugObj->getOutput((DWORD) pParam);
		break;	
	case FF_SETTHREADLOCK:
         retval.ivalue = pPlugObj->setThreadLock((DWORD) pParam);
         break;
         
//spreaded inputs
 case FF_SETINPUT:
	retval.ivalue =  pPlugObj->setInput( (InputStruct*) pParam );
	break;

// ....................................

	default:
		retval.ivalue = FF_FAIL;
		break;
	}
	return retval;
}
#ifdef linux	

} /* extern "C" */
#endif

// }


