//////project name
//SourceBufer

//////description
//directshow source filter.
//Value to Multichannel Sound

//////licence
//GNU Lesser General Public License (LGPL)
//english: http://www.gnu.org/licenses/lgpl.html
//german: http://www.gnu.de/lgpl-ger.html

//////language/ide
//delphi 5

//////dependencies
//directshow baseclasses coming with DSPack:
//http://sourceforge.net/projects/dspack/

//////initial author
//xsnej@gmx.de


unit Main;

interface

uses
  Classes, SysUtils, ActiveX, MMSystem, Math, Windows,
  BaseClass, DirectShow9, DSUtil, DirectSound;


const
  CLSID_SourceBuffer : TGUID = '{6F7AAC61-8E33-4cf4-A349-7976C08D7CCD}';
  FilterName: String = 'SourceBuffer';

  WAVEBUFFERSIZE =   256;
  NCHANNELS      =     2;
  SAMPLERATE     = 44100;
  BITSPERSAMPLE  =    16;
  BITSPERBYTE    =     8;
  HALFWAVE       = 32767;
  NANOSHIFT      = 1.0 / 1000000000;
  MAXCHANNELS    =    18;
  STRLENGTH      =   256;

  sudPinTypes: TRegPinTypes =
  (
    clsMajorType: @MEDIATYPE_Audio;
    clsMinorType: @MEDIASUBTYPE_PCM
  );

  sudOutputPin: array[0..0] of TRegFilterPins =
  (
    (
    strName: 'Output'; // Pins string name
    bRendered: FALSE; // Is it rendered
    bOutput: TRUE; // Is it an output
    bZero: FALSE; // Are we allowed none
    bMany: FALSE; // And allowed many
    oFilter: nil; // Connects to filter
    strConnectsToPin: nil; //'Input'; // Connects to pin
    nMediaTypes: 1; // Number of types
    lpMediaType: @sudPinTypes // Pin information
    )
  );

