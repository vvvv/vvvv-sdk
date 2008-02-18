#ifndef _IDSVSTHOST_H
#define _IDSVSTHOST_H

#ifdef __cplusplus
extern "C" {
#endif

// {D23E4EB0-E697-4df9-9079-CDBC0B24EDBE}
DEFINE_GUID(IID_IDSVSTHost, 0xd23e4eb0, 0xe697, 0x4df9, 0x90, 0x79, 0xcd, 0xbc, 0xb, 0x24, 0xed, 0xbe);

DECLARE_INTERFACE_(IDSVSTHost, IUnknown)
{
  STDMETHOD (interfacetest)
  (
    THIS_

  ) PURE;

  STDMETHOD (setEnabled)
  (
    THIS_
	unsigned char enabled
  ) PURE;

  STDMETHOD (canDoMidi)
  (
    THIS_
    unsigned char *can
  ) PURE;

  STDMETHOD (sendMidiNotes)
  (
    THIS_
    unsigned char note, unsigned char velocity
  ) PURE;

  STDMETHOD (sendMidiController)
  (
    THIS_
	unsigned char controllerID, unsigned char controllerValue
  ) PURE;


};

#ifdef __cplusplus
}
#endif


#endif