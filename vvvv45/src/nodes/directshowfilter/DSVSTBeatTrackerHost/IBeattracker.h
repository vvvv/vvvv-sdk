
#ifndef _IBEATTRACKER_H
#define _IBEATTRACKER_H

#ifdef __cplusplus
extern "C" {
#endif


DEFINE_GUID (IID_IBeattracker, 0xda8b3d70, 0xfe6f, 0x4e81, 0x93, 0x7b, 0xaf, 0x45, 0x8a, 0x9, 0xd5, 0xc4);

DECLARE_INTERFACE_(IBeattracker, IUnknown)
{
  STDMETHOD(getFeedback)
  ( THIS_
    int *opcode,int *beat,double *phase,double *fx0,double *fx1,double *fx2,double *fx3,int *bpm
  ) PURE;

  STDMETHOD(setDelay)
  ( THIS_
    int delay 
  ) PURE;

  STDMETHOD(setResolution)
  ( THIS_
    int resolution
  ) PURE;

  STDMETHOD(setMinBPM)
  ( THIS_
    int minBPM
  ) PURE;

  STDMETHOD(setMaxBPM)
  ( THIS_
    int maxBPM
  ) PURE;

  STDMETHOD(destroy)
  ( THIS_
    
  ) PURE;

};

#ifdef __cplusplus
}
#endif


#endif