type

  TDynamicDouble  = Array of Double;
  TChannelArray   = Array [0..17] of TDynamicDouble;
  TChannelVolume  = Array [0..17] of Double;
  TMValue         = Double;
  PMValue         = ^TMValue;
  TDoubleData     = Array of Double;
  TSourcePtrArray = Array of PMValue;
  TSourceIntArray = Array of Integer;
  TRoutingArray   = Array of Integer;

  ISourceBuffer = interface(IUnknown)
    ['{6F7AAC61-8E33-4cf4-A349-7976C08D7CCD}']
    function Play(Index: Integer; State: Integer): HResult; stdcall;
    function Loop(Index: Integer; State: Integer): HResult; stdcall;
    function StartPosition(Index: Integer; StartPosition: Integer): HResult; stdcall;
    function EndPosition(Index: Integer; EndPosition: Integer): HResult; stdcall;
    function DoSeek(Index: Integer; State: Integer): HResult; stdcall;
    function SeekPosition(Index: Integer; SeekPosition: Integer): HResult; stdcall;
    function Pitch(Index: Integer; Pitch: Double): HResult; stdcall;
    function WriteBuffer(BufferSize: Integer; IndexCount: Integer; IndexPtr: PMValue; ChannelNumber: Integer; ChannelCount: TSourceIntArray; ChannelPtr: TSourcePtrArray): HRESULT; stdcall;
    function DumpBuffer(Filename: String): HRESULT; stdcall;

    function SetVoiceCount(VoiceCount: Integer): HResult; stdcall;
    function SetChannelConfiguration(ActiveChannelCount: Integer; ChannelCode: Integer): HRESULT; stdcall;
    function SetChannelVolume(ChannelVolume: TChannelVolume): HRESULT; stdcall;
    function SetBufferLength(Length: Integer): HRESULT; stdcall;
    function SetBuffer(Index: Integer; Buffer: TDynamicDouble): HRESULT; stdcall;
    function SetBufferChanged: HRESULT; stdcall;
    function SetBlockSize(BlockSize: Integer): HRESULT; stdcall;
    function SetGlobalVolume(Index: Integer; GlobalVolume: Double): HResult; stdcall;

    function GetActualFrame(Index: Integer; out ActualFrame: Integer):HResult; stdcall;
    function GetActualBlockSize(out ActualBlockSize: Integer): HRESULT; stdcall;
  end;

  PAudioSample  = ^AudioSample;
  AudioSample   = -32767..32767;       // for 16-bit audio
  PAudioSample8 = ^AudioSample8;
  AudioSample8  = -16383..16384;       // for 16-bit audio

  PBCVoice = ^TMVoice;
  TMVoice = class(TObject)
  private
    FPlay: Boolean;
    FGain: double;
    FPhase: double;
    FPitch: double;
    FStartPosition: Integer;
    FEndPosition: Integer;
    FSeekPosition: Integer;
    FDoSeek: Boolean;
    FFramePosition: Integer;
    FLoop: Boolean;
    FFading: Double;
    FChange: Boolean;
    FFraction: Double;
    FChannelGain: Array [0..MAXCHANNELS-1] of Double;
    FChannelVolume: TChannelVolume;


    FNumChannels  : Integer;
    FBuffer       : TChannelArray;
    FBufferTmp    : TChannelArray;
    FBufferSize   : Integer;
    FBufferSizeTmp: Integer;

  public
    destructor Destroy; override;
    constructor Create;
    function ReadTheFile(AFileName: PChar): Boolean;
    function FillBuffer( mediaBuffer: PDouble; size: integer): HResult;
  end;

  TMSourceBufferPin = class(TBCSourceStream)
  private
    voicelist: TList;
    voicecount: Integer;
  public
    pwf: PWAVEFORMATEXTENSIBLE;

    function VoiceCheck(index: Integer): PBCVoice;
    procedure VoiceFree(index: Integer);
    constructor Create(out hr: HResult; Filter: TBCSource);
    destructor Destroy; override;
    function GetMediaType(pmt: PAMMediaType): HResult; override;
    function DecideBufferSize(Allocator: IMemAllocator; Properties: PAllocatorProperties): HRESULT; override;
    function Notify(Filter: IBaseFilter; q: TQuality): HRESULT; override; stdcall;
    function FillBuffer(ims: IMediaSample): HResult; override;
  end;

  TMSourceBuffer = class(TBCSource, ISourceBuffer)
  private
    FPin: TMSourceBufferPin;
    FNumChannels: Integer;

    FBuffer   : Array of SmallInt;
    FData     : TDoubleData;
    //FWaveFile : CWaveFile;

  public
    constructor Create(ObjName: string; Unk: IUnKnown; out hr: HRESULT);
    constructor CreateFromFactory(Factory: TBCClassFactory; const Controller: IUnknown); override;
    destructor Destroy; override;

    function NonDelegatingQueryInterface(const IID: TGUID; out Obj): HResult; override;

    //ISourceBuffer functions
    function Play(Index: Integer; State: Integer): HResult; stdcall;
    function Loop(Index: Integer; State: Integer): HResult; stdcall;
    function StartPosition(Index: Integer; StartPosition: Integer): HResult; stdcall;
    function EndPosition(Index: Integer; EndPosition: Integer): HResult; stdcall;
    function DoSeek(Index: Integer; State: Integer): HResult; stdcall;
    function SeekPosition(Index: Integer; SeekPosition: Integer): HResult; stdcall;
    function Pitch(Index: Integer; Pitch: Double): HResult; stdcall;
    function WriteBuffer(BufferSize: Integer; IndexCount: Integer; IndexPtr: PMValue; ChannelNumber: Integer; ChannelCount: TSourceIntArray; ChannelPtr: TSourcePtrArray): HRESULT; stdcall;
    function DumpBuffer(Filename: String): HRESULT; stdcall;

    function SetVoiceCount(VoiceCount: Integer): HResult; stdcall;
    function SetChannelConfiguration(ActiveChannelCount: Integer; ChannelCode: Integer): HRESULT; stdcall;
    function SetChannelVolume(ChannelVolume: TChannelVolume): HRESULT; stdcall;
    function SetBufferLength(Length: Integer): HRESULT; stdcall;
    function SetBuffer(Index: Integer; Buffer: TDynamicDouble): HRESULT; stdcall;
    function SetBufferChanged: HRESULT; stdcall;
    function SetBlockSize(BlockSize: Integer): HRESULT; stdcall;
    function SetGlobalVolume(Index: Integer; GlobalVolume: Double): HResult; stdcall;

    function GetActualFrame(Index: Integer; out ActualFrame: Integer):HResult; stdcall;
    function GetActualBlockSize(out ActualBlockSize: Integer): HRESULT; stdcall;
 end;

