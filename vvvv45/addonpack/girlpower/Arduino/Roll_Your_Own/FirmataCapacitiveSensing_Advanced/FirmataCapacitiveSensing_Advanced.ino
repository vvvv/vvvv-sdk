/*
  How to use Firmata as a flexible transport prototcol
  in custom firmware (advanced).
  
  Make sure you have the CapSense library installed:
  http://playground.arduino.cc//Main/CapacitiveSensor
  
  From the firmata & vvvv workshop http://node13.vvvv.org/
 */

#include <Boards.h>
#include <Firmata.h>

#include <CapacitiveSensor.h>

CapacitiveSensor sensor  = CapacitiveSensor(2, 3);

int reportValues = 0;

const int MINIMUM_SAMPLING_INTERVAL = 10;
int samplingInterval = MINIMUM_SAMPLING_INTERVAL;

void reportAnalogCallback(byte analogPin, int value) {
  reportValues = value;
}

void sysexCallback(byte command, byte argc, byte argv[]) {
  switch(command) {
  case SAMPLING_INTERVAL:
    if (argc > 1) {
      samplingInterval = argv[0] + (argv[1] << 7);
      if (samplingInterval < MINIMUM_SAMPLING_INTERVAL) {
        samplingInterval = MINIMUM_SAMPLING_INTERVAL;
      }      
    } 
    break;
  }
}

void setup(){
  sensor.set_CS_AutocaL_Millis(0xFFFFFFFF);
  Firmata.attach(REPORT_ANALOG, reportAnalogCallback);
  Firmata.attach(START_SYSEX, sysexCallback);
  Firmata.begin();
}


void loop(){
  while(Firmata.available()) {
    Firmata.processInput();
  }

  long sensorValue = sensor.capacitiveSensor(30);

  if(reportValues > 0) {
    Firmata.sendAnalog(0,sensorValue);
  }

  delay(samplingInterval);
}

