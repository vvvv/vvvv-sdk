
unit DXUTsound;

interface

uses
  Windows, MMSystem, DirectSound;

//-----------------------------------------------------------------------------
// Typing macros
//-----------------------------------------------------------------------------
const
  WAVEFILE_READ   = 1;
  WAVEFILE_WRITE  = 2;
{$IFDEF FPC}
//todo: FPC_1.9.2 bug!!!
type
  TMMIOInfo = _MMIOINFO;
  TMMCKInfo = _MMCKINFO;
  pcmwaveformat_tag = PCMWAVEFORMAT;
  TPCMWaveFormat = pcmwaveformat_tag;
{$ENDIF}



type
  PIDirectSoundBuffer = ^IDirectSoundBuffer;

  PAIDirectSoundBuffer = ^AIDirectSoundBuffer;
  AIDirectSoundBuffer = array[0..MaxInt div SizeOf(IDirectSoundBuffer) - 1] of IDirectSoundBuffer;

  PADSBPositionNotify = ^ADSBPositionNotify;
  ADSBPositionNotify =  array[0..MaxInt div SizeOf(TDSBPositionNotify) - 1] of TDSBPositionNotify;

type
  //-----------------------------------------------------------------------------
  // Classes used by this header
  //-----------------------------------------------------------------------------
  CWaveFile = class;

  //-----------------------------------------------------------------------------
  // Name: class CWaveFile
  // Desc: Encapsulates reading or writing sound data to or from a wave file
  //-----------------------------------------------------------------------------
  CWaveFile = class
  public
    m_pwfx:         PWaveFormatEx;  // Pointer to WAVEFORMATEX structure
    m_hmmio:        HMMIO;          // MM I/O handle for the WAVE
    m_ck:           MMCKINFO;       // Multimedia RIFF chunk
    m_ckRiff:       MMCKINFO;       // Use in opening a WAVE file
    m_dwSize:       DWORD;          // The size of the wave file
    m_mmioinfoOut:  MMIOINFO;
    m_dwFlags:      DWORD;
    m_bIsReadingFromMemory: BOOL;
    m_pbData: PByte;
    m_pbDataCur: PByte;
    m_ulDataSize: Cardinal;
    m_pResourceBuffer: PChar;

  protected
    function ReadMMIO: HRESULT;
    function WriteMMIO(pwfxDest: PWaveFormatEx): HRESULT;

  public
    constructor Create;
    destructor Destroy; override;

    function Open(strFileName: PWideChar; pwfx: PWaveFormatEx; dwFlags: DWORD): HRESULT;
    function OpenFromMemory(pbData: PByte; ulDataSize: Cardinal; pwfx: PWaveFormatEx; dwFlags: DWORD): HRESULT;
    function Close: HRESULT;

    function Read(pBuffer: PByte; dwSizeToRead: DWORD; pdwSizeRead: PDWORD): HRESULT;
    function Write(nSizeToWrite: LongWord; buffer : Array of Short; out pnSizeWrote: LongWord): HRESULT;

    function GetSize: DWORD;
    function ResetFile: HRESULT;
    property GetFormat: PWaveFormatEx read m_pwfx;
  end;

implementation

function mmioFOURCC(ch0, ch1, ch2, ch3: Char): DWord;
begin
  Result:= Byte(ch0) or (Byte(ch1) shl 8) or (Byte(ch2) shl 16) or (Byte(ch3) shl 24 );
end;

{ CWaveFile }

//-----------------------------------------------------------------------------
// Name: CWaveFile::CWaveFile()
// Desc: Constructs the class.  Call Open() to open a wave file for reading.
//       Then call Read() as needed.  Calling the destructor or Close()
//       will close the file.
//-----------------------------------------------------------------------------
constructor CWaveFile.Create;
begin
  m_pwfx    := nil;
  m_hmmio   := 0;
  m_pResourceBuffer := nil;
  m_dwSize  := 0;
  m_bIsReadingFromMemory := False;
end;


//-----------------------------------------------------------------------------
// Name: CWaveFile::~CWaveFile()
// Desc: Destructs the class
//-----------------------------------------------------------------------------
destructor CWaveFile.Destroy;
begin
  Close;

  if (not m_bIsReadingFromMemory) then FreeMem(m_pwfx);

  inherited;
end;