var
 GlobalNumActiveChannels: Integer = 0;
 GlobalChannelCode      : Integer = 0;
 GlobalBlockSize        : Integer = 100;

implementation

uses Variants;

function wrap(val: Double): Double;
begin
  if val >= 1. then
    result := val - floor(val)
  else if val < 0. then
    result := val - floor(val)
  else
    result := val;
end;

constructor TMSourceBufferPin.Create(out hr: HResult; Filter: TBCSource);
begin
  inherited Create(FilterName, hr, Filter, 'Out');

  VoiceList := TList.Create;
  VoiceList.Clear;
end;

destructor TMSourceBufferPin.Destroy;
var
  i: Integer;
begin
  {
  for i := 0 to VoiceList.Count - 1 do
  begin
    if assigned(VoiceList.Items[i]) then VoiceFree(i);
  end;
  VoiceList.Free;
  }

  inherited;
end;

constructor TMVoice.Create;
var
  i: Integer;
begin
  FPlay           :=  false;
  FDoSeek         :=  false;
  FLoop           :=  false;
  FGain           :=  1;
  FPhase          :=  0;
  FPitch          :=  1;
  FStartPosition  :=  0;
  FEndPosition    :=  0;
  FSeekPosition   :=  0;
  FFramePosition  :=  0;
  FFading         :=  0;
  FPhase          :=  0;
  FBufferSize     :=  0;
  FBufferSizeTmp  :=  0;
  FChange         :=  false;
  FNumChannels    :=  GlobalNumActiveChannels;

  for i := 0 to MAXCHANNELS - 1 do
    FChannelVolume [i] := 1;
end;

destructor TMVoice.Destroy;
begin

  inherited;
end;

function TMSourceBufferPin.Notify(Filter: IBaseFilter; q: TQuality): HRESULT;
begin
  Result := E_FAIL;
end;

function TMSourceBuffer.NonDelegatingQueryInterface(const IID: TGUID;
  out Obj): HResult;
begin
  if IsEqualGUID(IID, CLSID_SourceBuffer) then
    if GetInterface(CLSID_SourceBuffer, Obj) then
      Result := S_OK
    else
      Result := E_FAIL
  else
    Result := Inherited NonDelegatingQueryInterface(IID, Obj);
end;

function TMSourceBuffer.SetVoiceCount(VoiceCount: integer): HResult; stdcall;
begin
  FPin.voicecount := voicecount;
  Result := S_OK;
end;

function TMSourceBuffer.Play(index: integer; state: integer): HResult; stdcall;
var
  v: PBCVoice;
  basefilter: IBaseFilter;

begin
  v := FPin.VoiceCheck(index);
  if v = nil then
    exit;

  v.FPlay := Boolean(state);

  Result  := S_OK
end;

function TMSourceBuffer.SetGlobalVolume(Index: Integer; GlobalVolume: Double): HResult; stdcall;
var
  v: PBCVoice;
  i: Integer;
