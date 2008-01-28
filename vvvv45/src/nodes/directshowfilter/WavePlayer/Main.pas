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
  CLSID_WavePlayer : TGUID = '{79A98DE0-BC00-11ce-AC2E-444553540001}';
  FilterName: String = 'WavePlayer';

  //WaveBufferSize: Integer = 4*512;

  WaveBufferSize : Integer = 256;

  outChans = 2;
  outSR = 44100;
  outBits = 16;
  BITS_PER_BYTE = 8;

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
    FSourceFrames : Double;
    FFading : Double;
    FFrameFraction : Double;
    FPrevPhase : Double;
    FReload : Boolean;

    FTest : Boolean;

    wf: TWAVEFORMATEX;
  public
    destructor Destroy; override;
    constructor Create;
    function ReadTheFile(AFileName: PChar): Boolean;
    function FillBuffer(buf: PDouble; size: integer): HResult;
  end;

  TMWavePlayerPin = class(TBCSourceStream)
  private
    voicelist: TList;
    voicecount: Integer;
  public

    FSampleStartTime : REFERENCE_TIME;
    FSampleEndTime   : REFERENCE_TIME;

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
  end;

implementation

uses Variants;

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
  FFrameFraction  :=  1;
  FPrevPhase      :=  0;

  FTest := false; //TEST

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
  if v = nil then exit;
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
      v.FPrevPhase      :=  vv.FPrevPhase;

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
  basefilter : IBaseFilter;

