//////project name
//WavePlayer

//////description
//directshow source filter.
//plays several wavefiles in parallel and can do some granular stuff and more..

//////licence
//GNU Lesser General Public License (LGPL)
//english: http://www.gnu.org/licenses/lgpl.html
//german: http://www.gnu.de/lgpl-ger.html

//////language/ide
//delphi 5

//////dependencies
//directshow baseclasses coming with DSPack2.3.4:
//http://www.progdigy.com/modules.php?name=DSPack

//////initial author
//jhno -> ear@subminimal.com

//////edited by
//your name here
//joreg@meso.net
//jens@meso.net


unit Main;

interface

uses
  Classes, SysUtils, ActiveX, MMSystem, Math, Windows,
  BaseClass, DirectShow9, DSUtil, DirectSound;

const
  CLSID_SourceBuffer : TGUID = '{6F7AAC61-8E33-4cf4-A349-7976C08D7CCD}';
  FilterName: String = 'SourceBuffer';

  WAVEBUFFERSIZE =   256;
  //NCHANNELS    =     3;
  NCHANNELS      =     2;
  SAMPLERATE     = 44100;
  BITSPERSAMPLE  =    16;
  BITSPERBYTE    =     8;
  HALFWAVE       = 32767;
  NANOSHIFT      = 1.0 / 1000000000;

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

 TDynamicDouble = Array of Double;

 ISourceBuffer = interface(IUnknown)
    ['{6F7AAC61-8E33-4cf4-A349-7976C08D7CCD}']
    function Voices(voicecount: integer): HResult; stdcall;
    function Play(index: integer; state : integer): HResult; stdcall;
    function Volume(index: integer; val : double): HResult; stdcall;
    function Pitch(index: integer; val : double): HResult; stdcall;
    function Loop(index: integer; state:integer):HResult; stdcall;
    function DoSeek(index: integer; state:integer):HResult; stdcall;
    function StartPosition(index: integer; val:integer):HResult; stdcall;
    function EndPosition(index:integer; val:integer):HResult; stdcall;
    function SeekPosition(index:integer; val:integer):HResult; stdcall;
    function GetActualFrame(index:integer; out val:Integer):HResult; stdcall;
    function setBufferLeft(index : Integer; length : Integer; buffer : TDynamicDouble) : HRESULT; stdcall;
    function setBufferRight(index : Integer; length : Integer; buffer : TDynamicDouble) : HRESULT; stdcall;
    function setBufferCenter(index : Integer; length : Integer; buffer : TDynamicDouble) : HRESULT; stdcall;
    function setChannels(number : Integer) : HRESULT; stdcall;
  end;

  PAudioSample = ^AudioSample;
  AudioSample = -32767..32767;       // for 16-bit audio
  PAudioSample8 = ^AudioSample8;
  AudioSample8 = -16383..16384;       // for 16-bit audio

  PBCVoice = ^TMVoice;
  TMVoice = class(TObject)
  private
    FPlay: Boolean;
    FGain: double;
    FPhase: double;
    FPitch: double;
    FStartPosition : Integer;
    FEndPosition : Integer;
    FSeekPosition : Integer;
    FDoSeek : Boolean;
    FFramePosition : Integer;
    FLoop : Boolean;
    FFading : Double;
    FBuffer : TDynamicDouble;
    FBufferTmp : TDynamicDouble;
    FBufferSize : Integer;
    FBufferSizeTmp : Integer;
    FChange : Boolean;
    FFraction : Double;

    FBufferLeft      : TDynamicDouble;
    FBufferRight     : TDynamicDouble;
    FBufferCenter    : TDynamicDouble;
    FBufferLeftTmp   : TDynamicDouble;
    FBufferRightTmp  : TDynamicDouble;
    FBufferCenterTmp : TDynamicDouble;

  public
    destructor Destroy; override;
    constructor Create;
    function ReadTheFile(AFileName: PChar): Boolean;
    function FillBuffer( mediaBuffer : PDouble; size: integer): HResult;
  end;

  TMSourceBufferPin = class(TBCSourceStream)
  private
    voicelist: TList;
    voicecount: Integer;
  public

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
  public
    constructor Create(ObjName: string; Unk: IUnKnown; out hr: HRESULT);
    constructor CreateFromFactory(Factory: TBCClassFactory; const Controller: IUnknown); override;
    destructor Destroy; override;

    function NonDelegatingQueryInterface(const IID: TGUID; out Obj): HResult; override;
    function Voices(voicecount: integer): HResult; stdcall;
    function Play(index: integer; state : integer): HResult; stdcall;
    function Volume(index: integer; val : double): HResult; stdcall;
    function Pitch(index: integer; val : double): HResult; stdcall;
    function Loop(index: integer; state:integer):HResult; stdcall;
    function DoSeek(index: integer; state:integer):HResult; stdcall;
    function StartPosition(index: integer; val:integer):HResult; stdcall;
    function EndPosition(index:integer; val:integer):HResult; stdcall;
    function SeekPosition(index:integer; val:integer):HResult; stdcall;
    function GetActualFrame(index:integer; out val:Integer):HResult; stdcall;
    function setBufferLeft(index : Integer; length : Integer; buffer : TDynamicDouble) : HRESULT; stdcall;
    function setBufferRight(index : Integer; length : Integer; buffer : TDynamicDouble) : HRESULT; stdcall;
    function setBufferCenter(index : Integer; length : Integer; buffer : TDynamicDouble) : HRESULT; stdcall;
    function setChannels(number : Integer) : HRESULT; stdcall;
 end;

