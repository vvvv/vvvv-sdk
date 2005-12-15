unit Main;

interface

uses
  Classes, SysUtils, ActiveX, MMSystem, Math, Windows,
  BaseClass, DirectShow9, DSUtil, DirectSound;

const
  CLSID_WavePlayer : TGUID = '{79A98DE0-BC00-11ce-AC2E-444553540001}';
  FilterName: String = 'WavePlayer';

  WaveBufferSize: Integer = 4*512;
  outChans = 2;
  outSR = 44100;
  outBits = 16;
  BITS_PER_BYTE = 8;

  sudPinTypes: TRegPinTypes =
  (
    clsMajorType: @MEDIATYPE_Audio;
    clsMinorType: @MEDIASUBTYPE_PCM
  );

  sudOutputPinBitmap: array[0..0] of TRegFilterPins =
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
    function GetPosition(index: integer; out position: double): HResult; stdcall;
    function Read(index: integer; FileName : String): HResult; stdcall;
    function Play(voice: integer; state : integer): HResult; stdcall;
    function Volume(voice: integer; val : double): HResult; stdcall;
    function Pan(voice: integer; val : double): HResult; stdcall;
    function Position(voice: integer; val : double): HResult; stdcall;
    function Length(voice: integer; val : double): HResult; stdcall;
    function Phase(voice: integer; val : double): HResult; stdcall;
    function Pitch(voice: integer; val : double): HResult; stdcall;
    function Sync(voice: integer; state : integer): HResult; stdcall;
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
    prevphase, prevprevphase: Double;
    FFading, FlyWheel, FlyWheel2, FlyInc, FadeInc: Double;
    FPlay: Boolean;
    FSync: Boolean;
    FGain: double;
    FPan: double;
    FLength, FFLength: double;
    FPosition, FFPosition: double;
    FPhase, FFPhase: double;
    FPitch, FFPitch: double;
    wf: TWAVEFORMATEX;
  public
    destructor Destroy; override;
    function ReadTheFile(AFileName: PChar): Boolean;
    function FillBuffer(buf: PDouble; size: integer): HResult;
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
    function GetPosition(index: integer; out position: double): HResult; stdcall;
    function Read(index: integer; FileName : String): HResult; stdcall;
    function Play(index: integer; state: integer): HResult; stdcall;
    function Volume(index: integer; val: double): HResult; stdcall;
    function Pan(index: integer; val: double): HResult; stdcall;
    function Position(index: integer; val: double): HResult; stdcall;
    function Length(index: integer; val: double): HResult; stdcall;
    function Phase(index: integer; val: double): HResult; stdcall;
    function Pitch(index: integer; val: double): HResult; stdcall;
    function Sync(index: integer; state: integer): HResult; stdcall;
  end;

implementation

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

function TMWavePlayer.GetPosition(index: integer; out position: double): HResult; stdcall;
var
  v: PBCVoice;
begin
  Result := S_OK;
  if index < FPin.voicecount then
  begin
    v := FPin.voicelist.Items[index];
    position := v.prevphase;
  end else
    position := 0.;
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
    vv := FPin.VoiceList.Items[i];
    if vv.FFileName = FileName then
    begin
      FPin.VoiceFree(index);
      v.FSize := vv.FSize;
      v.FData := vv.FData;
      v.wf := vv.wf;
      v.FFileName := vv.FFileName;
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
  if v = nil then exit;
  v.FPlay := Boolean(state);
  v.FWheel := 0.;
  v.FFading := 0.;
  v.prevphase := 1.;
  v.prevprevphase := 1.;
  Result := S_OK
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
  v.FGain := val;
  Result := S_OK
end;

function TMWavePlayer.Pan(index: integer; val: double) : HResult; stdcall;
var
  v: PBCVoice;
begin
  v := FPin.VoiceCheck(index);
  if v = nil then exit;
  v.FPan := val;
  Result := S_OK
end;

function TMWavePlayer.Position(index: integer; val: double) : HResult; stdcall;
var
  v: PBCVoice;
begin
  v := FPin.VoiceCheck(index);
  if v = nil then exit;
  v.FFPosition := val;
  Result := S_OK
end;

function TMWavePlayer.Length(index: integer; val: double) : HResult; stdcall;
var
  v: PBCVoice;
begin
  v := FPin.VoiceCheck(index);
  if v = nil then exit;
  v.FFLength := val;
  Result := S_OK
end;

function TMWavePlayer.Phase(index: integer; val: double) : HResult; stdcall;
var
  v: PBCVoice;
begin
  v := FPin.VoiceCheck(index);
  if v = nil then exit;
  v.FFPhase := val;
  Result := S_OK
end;

function TMWavePlayer.Pitch(index: integer; val: double) : HResult; stdcall;
var
  v: PBCVoice;
