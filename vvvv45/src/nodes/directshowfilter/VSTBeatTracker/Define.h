#ifndef VVVVBEATTRACKERDEFINE_H
#define VVVVBEATTRACKERDEFINE_H

const int NSAMPLES      =  1024;
const int NFIELD        =  1024;
const int FIELDLENGTH   =  1024;


const int NRESOLUTION   =    25; // 11025/NSAMPLES
const int NBAND         =    24;
const int NCHANNEL      =     4;
const int NRESONANCE    =   188;
const int MAXINTERVAL   =    48;
const int MININTERVAL   =    16;
const int MINLENGTH     =   256;
const int MAXLENGTH     =  2048;

const int MINBPM        =  30;
const int MAXBPM        = 140;

//const int MINBPM        =  50;
//const int MAXBPM        = 140;


const int INTERVALCYCLE =    40;

const double NOISELEVEL = 0.000125;
const double PI = 3.1415926535;
const bool DEBUG_WINDOW  = false;
const bool DEBUG_SOUND   = false;

#endif

//NSAMPLES___________
//i 1  d 11025.000000
//i 3  d  3675.000000
//i 5  d  2205.000000
//i 7  d  1575.000000
//i 9  d  1225.000000
//i 15 d   735.000000
//i 21 d   525.000000
//i 25 d   441.000000
//i 35 d   315.000000

//  Alle 0.25 Sekunden:
//  22050 / 2    = 11025 Samples
//  11025 / 441  = 25    Einheiten pro Aufruf
//  25 * 4       = 100   Einheiten pro Minute
//  100/(60/ 40) = 100 * 1.5   = 150  Samples -> alle 66.66 Samples kommt ein Beat
//  100/(60/160) = 100 * 0.375 = 37.5   

//  Formel : (11025 / NSAMPLES) / (60/BPM) 

//  Zeit pro Einheit:
//  100 pro Sekunde daher dauert eine Einheit 10 Milisekunden oder 10000 Microsekunden 




