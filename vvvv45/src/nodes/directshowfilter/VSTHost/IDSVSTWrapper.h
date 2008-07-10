
#ifndef _IDSVSTWRAPPER_H
#define _IDSVSTWRAPPER_H

#ifdef __cplusplus
extern "C" {
#endif

// {D23E4EB0-E697-4df9-9079-CDBC0B24EDBE}
DEFINE_GUID(IID_IDSVSTWrapper, 0xd23e4eb0, 0xe697, 0x4df9, 0x90, 0x79, 0xcd, 0xbc, 0xb, 0x24, 0xed, 0xbe);

DECLARE_INTERFACE_(IDSVSTWrapper, IUnknown)
{
  STDMETHOD (load)
  (
    THIS_
	char *filename
  ) PURE;

  STDMETHOD (setEnable)
  (
    THIS_
	unsigned char value
  ) PURE;



  STDMETHOD (getParameterCount)
  (
    THIS_
	int *count
  ) PURE;

  STDMETHOD (getParameterProperties)
  (
    THIS_
	wchar_t paramDisplay[][256],
	wchar_t paramName   [][256],
	wchar_t paramLabel  [][256],
	double  paramValue  []
  ) PURE;

  STDMETHOD (getParameter)
  (
    THIS_
	int index,double *value
  ) PURE;

  STDMETHOD (setParameter)
  (
    THIS_
	int index,double value
  ) PURE;



  STDMETHOD (getMidiIsInstrument)
  (
    THIS_
  ) PURE;

  STDMETHOD (sendMidiNote)
  (
    THIS_
	int count, int note[], int velocity[]
  ) PURE;

  STDMETHOD (sendMidiNoteAllOff)
  (
    THIS_
  ) PURE;

  STDMETHOD (sendMidiPolyphonic)
  (
    THIS_
	unsigned char polyphonicNote, unsigned char polyphonicValue
  ) PURE;

  STDMETHOD (sendMidiController)
  (
    THIS_
	unsigned char controllerID, unsigned char controllerValue
  ) PURE;

  STDMETHOD (sendMidiProgram)
  (
    THIS_
	unsigned char programID
  ) PURE;

  STDMETHOD (sendMidiMonophonic)
  (
    THIS_
    unsigned char monophonicValue
  ) PURE;

  STDMETHOD (sendMidiPitchbend)
  (
    THIS_
	unsigned char pitchbendValue
  ) PURE;



  STDMETHOD (getHasWindow)
  (
    THIS_	
  ) PURE;

  STDMETHOD (setWindowHandle)
  (
    THIS_
	HWND hwnd
  ) PURE;

  STDMETHOD (getWindowSize)
  (
    THIS_
	int *width,
	int *height
  ) PURE;

  STDMETHOD (setWindowIdle)
  (
    THIS_
  ) PURE;



  STDMETHOD (getInputCount)
  (
    THIS_
	int *count
  ) PURE;

  STDMETHOD (getOutputCount)
  (
    THIS_
	int *count
  ) PURE;

  STDMETHOD (getProgramNames)
  (
    THIS_
	int *count,
	wchar_t names[][256]
  ) PURE;

  STDMETHOD (getActualProgram)
  (
    THIS_
    int *count
  ) PURE;
  
  STDMETHOD (setActualProgram)
  (
    THIS_
    int count
  ) PURE;

  STDMETHOD (setBpm)
  (
    THIS_
    int val
  ) PURE;
  
};

#ifdef __cplusplus
}
#endif


#endif