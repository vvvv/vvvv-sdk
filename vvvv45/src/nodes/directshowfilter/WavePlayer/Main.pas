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
//directshow baseclasses coming with DSPack:
//http://sourceforge.net/projects/dspack/

//////initial author
//jhno -> ear@subminimal.com

//////edited by
//your name here
//xsnej@gmx.de

unit Main;

interface

uses
  Classes, SysUtils, ActiveX, MMSystem, Math, Windows,
  BaseClass, DirectShow9, DSUtil, DirectSound;

const
  CLSID_WavePlayer : TGUID = '{79A98DE0-BC00-11ce-AC2E-444553540001}';
  FilterName: String = 'WavePlayer';

  HALFWAVE       =  32767;
  MAXWAVENEGATIV = -32767;
  MAXWAVEPOSITIV =  32767;
  BYTESPERSAMPLE =  2;
  WAVEBUFFERSIZE =  256;
  NUMCHANNELS    =  2;
  SAMPLERATE     =  44100;
  BITSPERSAMPLE  =  16;
  BITSPERBYTE    =  8;

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

  IWavePlayer = interface(IUnknown)
    ['{79A98DE0-BC00-11ce-AC2E-444553540001}']
    function Voices(voicecount: integer): HResult; stdcall;
    function Read(voice: integer; FileName : String): HResult; stdcall;
    function Play(voice: integer; state : integer): HResult; stdcall;
    function Volume(voice: integer; val : double): HResult; stdcall;
    function Pan(voice: integer; val : double): HResult; stdcall;
    function Phase(voice: integer; val : double): HResult; stdcall;
    function Pitch(voice: integer; val : double): HResult; stdcall;
    function Sync(index: integer; state: integer): HResult; stdcall;
    function Loop(index: integer; state:integer):HResult; stdcall;
    function DoSeek(index: integer; state:integer):HResult; stdcall;
    function StartTime(index: integer; val:double):HResult; stdcall;
    function EndTime(index:integer; val:double):HResult; stdcall;
    function SeekPosition(index:integer; val:double):HResult; stdcall;
    function GetDuration(index:integer; out val:double):HResult; stdcall;
    function GetActualPosition(index:integer; out val:double):HResult; stdcall;
    function Fading(index: integer; val:double) : HResult; stdcall;
    function setBlockSize( size : Integer) : HRESULT; stdcall;
    function getBlockSize(out size : Integer) : HRESULT; stdcall;
  end;

  PAudioSample = ^AudioSample;
  AudioSample = -32767..32767;       // for 16-bit audio
  PAudioSample8 = ^AudioSample8;
  AudioSample8 = -16383..16384;       // for 16-bit audio

  PBCVoice = ^TMVoice;
  TMVoice = class(TObject)
  private
    FFileName: String;
    FSize: LONGLONG;
    FData: PByte;
    FWheel: Double;
    FPlay: Boolean;
    FSync: Boolean;
    FGain: double;
    FPan: double;
    FPosition: double;
    FPhase: double;
    FPitch: double;
    FStartTime,FFStartTime : double;
    FEndTime,FFEndTime : double;
    FSeekPosition : double;
    FDoSeek : Boolean;
    FDuration : double;
    FActualPosition : double;
    FLoop : Boolean;
    FFading : Double;
    FFrameFraction : Double;
    FReload : Boolean;
    FSourceFrames : Integer;
    FPhaseShift : Double;
    FPhaseShiftPrev : Double;
    FBytesPerSample : Integer;

    wf: TWAVEFORMATEX;
  public
    destructor Destroy; override;
    constructor Create;
    function ReadTheFile(AFileName: PChar): Boolean;
    function FillBuffer(mediaSample: PDouble; size: integer): HResult;
  end;

  TMWavePlayerPin = class(TBCSourceStream)
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

  TMWavePlayer = class(TBCSource, IWavePlayer)
  private
    FPin: TMWavePlayerPin;
  public
    constructor Create(ObjName: string; Unk: IUnKnown; out hr: HRESULT);
    constructor CreateFromFactory(Factory: TBCClassFactory; const Controller: IUnknown); override;
    destructor Destroy; override;

    function NonDelegatingQueryInterface(const IID: TGUID; out Obj): HResult; override;
    function Voices(voicecount: integer): HResult; stdcall;
    function Read(index: integer; FileName : String): HResult; stdcall;
    function Play(index: integer; state: integer): HResult; stdcall;
    function Volume(index: integer; val: double): HResult; stdcall;
    function Pan(index: integer; val: double): HResult; stdcall;
    function Phase(index: integer; val: double): HResult; stdcall;
    function Pitch(index: integer; val: double): HResult; stdcall;
    function Sync(index: integer; state: integer): HResult; stdcall;
    function Loop(index: integer; state:integer):HResult; stdcall;
    function DoSeek(index: integer; state:integer):HResult; stdcall;
    function StartTime(index: integer; val:double):HResult; stdcall;
    function EndTime(index:integer; val:double):HResult; stdcall;
    function SeekPosition(index:integer; val:double):HResult; stdcall;
    function GetDuration(index:integer; out val:double):HResult; stdcall;
    function GetActualPosition(index:integer; out val:double):HResult; stdcall;
    function Fading(index: integer; val:double) : HResult; stdcall;
    function setBlockSize( size : Integer) : HRESULT; stdcall;
    function getBlockSize(out size : Integer) : HRESULT; stdcall;
  end;