begin
  v := FPin.VoiceCheck(index);
  if v = nil then
    exit;

  v.FGain := GlobalVolume;

  Result  := S_OK
end;

function TMSourceBuffer.Pitch(Index: Integer; Pitch: Double): HResult; stdcall;
var
  v: PBCVoice;
begin
  v := FPin.VoiceCheck(index);
  if v = nil then
    exit;

  v.FPitch := Pitch;

  Result := S_OK
end;

function TMSourceBuffer.Loop(index:integer; state:integer): HResult; stdcall;
var
  v: PBCVoice;
begin
  v := FPin.VoiceCheck(index);

  if v = nil then
    exit;

  v.FLoop := Boolean(state);

  Result := S_OK;
end;

function TMSourceBuffer.DoSeek(index: integer; state:integer): HResult; stdcall;
var
  v: PBCVoice;
begin
  v := FPin.VoiceCheck(index);

  if v = nil then
    exit;

  v.FDoSeek := Bool(state);

  Result := S_OK;
end;

function TMSourceBuffer.SeekPosition(Index: Integer; SeekPosition: Integer): HResult; stdcall;
var
  v: PBCVoice;
begin
  v := FPin.VoiceCheck(index);
  if v = nil then
    exit;

  v.FSeekPosition := SeekPosition;

  Result := S_OK;
end;

function TMSourceBuffer.StartPosition(Index: Integer; StartPosition: Integer): HResult; stdcall;
var
  v: PBCVoice;
begin
  v := FPin.VoiceCheck(index);

  if v = nil then
    exit;

  v.FStartPosition := StartPosition;

  Result := S_OK;
end;

function TMSourceBuffer.EndPosition(Index: Integer; EndPosition: Integer): HResult; stdcall;
var
  v: PBCVoice;
begin
  v := FPin.VoiceCheck(index);

  if v = nil then
    exit;

  v.FEndPosition := EndPosition;

  Result := S_OK;
end;

function TMSourceBuffer.GetActualFrame(Index: Integer; out ActualFrame: Integer): HResult; stdcall;
var
  v: PBCVoice;
begin
  v := FPin.VoiceCheck(index);

  if v = nil then
    exit;

  ActualFrame := v.FFramePosition;

  Result := S_OK;
end;

function TMSourceBuffer.setBufferLength(length: Integer): HRESULT; stdcall;
var
  v  : PBCVoice;
  i,k: Integer;
begin
  v := FPin.VoiceCheck(0);

  if v = nil then
    exit;

  v.FBufferSizeTmp := length;

  for i := 0 to MAXCHANNELS - 1 do
  begin
    SetLength(v.FBufferTmp[i],length);

    for k := 0 to length - 1 do
      v.FBufferTmp[i][k] := 0;
  end;

  Result := S_OK;
end;

function TMSourceBuffer.setBuffer(index: Integer; buffer: TDynamicDouble): HRESULT; stdcall;
var
  v: PBCVoice;
  i: Integer;
begin
  v := FPin.VoiceCheck(0);

  if v = nil then
    exit;

  if index >= FNumChannels then
    Exit;

  for i := 0 to v.FBufferSizeTmp - 1 do
    v.FBufferTmp[index][i] := buffer[i];

  Result := S_OK;
end;


function TMSourceBuffer.setBufferChanged: HRESULT; stdcall;
var
  v: PBCVoice;
begin
  v := FPin.VoiceCheck(0);

  if v = nil then
    exit;

  v.FChange := true;

  Result := S_OK;
end;

function TMSourceBuffer.SetChannelConfiguration(ActiveChannelCount: Integer; ChannelCode: Integer): HRESULT; stdcall;
var
  i: Integer;
begin
 GlobalNumActiveChannels := ActiveChannelCount;
 GLobalChannelCode := ChannelCode;

 Result := S_OK;
end;

function TMSourceBuffer.SetBlockSize(BlockSize: Integer): HRESULT; stdcall;
begin
  GlobalBlockSize := BlockSize;

  Result := S_OK;
