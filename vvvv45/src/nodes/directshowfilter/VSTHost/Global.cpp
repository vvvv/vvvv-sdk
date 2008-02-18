#include "Global.h"

void out(wchar_t *str)
{
  if(DEBUG)
  if( lstrlen(str) < 512 )
	OutputDebugString( str );
}


void outputString(char str[],bool endline)
{
  int i=0;

  wchar_t buffer[512];

  for(i=0;i<64;i++)
  {
   if(str[i] == '\0') break;
   buffer[i] = (wchar_t) str[i]; 
  }

  if(endline)
   buffer[i]   = '\n';
  else
   buffer[i]   = ' ';
	
  buffer[i+1] = '\0';

  if(DEBUG) 
	OutputDebugString(buffer);

}

void outputString(char label[],char str[],bool endline)
{
  int h=0;
  int i=0;

  wchar_t buffer[512];

  for(h=0;h<64;h++)
  {
   if(label[h] == '\0') break;
   buffer[h] = (wchar_t) label[h]; 
  }

  buffer[h]   = ' ';
  buffer[h+1] = '\0';

  if(DEBUG) 
	OutputDebugString(buffer);


  buffer[0] = '\0';

  for(i=0;i<64;i++)
  {
   if(str[i] == '\0') break;
   buffer[i] = (wchar_t) str[i]; 
  }

  if(endline)
   buffer[i]   = '\n';
  else
   buffer[i]   = ' ';
	
  buffer[i+1] = '\0';

  if(DEBUG) 
	OutputDebugString(buffer);

}