var

 GlobalNumChannels : Integer = NCHANNELS;


implementation

uses Variants;

function wrap(val: Double) : Double;
begin
  if val >= 1. then result := val - floor(val)
  else if val < 0. then result := val - floor(val)
  else result := val;
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
  i : Integer;
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
  FBufferSize     :=  0;
  FBufferSizeTmp  :=  0;
  FPhase          :=  0;

  SetLength(FBuffer,0);
  SetLength(FBufferTmp,0);

  SetLength(FBufferLeft,0);
  SetLength(FBufferRight,0);
  SetLength(FBufferCenter,0);
  SetLength(FBufferLeftTmp,0);
  SetLength(FBufferRightTmp,0);
  SetLength(FBufferCenterTmp,0);


end;

destructor TMVoice.Destroy;
begin
  SetLength(FBuffer,0);

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

function TMSourceBuffer.Voices(voicecount: integer): HResult; stdcall;
begin
  Result := s_ok;
  FPin.voicecount := voicecount;
end;

function TMSourceBuffer.Play(index: integer; state: integer) : HResult; stdcall;
var
  v: PBCVoice;
  basefilter : IBaseFilter;

begin
  v := FPin.VoiceCheck(index);
  if v = nil then exit;

  v.FPlay := Boolean(state);

  Result  := S_OK
end;

function TMSourceBuffer.Volume(index: integer; val: double) : HResult; stdcall;
var
  v: PBCVoice;
begin
  v := FPin.VoiceCheck(index);
  if v = nil then exit;

  v.FGain := val;

  Result := S_OK
end;

function TMSourceBuffer.Pitch(index: integer; val: double) : HResult; stdcall;
var
  v: PBCVoice;
begin
  v := FPin.VoiceCheck(index);
  if v = nil then exit;

  v.FPitch := val;

  Result := S_OK
end;

function TMSourceBuffer.Loop(index:integer; state:integer) : HResult; stdcall;
var
  v: PBCVoice;
begin
  v := FPin.VoiceCheck(index);

  if v = nil then exit;

  v.FLoop := Boolean(state);

  Result := S_OK;
end;

function TMSourceBuffer.DoSeek(index: integer; state:integer):HResult; stdcall;
var
  v: PBCVoice;