end;


function TMSourceBuffer.GetActualBlockSize(out ActualBlockSize: Integer): HRESULT; stdcall;
begin           
 ActualBlockSize := GlobalBlockSize;

 Result := S_OK;
end;

function TMSourceBuffer.WriteBuffer(bufferSize: Integer; indexCount: Integer; indexPtr: PMValue; channelNumber: Integer; channelCount: TSourceIntArray; channelPtr: TSourcePtrArray): HRESULT; stdcall;
var
  v     : PBCVoice;
  i,k   : Integer;
  index : Integer;
  ptr   : PMValue;
begin
  v := FPin.VoiceCheck(0);

  if v = nil then
    exit;

  v.FChange := true;

  if bufferSize <> v.FBufferSize then
  begin
    v.FBufferSize := bufferSize;
    v.FFraction   := 1 / bufferSize;

    for k := 0 to channelNumber - 1 do
    SetLength(v.FBuffer[k],bufferSize);
  end;

  for i := 0 to indexCount - 1 do
  begin
    index := trunc(indexPtr^);

    for k := 0 to channelNumber - 1 do
    begin
      ptr := channelPtr[k];

      Inc(ptr,i mod channelCount[k]);

      v.FBuffer[k][index] := ptr^;
    end;

    Inc(indexPtr);
  end;//end for i

  v.FChange := false;

  Result := S_OK;
end;

function TMSourceBuffer.DumpBuffer(Filename: String): HRESULT;
var
  v          : PBCVoice;
  i,k        : integer;
  count      : longlong;
  length     : longlong;
  bytesWrote : Cardinal;
  hr         : HRESULT;
  pFilename   : PWideChar;
  tmpWideChar: PWideChar;
begin
  v := FPin.VoiceCheck(0);

  if v = nil then
    exit;

  if FPin.pwf = nil then
    exit;

  length := v.FBufferSize * v.FNumChannels;
  count := 0;

  SetLength(FData, length);
  SetLength(FBuffer, length);

  for i := 0 to v.FBufferSize - 1 do
    for k := 0 to v.FNumChannels - 1 do
    begin
      FData[count] := v.FBuffer[k][i];

      if FData[count] > 1 then
        FData[count] := 1;

      if FData[count] <-1 then
        FData[count] :=-1;

      inc(count);
    end;

  for i := 0 to length - 1 do
    FBuffer[i] := round(FData[i] * HALFWAVE);

  GetMem(pFilename, STRLENGTH*2);

  if Filename <> '' then
  begin
    Filename := Filename + '.wav';
    StringToWideChar(Filename, pFilename, STRLENGTH);
  end
  else
    StringToWideChar('SourceBuffer_Dump.wav', pFilename, STRLENGTH);

  if(Succeeded(FWaveFile.Open(pFilename, @FPin.pwf.Format, WAVEFILE_WRITE))) then
    FWaveFile.Write(length, FBuffer, bytesWrote);

  FWaveFile.Close();

  Result := S_OK;
end;


function TMSourceBuffer.setChannelVolume(val: TChannelVolume): HRESULT; stdcall;
var
  v: PBCVoice;
  i: Integer;
begin
  v := FPin.VoiceCheck(0);

  if v = nil then exit;

  for i := 0 to v.FNumChannels - 1 do
  v.FChannelVolume [i] := val [i];

  Result := S_OK;
end;