//-----------------------------------------------------------------------------
// Name: CWaveFile::Open()
// Desc: Opens a wave file for reading
//-----------------------------------------------------------------------------
function CWaveFile.Open(strFileName: PWideChar; pwfx: PWaveFormatEx;
  dwFlags: DWORD): HRESULT;
var
  hResInfo: HRSRC;
  hResData: HGLOBAL;
  dwSize:   DWORD;
  pvRes:    Pointer;
  mmioInfo: TMMIOInfo;
begin
  m_dwFlags := dwFlags;
  m_bIsReadingFromMemory := False;

  if (m_dwFlags = WAVEFILE_READ) then
  begin
    if (strFileName = nil) then
    begin
      Result:= E_INVALIDARG;
      Exit;
    end;
    FreeMem(m_pwfx);

    //m_hmmio := mmioOpenW(strFileName, nil, MMIO_ALLOCBUF or MMIO_READ);
    m_hmmio := mmioOpenW(PChar(strFileName), nil, MMIO_ALLOCBUF or MMIO_READ);

    if (0 = m_hmmio) then
    begin
      // Loading it as a file failed, so try it as a resource
      hResInfo := FindResourceW(0, strFileName, 'WAVE');
      if (hResInfo = 0) then
      begin
        hResInfo := FindResourceW(0, strFileName, 'WAV');
        if (hResInfo = 0) then
        begin
          ////Result:= //DXUT_ERR('FindResource', E_FAIL, UnitName, $FFFFFFFF);
          Exit;
        end;
      end;

      hResData := LoadResource(0, hResInfo);
      if (hResData = 0) then
      begin
        //Result:= //DXUT_ERR('LoadResource', E_FAIL, UnitName, $FFFFFFFF);
        Exit;
      end;

      dwSize := SizeofResource(0, hResInfo);
      if (dwSize = 0) then
      begin
        //Result:= //DXUT_ERR('SizeofResource', E_FAIL, UnitName, $FFFFFFFF);
        Exit;
      end;

      pvRes := LockResource(hResData);
      if (pvRes = nil) then
      begin
        //Result:= //DXUT_ERR('LockResource', E_FAIL, UnitName, $FFFFFFFF);
        Exit;
      end;

      try
        GetMem(m_pResourceBuffer, SizeOf(Char)*dwSize);
      except
        //on EOutOfMemory do
        begin
          //Result:= //DXUT_ERR('GetMem', E_OUTOFMEMORY, UnitName, $FFFFFFFF);
          Exit;
        end;
      end;
      Move(pvRes^, m_pResourceBuffer^, dwSize);

      ZeroMemory(@mmioInfo, SizeOf(mmioInfo));
      mmioInfo.fccIOProc := FOURCC_MEM;
      mmioInfo.cchBuffer := dwSize;
      mmioInfo.pchBuffer := m_pResourceBuffer;

      m_hmmio := mmioOpen(nil, @mmioInfo, MMIO_ALLOCBUF or MMIO_READ);
    end;

    Result := ReadMMIO;
    if FAILED(Result) then
    begin
      // ReadMMIO will fail if its an not a wave file
      mmioClose(m_hmmio, 0);
      ////DXUT_ERR('ReadMMIO', Result, UnitName, $FFFFFFFF);
      Exit;
    end;

    Result := ResetFile;
    if FAILED(Result) then
    begin
      ////DXUT_ERR('ResetFile', Result, UnitName, $FFFFFFFF);
      Exit;
    end;

    // After the reset, the size of the wav file is m_ck.cksize so store it now
    m_dwSize := m_ck.cksize;
  end else
  begin
    m_hmmio := mmioOpenW(PChar(strFileName), nil, MMIO_ALLOCBUF or
                                             MMIO_READWRITE or
                                             MMIO_CREATE);

    if (0 = m_hmmio) then
    begin
      //Result:= //DXUT_ERR('mmioOpen', E_FAIL, UnitName, $FFFFFFFF);
      Exit;
    end;

    Result := WriteMMIO(pwfx);
    if FAILED(Result) then
    begin
      ////DXUT_ERR('WriteMMIO', Result, UnitName, $FFFFFFFF);
      Exit;
    end;

    Result := ResetFile;
    if FAILED(Result) then
    begin
      //DXUT_ERR('ResetFile', Result, UnitName, $FFFFFFFF);
      Exit;
    end;
  end;