implementation

uses Variants;

var

 GlobalNumActiveChannels : Integer = 2;
 GlobalChannelCode       : Integer = 0;
 GlobalBlockSize         : Integer = 128;

function wrap(val: Double) : Double;
begin
  if val >= 1. then result := val - floor(val)
  else if val < 0. then result := val - floor(val)
  else result := val;
end;

constructor TMWavePlayerPin.Create(out hr: HResult; Filter: TBCSource);
begin
  inherited Create(FilterName, hr, Filter, 'Out');
  VoiceList := TList.Create;
  VoiceList.Clear;
end;

destructor TMWavePlayerPin.Destroy;
var
  i: Integer;
begin
  for i := 0 to VoiceList.Count - 1 do
  begin
    if assigned(VoiceList.Items[i]) then VoiceFree(i);
  end;
  VoiceList.Free;
  inherited;
end;

constructor TMVoice.Create;
begin
  FPlay           :=  false;
  FSync           :=  false;
  FDoSeek         :=  false;
  FLoop           :=  false;
  FReload         :=  false;
  FGain           :=  1;
  FPan            :=  0.5;
  FPosition       :=  0;
  FPhase          :=  0;
  FPitch          :=  1;
  FStartTime      :=  0;
  FFStartTime     :=  0;
  FEndTime        :=  0;
  FFEndTime       :=  0;
  FSeekPosition   :=  0;
  FDuration       :=  0;
  FActualPosition :=  0;
  FSourceFrames   :=  0;
  FFading         :=  0;
  FFileName       :=  '';
  FSize           :=  0;
  FData           :=  nil;
  FWheel          :=  0;
  //FFrameFraction  :=  1;
  FPhaseShift     :=  0;
  FPhaseShiftPrev :=  0;

end;

destructor TMVoice.Destroy;
begin
  if Assigned(FData) then
  begin
    CoTaskMemFree(FData);
    FData := nil;
    FSize := 0;
  end;
  inherited;
end;

function TMWavePlayerPin.Notify(Filter: IBaseFilter; q: TQuality): HRESULT;
begin
  Result := E_FAIL;
end;

function TMWavePlayer.NonDelegatingQueryInterface(const IID: TGUID;
  out Obj): HResult;
begin
  if IsEqualGUID(IID, CLSID_WavePlayer) then
    if GetInterface(CLSID_WavePlayer, Obj) then
      Result := S_OK
    else
      Result := E_FAIL
  else
    Result := Inherited NonDelegatingQueryInterface(IID, Obj);
end;

function TMWavePlayer.Voices(voicecount: integer): HResult; stdcall;
begin
  Result := s_ok;
  FPin.voicecount := voicecount;
end;

function TMWavePlayer.Read(index: integer; FileName : String) : HResult; stdcall;
var
  v,vv: PBCVoice;
  i: Integer;
