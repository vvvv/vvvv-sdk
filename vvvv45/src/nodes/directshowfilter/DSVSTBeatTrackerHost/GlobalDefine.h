#ifndef _GLOBALDEFINE_H
#define _GLOBALDEFINE_H

const CLSID CLSID_DSVSTBeattrackerHost = { 0x63e2584a, 0x2180, 0x4006, { 0xb9, 0x1c, 0x2b, 0x10, 0x52, 0xf8, 0xcf, 0xc2 } };

const CLSID IID_IBeattracker   = { 0xda8b3d70, 0xfe6f, 0x4e81, { 0x93, 0x7b, 0xaf, 0x45, 0x8a, 0x9,  0xd5, 0xc4 } };

const long   MAXDATALENGTH  = 44100;
const int    FRAMESIZE      = 512; 
const int    NFRAMES        = 25;
const int    NBLOCKS        = 64;
const int    NCHANNEL       = 4;
const int    BEATPERIOD     = 1000000000;
const double MILLI          = 1000.0;
const int    ZEROPHASE      = 4;
const int    NBEAT          = 4;
const long   TIMEOUT        = 250000;


enum inputSourceType {FILESTREAM, AUDIOIN, UNDEFINED};

#endif