function TMSourceBufferPin.GetMediaType(pmt: PAMMediaType): HResult;
begin
  pwf := CoTaskMemAlloc(sizeof(TWAVEFORMATEXTENSIBLE));
  if pwf = nil then
  begin
    result := E_OUTOFMEMORY;
    exit;
  end;

  pmt.pbFormat             := @pwf.format;
  pmt.cbFormat             := sizeof(TWAVEFORMATEXTENSIBLE);
  pmt.majortype            := MEDIATYPE_Audio;
  pmt.subtype              := MEDIASUBTYPE_PCM;
  pmt.formattype           := FORMAT_WaveFormatEx;
  pmt.bFixedSizeSamples    := TRUE;
  pmt.bTemporalCompression := FALSE;
  pmt.lSampleSize          := pwf.Format.nBlockAlign;
  pmt.pUnk                 := nil;

  pwf.format.cbSize          := sizeof(TWAVEFORMATEXTENSIBLE) + sizeof(TWAVEFORMATEX);
  pwf.Format.wFormatTag      := WAVE_FORMAT_EXTENSIBLE;
  //pwf.Format.wFormatTag    := WAVE_FORMAT_PCM; //for the VSTHost
  pwf.format.nChannels       := GlobalNumActiveChannels;
  pwf.format.nSamplesPerSec  := SAMPLERATE;
  pwf.format.wBitsPerSample  := BITSPERSAMPLE;
  pwf.format.nBlockAlign     := (pwf.format.wBitsPerSample * pwf.format.nChannels) div BITSPERBYTE;
  pwf.format.nAvgBytesPerSec := pwf.format.nBlockAlign * pwf.format.nSamplesPerSec;

  pwf.Samples.wValidBitsPerSample := BITSPERSAMPLE;
  pwf.Samples.wSamplesPerBlock    := 0;
  pwf.Samples.wReserved           := 0;
  pwf.SubFormat                   := KSDATAFORMAT_SUBTYPE_PCM;
  pwf.dwChannelMask               := GlobalChannelCode;

  Result := S_OK;
end;


function TMSourceBufferPin.DecideBufferSize(Allocator: IMemAllocator;
  Properties: PAllocatorProperties): HRESULT;
var
  Actual: ALLOCATOR_PROPERTIES;
begin
  ASSERT(Allocator <> nil);
  ASSERT(Properties <> nil);

  Properties.cbBuffer := GlobalBlockSize * 2 * GlobalNumActiveChannels;
  Properties.cBuffers := 1;
  Properties.cbAlign  := GlobalBlockSize * 2 * GlobalNumActiveChannels;
  Properties.cbPrefix := 0;

  // Ask the allocator to reserve us the memory
  Result := Allocator.SetProperties(Properties^, Actual);

  if Result <> S_OK then
  begin
   GlobalBlockSize := 100;
   DecideBufferSize(Allocator,Properties);
  end;

  Result := S_OK;
end;

function TMVoice.ReadTheFile(AFileName: PChar): Boolean;
begin
  Result := true;
end;