begin
  v := FPin.VoiceCheck(index);

  if v = nil then
  begin
   Result := ERROR;
   exit;
  end;

  Result := S_OK;
  if FileName = v.FFileName then exit;
  i := FPin.VoiceList.Count - 1;
  while i >= 0 do
  begin
    vv := FPin.VoiceList.Items[i];     //Achtung!!!
    if vv.FFileName = FileName then
    begin
      FPin.VoiceFree(index);

      v.FFileName       :=  vv.FFileName;
      v.FSize           :=  vv.FSize;
      v.FData           :=  vv.FData;
      v.FWheel          :=  vv.FWheel;
      v.FPlay           :=  vv.FPlay;
      v.FSync           :=  vv.FSync;
      v.FGain           :=  vv.FGain;
      v.FPan            :=  vv.FGain;
      v.FPosition       :=  vv.FPosition;
      v.FPhase          :=  vv.FPhase;
      v.FPitch          :=  vv.FPitch;
      v.FStartTime      :=  vv.FStartTime;
      v.FFStartTime     :=  vv.FFStartTime;
      v.FEndTime        :=  vv.FEndTime;
      v.FFEndTime       :=  vv.FFEndTime;
      v.FSeekPosition   :=  vv.FSeekPosition;
      v.FDoSeek         :=  vv.FDoSeek;
      v.FDuration       :=  vv.FDuration;
      v.FActualPosition :=  vv.FActualPosition;
      v.FLoop           :=  vv.FLoop;
      v.FSourceFrames   :=  vv.FSourceFrames;
      v.FFading         :=  vv.FFading;
      v.FFrameFraction  :=  vv.FFrameFraction;
      v.wf              :=  vv.wf;
      v.FReload         :=  vv.FReload;
      v.FPhaseShift     :=  vv.FPhaseShift;
      v.FPhaseShiftPrev :=  vv.FPhaseShiftPrev;
      v.FBytesPerSample :=  vv.FBytesPerSample;


      //outputdebugstring(pchar(format('link %d %d %d %p',[index,i,v.FSize,v.FData])));
      exit;
    end;
    Dec(i);
  end;
  FPin.VoiceFree(index);
  v.FFileName := FileName;
  //outputdebugstring(pchar(format('read %d: %s',[index,filename])));
  v.ReadTheFile(pchar(FileName));
end;

function TMWavePlayer.Play(index: integer; state: integer) : HResult; stdcall;
var
  v: PBCVoice;

begin
  v := FPin.VoiceCheck(index);

  if v = nil then
  begin
   Result := ERROR;
   exit;
  end;

  v.FPlay := Boolean(state);

  Result  := S_OK
end;

function TMWavePlayer.Sync(index: integer; state: integer) : HResult; stdcall;
var
  v: PBCVoice;
begin
  v := FPin.VoiceCheck(index);

  if v = nil then
  begin
   Result := ERROR;
   exit;
  end;

  if state = 0 then
   v.FSync := false;

  if state = 1 then
   v.FSync := true;

  Result := S_OK
end;

function TMWavePlayer.Volume(index: integer; val: double) : HResult; stdcall;
var
  v: PBCVoice;
begin
  v := FPin.VoiceCheck(index);

  if v = nil then
  begin
   Result := ERROR;
   exit;
  end;

  v.FGain := val;

  Result := S_OK
end;

function TMWavePlayer.Pan(index: integer; val: double) : HResult; stdcall;
var
  v: PBCVoice;
begin
  v := FPin.VoiceCheck(index);

  if v = nil then
  begin
   Result := ERROR;
   exit;
  end;

  if (val>=0) and (val<=1) then
  v.FPan := val;

  Result := S_OK
end;

function TMWavePlayer.Phase(index: integer; val: double) : HResult; stdcall;
var
  v: PBCVoice;
begin
  v := FPin.VoiceCheck(index);

  if v = nil then
  begin
   Result := ERROR;
   exit;
  end;

  v.FPhaseShift := val;

  Result := S_OK
end;

function TMWavePlayer.Pitch(index: integer; val: double) : HResult; stdcall;
var
  v: PBCVoice;
begin
  v := FPin.VoiceCheck(index);

  if v = nil then
  begin
   Result := ERROR;
   exit;
  end;

  v.FPitch := val;

  Result := S_OK
end;

function TMWavePlayer.Loop(index:integer; state:integer) : HResult; stdcall;
var
  v: PBCVoice;