end;


//-----------------------------------------------------------------------------
// Name: CWaveFile::OpenFromMemory()
// Desc: copy data to CWaveFile member variable from memory
//-----------------------------------------------------------------------------
function CWaveFile.OpenFromMemory(pbData: PByte; ulDataSize: Cardinal;
  pwfx: PWaveFormatEx; dwFlags: DWORD): HRESULT;
begin
  m_pwfx       := pwfx;
  m_ulDataSize := ulDataSize;
  m_pbData     := pbData;
  m_pbDataCur  := m_pbData;
  m_bIsReadingFromMemory := True;

  if (dwFlags <> WAVEFILE_READ) then Result:= E_NOTIMPL
  else Result:= S_OK;
end;


//-----------------------------------------------------------------------------
// Name: CWaveFile::ReadMMIO()
// Desc: Support function for reading from a multimedia I/O stream.
//       m_hmmio must be valid before calling.  This function uses it to
//       update m_ckRiff, and m_pwfx.
//-----------------------------------------------------------------------------
function CWaveFile.ReadMMIO: HRESULT;
var
  ckIn:          TMMCKInfo;      // chunk info. for general use.
  pcmWaveFormat: TPCMWaveFormat; // Temp PCM structure to load in.
  cbExtraBytes:  Word;
begin
  m_pwfx := nil;

  if (0 <> mmioDescend(m_hmmio, @m_ckRiff, nil, 0)) then
  begin
    //Result:= //DXUT_ERR('mmioDescend', E_FAIL, UnitName, $FFFFFFFF);
    Exit;
  end;

  // Check to make sure this is a valid wave file
  if (m_ckRiff.ckid <> FOURCC_RIFF) or
     (m_ckRiff.fccType <>  DWORD(Byte('W') or (Byte('A') shl 8) or (Byte('V') shl 16) or (Byte('E') shl 24))) // mmioFOURCC('W', 'A', 'V', 'E'))
   then
  begin
    //Result:= //DXUT_ERR('mmioFOURCC', E_FAIL, UnitName, $FFFFFFFF);
    Exit;
  end;

  // Search the input file for for the 'fmt ' chunk.
  ckIn.ckid := DWORD(Byte('f') or (Byte('m') shl 8) or (Byte('t') shl 16) or (Byte(' ') shl 24)); // mmioFOURCC('f', 'm', 't', ' ');
  if (0 <> mmioDescend(m_hmmio, @ckIn, @m_ckRiff, MMIO_FINDCHUNK)) then
  begin
    //Result:= //DXUT_ERR('mmioDescend', E_FAIL, UnitName, $FFFFFFFF);
    Exit;
  end;

  // Expect the 'fmt' chunk to be at least as large as <PCMWAVEFORMAT>;
  // if there are extra parameters at the end, we'll ignore them
  if (ckIn.cksize < SizeOf(TPCMWaveFormat)) then
  begin
    //Result:= //DXUT_ERR('sizeof(PCMWAVEFORMAT)', E_FAIL, UnitName, $FFFFFFFF);
    Exit;
  end;

  // Read the 'fmt ' chunk into <pcmWaveFormat>.
  if (mmioRead(m_hmmio, @pcmWaveFormat, SizeOf(pcmWaveFormat)) <> SizeOf(pcmWaveFormat)) then
  begin
    //Result:= //DXUT_ERR('mmioRead', E_FAIL, UnitName, $FFFFFFFF);
    Exit;
  end;

  // Allocate the waveformatex, but if its not pcm format, read the next
  // word, and thats how many extra bytes to allocate.
  if (pcmWaveFormat.wf.wFormatTag = WAVE_FORMAT_PCM) then
  begin
{$IFDEF SUPPORTS_EXCEPTIONS}
    try
      GetMem(m_pwfx, SizeOf(TWaveFormatEx));
    except
      on EOutOfMemory do
      begin
        //Result:= //DXUT_ERR('m_pwfx', E_FAIL, UnitName, $FFFFFFFF);
        Exit;
      end;
      else raise;
    end;
{$ELSE}
    GetMem(m_pwfx, SizeOf(TWaveFormatEx));
    if (m_pwfx = nil) then 
    begin
      //Result:= //DXUT_ERR('m_pwfx', E_FAIL, UnitName, $FFFFFFFF);
      Exit;
    end;
{$ENDIF}

    // Copy the bytes from the pcm structure to the waveformatex structure
    Move(pcmWaveFormat, m_pwfx^, SizeOf(pcmWaveFormat));
    m_pwfx.cbSize := 0;
  end else
  begin
    // Read in length of extra bytes.
    cbExtraBytes := 0;
    if (mmioRead(m_hmmio, PChar(@cbExtraBytes), SizeOf(Word)) <> SizeOf(Word)) then
    begin
      //Result:= //DXUT_ERR('mmioRead', E_FAIL, UnitName, $FFFFFFFF);
      Exit;
    end;