begin
  v := FPin.VoiceCheck(index);

  if v = nil then exit;

  v.FDoSeek := Bool(state);

  Result := S_OK;
end;

function TMSourceBuffer.SeekPosition(index:integer; val:Integer):HResult; stdcall;
var
  v: PBCVoice;
begin
  v := FPin.VoiceCheck(index);
  if v = nil then exit;

  v.FSeekPosition := val;

  Result := S_OK;
end;

function TMSourceBuffer.StartPosition(index: integer; val:integer):HResult; stdcall;
var
  v: PBCVoice;
begin
  v := FPin.VoiceCheck(index);

  if v = nil then exit;

  v.FStartPosition := val;

  Result := S_OK;
end;

function TMSourceBuffer.EndPosition(index:integer; val:integer):HResult; stdcall;
var
  v: PBCVoice;
begin
  v := FPin.VoiceCheck(index);

  if v = nil then exit;

  v.FEndPosition := val;

  Result := S_OK;
end;

function TMSourceBuffer.GetActualFrame(index:integer; out val:integer):HResult; stdcall;
var
  v: PBCVoice;
begin
  v := FPin.VoiceCheck(index);

  if v = nil then exit;

  val := v.FFramePosition;

  Result := S_OK;
end;

function TMSourceBuffer.setBufferLeft(index : Integer; length : Integer; buffer : TDynamicDouble) : HRESULT; stdcall;
var
  v: PBCVoice;
  i: Integer;
begin
  v := FPin.VoiceCheck(index);

  if v = nil then exit;

  v.FBufferSizeTmp := length;


  SetLength(v.FBufferLeftTmp,length);

  for i:= 0 to length - 1 do
  if (buffer[i] <= 1) and (buffer[i] >= -1) then
   v.FBufferLeftTmp[i] := buffer[i]
  else
   v.FBufferLeftTmp[i] := 0;

  //v.FChange := true;

end;

function TMSourceBuffer.setBufferRight(index : Integer; length : Integer; buffer : TDynamicDouble) : HRESULT; stdcall;
var
  v: PBCVoice;
  i: Integer;
begin
  v := FPin.VoiceCheck(index);

  if v = nil then exit;

  v.FBufferSizeTmp := length;


  SetLength(v.FBufferRightTmp,length);

  for i:= 0 to length - 1 do
  if (buffer[i] <= 1) and (buffer[i] >= -1) then
   v.FBufferRightTmp[i] := buffer[i]
  else
   v.FBufferRightTmp[i] := 0;

  //v.FChange := true;

end;

function TMSourceBuffer.setBufferCenter(index : Integer; length : Integer; buffer : TDynamicDouble) : HRESULT; stdcall;
var
  v: PBCVoice;
  i: Integer;
begin
  v := FPin.VoiceCheck(index);

  if v = nil then exit;

  v.FBufferSizeTmp := length;

  SetLength(v.FBufferCenterTmp,length);

  for i:= 0 to length - 1 do
  if (buffer[i] <= 1) and (buffer[i] >= -1) then
   v.FBufferCenterTmp[i] := buffer[i]
  else
   v.FBufferCenterTmp[i] := 0;
  
  v.FChange := true;

end;

function TMSourceBuffer.setChannels(number : Integer) : HRESULT; stdcall;
begin

  if (number = 1) or (number = 2) or (number = 3) then
  GlobalNumChannels := number;

end;


function TMSourceBufferPin.GetMediaType(pmt: PAMMediaType): HResult;
var
  pwf  : PWAVEFORMATEXTENSIBLE;

