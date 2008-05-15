#include "Global.h"

void out(wchar_t *str)
{
  if(DEBUG)
  if( lstrlen(str) < 512 )
	OutputDebugString( str );
}