function TMVoice.FillBuffer(mediaBuffer: PDouble; size: integer): HRESULT;
var
  i            : Integer;
  numFrames    : Integer;
  numChannels  : Integer;
  sampleLeft   : PDouble;
  sampleRight  : PDouble;
  sampleCenter : PDouble;
  position     : Integer;
  loop         : Boolean;
  phaseStart   : Double;
  phaseEnd     : Double;
  interval     : Double;
  pitch        : Double;
  gain         : Double;
  channelVolume: TChannelVolume;

  procedure ChangeBuffer();
  var
    i,k: Integer;
  begin
    FBufferSize := FBufferSizeTmp;

    for i := 0 to MAXCHANNELS - 1 do
    begin
      SetLength(FBuffer[i],FBufferSize);

      for k := 0 to FBufferSize - 1 do
        FBuffer [i][k] := FBufferTmp  [i][k];
    end;

    FChange   := false;
    FFraction := 1.0 / FBufferSize;
  end;

  procedure SetFrame (i: Integer; position: Integer);
  var
    channel: Integer;
    sample : PDouble;
    shift  : Integer;
  begin
    for channel := 0 to numChannels - 1 do
    begin
      sample := mediaBuffer;
      Inc(sample,(i*numChannels) + channel);

      if (position < 0) or (position >= FBufferSize) then
        sample^ := 0
      else
      begin
        sample^:= FBuffer[channel][position] * gain * channelVolume[channel];;

        if sample^ > HALFWAVE then
          sample^ := HALFWAVE;

        if sample^ < HALFWAVE * -1 then
          sample^ := HALFWAVE * -1;
      end;
    end;
  end;

  function PositionToPhase(value: Integer): Double;
  begin
    if value >= FBufferSize then
    begin
      Result := 1;
      Exit;
    end;

    if value < 0 then
    begin
      Result := 0;
      Exit;
    end;

    Result := value / FBufferSize;
  end;

  procedure SetVariables;
  var
    i: integer;
  begin
    numChannels := FNumChannels;
    numFrames   := trunc(size / numChannels);
    loop        := FLoop;
    phaseStart  := PositionToPhase(FStartPosition);
    phaseEnd    := PositionToPhase(FEndPosition);
    interval    := phaseEnd - phaseStart;
    position    := FFramePosition;
    pitch       := FPitch;
    gain        := FGain * HALFWAVE;

    for i := 0 to FNumChannels - 1 do
      channelVolume [i] := FChannelVolume [i];
  end;

  procedure SetPosition();
  var
    phaseSeek: Double;
  begin
    phaseSeek := PositionToPhase(FSeekPosition);

    if loop then
    begin
      if (phaseSeek >= phaseStart) and (phaseSeek <= phaseEnd) then
        phaseSeek := phaseSeek - phaseStart
      else
      if phaseSeek < phaseStart then
        phaseSeek := phaseStart
      else
      if phaseSeek > phaseEnd then
        phaseEnd  := phaseSeek;
      end;

      if not loop then
      begin
        if phaseSeek < 0 then
          phaseSeek := 0;

        if phaseSeek >= 1.0  then
          phaseSeek := 1.0;
      end;

    FPhase  := phaseSeek;
    FDoSeek := false;
  end;

  function PhaseToPosition: Integer;
  var
    value: Double;
    next : Integer;
    valid: Boolean;
  begin
    value  := FPhase * FBufferSize;
    next   := round(value);

    if (next < 0) or (next >= FBufferSize)then
      valid := false
    else
      valid := true;

    FPhase := FPhase + (FFraction * pitch);

    if FPhase <= 0.0 + NANOSHIFT then
      FPhase := 0;

    if FPhase >= 1.0 - NANOSHIFT then
      FPhase := 1.0;

    if valid then
      position := round(value);

    if valid then
      Result := position
    else
      Result := -1;
  end;

  function PhaseToPositionLoop: Integer;
  var
    i       : Integer;
    value   : Double;
  begin
    if interval <= 0 then
    begin
      Result   := -1;
      Exit;
    end;

    value := FPhase * FBufferSize;

    position := round(value);

    while position < 0 do
      position := position + FBufferSize;

    position := position mod FBufferSize;

    FPhase := FPhase + (FFraction * Pitch);

    if FPhase >= 1.0 - NANOSHIFT  then
      FPhase := 0.0;

    Result := position;

    if pitch > 0 then
      if FPhase <= PhaseStart - NANOSHIFT then
        FPhase := PhaseStart
      else if FPhase >= PhaseEnd - NANOSHIFT then
        FPhase := PhaseStart;

    if pitch < 0 then
      if FPhase <= PhaseStart then
        FPhase := PhaseEnd
      else if FPhase >= PhaseEnd - NANOSHIFT then
        FPhase := PhaseEnd;
  end;

//----------------------------------------------------------------------------//

begin
  if FChange then
    ChangeBuffer();

  SetVariables;

  if FDoSeek then
    SetPosition();

  for i := 0 to numFrames - 1 do
  begin
    if FBufferSize = 0 then Continue;

    if loop then
      SetFrame(i,PhaseToPositionLoop);

    if not loop then
      SetFrame(i,PhaseToPosition);
  end;

  FFramePosition := position;
end;

//----------------------------------------------------------------------------//