{$IFDEF SUPPORTS_EXCEPTIONS}
    try
      GetMem(m_pwfx, SizeOf(TWaveFormatEx) + cbExtraBytes);
    except
      on EOutOfMemory do
      begin
        //Result:= //DXUT_ERR('new', E_FAIL, UnitName, $FFFFFFFF);
        Exit;
      end;
      else raise;
    end;
{$ELSE}
    GetMem(m_pwfx, SizeOf(TWaveFormatEx) + cbExtraBytes);
    if (m_pwfx = nil) then
    begin
      //Result:= //DXUT_ERR('new', E_FAIL, UnitName, $FFFFFFFF);
      Exit;
    end;
{$ENDIF}

    // Copy the bytes from the pcm structure to the waveformatex structure
    Move(pcmWaveFormat, m_pwfx^, SizeOf(pcmWaveFormat));
    m_pwfx.cbSize := cbExtraBytes;

    // Now, read those extra bytes into the structure, if cbExtraAlloc != 0.
    if (mmioRead(m_hmmio, PChar(Pointer(Integer(@(m_pwfx.cbSize))+SizeOf(Word))), 
          cbExtraBytes ) <> cbExtraBytes) then
    begin
      FreeMem(m_pwfx);
      //Result:= //DXUT_ERR('mmioRead', E_FAIL, UnitName, $FFFFFFFF);
      Exit;
    end;
  end;

  // Ascend the input file out of the 'fmt ' chunk.
  if (0 <> mmioAscend(m_hmmio, @ckIn, 0)) then
  begin
    //SAFE_DELETE(m_pwfx);
    //Result:= //DXUT_ERR('mmioAscend', E_FAIL, UnitName, $FFFFFFFF);
    Exit;
  end;

  Result:= S_OK;
end;


//-----------------------------------------------------------------------------
// Name: CWaveFile::GetSize()
// Desc: Retuns the size of the read access wave file
//-----------------------------------------------------------------------------
function CWaveFile.GetSize: DWORD;
begin
  Result:= m_dwSize;
end;


//-----------------------------------------------------------------------------
// Name: CWaveFile::ResetFile()
// Desc: Resets the internal m_ck pointer so reading starts from the
//       beginning of the file again
//-----------------------------------------------------------------------------
function CWaveFile.ResetFile: HRESULT;
begin
  if (m_bIsReadingFromMemory) then
  begin
    m_pbDataCur := m_pbData;
  end else
  begin
    if (m_hmmio = 0) then
    begin
      Result:= CO_E_NOTINITIALIZED;
      Exit;
    end;

    if (m_dwFlags = WAVEFILE_READ) then
    begin
      // Seek to the data
      if (-1 = mmioSeek(m_hmmio, m_ckRiff.dwDataOffset + SizeOf(FOURCC), SEEK_SET)) then
      begin
        //Result:= //DXUT_ERR('mmioSeek', E_FAIL, UnitName, $FFFFFFFF);
        Exit;
      end;

      // Search the input file for the 'data' chunk.
      m_ck.ckid := mmioFOURCC('d', 'a', 't', 'a');
      if (0 <> mmioDescend(m_hmmio, @m_ck, @m_ckRiff, MMIO_FINDCHUNK)) then
      begin
        //Result:= //DXUT_ERR('mmioDescend', E_FAIL, UnitName, $FFFFFFFF);
        Exit;
      end;
    end else
    begin
      // Create the 'data' chunk that holds the waveform samples.
      m_ck.ckid := mmioFOURCC('d', 'a', 't', 'a');
      m_ck.cksize := 0;

      if (0 <> mmioCreateChunk(m_hmmio, @m_ck, 0)) then
      begin
        //Result:= //DXUT_ERR('mmioCreateChunk', E_FAIL, UnitName, $FFFFFFFF);
        Exit;
      end;

      if (0 <> mmioGetInfo(m_hmmio, @m_mmioinfoOut, 0)) then
      begin
        //Result:= //DXUT_ERR('mmioGetInfo', E_FAIL, UnitName, $FFFFFFFF);
        Exit;
      end;
    end;
  end;

  Result:= S_OK;