begin
  v := FPin.VoiceCheck(index);
  if v = nil then exit;
  v.FFPitch := val;
  Result := S_OK
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
    Properties.cbBuffer := WaveBufferSize; //WaveBufferSize; //pvi.bmiHeader.biSizeImage;
    Properties.cBuffers := 1;
    // Ask the allocator to reserve us the memory
    Result := Allocator.SetProperties(Properties^, Actual);
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
  Result := FALSE;

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

  _Mem := CoTaskMemAlloc(mmiParent.ckSize);
  if (_Mem = nil) then
  begin
    OutputDebugString('waveplayer: out of memory');
    MMIOClose(hmm,0);
    Exit;
  end;

  i := mmiParent.cksize;
  ires := i;
  while ires > 0 do
  begin
    ires := mmioRead( hmm, PChar(_Mem), i);
    i := i - ires;
  end;

  // Save a pointer to the data that was read from the file
  FData := _Mem;
  FSize := mmiParent.cksize;

  // Close the file
  MMIOClose(hmm,0);
  Result := True;

end;

function TMVoice.FillBuffer(buf: PDouble; size: integer): HResult;
var
  p: PDouble;
  p1,p2: PAudioSample;
  frames, i: Longint;
  f, phasewrap : Double;
  sourcesamps, sourceframes, sourceframes_r: Double;
  gain1, gain2: Double;
  wheelinc: Double;
  once: Boolean;
  function phase2val(phase: Double) : Double;
  var
    pSource: PByte;
    frac: Double;
    Index: Longint;
  begin
      pSource := FData;
      frac := phase * sourceframes;
      Index := trunc(frac);
      frac := frac - Index;
      Index := Index * 2 * wf.nChannels;
      Inc(pSource,Index);
      p1 := PAudioSample(pSource);

      if Index + 2 * wf.nChannels < FSize then Inc(pSource, 2* wf.nChannels)
      else pSource := FData;
      p2 := PAudioSample(pSource);

      result := p1^ * (1. - frac) + p2^ * frac;
  end;

begin

  Result := -1;
  if (FData = nil) or (FSize <= 0) then exit;
  Result := S_OK;

  p := buf;
  frames := size div 2;

  sourcesamps := FSize / 2;
  sourceframes := sourcesamps / wf.nChannels;
  sourceframes_r := 1. / sourceframes;
  if FLength < sourceframes_r then FLength := sourceframes_r;
  wheelinc := FPitch / (FLength * sourceframes);


  gain1 := FPan * 2.;
  if gain1 > 1. then gain1 := 1.;
  gain1 := gain1 * FGain;
  gain2 := (1. - FPan) * 2.;
  if gain2 > 1. then gain2 := 1.;
  gain2 := gain2 * FGain;

  i := frames;
  once := true;

  while i > 0 do
  begin
    if not FSync then if FFading <= 0. then
    begin
      if once then
      begin
        FLength := FFLength;
        if FLength < sourceframes_r then FLength := sourceframes_r;
        FPosition := FFPosition;
        FPitch := FFPitch;
        FPhase := FFPhase;
        wheelinc := FPitch / (FLength * sourceframes);
        once := false;
      end;
    end;
    FWheel := FWheel + wheelinc;
    FWheel := wrap(FWheel);

    phasewrap := FWheel + FPhase;
    phasewrap := wrap(phasewrap);
    phasewrap := phasewrap * FLength + FPosition;

    phasewrap := wrap(phasewrap);

    if FFading <= 0. then
    begin
      if abs((phasewrap - prevphase) - (prevphase - prevprevphase)) > 4. * sourceframes_r then
      begin
        FFading := 1.;
        FlyWheel := prevphase;
        FlyInc := prevphase - prevprevphase;

        FLength := FFLength;
        if FLength < sourceframes_r then FLength := sourceframes_r;
        FPosition := FFPosition;
        FPitch := FFPitch;
        FPhase := FFPhase;
        wheelinc := FPitch / (FLength * sourceframes);

        FadeInc := wheelinc * 10.;
        phasewrap := FWheel + FPhase;
        phasewrap := wrap(phasewrap);
        phasewrap := phasewrap * FLength + FPosition;

        phasewrap := wrap(phasewrap);

      end;
    end;

    prevprevphase := prevphase;
    prevphase := phasewrap;
    f := phase2val(phasewrap);

    if FFading > 0. then
    begin
      FlyWheel := FlyWheel + FlyInc;
      FlyWheel := wrap(FlyWheel);

      f := (f * (1. - FFading)) + (phase2val(FlyWheel) * FFading);
      FFading := FFading - FadeInc;
    end;

    p^ := p^ + f * gain1;
    Inc(p);
    p^ := p^ + f * gain2;
    Inc(p);

    Dec(i);

  end;
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
    MERIT_DO_NOT_USE, 1, @sudOutputPinBitmap
    );
end.