begin
  pwf := CoTaskMemAlloc(sizeof(TWAVEFORMATEXTENSIBLE));
  if pwf = nil then begin result := E_OUTOFMEMORY; exit; end;

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
  pwf.format.nChannels       := GlobalNumChannels;
  pwf.format.nSamplesPerSec  := SAMPLERATE;
  pwf.format.wBitsPerSample  := BITSPERSAMPLE;
  pwf.format.nBlockAlign     := (pwf.format.wBitsPerSample * pwf.format.nChannels) div BITSPERBYTE;
  pwf.format.nAvgBytesPerSec := pwf.format.nBlockAlign * pwf.format.nSamplesPerSec;

  pwf.Samples.wValidBitsPerSample := BITSPERSAMPLE;
  pwf.Samples.wSamplesPerBlock    := 0;
  pwf.Samples.wReserved           := 0;
  pwf.SubFormat                   := KSDATAFORMAT_SUBTYPE_PCM;

   if (GlobalNumChannels = 1) then
  pwf.dwChannelMask := SPEAKER_FRONT_LEFT;

  if (GlobalNumChannels = 2) then
  pwf.dwChannelMask := SPEAKER_FRONT_LEFT or SPEAKER_FRONT_RIGHT;

  if (GlobalNumChannels = 3) then
  pwf.dwChannelMask := SPEAKER_FRONT_LEFT or SPEAKER_FRONT_RIGHT or SPEAKER_FRONT_CENTER;

  Result := S_OK;
end;


function TMSourceBufferPin.DecideBufferSize(Allocator: IMemAllocator;
  Properties: PAllocatorProperties): HRESULT;
var
  Actual: ALLOCATOR_PROPERTIES;
begin
  ASSERT(Allocator <> nil);
  ASSERT(Properties <> nil);

  Properties.cbBuffer := 100 * 2 * GlobalNumChannels;
  Properties.cBuffers := 1;
  Properties.cbAlign  := 100 * 2 * GlobalNumChannels;
  Properties.cbPrefix := 0;

  // Ask the allocator to reserve us the memory
  Result := Allocator.SetProperties(Properties^, Actual);

end;

function TMVoice.ReadTheFile(AFileName: PChar): Boolean;
begin
  Result := true;

end;

