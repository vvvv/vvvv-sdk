#include "VSTHost.h"

VstIntPtr VSTCALLBACK HostCallback (AEffect *effect,
									       VstInt32 opcode,
										   VstInt32 index,
										   VstIntPtr value,
										   void *ptr,
										   float opt)

{

  return 1;
}