begin
  v := FPin.VoiceCheck(index);
  if v = nil then exit;

  v.FPlay := Boolean(state);

  {
  //windows.Beep(1000,10);

  v.FTest := true;
  v.FPlay := true;

  self.QueryInterface(IID_IBaseFilter,basefilter);

  {
  if Boolean(state)  then
  begin
   OutputDebugString('STOPSTOPSTOP');
   basefilter.Stop;
  end
  else
  begin
   OutputDebugString('RUNRUNRUN');
   basefilter.Run(FPin.FSampleEndTime);
  end;
  }

  Result  := S_OK
end;

function TMWavePlayer.Sync(index: integer; state: integer) : HResult; stdcall;
var
  v: PBCVoice;
begin
  v := FPin.VoiceCheck(index);
  if v = nil then exit;
  v.FSync := Boolean(state);
  Result := S_OK
end;

function TMWavePlayer.Volume(index: integer; val: double) : HResult; stdcall;
var
  v: PBCVoice;
begin
  v := FPin.VoiceCheck(index);
  if v = nil then exit;

  if (val>=0) and (val<=1) then
  v.FGain := val;

  Result := S_OK
end;

function TMWavePlayer.Pan(index: integer; val: double) : HResult; stdcall;
var
  v: PBCVoice;
begin
  v := FPin.VoiceCheck(index);
  if v = nil then exit;

  if (val>=0) and (val<=1) then
  v.FPan := val;

  Result := S_OK
end;

function TMWavePlayer.Phase(index: integer; val: double) : HResult; stdcall;
var
  v: PBCVoice;
begin
  v := FPin.VoiceCheck(index);
  if v = nil then exit;

  if (val>=0) and (val<=1) then
  v.FPhase := val;

  Result := S_OK
end;

function TMWavePlayer.Pitch(index: integer; val: double) : HResult; stdcall;
var
  v: PBCVoice;
begin
  v := FPin.VoiceCheck(index);
  if v = nil then exit;

  if (val >= 0) then
  v.FPitch := val;

  Result := S_OK
end;

function TMWavePlayer.Loop(index:integer; state:integer) : HResult; stdcall;
var
  v: PBCVoice;
begin
  v := FPin.VoiceCheck(index);

  if v = nil then exit;

  v.FLoop := Boolean(state);

  Result := S_OK;
end;

function TMWavePlayer.DoSeek(index: integer; state:integer):HResult; stdcall;
var
  v: PBCVoice;
begin
  v := FPin.VoiceCheck(index);

  if v = nil then exit;

  v.FDoSeek := Boolean(state);

  if v.FDoSeek then
  begin
   v.FDoSeek := false;

   v.FStartTime := v.FFStartTime;
   v.FEndTime   := v.FFEndTime;

   if v.FSeekPosition < v.FStartTime then
   v.FWheel := 0;

   if v.FSeekPosition > v.FEndTime then
   v.FWheel := (v.FEndTime - v.FStartTime) / v.FDuration;

   if (v.FSeekPosition >= v.FStartTime) and
      (v.FSeekPosition <= v.FEndTime) then
   v.FWheel := (v.FSeekPosition - v.FStartTime) / v.FDuration;

   v.FActualPosition := v.FStartTime + (v.FWheel * v.FDuration);

  end;

  Result := S_OK;
end;

function TMWavePlayer.SeekPosition(index:integer; val:double):HResult; stdcall;
var
  v: PBCVoice;
begin
  v := FPin.VoiceCheck(index);
  if v = nil then exit;

  //if val < v.FFStartTime then
  //v.FSeekPosition := v.FStartTime;

  if val < v.FFStartTime then
  v.FSeekPosition := val;

  if val > v.FFEndTime then
  v.FSeekPosition := v.FEndTime;

  if (val >= v.FFStartTime) and (val <= v.FFEndTime) then
  v.FSeekPosition := val;

  Result := S_OK;
end;

function TMWavePlayer.StartTime(index: integer; val:double):HResult; stdcall;
var
  v: PBCVoice;
begin
  v := FPin.VoiceCheck(index);

  if v = nil then exit;

  if val < 0 then
  v.FFStartTime := 0;

  if val > v.FFEndTime then
  v.FFStartTime := v.FFEndTime;

  if (val >= 0) and (val <= v.FFEndTime) then
  v.FFStartTime := val;

  Result := S_OK;
end;

function TMWavePlayer.EndTime(index:integer; val:double):HResult; stdcall;
var
  v: PBCVoice;
begin
  v := FPin.VoiceCheck(index);

  if v = nil then exit;

  if val < 0 then
  v.FFEndTime := 0;

  if val > v.FDuration then
  v.FFEndTime := v.FDuration;

  if (val >= v.FFStartTime) and (val <= v.FDuration) then
  v.FFEndTime := val;

  Result := S_OK;
end;

function TMWavePlayer.GetDuration(index:integer; out val:double):HResult; stdcall;
var
  v: PBCVoice;
begin
  v := FPin.VoiceCheck(index);

  if v = nil then exit;

  val := v.FDuration;

  Result := S_OK;
end;

function TMWavePlayer.GetActualPosition(index:integer; out val:double):HResult; stdcall;
var
  v: PBCVoice;
begin
  v := FPin.VoiceCheck(index);

  if v = nil then exit;

  val := v.FActualPosition;

  Result := S_OK;
end;

function TMWavePlayer.Fading(index: integer; val:double) : HResult; stdcall;
var
  v: PBCVoice;
begin
  v := FPin.VoiceCheck(index);

  if v = nil then exit;

  if(val < 0)then
  v.FFading := 0;

  if(val > 1)then
  v.FFading := 1;

  if(val >= 0) and (val <= 1) then
  v.FFading := val;

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

  pmt.pbFormat := pwf;
  pmt.cbFormat := size;
  pmt.majortype := MEDIATYPE_Audio;
  pmt.subtype := MEDIASUBTYPE_PCM; //MEDIASUBTYPE_IEEE_FLOAT;
  pmt.formattype := FORMAT_WaveFormatEx;
  pmt.bFixedSizeSamples    := TRUE;
  pmt.bTemporalCompression := FALSE;
  pmt.lSampleSize          := 4; //pwfx.nBlockAlign;
  pmt.pUnk                 := nil;

  pwf.cbSize := 0;
  pwf.wFormatTag := WAVE_FORMAT_PCM;
  pwf.nChannels := outChans;
  pwf.nSamplesPerSec := outSR;
  pwf.wBitsPerSample := outBits;
  pwf.nBlockAlign := (pwf.wBitsPerSample * pwf.nChannels) div BITS_PER_BYTE;
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

  FFilter.StateLock.Lock;
  try

  
    Properties.cbBuffer := 256; // 256/2(bytespersample)/2(channels) = 64 frames -> ~1.5 milliseconds
    Properties.cBuffers := 1;
    Properties.cbAlign  := 256;
    Properties.cbPrefix := 0;

    // Ask the allocator to reserve us the memory
    Result := Allocator.SetProperties(Properties^, Actual);

    //Outputdebugstring(pchar(format('buffer align: %d',[Properties.cbAlign])));

    if Failed(Result) then
      ASSERT(FALSE);
    // Is this allocator unsuitable?
    if (Actual.cbBuffer < Properties.cbBuffer) then
      ASSERT(FALSE)
    else
      Result := S_OK;
  finally
    FFilter.StateLock.UnLock;
  end;
  ASSERT(Actual.cbBuffer > 0);
  ASSERT(Actual.cBuffers > 0);
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
  FSourceFrames   := FSize / 2 / wf.nChannels;
  FDuration       := FSourceFrames / 44100;
  FFrameFraction  := 1.0 / FSourceFrames;

  if FFEndTime > FDuration then
  FFEndTime := FDuration;

  if FEndTime > FDuration then
  FEndTime := FDuration;

  if FFStartTime > FFEndTime then
  FFStartTime := FFEndTime;

  if FStartTime > FFEndTime then
  FStartTime := FFEndTime;

  if FSeekPosition > FFEndTime then
  FSeekPosition := FFendTime;

  if FWheel > (FFEndTime / FDuration) then
  FWheel := (FFEndTime / FDuration);

  FReload := false;
  
  // Close the file
  MMIOClose(hmm,0);
  Result := True;

end;

function TMVoice.FillBuffer(buf: PDouble; size: integer): HResult;
var
  p: PDouble;
  p1,p2: PAudioSample;
  i : Longint;
  gain1, gain2: Double;
  wheelinc: Double;
  startTime,endTime : Double;
  phaseshift : Double;
  interval : Double;
  minInterval : Double;
  sampleLeft, sampleRight : Double;
  f : Double;
  fading, wheel, position : Double;
  loop : Boolean;
  numFrames : Integer;

  //----------------------------------------------------------------//

  procedure phase2val(phase: Double; left : PDouble; right : PDouble);
  var
   pSource: PByte;
   frac: Double;
   Index: Longint;
  begin
   pSource := FData;
   frac := phase * FSourceFrames;
   Index := trunc(frac);
   Index := Index * 2 * wf.nChannels;
   Inc(pSource,Index);

   p1 := PAudioSample(pSource);
   p2 := p1;    //if there is only one channel

   if wf.nChannels = 2 then
   if Index + 2 * wf.nChannels < FSize then
   begin
    Inc(pSource, wf.nChannels);

    p2 := PAudioSample(pSource);
   end;

   left^  := p1^;
   right^ := p2^;

  end;

  //----------------------------------------------------------------//

  procedure setSyncTime();
  begin
   FStartTime  := FFStartTime;             //update to the time the user set
   FEndTime    := FFEndTime;
   startTime   := FStartTime / FDuration;  //from time to 0..1
   endTime     := FEndTime   / FDuration;
   interval    := endTime - startTime;

   if interval < minInterval then interval := minInterval;

   fading      := FFading * 0.5 * interval;
  end;

  //----------------------------------------------------------------//

begin

  Result := -1;
  if (FData = nil) or (FSize <= 0) or (FDuration <= 0)then exit;
  Result := S_OK;

  //Set Gain------------------------------------------------//
  gain1 := FPan * 2.;
  if gain1 > 1. then gain1 := 1.;
  gain1 := gain1 * FGain;
  gain2 := (1. - FPan) * 2.;
  if gain2 > 1. then gain2 := 1.;
  gain2 := gain2 * FGain;

  //--------------------------------------------------------//

  if FSync = false then
  begin
    FStartTime := FFStartTime;
    FEndTime   := FFEndTime;
  end;

  //--------------------------------------------------------//

  p           := buf;
  numFrames   := size div 2;
  wheelinc    := FPitch     / FSourceFrames;
  startTime   := FStartTime / FDuration;        //from seconds to 0..1
  endTime     := FEndTime   / FDuration;

  if (not FLoop) and (FEndTime = 0) then
  endTime     := 1.0;

  interval    := endTime - startTime;
  minInterval := FFrameFraction * numFrames; //fraction of a imediasample-block
  phaseshift  := 0;
  position    := 0;

  if interval < minInterval then interval := minInterval;

  if FPhase <> FPrevPhase then
  begin
   phaseshift := (FPhase - FPrevPhase) * interval;

   FPrevPhase := FPhase;
  end;

  fading      := FFading * 0.5 * interval;
  loop        := FLoop;
  wheel       := FWheel;

  //--------------------------------------------------------//
  for i:= 0 to numFrames - 1 do
  begin
    sampleLeft  := 0;
    sampleRight := 0;

    f := 1.0;

    if loop or (wheel + wheelinc <= interval) then//--------//
    begin
     wheel := wheel + wheelinc + phaseshift;

     phaseshift := 0;

     while wheel > interval do
     begin
      wheel := wheel - interval;

      if FSync then setSyncTime();
     end;

     position := startTime + wheel;

     if wheel < fading then
      f := wheel / fading
     else
     if wheel > (interval - fading) then
      f := (interval - wheel) / fading;

     if(position < 1) and (not FReload) then
      phase2val(position, @sampleLeft, @sampleRight );

    end else position := endtime;//------------------------//

    p^ := p^ + sampleLeft  * gain1 * f;
    Inc(p);
    p^ := p^ + sampleRight * gain2 * f;
    Inc(p);

  end;
  //-------------------------------------------------------//

  FWheel          := wheel;
  FPosition       := position;
  FActualPosition := FPosition * FDuration;

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
  hr : HResult;

begin
  result := -1;
  if ims = nil then exit;
  //ASSERT(ims <> nil);

  hr := ims.GetTime(FSampleStartTime, FSampleEndTime );

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