end;


//-----------------------------------------------------------------------------
// Name: CWaveFile::Read()
// Desc: Reads section of data from a wave file into pBuffer and returns 
//       how much read in pdwSizeRead, reading not more than dwSizeToRead.
//       This uses m_ck to determine where to start reading from.  So 
//       subsequent calls will be continue where the last left off unless 
//       Reset() is called.
//-----------------------------------------------------------------------------
function CWaveFile.Read(pBuffer: PByte; dwSizeToRead: DWORD;
  pdwSizeRead: PDWORD): HRESULT;
var
  mmioinfoIn: TMMIOInfo; // current status of m_hmmio
  cbDataIn: LongWord;
  cT: Integer;
begin
  if (m_bIsReadingFromMemory) then
  begin
    if (m_pbDataCur = nil) then
    begin
      Result:= CO_E_NOTINITIALIZED;
      Exit;
    end;
    if (pdwSizeRead <> nil) then pdwSizeRead^ := 0;

    if (DWORD(m_pbDataCur) + dwSizeToRead) >
       (DWORD(m_pbData) + m_ulDataSize) then
    begin
      dwSizeToRead := m_ulDataSize - (DWORD(m_pbDataCur) - DWORD(m_pbData));
    end;

    CopyMemory(pBuffer, m_pbDataCur, dwSizeToRead);

    if (pdwSizeRead <> nil) then pdwSizeRead^ := dwSizeToRead;
  end else
  begin
    if (m_hmmio = 0) then
    begin
      Result:= CO_E_NOTINITIALIZED;
      Exit;
    end;
    if (pBuffer = nil) or (pdwSizeRead = nil) then
    begin
      Result:= E_INVALIDARG;
      Exit;
    end;

    if (pdwSizeRead <> nil) then pdwSizeRead^ := 0;

    if (0 <> mmioGetInfo(m_hmmio, @mmioinfoIn, 0)) then
    begin
      //Result:= //DXUT_ERR('mmioGetInfo', E_FAIL, UnitName, $FFFFFFFF);
      Exit;
    end;

    cbDataIn := dwSizeToRead;
    if (cbDataIn > m_ck.cksize) then cbDataIn := m_ck.cksize;

    m_ck.cksize := m_ck.cksize - cbDataIn;

    for cT := 0 to cbDataIn - 1 do
    begin
      // Copy the bytes from the io to the buffer.
      if (mmioinfoIn.pchNext = mmioinfoIn.pchEndRead) then
      begin
        if (0 <> mmioAdvance(m_hmmio, @mmioinfoIn, MMIO_READ)) then
        begin
          //Result:= //DXUT_ERR('mmioAdvance', E_FAIL, UnitName, $FFFFFFFF);
          Exit;
        end;

        if (mmioinfoIn.pchNext = mmioinfoIn.pchEndRead) then 
        begin
          //Result:= //DXUT_ERR('mmioinfoIn.pchNext', E_FAIL, UnitName, $FFFFFFFF);
          Exit;
        end;
      end;

      // Actual copy.
      //*((BYTE*)pBuffer+cT) = *((BYTE*)mmioinfoIn.pchNext);
      PByte(Integer(pBuffer)+cT)^ := PByte(mmioinfoIn.pchNext)^;
      Inc(mmioinfoIn.pchNext);
    end;

    if (0 <> mmioSetInfo(m_hmmio, @mmioinfoIn, 0)) then
    begin
      //Result:= //DXUT_ERR('mmioSetInfo', E_FAIL, UnitName, $FFFFFFFF);
      Exit;
    end;

    if (pdwSizeRead <> nil) then pdwSizeRead^ := cbDataIn;
  end;
  Result:= S_OK;
end;


//-----------------------------------------------------------------------------
// Name: CWaveFile::Close()
// Desc: Closes the wave file
//-----------------------------------------------------------------------------
function CWaveFile.Close: HRESULT;
var
  dwSamples: DWORD;