function TMSourceBufferPin.FillBuffer(ims: IMediaSample): HResult;
var
  size, i: longint;
  p: PByte;
  pp: PAudioSample;
  pf, buf: PDouble;
  vcr: double;
  count: integer;
  v: PBCVoice;
  hr: HResult;
begin
  result := -1;
  if ims = nil then
    exit;
  //ASSERT(ims <> nil);

  size := ims.GetSize div 2;
  //Outputdebugstring(pchar(format('pins buffersize: %d',[size])));

  buf := CoTaskMemAlloc(size * sizeof(double));
  if buf = nil then
  begin
    result := E_OUTOFMEMORY;
    exit;
  end;

  pf := buf;
  for i := 1 to size do
  begin
    pf^ := 0;
    Inc(pf);
  end;

  count := 0;
  i := VoiceList.Count;

  if voicecount < i then i := voicecount;
  while i > 0 do
  begin
    Dec(i);
    v := VoiceList.Items[i];
    if v <> nil then
    begin
      Inc(count);
      if v.FPlay then
      begin
        v.FillBuffer(buf, size);
      end;
    end;
  end;

  ims.GetPointer(p);
  pp := PAudioSample(p);

  vcr := 1.;
  //vcr := 1./count;  //???

  pf := buf;

  for i := 1 to size do
  begin
    pp^ := round(pf^ * vcr);
    Inc(pp);
    Inc(pf);
  end;
  cotaskmemfree(buf);

  Result := S_OK;
end;

// --- TMSourceBuffer ------------

constructor TMSourceBuffer.Create(ObjName: string; Unk: IUnKnown;
  out hr: HRESULT);
begin
  inherited Create(ObjName, Unk, CLSID_SourceBuffer);

  FBuffer      := nil;
  FData        := nil;
  FWaveFile    := CWaveFile.Create;

  FNumChannels := GlobalNumActiveChannels;

  // The pin magically adds itself to our pin array.
  FPin := TMSourceBufferPin.Create(hr, Self);

  if (hr <> S_OK) then
    if (FPin = nil) then
      hr := E_OUTOFMEMORY;
end;

constructor TMSourceBuffer.CreateFromFactory(Factory: TBCClassFactory;
  const Controller: IUnknown);
var
  hr: HRESULT;
begin
  Create(Factory.Name, Controller, hr);
end;

destructor TMSourceBuffer.Destroy;
begin
  FreeAndNil(FPin);
  inherited;
end;

function TMSourceBufferPin.VoiceCheck(index: Integer): PBCVoice;
var
  v: PBCVoice;
begin
  if index >= VoiceList.Count then
    VoiceList.Count := index + 1;

  if VoiceList.Items[index] = nil then
  begin
    v := CoTaskMemAlloc(sizeof(TMVoice));
    if v <> nil then
      v^ := TMVoice.Create;
    VoiceList.Items[index] := v;
  end;

  if VoiceList.Items[index] = nil then
    outputdebugstring(pchar(format('vc %d nil!',[index])));

  Result := VoiceList.Items[index];
end;

procedure TMSourceBufferPin.VoiceFree(index: Integer);
var
  v, vv: PBCVoice;
  i: Integer;
begin
  {
  v := VoiceList.Items[index];
  for i := 0 to VoiceList.Count - 1 do if i <> index then
  begin
    if assigned(VoiceList.Items[i]) then
    begin
      vv := VoiceList.Items[i];
      if vv.FFileName = v.FFileName then exit;
    end;
  end;
  if assigned(v.FData) then
    CoTaskMemFree(v.FData);
  v.FData := nil;
  v.FFileName := '';
  }                   
end;

initialization
  // provide entries in the CFactoryTemplate array
  TBCClassFactory.CreateFilter(TMSourceBuffer, FilterName,
    CLSID_SourceBuffer, CLSID_LegacyAmFilterCategory,
    MERIT_DO_NOT_USE, 1, @sudOutputPin
    );
end.