begin
  v := FPin.VoiceCheck(index);

  if v = nil then
  begin
   Result := ERROR;
   exit;
  end;

  v.FLoop := Boolean(state);

  Result := S_OK;
end;

function TMWavePlayer.DoSeek(index: integer; state:integer):HResult; stdcall;
var
  v: PBCVoice;
begin
  v := FPin.VoiceCheck(index);

  if v = nil then
  begin
   Result := ERROR;
   exit;
  end;

  v.FDoSeek := true;

  Result := S_OK;
end;

function TMWavePlayer.SeekPosition(index:integer; val:double):HResult; stdcall;
var
  v: PBCVoice;
begin
  v := FPin.VoiceCheck(index);

  if v = nil then
  begin
   Result := ERROR;
   exit;
  end;

  v.FSeekPosition := val;

  Result := S_OK;
end;

function TMWavePlayer.StartTime(index: integer; val:double):HResult; stdcall;
var
  v: PBCVoice;
begin
  v := FPin.VoiceCheck(index);

  if v = nil then
  begin
   Result := ERROR;
   exit;
  end;

  v.FFStartTime := val;

  Result := S_OK;
end;

function TMWavePlayer.EndTime(index:integer; val:double):HResult; stdcall;
var
  v: PBCVoice;
begin
  v := FPin.VoiceCheck(index);

  if v = nil then
  begin
   Result := ERROR;
   exit;
  end;

  v.FFEndTime := val;

  Result := S_OK;
end;

function TMWavePlayer.GetDuration(index:integer; out val:double):HResult; stdcall;
var
  v: PBCVoice;
begin
  v := FPin.VoiceCheck(index);

  if v = nil then
  begin
   Result := ERROR;
   exit;
  end;

  val := v.FDuration;

  Result := S_OK;
end;

function TMWavePlayer.GetActualPosition(index:integer; out val:double):HResult; stdcall;
var
  v: PBCVoice;
begin
  v := FPin.VoiceCheck(index);

  if v = nil then
  begin
   Result := ERROR;
   exit;
  end;

  val := v.FActualPosition;

  Result := S_OK;
end;

function TMWavePlayer.Fading(index: integer; val:double) : HResult; stdcall;
var
  v: PBCVoice;
begin
  v := FPin.VoiceCheck(index);

  if v = nil then
  begin
   Result := ERROR;
   exit;
  end;

  if (val >= 0) and (val <= 1) then
  v.FFading := val;

  Result := S_OK;
end;

function TMWavePlayer.setBlockSize( size : Integer) : HRESULT; stdcall;
begin           
 GlobalBlockSize := size;

 Result := S_OK;

end;


function TMWavePlayer.getBlockSize(out size : Integer) : HRESULT; stdcall;
begin           
 size := GlobalBlockSize;

 Result := S_OK;
 
end;


function TMWavePlayerPin.GetMediaType(pmt: PAMMediaType): HResult;
var
  pwf : PWAVEFORMATEX;
  size : longint;
begin
  size := sizeof(TWAVEFORMATEX);
  pwf := CoTaskMemAlloc(size);
  if pwf = nil then begin result := E_OUTOFMEMORY; exit; end;

  pmt.pbFormat             := pwf;
  pmt.cbFormat             := size;
  pmt.majortype            := MEDIATYPE_Audio;
  pmt.subtype              := MEDIASUBTYPE_PCM; //MEDIASUBTYPE_IEEE_FLOAT;
  pmt.formattype           := FORMAT_WaveFormatEx;
  pmt.bFixedSizeSamples    := TRUE;
  pmt.bTemporalCompression := FALSE;
  pmt.lSampleSize          := 4; //pwfx.nBlockAlign;
  pmt.pUnk                 := nil;

  pwf.cbSize          := 0;
  pwf.wFormatTag      := WAVE_FORMAT_PCM;
  pwf.nChannels       := NUMCHANNELS;
  pwf.nSamplesPerSec  := SAMPLERATE;
  pwf.wBitsPerSample  := BITSPERSAMPLE;
  pwf.nBlockAlign     := (pwf.wBitsPerSample * pwf.nChannels) div BITSPERBYTE;
  pwf.nAvgBytesPerSec := pwf.nBlockAlign * pwf.nSamplesPerSec;

  Result := S_OK;