begin
  if (m_dwFlags = WAVEFILE_READ) then
  begin
    mmioClose(m_hmmio, 0);
    m_hmmio := 0;
    FreeMem(m_pResourceBuffer);
  end else
  begin
    m_mmioinfoOut.dwFlags := m_mmioinfoOut.dwFlags or MMIO_DIRTY;

    if (m_hmmio = 0) then
    begin
      Result:= CO_E_NOTINITIALIZED;
      Exit;
    end;

    if (0 <> mmioSetInfo( m_hmmio, @m_mmioinfoOut, 0)) then
    begin
      //Result:= //DXUT_ERR('mmioSetInfo', E_FAIL, UnitName, $FFFFFFFF);
      Exit;
    end;

    // Ascend the output file out of the 'data' chunk -- this will cause
    // the chunk size of the 'data' chunk to be written.
    if (0 <> mmioAscend(m_hmmio, @m_ck, 0)) then 
    begin
      //Result:= //DXUT_ERR('mmioAscend', E_FAIL, UnitName, $FFFFFFFF);
      Exit;
    end;

    // Do this here instead...
    if (0 <> mmioAscend(m_hmmio, @m_ckRiff, 0)) then
    begin
      //Result:= //DXUT_ERR('mmioAscend', E_FAIL, UnitName, $FFFFFFFF);
      Exit;
    end;

    mmioSeek(m_hmmio, 0, SEEK_SET);

    if (0 <> mmioDescend(m_hmmio, @m_ckRiff, nil, 0)) then
    begin
      //Result:= //DXUT_ERR('mmioDescend', E_FAIL, UnitName, $FFFFFFFF);
      Exit;
    end;

    m_ck.ckid := mmioFOURCC('f', 'a', 'c', 't');

    if (0 = mmioDescend(m_hmmio, @m_ck, @m_ckRiff, MMIO_FINDCHUNK)) then
    begin
      dwSamples := 0;
      mmioWrite(m_hmmio, PChar(@dwSamples), SizeOf(DWORD));
      mmioAscend(m_hmmio, @m_ck, 0);
    end;

    // Ascend the output file out of the 'RIFF' chunk -- this will cause
    // the chunk size of the 'RIFF' chunk to be written.
    if (0 <> mmioAscend( m_hmmio, @m_ckRiff, 0)) then
    begin
      //Result:= //DXUT_ERR('mmioAscend', E_FAIL, UnitName, $FFFFFFFF);
      Exit;
    end;

    mmioClose(m_hmmio, 0);
    m_hmmio := 0;
  end;

  Result:= S_OK;
end;



//-----------------------------------------------------------------------------
// Name: CWaveFile::WriteMMIO()
// Desc: Support function for reading from a multimedia I/O stream
//       pwfxDest is the WAVEFORMATEX for this new wave file.
//       m_hmmio must be valid before calling.  This function uses it to
//       update m_ckRiff, and m_ck.
//-----------------------------------------------------------------------------
function CWaveFile.WriteMMIO(pwfxDest: PWaveFormatEx): HRESULT;
var
  dwFactChunk: DWORD; // Contains the actual fact chunk. Garbage until WaveCloseWriteFile.
  ckOut1: MMCKINFO;