function TMVoice.FillBuffer(mediaBuffer : PDouble; size : integer): HRESULT;
var
  i             : Integer;
  numFrames     : Integer;
  numChannels   : Integer;
  sampleLeft    : PDouble;
  sampleRight   : PDouble;
  sampleCenter  : PDouble;
  position      : Integer;
  loop          : Boolean;
  phaseStart    : Double;
  phaseEnd      : Double;
  interval      : Double;
  pitch         : Double;
  gain          : Double;

  procedure ChangeBuffer();
  var
   i : Integer;
  begin
   FBufferSize := FBufferSizeTmp;

   SetLength( FBufferLeft,   FBufferSize);
   SetLength( FBufferRight,  FBufferSize);
   SetLength( FBufferCenter, FBufferSize);

   for i := 0 to FBufferSize - 1 do
   begin
     FBufferLeft  [i] := FBufferLeftTmp  [i];
     FBufferRight [i] := FBufferRightTmp [i];
     FBufferCenter[i] := FBufferCenterTmp[i];
   end;

   FChange   := false;
   FFraction := 1.0 / FBufferSize;

  end;

  procedure SetFrame( i : Integer );
  begin
    sampleLeft    := mediaBuffer;
    Inc(sampleLeft, numChannels * i);
    sampleLeft^   := 0;

   if numChannels > 1 then
   begin
    sampleRight   := mediaBuffer;
    Inc(sampleRight,(numChannels * i) + 1);
    sampleRight^  := 0;
   end;

   if numChannels > 2 then
   begin
    sampleCenter  := mediaBuffer;
    Inc(sampleCenter,(numChannels * i) + 2);
    sampleCenter^ := 0;
   end;

  end;

  procedure FillFrame;
  begin
   if (position >= FBufferSize) or (position < 0) then
   Exit;

    sampleLeft^   := FBufferLeft   [position] * gain;

    if sampleLeft^ < -1 * HALFWAVE then
     sampleLeft^ := -1 * HALFWAVE
    else
    if sampleLeft^ > HALFWAVE then
     sampleLeft^ := HALFWAVE;

   if numChannels > 1 then
   begin
    sampleRight^  := FBufferRight  [position] * gain;

    if sampleRight^ < -1 * HALFWAVE then
     sampleRight^ := -1 * HALFWAVE
    else
    if sampleRight^ > HALFWAVE then
     sampleRight^ := HALFWAVE;
   end;

   if numChannels > 2 then
   begin
    sampleCenter^ := FBufferCenter [position] * gain;

    if sampleCenter^ < -1 * HALFWAVE then
     sampleCenter^ := -1 * HALFWAVE
    else
    if sampleCenter^ > HALFWAVE then
     sampleCenter^ := HALFWAVE;
   end;

  end;


  function PositionToPhase(value : Integer) : Double;
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
  begin
   numChannels := GlobalNumChannels;
   numFrames   := trunc(size / numChannels);
   loop        := FLoop;
   phaseStart  := PositionToPhase(FStartPosition);
   phaseEnd    := PositionToPhase(FEndPosition);
   interval    := phaseEnd - phaseStart;
   position    := FFramePosition;
   pitch       := FPitch;
   gain        := FGain * HALFWAVE;

  end;

  procedure SetPosition();
  var
   phaseSeek : Double;

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

  function PhaseToPosition : Boolean;
  var
   value : Double;
   next  : Integer;
   valid : Boolean;

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

   Result := valid;

  end;

  function PhaseToPositionLoop : Boolean;
  var
   i     : Integer;
   value : Double;

  begin

   if interval <= 0 then
   begin
    Result := false;
    Exit;
   end;

   value := FPhase * FBufferSize;

   position := round(value);


   while position < 0 do
   position := position + FBufferSize;

   position := position mod FBufferSize;


   FPhase := FPhase + (FFraction * Pitch);

   if pitch > 0 then
   begin
    if FPhase <= PhaseStart - NANOSHIFT then
     FPhase := PhaseStart
    else
    if FPhase >= PhaseEnd - NANOSHIFT then
     FPhase := PhaseStart + (FPhase - PhaseEnd);
   end;

   if pitch < 0 then
   begin
    if FPhase >= PhaseEnd - NANOSHIFT then
     FPhase := PhaseEnd
    else
    if FPhase <= PhaseStart + NANOSHIFT then
     FPhase := PhaseEnd - (PhaseStart - FPhase);
   end;

   Result := true;

  end;


//----------------------------------------------------------------------------//

begin

  if FChange then ChangeBuffer();

  SetVariables;

  if FDoSeek then SetPosition();


  for i := 0 to numFrames - 1 do
  begin
   SetFrame(i);

   if FBufferSize <= 0 then Continue;

   if loop then
   if PhaseToPositionLoop then
    FillFrame;

   if not loop then
   if PhaseToPosition then
    FillFrame;

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
  hr : HResult;

begin

  result := -1;
  if ims = nil then exit;
  //ASSERT(ims <> nil);

  size := ims.GetSize div 2;
  //Outputdebugstring(pchar(format('pins buffersize: %d',[size])));

  buf := CoTaskMemAlloc(size * sizeof(double));
  if buf = nil then begin result := E_OUTOFMEMORY; exit; end;

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
  result := S_OK;
end;

// --- TBCWavePlayer ------------

constructor TMSourceBuffer.Create(ObjName: string; Unk: IUnKnown;
  out hr: HRESULT);
begin
  inherited Create(ObjName, Unk, CLSID_SourceBuffer);

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
  if index >= VoiceList.Count then VoiceList.Count := index + 1;
  if VoiceList.Items[index] = nil then
  begin
    v := CoTaskMemAlloc(sizeof(TMVoice));
    if v <> nil then v^ := TMVoice.Create;
    VoiceList.Items[index] := v;
  end;
  if VoiceList.Items[index] = nil then outputdebugstring(pchar(format('vc %d nil!',[index])));
  result := VoiceList.Items[index];
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



