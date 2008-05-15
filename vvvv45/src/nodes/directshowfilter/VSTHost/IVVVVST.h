#ifndef _IVVVVST_H
#define _IVVVVST_H

#ifdef __cplusplus
extern "C" {
#endif

// {D23E4EB0-E697-4df9-9079-CDBC0B24EDBE}
DEFINE_GUID(IID_IVVVVST, 0xd23e4eb0, 0xe697, 0x4df9, 0x90, 0x79, 0xcd, 0xbc, 0xb, 0x24, 0xed, 0xbe);

DECLARE_INTERFACE_(IVVVVST, IUnknown)
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

  STDMETHOD (isInstrument)
  (
    THIS_
	unsigned char *value
  ) PURE;

  STDMETHOD (sendMidiNotes)
  (
    THIS_
	int count, int note[], int velocity[]
  ) PURE;

  STDMETHOD (sendMidiNotesOff)
  (
    THIS_
  ) PURE;

  STDMETHOD (sendPolyphonic)
  (
    THIS_
	unsigned char polyphonicNote, unsigned char polyphonicValue
  ) PURE;

  STDMETHOD (sendController)
  (
    THIS_
	unsigned char controllerID, unsigned char controllerValue
  ) PURE;

  STDMETHOD (sendProgram)
  (
    THIS_
	unsigned char programID
  ) PURE;

  STDMETHOD (sendMonophonic)
  (
    THIS_
    unsigned char monophonicValue
  ) PURE;

  STDMETHOD (sendPitchbend)
  (
    THIS_
	unsigned char pitchbendValue
  ) PURE;


};

#ifdef __cplusplus
}
#endif


#endif