begin
  dwFactChunk := DWORD(-1);

  // Create the output file RIFF chunk of form type 'WAVE'.
  m_ckRiff.fccType := mmioFOURCC('W', 'A', 'V', 'E');
  m_ckRiff.cksize := 0;

  if (0 <> mmioCreateChunk(m_hmmio, @m_ckRiff, MMIO_CREATERIFF)) then 
  begin
    //Result:= //DXUT_ERR('mmioCreateChunk', E_FAIL, UnitName, $FFFFFFFF);
    Exit;
  end;

  // We are now descended into the 'RIFF' chunk we just created.
  // Now create the 'fmt ' chunk. Since we know the size of this chunk,
  // specify it in the MMCKINFO structure so MMIO doesn't have to seek
  // back and set the chunk size after ascending from the chunk.
  m_ck.ckid := mmioFOURCC('f', 'm', 't', ' ');
  m_ck.cksize := SizeOf(TPCMWaveFormat);

  if (0 <> mmioCreateChunk(m_hmmio, @m_ck, 0)) then
  begin
    //Result:= //DXUT_ERR('mmioCreateChunk', E_FAIL, UnitName, $FFFFFFFF);
    Exit;
  end;

  // Write the PCMWAVEFORMAT structure to the 'fmt ' chunk if its that type.
  if (pwfxDest.wFormatTag = WAVE_FORMAT_PCM) then
  begin
    if (mmioWrite(m_hmmio, PChar(pwfxDest), SizeOf(TPCMWaveFormat)) <> SizeOf(TPCMWaveFormat)) then
    begin
      //Result:= //DXUT_ERR('mmioWrite', E_FAIL, UnitName, $FFFFFFFF);
      Exit;
    end;
  end else
  begin
    // Write the variable length size.
    if mmioWrite(m_hmmio, PChar(pwfxDest), SizeOf(pwfxDest^) + pwfxDest.cbSize) <>
       (SizeOf(pwfxDest^) + pwfxDest.cbSize) then
    begin
      //Result:= //DXUT_ERR('mmioWrite', E_FAIL, UnitName, $FFFFFFFF);
      Exit;
    end;
  end;  
    
  // Ascend out of the 'fmt ' chunk, back into the 'RIFF' chunk.
  if (0 <> mmioAscend(m_hmmio, @m_ck, 0)) then
  begin
    //Result:= //DXUT_ERR('mmioAscend', E_FAIL, UnitName, $FFFFFFFF);
    Exit;
  end;

  // Now create the fact chunk, not required for PCM but nice to have.  This is filled
  // in when the close routine is called.
  ckOut1.ckid := mmioFOURCC('f', 'a', 'c', 't');
  ckOut1.cksize := 0;

  if (0 <> mmioCreateChunk(m_hmmio, @ckOut1, 0)) then
  begin
    //Result:= //DXUT_ERR('mmioCreateChunk', E_FAIL, UnitName, $FFFFFFFF);
    Exit;
  end;

  if (mmioWrite(m_hmmio, PChar(@dwFactChunk), SizeOf(dwFactChunk)) <> SizeOf(dwFactChunk)) then
  begin
    //Result:= //DXUT_ERR('mmioWrite', E_FAIL, UnitName, $FFFFFFFF);
    Exit;
  end;

  // Now ascend out of the fact chunk...
  if (0 <> mmioAscend( m_hmmio, @ckOut1, 0)) then
  begin
    //Result:= //DXUT_ERR('mmioAscend', E_FAIL, UnitName, $FFFFFFFF);
    Exit;
  end;

  Result:= S_OK;
end;


//-----------------------------------------------------------------------------
// Name: CWaveFile::Write()
// Desc: Writes data to the open wave file
//-----------------------------------------------------------------------------
function CWaveFile.Write(nSizeToWrite: LongWord; buffer : Array of Short;
  out pnSizeWrote: LongWord): HRESULT;
var
  cT: Integer;
  test : BYTE;
begin
  Result:= S_OK;
  if (m_bIsReadingFromMemory)                  then Result:= E_NOTIMPL;
  if (m_hmmio = 0)                             then Result:= CO_E_NOTINITIALIZED;
  //if (@pnSizeWrote = nil) or (pbSrcData = nil) then Result:= E_INVALIDARG;
  if (Result <> S_OK) then Exit;

  pnSizeWrote := 0;

  for cT := 0 to nSizeToWrite - 1 do
  begin
    if (m_mmioinfoOut.pchNext = m_mmioinfoOut.pchEndWrite) then
    begin
      m_mmioinfoOut.dwFlags := m_mmioinfoOut.dwFlags or MMIO_DIRTY;
      if (0 <> mmioAdvance(m_hmmio, @m_mmioinfoOut, MMIO_WRITE)) then
      begin
        //Result:= //DXUT_ERR('mmioAdvance', E_FAIL, UnitName, $FFFFFFFF);
        Exit;
      end;
    end;

    //*((BYTE*)m_mmioinfoOut.pchNext) = *((BYTE*)pbSrcData+cT);
    //PByte(m_mmioinfoOut.pchNext)^ := PByte(Integer(pbSrcData)+cT)^;

    PSmallInt(m_mmioinfoOut.pchNext)^ := buffer[cT];

    Inc(PSmallInt(m_mmioinfoOut.pchNext));

    Inc(pnSizeWrote);
  end;

  Result:= S_OK;
end;


end.
