
#include "MIDI.h"


Midi *GlobalMidi;


//---------------------------------------------------------------------------------------------------------------//

bool midiErrorCheck(MMRESULT result)
{
  if(result == MMSYSERR_NOERROR) return true;

  switch(result)
  {
    case MMSYSERR_NOERROR      : OutputDebugString(L"MMSYSERR_NOERROR\n");      break;
	case MMSYSERR_ERROR        : OutputDebugString(L"MMSYSERR_ERROR\n");        break;
	case MMSYSERR_BADDEVICEID  : OutputDebugString(L"MMSYSERR_BADDEVICEID\n");  break;
	case MMSYSERR_NOTENABLED   : OutputDebugString(L"MMSYSERR_NOTENABLED\n");   break;
	case MMSYSERR_ALLOCATED    : OutputDebugString(L"MMSYSERR_ALLOCATED\n");    break;
	case MMSYSERR_INVALHANDLE  : OutputDebugString(L"MMSYSERR_INVALHANDLE\n");  break;
	case MMSYSERR_NODRIVER     : OutputDebugString(L"MMSYSERR_NODRIVER\n");     break;
	case MMSYSERR_NOMEM        : OutputDebugString(L"MMSYSERR_NOMEM\n");        break;
	case MMSYSERR_NOTSUPPORTED : OutputDebugString(L"MMSYSERR_NOTSUPPORTED\n"); break;
	case MMSYSERR_BADERRNUM    : OutputDebugString(L"MMSYSERR_BADERRNUM\n");    break;
	case MMSYSERR_INVALFLAG    : OutputDebugString(L"MMSYSERR_INVALFLAG\n");    break;
	case MMSYSERR_INVALPARAM   : OutputDebugString(L"MMSYSERR_INVALPARAM\n");   break;
	case MMSYSERR_HANDLEBUSY   : OutputDebugString(L"MMSYSERR_HANDLEBUSY\n");   break;
	case MMSYSERR_INVALIDALIAS : OutputDebugString(L"MMSYSERR_INVALIDALIAS\n"); break;
	case MMSYSERR_BADDB        : OutputDebugString(L"MMSYSERR_BADDB\n");        break;
	case MMSYSERR_KEYNOTFOUND  : OutputDebugString(L"MMSYSERR_KEYNOTFOUND\n");  break;
	case MMSYSERR_READERROR    : OutputDebugString(L"MMSYSERR_READERROR\n");    break;
	case MMSYSERR_WRITEERROR   : OutputDebugString(L"MMSYSERR_WRITEERROR\n");   break;
	case MMSYSERR_DELETEERROR  : OutputDebugString(L"MMSYSERR_DELETEERROR\n");  break;
	case MMSYSERR_VALNOTFOUND  : OutputDebugString(L"MMSYSERR_VALNOTFOUND\n");  break;
	case MMSYSERR_NODRIVERCB   : OutputDebugString(L"MMSYSERR_NODRIVERCB\n");   break;
	case MMSYSERR_MOREDATA     : OutputDebugString(L"MMSYSERR_MOREDATA\n");     break;

	default : OutputDebugString(L"UNDEFINED ERROR\n"); break;
  }

  return false;

}

//---------------------------------------------------------------------------------------------------------------//

MidiMsg::MidiMsg()
{
  for(int i=0;i<MSGLEN;i++)
	data[i] = 0;

  for(int i=0;i<SYSEXLEN;i++)
    sysEx[i] = '0';

  header.dwBufferLength = SYSEXLEN;
  header.dwFlags        = 0;
  header.lpData         = (LPSTR)this->sysEx;

}

//---------------------------------------------------------------------------------------------------------------//

void __stdcall Midi::MidiInCallback(HMIDIIN midiDevice, UINT msg, DWORD dwInstance, DWORD dwParam1, DWORD dwParam2)
{
  if(msg != MM_MIM_DATA || !GlobalMidi) 
	return;


  unsigned char status   = dwParam1         & 0xF0;

  unsigned char note     = (dwParam1 >> 8)  & 0x7F;

  unsigned char velocity = (dwParam1 >> 16) & 0x7F;

  //Use a global pointer, not very elegant
  GlobalMidi->setMsg(status,note,velocity);


  /*
  switch(msg)
  {
    case MM_MIM_OPEN      : OutputDebugString(L"MidiInCallback : MM_MIM_OPEN\n");      break;
    case MM_MIM_CLOSE     : OutputDebugString(L"MidiInCallback : MM_MIM_CLOSE\n");     break;
    case MM_MIM_DATA      : OutputDebugString(L"MidiInCallback : MM_MIM_DATA\n");      break;
    case MM_MIM_LONGDATA  : OutputDebugString(L"MidiInCallback : MM_MIM_LONGDATA\n");  break;
    case MM_MIM_ERROR     : OutputDebugString(L"MidiInCallback : MM_MIM_ERROR\n");     break;
    case MM_MIM_LONGERROR : OutputDebugString(L"MidiInCallback : MM_MIM_LONGERROR\n"); break;

	default : OutputDebugString(L"MidiInCallback : Undefined Msg\n"); break;
  }
  */
  
}

//---------------------------------------------------------------------------------------------------------------//

Midi::Midi()
{
  count = 0;
  open  = false;

  if(MIDION)
  {
   GlobalMidi = this;

   int nDevices = midiInGetNumDevs();

   if(nDevices>0)
 	open = openDevice ();
  }

}

Midi::~Midi()
{
  if(open)
	closeDevice();
}


void Midi::setMsg(unsigned int status, unsigned int note, unsigned int velocity)
{
  if(count >= NMSG) 
	return;

  msgBuffer[count].data[0] = (unsigned char)status;

  msgBuffer[count].data[1] = (unsigned char)note;

  msgBuffer[count].data[2] = (unsigned char)velocity;

  count++;

}

//open the first midi-device in the list 
bool Midi::openDevice()
{
  int id = 0;

  if(!midiErrorCheck(midiInGetDevCaps( id, &midiInDevice.caps, sizeof(MIDIINCAPS)))) 
	return false;

  if(!midiErrorCheck(midiInOpen(&midiInDevice.handle, id, (DWORD)MidiInCallback, 0, CALLBACK_FUNCTION)))
	return false;

  if(!midiErrorCheck(midiInPrepareHeader(midiInDevice.handle, &midiMsg.header, sizeof(MIDIHDR))))
	return false;

  if(!midiErrorCheck(midiInAddBuffer(midiInDevice.handle, &midiMsg.header, sizeof(MIDIHDR)))) 
	return false;

  if(!midiErrorCheck(midiInStart(midiInDevice.handle)))
	return false;

  return true;
}


bool Midi::closeDevice()
{
  int id = 0;

  if(!midiErrorCheck(midiInReset(midiInDevice.handle)))
	return false;

  if(!midiErrorCheck(midiInUnprepareHeader(midiInDevice.handle, &midiMsg.header, sizeof(MIDIHDR))))
	return false;

  if(!midiErrorCheck(midiInClose(midiInDevice.handle)))
	return false;

  return true;
}