end;

function TMWavePlayerPin.DecideBufferSize(Allocator: IMemAllocator;
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
   GlobalBlockSize     := 2048;
   Properties.cbBuffer := 2048;
   Properties.cBuffers := 1;
   Properties.cbAlign  := 512;
   Properties.cbPrefix := 0;

   // Ask the allocator to reserve us the memory
   Result := Allocator.SetProperties(Properties^, Actual);
  end;



end;

function TMVoice.ReadTheFile(AFileName: PChar): Boolean;
var
  _Mem: PByte;
  hmm : HMMIO;
  mmiParent : MMCKINFO;
  mmiSub : MMCKINFO;
  mmres : MMRESULT;
  ires, i : longint;
  fcc : FOURCC;
begin
  Result  := FALSE;

  FReload := true;

  // Open the file for reading with buffered I/O. Let windows use its default internal buffer
  hmm := mmioOpen(AFileName, nil, MMIO_READ + MMIO_ALLOCBUF);
  if (hmm = NULL) then begin OutputDebugString('waveplayer open error'); Exit; end;

  // Locate a "RIFF" chunk with a "WAVE" form type to make sure the file is a waveform-audio file.
  fcc := MAKEFOURCC('W', 'A', 'V', 'E');
  mmres := mmioDescend(hmm, @mmiParent, nil, 0);
  if mmres <> MMSYSERR_NOERROR then begin OutputDebugString('waveplayer error 1'); Exit; end;
  if mmiParent.ckid <> FOURCC_RIFF then begin OutputDebugString('waveplayer error 2'); Exit; end;
  if mmiParent.fccType <> fcc  then begin OutputDebugString('waveplayer error 3'); Exit; end;

  mmiSub.ckid := MAKEFOURCC('f', 'm', 't', ' ');
  mmres := mmioDescend(hmm, @mmiSub, @mmiParent, MMIO_FINDCHUNK);

  if mmres <> MMSYSERR_NOERROR then begin OutputDebugString('waveplayer error 4'); Exit; end;
  if mmiSub.cksize < sizeof(PCMWAVEFORMAT) then  begin OutputDebugString('waveplayer error 5'); Exit; end;

  mmioRead(hmm, @wf, sizeof(PCMWAVEFORMAT));
  if wf.wFormatTag <> WAVE_FORMAT_PCM then begin OutputDebugString('waveplayer error 7'); Exit; end;

  mmres := mmioAscend(hmm, @mmiSub, 0);
  if mmres <> MMSYSERR_NOERROR then begin OutputDebugString('waveplayer error 8'); Exit; end;

  mmioSeek(hmm, mmiParent.dwDataOffset + sizeof(FOURCC), SEEK_SET);
  //if mmres < 0 then begin OutputDebugString('waveplayer error 9'); Exit; end;

  mmiSub.ckid := MAKEFOURCC('d', 'a', 't', 'a');
  mmres := mmioDescend(hmm, @mmiSub, @mmiParent, MMIO_FINDCHUNK);
  if mmres <> MMSYSERR_NOERROR then    begin OutputDebugString('error 10'); Exit; end;

  {if Assigned(FData) then
  begin
    CoTaskMemFree(FData);
    FData := nil;
    FSize := 0;
  end; }

  _Mem := CoTaskMemAlloc(mmiSub.ckSize);
  if (_Mem = nil) then
  begin
    OutputDebugString('waveplayer: out of memory');
    MMIOClose(hmm,0);
    Exit;
  end;

  i := mmiSub.cksize;
  ires := i;
  while ires > 0 do
  begin
    ires := mmioRead( hmm, PChar(_Mem), i);
    i := i - ires;
  end;

  //initialize
  FData           := _Mem;
  FSize           := mmiSub.cksize;
  FSourceFrames   := trunc(FSize / BYTESPERSAMPLE / wf.nChannels);
  FDuration       := FSourceFrames / wf.nSamplesPerSec;
  FFrameFraction  := 1.0 / FSourceFrames;
  FBytesPerSample := trunc(wf.wBitsPerSample / BITSPERBYTE);

  {???????????????????????????????????}
  if wf.nChannels = 1 then
  FFrameFraction  := FFrameFraction / 2;


  FReload := false;
  
  // Close the file
  MMIOClose(hmm,0);
  Result := True;

end;

function TMVoice.FillBuffer(mediaSample: PDouble; size: Integer) : HRESULT;
var
  nFrames     : Integer;
  wheel       : Double;
  sourceLeft  : Double;
  sourceRight : Double;
  sinkLeft    : PDouble;
  sinkRight   : PDouble;
  loop        : Boolean;
  phaseStart  : Double;
  phaseEnd    : Double;
  phaseSeek   : Double;
  interval    : Double;
  gainLeft    : Double;
  gainRight   : Double;
  pitch       : Double;
  nChannels   : Integer;
  fadingGain  : Double;
  frameIndex  : Integer;

  {****************************************************************************}

  function TimeToPhase(value : Double) : Double;
  begin
   Result := value / FDuration ;

  end;
 
  {****************************************************************************}

  procedure SetInterval;
  begin

   if (not FSync) or ((FPhase >= phaseEnd) or (FPhase <= phaseStart)) then
   begin
    FStartTime := FFStartTime;
    FEndTime   := FFEndTime;
   end;

   phaseStart := TimeToPhase(FStartTime);
   phaseEnd   := TimeToPhase(FEndTime);

   if (phaseStart < 0) or (phaseStart > 1) then phaseStart := 0;
   if (phaseEnd   < 0) or (phaseEnd   > 1) then phaseEnd   := 1;

   interval := phaseEnd - phaseStart;

   if (interval < 0) then interval := 0;

   if (interval > 1) then interval := 1;   

   if not loop then interval := 1;

  end;

  {****************************************************************************}

  procedure SetVariables;
  begin
   loop       := FLoop;
   nChannels  := wf.nChannels;
   nFrames    := trunc(size / nChannels);
   gainRight  := FGain * FPan;
   gainLeft   := FGain * (1.0 - FPan);
   pitch      := FPitch;
   fadingGain := 1;

   SetInterval;

  end;

  {****************************************************************************}

  procedure Seek;
  begin
   phaseSeek := TimeToPhase(FSeekPosition);

   if loop then
   begin
    if phaseSeek < phaseStart then phaseSeek := phaseStart;
    if phaseSeek > phaseEnd   then phaseSeek := phaseEnd;
   end;

   if phaseSeek < 0 then phaseSeek := 0;
   if phaseSeek > 1 then phaseSeek := 1;

   FPhase    := phaseSeek;

   FDoSeek   := false;

  end;

  {****************************************************************************}

  procedure ShiftPhase;
  var
   difference : Double;

  begin

   difference := FPhaseShift - FPhaseShiftPrev;

   FPhase := FPhase + (difference * interval);

   while FPhase > phaseEnd do
   FPhase := FPhase - interval;

   while FPhase < phaseStart do
   FPhase := FPhase + interval;

   FPhaseShiftPrev := FPhaseShift;

  end;

  {****************************************************************************}

  function SetWheel : Boolean;
  begin

   if not loop then
   if ((pitch >= 0) and (FPhase >= 1)) or
      ((pitch <  0) and (FPhase <  0)) then
   begin
    Result := false;
    Exit;
   end;

   if loop then
   begin

    if FSync then
    if (FPhase >= phaseEnd) or (FPhase <= phaseStart) then
    SetInterval;

    if interval > 0 then
    begin
     while FPhase > phaseEnd do
     FPhase := FPhase - interval;

     while FPhase < phaseStart do
     FPhase := FPhase + interval;
    end;

    if interval = 0 then
    FPhase := phaseStart;

   end;

   if FPhase >= 1 then FPhase := 0;

   wheel  := FPhase * FSourceFrames;

   FPhase := FPhase + (FFrameFraction * pitch);

   Result := true;

  end;

  {****************************************************************************}

  procedure SetSource;
  var
   buffer           : PAudioSample;
   byteIndex        : Integer;
   source           : PByte;
   sourceFrameIndex : Integer;

  begin

   source           := FData;
   sourceFrameIndex := round(wheel);

   if sourceFrameIndex >= FSourceFrames then  //security
   sourceFrameIndex := FSourceFrames - 1;

   if sourceFrameIndex < 0 then
   sourceFrameIndex := 0;

   byteIndex := sourceFrameIndex * (FBytesPerSample * nChannels);

   Inc(source,byteIndex);

   buffer := PAudioSample(source);

   sourceLeft  := buffer^;

   if(nChannels > 1)then
   begin
    Inc(buffer);

    sourceRight := buffer^;
   end;

  end;

  {****************************************************************************}

  procedure SetSink;
  var
   shift : Integer;

  begin
  
   sinkLeft  := mediaSample;
   sinkRight := mediaSample;

   shift := frameIndex * nChannels;

   Inc(sinkLeft,shift);

   if (nChannels > 1) then
   Inc(sinkRight,shift + 1);

  end;

  {****************************************************************************}

  procedure SetGain;
  var
   fraction : Double;
   fading   : Double;

  begin

   fadingGain := 1;

   if (FFading <= 0) or (FFading > 1) then Exit;

   fading     := FFading / 2;

   fraction   := (FPhase - phaseStart) / interval;


   if fraction < fading then
   fadingGain := fraction / fading;

   if fraction > 1 - fading then
   fadingGain := (1 - fraction) / fading;

  end;

  {****************************************************************************}

  procedure FillSink;
  begin
   sourceLeft := (sourceLeft  * gainLeft  * fadingGain);

   if sourceLeft > MAXWAVEPOSITIV then sourceLeft := MAXWAVEPOSITIV;
   if sourceLeft < MAXWAVENEGATIV then sourceLeft := MAXWAVENEGATIV;

   sinkLeft^  := sinkLeft^ + sourceLeft;

   if (nChannels > 1) then
   begin
   sourceRight := (sourceRight * gainRight * fadingGain);

   if sourceRight > MAXWAVEPOSITIV then sourceRight := MAXWAVEPOSITIV;
   if sourceRight < MAXWAVENEGATIV then sourceRight := MAXWAVENEGATIV;

   sinkRight^ := sinkRight^ + sourceRight;
   end;
   
  end;

  {****************************************************************************}

begin

  Result := ERROR;
  if (FData = nil) or (FSize <= 0) or FReload then exit;

  SetVariables;

  for frameIndex := 0 to nFrames - 1 do
  begin

   if FDoSeek then
   Seek;

   if (FPhaseShift <> FPhaseShiftPrev) and loop then
   ShiftPhase;

   if not SetWheel then
   Continue;

   if loop then
   SetGain;

   SetSink;

   SetSource;

   FillSink;

  end; {end for frameIndex}


  FActualPosition := FDuration * FPhase;

  Result := S_OK;

end;


function TMWavePlayerPin.FillBuffer(ims: IMediaSample): HResult;
var
  size, i: longint;
  p: PByte;
  pp: PAudioSample;
  pf, buf: PDouble;
  vcr: double;
  count: integer;
  v: PBCVoice;

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
  vcr := 1./count;
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

constructor TMWavePlayer.Create(ObjName: string; Unk: IUnKnown;
  out hr: HRESULT);
begin
  inherited Create(ObjName, Unk, CLSID_WavePlayer);

  // The pin magically adds itself to our pin array.
  FPin := TMWavePlayerPin.Create(hr, Self);

  if (hr <> S_OK) then
    if (FPin = nil) then
      hr := E_OUTOFMEMORY;
end;

constructor TMWavePlayer.CreateFromFactory(Factory: TBCClassFactory;
  const Controller: IUnknown);
var
  hr: HRESULT;
begin
  Create(Factory.Name, Controller, hr);
end;

destructor TMWavePlayer.Destroy;
begin
  FreeAndNil(FPin);
  inherited;
end;

function TMWavePlayerPin.VoiceCheck(index: Integer): PBCVoice;
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

procedure TMWavePlayerPin.VoiceFree(index: Integer);
var
  v, vv: PBCVoice;
  i: Integer;
begin
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
end;

initialization
  // provide entries in the CFactoryTemplate array
  TBCClassFactory.CreateFilter(TMWavePlayer, FilterName,
    CLSID_WavePlayer, CLSID_LegacyAmFilterCategory,
    MERIT_DO_NOT_USE, 1, @sudOutputPin
    );
end.



