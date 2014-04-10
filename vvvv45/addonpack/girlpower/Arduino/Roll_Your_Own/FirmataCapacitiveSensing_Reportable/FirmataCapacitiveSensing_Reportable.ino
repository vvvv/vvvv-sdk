/*
  How to use Firmata as a flexible transport prototcol
  in custom firmware (advanced). With adjustable sample rate.
  
  Make sure you have the CapSense library installed:
  http://playground.arduino.cc//Main/CapacitiveSensor
  
  From the firmata & vvvv workshop http://node13.vvvv.org/
 */

#include <Boards.h>
#include <Firmata.h>

#include <CapacitiveSensor.h>

CapacitiveSensor sensor  = CapacitiveSensor(2, 3);

int reportValues = 0;

void reportAnalogCallback(byte analogPin, int value) {
  reportValues = value;
}

void setup(){
  sensor.set_CS_AutocaL_Millis(0xFFFFFFFF);
  Firmata.attach(REPORT_ANALOG, reportAnalogCallback);
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

  delay(10);
}

