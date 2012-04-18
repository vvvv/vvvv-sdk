#ifndef _VSTHOST_H
#define _VSTHOST_H

#include <windows.h>
#include <stdio.h>
#include "aeffectx.h"
#include "audioeffectx.h"
#include "GlobalDefine.h"

typedef AEffect* (*PluginEntry) (audioMasterCallback audioMaster);

VstIntPtr VSTCALLBACK HostCallback (AEffect *effect,
 							              VstInt32 opcode,
							               VstInt32 index,
							               VstIntPtr value,
							               void *ptr,
							               float opt);

#endif