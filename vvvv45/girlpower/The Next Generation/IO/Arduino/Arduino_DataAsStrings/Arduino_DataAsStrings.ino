const int bufferSize = 20; // how big is the buffer
 
char buffer[bufferSize];  // array to store an incoming bytes
char command;             // one char to store a command
char pinNumber[3];        // array to store a pin number 
char floatValue[10];      // array to store a value
int byteCount;            // counts how many bytes arrives

int brightness;
 
void setup() 
{
    Serial.begin(57600);
    pinMode (13, OUTPUT);
}

 
void loop() 
{
    //Read the data and parse it
    SerialParser();
   
    //If something arrived, print the convert the data
    //to the suitable formats and send some data back
    if (byteCount  > 0) 
    {
      
      brightness=LOW;   
      
      brightness=command ? HIGH : LOW;
       
      digitalWrite(13, brightness);

      //sendBinary(atoi(pinNumber));
      //Serial.write(command);
      //Serial.write(brightness);
      
    }
}

void SerialParser() 
{

  byteCount = -1;
  
  // if something is arrived over serial port
  if (Serial.available()>0)
  {
    
    //read the first character
    char ch=Serial.read();
    
    //if it's 's', then it's the start of the message
    if (ch=='s')
    {
      
      //read bytes of the message until '\n'
       byteCount =  Serial.readBytesUntil('\n',buffer,bufferSize);
     
       //if the number of arrived bytes is exactly the same,
       //as vvvv sends in a message
       if (byteCount  > 0) 
       {
         // copy 1 byte from  buffer into 'command'
         strncpy (&command, buffer, 1);   
      
         Serial.write ((int)command);   
         
         // copy 2 bytes (offsetted by 1 byte) from buffer into 'pinNumber'
         strncpy (pinNumber, buffer+1, 2);   
         
         //pinNumber[2]='\0';                  // manually add 'end of the string' to the 'pinNumber' 
         
         // copy 4 bytes (offsetted by 3 bytes) from buffer into 'value'
         strncpy (floatValue, buffer+3, 4);       
         
         //check the documentation about strncpy() at:
         //http://pubs.opengroup.org/onlinepubs/009695399/functions/strncpy.html  
       }
       
       // clear contents of buffer
       memset(buffer, 0, sizeof(buffer));   
       //Serial.flush();
    }
  }
}

void sendBinary(int value)
{
  //integer value will be splitted into two bytes before sending.
  //it's important for the values bigger than 255 (one byte).
  
  Serial.write(lowByte(value));  // send the low byte
  Serial.write(highByte(value)); // send the high byte
}
