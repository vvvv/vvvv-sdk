unit SharedMemoryUtils;

interface

uses
  classes, windows;

procedure OpenMap(out HMap: THandle; out PData: Pointer; Name: String;
    ByteCount: Integer);
procedure CloseMap(HMap: THandle; PData: Pointer);

procedure GetSharedMem(PData: Pointer; lpszBuf: LPTSTR; cchSize: DWORD);
procedure SetSharedMem(Destination, Source: LPTSTR; SourceLength: DWord);

function LockMap(Name: String):Boolean;
procedure UnlockMap;

implementation

var
  HMapMutex: THandle;

const
  REQUEST_TIMEOUT = 1000;

procedure OpenMap(out HMap: THandle; out PData: Pointer; Name: String;
    ByteCount: Integer);
var
   llInit: Boolean;
   lInt: Integer;
   t: String;
   p: Pointer;
begin
  HMap := CreateFileMapping($FFFFFFFF, nil, PAGE_READWRITE, 0, ByteCount, pchar(Name));
  // Check if already exists
  llInit := (GetLastError() <> ERROR_ALREADY_EXISTS);

  if (HMap = 0) then
  begin
    //.LogWarning('Can''t Create Memory Map');
    exit;
  end;

  PData := MapViewOfFile(HMap, FILE_MAP_ALL_ACCESS, 0, 0, 0);
  if PData = nil then
  begin
    CloseHandle(HMap);
    //.LogWarning('Can''t View Memory Map');
    exit;
  end;

  if (llInit) then
  begin
  //   SetSharedMem(PData, PChar('asdfasdf'), 5);
  end;
end;

procedure CloseMap(HMap: THandle; PData: Pointer);
begin
  if PData <> nil then
    UnMapViewOfFile(PData);
  if HMap <> 0 then
    CloseHandle(HMap);
end;

procedure GetSharedMem(PData: Pointer; lpszBuf: LPTSTR; cchSize: DWORD);
var
   lpszTmp:LPTSTR;
   i:integer;
begin
  lpszTmp := LPTSTR(PData);
  i:=0;
  dec(cchSize);
  while (lpszTmp[i]<>chr(0)) and(cchSize<>0) do
  begin
    lpszBuf[i] := lpszTmp[i];
    inc(i);
  end;

  lpszBuf[i] := chr(0);
end;

procedure SetSharedMem(Destination, Source: LPTSTR; SourceLength: DWord);
begin
  Move(Source^, Destination^, SourceLength);
end;

function LockMap(Name: String):Boolean;
begin
  Result := true;

  HMapMutex := CreateMutex(nil, false, pchar('m'+Name));

  if HMapMutex = 0 then
  begin
    Result := false;
  end
  else
  begin
    if WaitForSingleObject(HMapMutex,REQUEST_TIMEOUT) = WAIT_FAILED then
    begin
      // timeout
      Result := false;
    end
  end
end;

procedure UnlockMap;
begin
  ReleaseMutex(HMapMutex);
  CloseHandle(HMapMutex);
end;

end.
