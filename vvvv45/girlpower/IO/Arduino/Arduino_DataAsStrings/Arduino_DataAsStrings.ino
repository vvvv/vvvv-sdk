/*
This is an example on how to communicate to vvvv 
using data encoded as strings.
For more about Arduino and vvvv check:
http://vvvv.org/documentation/arduino

To see the physical output plug an LED into the Arduino board 
between the pin (defined by the 'Pin number' in the vvvv patch, 
this should be one of PWM pins of the board) and 
the GND (ground) pin. Short leg of the LED (it is '-') 
goes to the GND.
*/


const int bufferSize = 20; // how big is the buffer
 
char buffer[bufferSize];  // Serial buffer
char commandBuffer[10];   // array to store a command
char pinBuffer[3];        // array to store a pin number 
char valueBuffer[10];     // array to store a value
int ByteCount;            // how many bytes arrived

boolean ledON;            // state of the LED
int pinNumber;            // pinNumber
int value;                // brightness value
 
void setup() 
{
    //start the serial communication at the speed of 9600 baud
    Serial.begin(9600);    
}

 
void loop() 
{
    //read the data and parse it
    SerialParser();
    
    //if something arrived 
    if (ByteCount > 0) 
    {
      //send some data back to vvvv
      //the values are encoded as ASCII characters
      //the last print command sends '\r\n' at the end
      //this defines the end of the message
      Serial.print(millis());
      Serial.print(",");
      Serial.print(pinNumber);
      Serial.print(",");
      Serial.println(value);
      
      //set the state of the pin according to the string received from vvvv 
      if (ledON)
      {
        //write PWM signal value to the pin
        analogWrite(pinNumber, value);
      }
      else
      {
       //write digital value to the pin
       //setting 0V in this example;
       digitalWrite(pinNumber, LOW);
      }
    }
    
}

void SerialParser() 
{
  ByteCount = -1;
  
  // if something has arrived over serial port
  if (Serial.available() > 0)
  {
    //read the first character
    char ch = Serial.read();
    
    //if it's 's', then it's the start of the message
    if (ch == 's')
    {
      //read all bytes of the message until the newline character ('\n')
       ByteCount =  Serial.readBytesUntil('\n',buffer,bufferSize); 
       
       //if the number of arrived bytes > 0
       if (ByteCount > 0) 
       {
            // copy the string until the first ','
            strcpy(commandBuffer, strtok(buffer, ","));
            
            // copy the same string until the next ','
            strcpy(pinBuffer, strtok(NULL, ","));
     
            // copy the same string until the next ',' 
            strcpy(valueBuffer, strtok(NULL, ","));
            
            //check the documentation about strtok() at:
            //http://www.gnu.org/software/libc/manual/html_node/Finding-Tokens-in-a-String.html
         
            //check the arrived command and set the LED state
            //this is how to compare two char arrays (simple strings)
            //if they are equal, strcmp returns 0
            if (strcmp(commandBuffer, "LED_ON") == 0)
            {
              ledON = true;
            }
            else
            {
              ledON = false;
            }
            
            // convert the string into an 'int' value
            pinNumber = atoi (pinBuffer);   
   
            // convert the sting into a 'float' value and bring it to (0..255) range;
            value = atof(valueBuffer) * 255;  
       }
       
       // clear contents of buffer
       memset(buffer, 0, sizeof(buffer));   
       Serial.flush();
    }
  }
}
