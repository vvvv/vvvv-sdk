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
//[meanimal] xsnej@gmx.de

//****************************************************************************//
//****************************************************************************//
//****************************************************************************//



unit Main;

interface

uses
 Classes, SysUtils, ActiveX, MMSystem, Math, Windows,
 BaseClass, DirectShow9, DXSUtil, DirectSound;

//****************************************************************************//

const

  //Filter-Properties------------------------------------------------//
  CLSID_WavePlayer : TGUID = '{79A98DE0-BC00-11ce-AC2E-444553540001}';
  FilterName: String = 'WavePlayer';

  sudPinTypes : TRegPinTypes =
  (
    clsMajorType : @MEDIATYPE_Audio;
    clsMinorType : @MEDIASUBTYPE_PCM;
  );

  sudOutputPin : array[0..0] of TRegFilterPins =
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


  //Standard-Values-------------------------------------------------//
  MAXWAVENEGATIV     = -32767;
  MAXWAVEPOSITIV     =  32767;
  BYTESPERSAMPLE     =  2;
  NUMCHANNELS        =  1;
  SAMPLERATE         =  44100;
  BITSPERSAMPLE      =  16;
  BITSPERBYTE        =  8;
  MAXCHANNELS        =  18;
  //BLOCKSIZESMALL   =  256;
  BLOCKSIZESMALL     =  512;
  BLOCKSIZE          =  2048;
  BLOCKALIGN         =  256;
  DEFAULTCHANNELMASK =  SPEAKER_FRONT_LEFT;
  CHANNELMASK5POINT1 =  SPEAKER_FRONT_LEFT    or
                        SPEAKER_FRONT_RIGHT   or
                        SPEAKER_FRONT_CENTER  or
                        SPEAKER_LOW_FREQUENCY or
                        SPEAKER_BACK_LEFT     or
                        SPEAKER_BACK_RIGHT;
  CHANNELMASK7POINT1 =  CHANNELMASK5POINT1    or
                        SPEAKER_SIDE_LEFT     or
                        SPEAKER_SIDE_RIGHT;

  CHANNELCODE : Array [0..MAXCHANNELS-1] of Integer = ( SPEAKER_FRONT_LEFT
                                                      , SPEAKER_FRONT_RIGHT
                                                      , SPEAKER_FRONT_CENTER
                                                      , SPEAKER_LOW_FREQUENCY
                                                      , SPEAKER_BACK_LEFT
                                                      , SPEAKER_BACK_RIGHT
                                                      , SPEAKER_FRONT_LEFT_OF_CENTER
                                                      , SPEAKER_FRONT_RIGHT_OF_CENTER
                                                      , SPEAKER_BACK_CENTER
                                                      , SPEAKER_SIDE_LEFT
                                                      , SPEAKER_SIDE_RIGHT
                                                      , SPEAKER_TOP_CENTER
                                                      , SPEAKER_TOP_FRONT_LEFT
                                                      , SPEAKER_TOP_FRONT_CENTER
                                                      , SPEAKER_TOP_FRONT_RIGHT
                                                      , SPEAKER_TOP_BACK_LEFT
                                                      , SPEAKER_TOP_BACK_CENTER
                                                      , SPEAKER_TOP_BACK_RIGHT );

  CHANNELNAME : Array [0..MAXCHANNELS-1] of String = (  'SPEAKER_FRONT_LEFT'
                                                      , 'SPEAKER_FRONT_RIGHT'
                                                      , 'SPEAKER_FRONT_CENTER'
                                                      , 'SPEAKER_LOW_FREQUENCY'
                                                      , 'SPEAKER_BACK_LEFT'
                                                      , 'SPEAKER_BACK_RIGHT'
                                                      , 'SPEAKER_FRONT_LEFT_OF_CENTER'
                                                      , 'SPEAKER_FRONT_RIGHT_OF_CENTER'
                                                      , 'SPEAKER_BACK_CENTER'
                                                      , 'SPEAKER_SIDE_LEFT'
                                                      , 'SPEAKER_SIDE_RIGHT'
                                                      , 'SPEAKER_TOP_CENTER'
                                                      , 'SPEAKER_TOP_FRONT_LEFT'
                                                      , 'SPEAKER_TOP_FRONT_CENTER'
                                                      , 'SPEAKER_TOP_FRONT_RIGHT'
                                                      , 'SPEAKER_TOP_BACK_LEFT'
                                                      , 'SPEAKER_TOP_BACK_CENTER'
                                                      , 'SPEAKER_TOP_BACK_RIGHT' );


//****************************************************************************//

type

  //Class-Declarations---------------------------------------------//
  TMWavePlayer      = class;
  TMWavePlayerPin   = class;
  TMWavePlayerRoute = class;
  TMVoice           = class;

  //---------------------------------------------------------------//
  PBCVoice = ^TMVoice;

  AudioSample  =  SmallInt; //-32768..32767 Int16
  PAudioSample = ^AudioSample;

  TInfoArray    = Array of String;
  TSampleArray  = Array [0..BLOCKSIZE-1] of Double;
  TChannelArray = Array [0..MAXCHANNELS-1] of TSampleArray;
  TSourceFrame  = Array [0..MAXCHANNELS-1] of Double;
  TRoutingArray = Array of Integer;

  //Interface-Definition-------------------------------------------//
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
    function SetBlockSize(val: Integer) : HRESULT; stdcall;
    function GetBlockSize(out val : Integer) : HRESULT; stdcall;
    function SetRouting(count: Integer; val: TRoutingArray) : HRESULT;  stdcall;
    function GetInfo(out count : integer; val : TInfoArray) : HRESULT; stdcall;
  end;

  //---------------------------------------------------------------//
  TMWavePlayer = class(TBCSource, IWavePlayer, IPersist)
  private
   FPin    : TMWavePlayerPin;
  public
   constructor Create (ObjName: string; Unk: IUnknown; out hr: HRESULT);
   destructor  Destroy; override;
   constructor CreateFromFactory (Factory: TBCClassFactory; const  Controller : IUnknown); override;
   function NonDelegatingQueryInterface (const IID: TGUID; out Obj) : HRESULT; override;
   //IWavePlayer
   function Voices(voicecount: integer): HResult; stdcall;
   function Read(index: integer; FileName : String): HResult; stdcall;
   function Play(index: integer; state : integer): HResult; stdcall;
   function Volume(index: integer; val : double): HResult; stdcall;
   function Pan(index: integer; val : double): HResult; stdcall;
   function Phase(index: integer; val : double): HResult; stdcall;
   function Pitch(index: integer; val : double): HResult; stdcall;
   function Sync(index: integer; state: integer): HResult; stdcall;
   function Loop(index: integer; state:integer):HResult; stdcall;
   function DoSeek(index: integer; state:integer):HResult; stdcall;
   function StartTime(index: integer; val:double):HResult; stdcall;
   function EndTime(index:integer; val:double):HResult; stdcall;
   function SeekPosition(index:integer; val:double):HResult; stdcall;
   function GetDuration(index:integer; out val:double):HResult; stdcall;
   function GetActualPosition(index:integer; out val:double):HResult; stdcall;
   function Fading(index: integer; val:double) : HResult; stdcall;
   function SetBlockSize(val: Integer) : HRESULT; stdcall;
   function GetBlockSize(out val : Integer) : HRESULT; stdcall;
   function SetRouting(count: Integer; val: TRoutingArray) : HRESULT;  stdcall;
   function GetInfo(out count : integer; val : TInfoArray) : HRESULT; stdcall;

  end;

  //---------------------------------------------------------------//
  TMWavePlayerRoute = class(TObject)
  public
   FChannelMask     : Integer;
   FChannelCount    : Integer;
   FChannelFactor   : TRoutingArray;
   FChannelId       : TRoutingArray;
   FChannelIdCount  : Integer;
   FChannelName     : TInfoArray;
   FRouting         : TRoutingArray;
   FRoutingCount    : Integer;
   FBlockSize       : Integer;
   FUseFileMapping  : Boolean;
   FReload          : Boolean;
   //tmp
   FFChannelCount   : Integer;
   FFChannelFactor  : TRoutingArray;
   FFRoutingCount   : Integer;
   FFRouting        : TRoutingArray;
   FFUseFileMapping : Boolean;

   Constructor Create;
   Destructor Destroy; override;
   procedure SetRouting(count : Integer; val : TRoutingArray);
   procedure SetFileMapping(channelMask : Integer; channelCount : Integer; map : Array of integer);
   procedure Update;
   property BlockSize : Integer read FBlockSize write FBlockSize;
   property ChannelCount : Integer read FChannelCount write FChannelCount;
   property ChannelMask : Integer read FChannelMask write FChannelMask;
   property Routing : TRoutingArray read FRouting write FRouting;
   property RoutingCount : Integer read FRoutingCount write FRoutingCount;
   property UseFileMapping : Boolean read FUseFileMapping;

  end;

  //---------------------------------------------------------------//
  TMWavePlayerPin = class(TBCSourceStream)
  private
   voicelist     : TList;
   FVoiceCount   : Integer;
   FRoute        : TMWavePlayerRoute;

   waveformat    : TWAVEFORMATEX;
  public
   constructor Create(out hr: HRESULT; Filter: TBCSource);
   destructor Destroy; override;
   function GetMediaType(pmt : PAMMediaType) : HRESULT; override;
   function DecideBufferSize(Allocator: IMemAllocator; Properties: PAllocatorProperties) : HRESULT; override;
   function Notify(Filter: IBaseFilter; q: TQuality) : HRESULT; override; stdcall;
   function FillBuffer(ims : IMediaSample) : HRESULT; override;
   function VoiceCheck(index: Integer): PBCVoice;
   procedure SetFileMapping;
   procedure SetRouting(count : Integer; val : TRoutingArray);
   procedure VoiceFree(index: Integer);
   property Route : TMWavePlayerRoute read FRoute;
  end;

  //---------------------------------------------------------------//
  TMVoice  = class(TObject)
  private
   FFilename       : String;
   FSize           : LONGLONG;
   FData           : PByte;
   FWaveFormat     : TWAVEFORMATEXTENSIBLE;
   FSourceFrames   : LONGLONG;
   FPlay           : Boolean;
   FSync           : Boolean;
   FGain           : Double;
   FPan            : Double;
   FPosition       : Double;
   FPhase          : Double;
   FPitch          : Double;
   FStartTime      : Double;
   FFStartTime     : Double;
   FEndTime        : Double;
   FFEndTime       : Double;
   FSeekPosition   : Double;
   FDoSeek         : Boolean;
   FDuration       : Double;
   FFading         : Double;
   FLoop           : Boolean;
   FFrameFraction  : Double;
   FReload         : Boolean;
   FPhaseShift     : Double;
   FPhaseShiftPrev : Double;
   FChannelCount   : Integer;
   FChannel        : TChannelArray;
   FChannelSize    : Integer;
   FChannelMap     : Array [0..MAXCHANNELS-1] of Integer;
   FInitialized    : Boolean;
   //tmp---------------------
   FFData          : PByte;
   FFSize          : LONGLONG;
   FFWaveFormat    : TWAVEFORMATEXTENSIBLE;
   FFSourceFrames  : LONGLONG;
   FFFrameFraction : Double;
   FFChannelCount  : Integer;
   FFChannelMap    : Array [0..MAXCHANNELS-1] of integer;
   FFDuration      : Double;     

  public
   destructor Destroy; override;
   constructor Create;
   function ReadTheFile(AFileName : String) : Boolean;
   procedure Update;
   procedure Reset;
   procedure Clone(other : PBCVoice);
   function FillBuffer(size: integer) : HRESULT;
  end;

//****************************************************************************//

implementation

uses Variants;

//TMWavePlayer----------------------------------------------------------------//
constructor TMWavePlayer.Create (ObjName: string; Unk: IUnknown; out hr: HRESULT);
begin
 inherited Create(ObjName, Unk, CLSID_WavePlayer);

 FPin    := TMWavePlayerPin.Create(hr, Self);

 if (hr <> S_OK) then
  if (FPin = nil) then
  hr := E_OUTOFMEMORY;

end;

destructor  TMWavePlayer.Destroy;
begin

 FreeAndNil(FPin);
 inherited;

end;

//Called first
constructor TMWavePlayer.CreateFromFactory (Factory: TBCClassFactory; const  Controller : IUnknown);
var
 hr : HRESULT;
begin
 Create(Factory.Name, Controller, hr);

end;

//Called after filtercreation
function TMWavePlayer.NonDelegatingQueryInterface (const IID: TGUID; out Obj) : HRESULT;
begin

 if IsEqualGUID(IID, CLSID_WavePlayer) then
  if GetInterface(CLSID_WavePlayer, Obj) then
   Result := S_OK
  else
   Result := E_FAIL
 else
  Result := inherited NonDelegatingQueryInterface(IID, Obj);

 Result := Result;

end;

//IWavePlayer-Definitions----//

function TMWavePlayer.GetInfo(out count : integer; val : TInfoArray) : HRESULT; stdcall;
var
 i,k,n   : integer;
 routeTo : integer;
 voice   : PBCVoice;
 str     : String;
 name    : String;
begin

  n     := 0;
  count := 0;

  for i := 0 to FPin.FVoiceCount - 1 do
  begin

    voice := FPin.VoiceCheck(i);

    if voice <> nil then
    if voice.FInitialized then
    for k := 0 to voice.FChannelCount - 1 do
    begin

       routeTo := FPin.Route.Routing[n];

       n := (n + 1) mod FPin.Route.FRoutingCount;


       name := '';

       if not FPin.Route.FUseFileMapping then
       begin
        name := 'default';

        if (routeTo >= 0) and (routeTo < MAXCHANNELS) then
         name := FPin.Route.FChannelName[routeTo];

       end;

       if FPin.Route.FUseFileMapping then
       case voice.FChannelMap[k] of
          0 : name := 'SPEAKER_FRONT_LEFT';
          1 : name := 'SPEAKER_FRONT_RIGHT';
          2 : name := 'SPEAKER_FRONT_CENTER';
          3 : name := 'SPEAKER_LOW_FREQUENCY';
          4 : name := 'SPEAKER_BACK_LEFT';
          5 : name := 'SPEAKER_BACK_RIGHT';
          6 : name := 'SPEAKER_FRONT_LEFT_OF_CENTER';
          7 : name := 'SPEAKER_FRONT_RIGHT_OF_CENTER';
          8 : name := 'SPEAKER_BACK_CENTER';
          9 : name := 'SPEAKER_SIDE_LEFT';
          10: name := 'SPEAKER_SIDE_RIGHT';
          11: name := 'SPEAKER_TOP_CENTER';
          12: name := 'SPEAKER_TOP_FRONT_LEFT';
          13: name := 'SPEAKER_TOP_FRONT_CENTER';
          14: name := 'SPEAKER_TOP_FRONT_RIGHT';
          15: name := 'SPEAKER_TOP_BACK_LEFT';
          16: name := 'SPEAKER_TOP_BACK_CENTER';
          17: name := 'SPEAKER_TOP_BACK_RIGHT';
       end;
       
       if name <> '' then
       begin
         str := ExtractFileName(voice.FFilename);

         val[count] := str +' '+ name;

         if count < 255 then
          inc(count);
       end;

    end;

  end;//end for i

  Result := S_OK;
end;

function TMWavePlayer.Voices(voicecount: integer): HResult; stdcall;
begin

 if FPin.voicelist.Count < voicecount then
  FPin.FVoiceCount := FPin.voicelist.Count
 else
  FPin.FVoiceCount := voiceCount;

 Result := S_OK;
end;


function TMWavePlayer.Read(index: integer; FileName : String): HResult; stdcall;
var
  v,vv : PBCVoice;
  i : integer;
begin

  v := FPin.VoiceCheck(index);

  if v = nil then
  begin
   Result := ERROR;
   exit;
  end;

  Result := S_OK;

  if FileName = v.FFileName then
  exit;


  for i := 0 to FPin.voicelist.Count - 1 do
  begin
   vv := FPin.voicelist.Items[i];

   if vv.FFilename = Filename then
   begin
    v.Clone(vv);
    Exit;
   end;

  end;//end for i------------------------//


  FPin.VoiceFree(index);

  v.ReadTheFile(pchar(FileName));

  FPin.SetFileMapping;//Error afterwards?


end;



function TMWavePlayer.Play(index: integer; state : integer): HResult; stdcall;
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

  Result := S_OK;
end;

function TMWavePlayer.Volume(index: integer; val : double): HResult; stdcall;
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

  Result := S_OK;
end;

function TMWavePlayer.Pan(index: integer; val : double): HResult; stdcall;
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

  Result := S_OK;
end;

function TMWavePlayer.Phase(index: integer; val : double): HResult; stdcall;
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

  Result := S_OK;
end;

function TMWavePlayer.Pitch(index: integer; val : double): HResult; stdcall;
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

  Result := S_OK;
end;

function TMWavePlayer.Sync(index: integer; state: integer): HResult; stdcall;
var
  v: PBCVoice;
begin
  v := FPin.VoiceCheck(index);

  if v = nil then
  begin
   Result := ERROR;
   exit;
  end;

  v.FSync := Boolean(state);

  Result := S_OK;
end;

function TMWavePlayer.Loop(index: integer; state:integer):HResult; stdcall;
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

  v.FDoSeek := Boolean(state);

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

  val := v.FPosition;

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

function TMWavePlayer.SetBlockSize(val: Integer) : HRESULT; stdcall;
begin
  if val < BLOCKSIZE then   
    FPin.Route.BlockSize := val;

  Result := S_OK;
end;

function TMWavePlayer.GetBlockSize(out val : Integer) : HRESULT; stdcall;
begin
  val := FPin.Route.BlockSize;

  Result := S_OK;
end;

function TMWavePlayer.setRouting( count : Integer; val : TRoutingArray ) : HRESULT; stdcall;
var
  i,n : integer;
begin

  n := 0;

  for i := 0 to count - 1 do
  if val[i] = -1 then inc(n);

  if n = count then
   FPin.SetFileMapping
  else
   FPin.SetRouting(count,val);

  Result := S_OK;
end;

//TMWavePlayerRoute-----------------------------------------------------------//
Constructor TMWavePlayerRoute.Create;
var
 i : integer;
begin
 inherited;

 FChannelMask     := DEFAULTCHANNELMASK;
 FBlockSize       := BLOCKSIZESMALL;
 FChannelIdCount  := 0;
 FReload          := false;

 FFChannelCount   := NUMCHANNELS;
 FFRoutingCount   := NUMCHANNELS;
 FFUseFileMapping := true;

 SetLength(FChannelId,      MAXCHANNELS);
 SetLength(FFChannelFactor, MAXCHANNELS);
 SetLength(FFRouting,       MAXCHANNELS);
 SetLength(FChannelFactor,  MAXCHANNELS);
 SetLength(FChannelName,    MAXCHANNELS);

 for i := 0 to MAXCHANNELS - 1 do
 begin
  FChannelName   [i] := '';
  FFRouting      [i] := 0;
  FFChannelFactor[i] := 1;
 end;

 Update;

end;

Destructor TMWavePlayerRoute.Destroy;
begin
 inherited;
end;

//Update the real values with the temporary ones
procedure TMWavePlayerRoute.Update;
var
  i: Integer;
begin

  FChannelCount   := FFChannelCount;
  FRoutingCount   := FFRoutingCount;
  FUseFileMapping := FFUseFileMapping;

  //-------------------------------------//

  if FChannelCount > 0 then
  begin
   for i := 0 to FChannelCount - 1 do
   FChannelFactor[i] := FFChannelFactor[i];
  end;

  if FChannelCount < 1 then
  begin
   FChannelCount := 1;

   FChannelFactor[0] := 1;
  end;

  //-------------------------------------//

  if FRoutingCount > 0 then
  begin
   SetLength(FRouting, FRoutingCount);

   for i := 0 to FRoutingCount - 1 do
   FRouting[i] := FFRouting[i];
  end;

  //-------------------------------------//

  if FRoutingCount < 1 then
  begin
   FRoutingCount := 1;

   SetLength(FRouting, MAXCHANNELS);

   for i := 0 to MAXCHANNELS - 1 do
   FRouting[i] := 0;
  end;

  FReload := false; //set toggle

end;


procedure TMWavePlayerRoute.SetRouting(count: Integer; val: TRoutingArray);
var
 i,k     : Integer;
 routing : TRoutingArray;
 factor  : Array [0..MAXCHANNELS-1] of Integer;
begin

 FFUseFileMapping := false;
 FFRoutingCount   := 0;
 FFChannelCount   := 0;
 FChannelMask     := 0;
 FChannelIdCount  := 0;

 SetLength(FFRouting,count);
 SetLength(routing,count);

 for i := 0 to MAXCHANNELS - 1 do
 begin
  factor[i]          := 0;
  FFChannelFactor[i] := 0;
 end;

 for i := 0 to count - 1 do
 if (val[i] >= -1) and (val[i] < MAXCHANNELS) then //all valid routing values [0..17]
 begin

  if val[i] = -1 then
   routing[FFRoutingCount] := 0
  else
   routing[FFRoutingCount] := val[i];

  Inc(FFRoutingCount);
 end;

 for i := 0 to FFRoutingCount - 1 do //how often is a channel contained in the routing-array
 Inc(factor[routing[i]]);

 for i := 0 to MAXCHANNELS - 1 do
 if factor[i] > 0 then
 begin
  Inc(FFChannelCount);

  FChannelMask := FChannelMask or CHANNELCODE[i];

  FChannelName [FChannelIdCount] := CHANNELNAME[i];
  FChannelId   [FChannelIdCount] := i; //map the channels to an array i0=c3 i1=c1 i2=c2...

  Inc(FChannelIdCount);
 end;

 for i := 0 to FFRoutingCount - 1 do //map the routing-array to the sequence in channelId
 for k := 0 to FFChannelCount - 1 do
 if FChannelId[k] = routing[i] then
 FFRouting[i] := k;

 for i := 0 to FFRoutingCount - 1 do //how often is a channel contained in the mapped routing-array
 Inc(FFChannelFactor[FFRouting[i]]);

 FReload := true; //set toggle

end;

procedure TMWavePlayerRoute.SetFileMapping(channelMask : Integer; channelCount : Integer; map : Array of integer);
var
 i : Integer;
begin

 FFUseFileMapping := true;
 FFChannelCount   := channelCount;
 FChannelMask     := channelMask;

 if FChannelMask = 0 then
 FChannelMask := CHANNELMASK;

 for i := 0 to FFChannelCount - 1 do
 FFChannelFactor[i] := map[i];

 FReload := true; //set toggle

end;


//TMWavePlayerPin-------------------------------------------------------------//
constructor TMWavePlayerPin.Create(out hr: HRESULT; Filter: TBCSource);
begin
 inherited Create(FilterName, hr, Filter, 'Out');

 VoiceList := TList.Create;
 VoiceList.Clear;

 FRoute        := TMWavePlayerRoute.Create;
 FVoiceCount   := 0;

end;

destructor TMWavePlayerPin.Destroy;
var
 i : Integer;
begin

 for i := 0 to voiceList.Count - 1 do
 if Assigned(VoiceList.Items[i]) then VoiceFree(i);

 VoiceList.Free;
 FreeAndNil(FRoute);

 inherited;

end;

function TMWavePlayerPin.GetMediaType(pmt : PAMMediaType) : HRESULT;
var
 size : longint;
 pwf  : PWAVEFORMATEXTENSIBLE;

begin

 size := sizeof(TWAVEFORMATEXTENSIBLE);
 pwf  := CoTaskMemAlloc(size);

 if pwf = nil then begin result := E_OUTOFMEMORY; exit; end;

 pmt.pbFormat             := @pwf.format;
 pmt.cbFormat             := sizeof(TWAVEFORMATEXTENSIBLE);
 pmt.majortype            := MEDIATYPE_Audio;
 pmt.subtype              := MEDIASUBTYPE_PCM;
 pmt.formattype           := FORMAT_WaveFormatEx;
 pmt.bFixedSizeSamples    := TRUE;
 pmt.bTemporalCompression := FALSE;
 pmt.lSampleSize          := (BITSPERSAMPLE div BITSPERBYTE) * Route.FFChannelCount;
 pmt.pUnk                 := nil;

 pwf.Format.cbSize          := 22;
 pwf.Format.wFormatTag      := WAVE_FORMAT_EXTENSIBLE;
 pwf.Format.nChannels       := Route.FFChannelCount;
 pwf.Format.nSamplesPerSec  := SAMPLERATE;
 pwf.Format.wBitsPerSample  := BITSPERSAMPLE;
 pwf.Format.nBlockAlign     := Route.FFChannelCount * (BITSPERSAMPLE div BITSPERBYTE);
 pwf.Format.nAvgBytesPerSec := pwf.Format.nBlockAlign * SAMPLERATE;

 pwf.Samples.wValidBitsPerSample := BITSPERSAMPLE;
 pwf.Samples.wSamplesPerBlock    := 0;
 pwf.Samples.wReserved           := 0;
 pwf.SubFormat                   := KSDATAFORMAT_SUBTYPE_PCM;
 pwf.dwChannelMask               := Route.FChannelMask;


 Result := S_OK;
end;


function TMWavePlayerPin.DecideBufferSize(Allocator: IMemAllocator;
  Properties: PAllocatorProperties): HRESULT;
var
  Actual: ALLOCATOR_PROPERTIES;
begin
  ASSERT(Allocator <> nil);
  ASSERT(Properties <> nil);

  Properties.cbBuffer := Route.BlockSize * Route.FFChannelCount * BYTESPERSAMPLE;
  Properties.cBuffers := 1;
  Properties.cbAlign  := Route.BlockSize * Route.FFChannelCount * BYTESPERSAMPLE;
  Properties.cbPrefix := 0;

  // Ask the allocator to reserve us the memory
  Result := Allocator.SetProperties(Properties^, Actual);

  if Result <> S_OK then
  begin
   Route.BlockSize := BLOCKSIZE;

   Properties.cbBuffer := BLOCKSIZE;
   Properties.cBuffers := 1;
   Properties.cbAlign  := BLOCKALIGN;
   Properties.cbPrefix := 0;

   // Ask the allocator to reserve us the memory
   Result := Allocator.SetProperties(Properties^, Actual);
  end;

end;

function TMWavePlayerPin.Notify(Filter: IBaseFilter; q: TQuality) : HRESULT;
begin
 Result := E_FAIL;

end;

procedure TMWavePlayerPin.SetRouting(count : Integer; val : TRoutingArray);
var
 voice : PBCVoice;
begin
 FRoute.SetRouting(count,val);

end;

procedure TMWavePlayerPin.SetFileMapping;
var
 i,k : Integer;
 voice : PBCVoice;
 map : Array [0..MAXCHANNELS-1] of integer;
 channelMask, channelCount : Integer;
begin

   channelMask  := 0;
   channelCount := 0;

   for i := 0 to MAXCHANNELS - 1 do
   map[i] := 0;

   for i := 0 to VoiceList.Count - 1 do//----------------------------//
   begin
    voice := VoiceCheck(i);

    if voice = nil then
      Continue;

    if not voice.FReload then
    for k := 0 to voice.FFChannelCount - 1 do
    begin
     channelMask := channelMask or CHANNELCODE[voice.FChannelMap[k]];
     inc(map[voice.FChannelMap[k]]);
    end;

    if voice.FReload then
    for k := 0 to voice.FFChannelCount - 1 do
    begin
     channelMask := channelMask or CHANNELCODE[voice.FFChannelMap[k]];
     inc(map[voice.FFChannelMap[k]]);
    end;

   end;//end for i--------------------------------------------------//

   for i := 0 to MAXCHANNELS - 1 do
   if map[i] > 0 then
   inc(channelCount);

   Route.SetFileMapping(channelMask, channelCount, map);

end;

function TMWavePlayerPin.FillBuffer(ims: IMediaSample) : HRESULT;
var
 voice      : PBCVoice;
 nSamples   : Integer;
 nFrames    : Integer;
 nChannels  : Integer;
 nVoices    : Integer;
 sinkSample : PAudioSample;
 channel    : TChannelArray;
 buffer     : Array [0..MAXCHANNELS*BLOCKSIZE] of Double;

//----------------------------------------------------------------------------//

procedure SetSilent;
var
 sinkByte : PByte;
 i : Integer;
begin
 ims.GetPointer(sinkByte);
 sinkSample := PAudioSample(sinkByte);

 for i := 0 to ims.GetSize - 1 do
 begin
  sinkByte^ := 0;
  Inc(sinkByte);
 end;

end;

procedure SetVariables;
var
 sinkByte : PByte;
 k,i      : Integer;

begin
 ims.GetPointer(sinkByte);
 sinkSample := PAudioSample(sinkByte);

 nChannels  := Route.ChannelCount;
 nSamples   := ims.GetSize div BYTESPERSAMPLE;
 nVoices    := FVoiceCount;             //???
 nFrames    := nSamples div nChannels;  

 for i := 0 to nSamples - 1 do
 buffer[i] := 0;

 for k := 0 to MAXCHANNELS - 1 do
 for i := 0 to nFrames - 1 do
  channel[k][i] := 0;

end;

//----------------------------------------------------------------------------//

procedure UpdateVoices;
var
 i : integer;
begin

 for i := 0 to VoiceList.Count - 1 do
 begin
  voice := VoiceList.Items[i];

  if voice <> nil then
  if voice.FReload then
   voice.Update;

 end;//end for i

end;

//----------------------------------------------------------------------------//

procedure GetVoices;
var
 v : Integer;
begin

 for v := 0 to nVoices - 1 do
 begin
  voice := VoiceList.Items[v];

  if voice <> nil then
  voice.FillBuffer(nFrames);

 end;

end;

//----------------------------------------------------------------------------//

  procedure FillChannelFileMapping;
  var
   v,k,f : Integer;
   map : Integer;
  begin

   for v := 0 to nVoices - 1 do
   begin

    voice := voiceList.Items[v];

    if voice.FInitialized then
    for k := 0 to voice.FChannelCount - 1 do
    begin

      map := voice.FChannelMap[k];

      for f := 0 to nFrames - 1 do
      if voice.FPlay then
      channel[map][f] := channel[map][f] + (voice.FChannel[k][f] / Route.FChannelFactor[map]);

    end;

   end;//end for v

  end;

//----------------------------------------------------------------------------//

  procedure FillChannelRouting;
  var
   v,k,f,i       : Integer;
   routeTo       : Integer;
   count         : Integer;
   channelFactor : Array [0..MAXCHANNELS-1] of Double;
  begin

   count := 0;

   //Set Channel-Factor---------------------------------//

   for i := 0 to MAXCHANNELS - 1 do
    channelFactor[i] := 0;

   for v := 0 to nVoices - 1 do
   begin

    voice := voiceList.Items[v];

    if voice.FInitialized then
    for k := 0 to voice.FChannelCount - 1 do
    begin

     routeTo := Route.Routing[count];

     channelFactor[routeTo] := channelFactor[routeTo] + 1;

     count := (count + 1) mod Route.RoutingCount;

    end;

   end;

   count := 0;

   //---------------------------------------------------//

   for v := 0 to nVoices - 1 do
   begin

    voice := voiceList.Items[v];

    if voice.FInitialized then
    for k := 0 to voice.FChannelCount - 1 do
    begin

     routeTo := Route.Routing[count];

     for f := 0 to nFrames - 1 do
     if voice.FPlay then
     channel[routeTo][f] := channel[routeTo][f] + voice.FChannel[k][f] / channelFactor[routeTo];

     count := (count + 1) mod Route.RoutingCount;

    end;

   end;//end for v

  end;

//----------------------------------------------------------------------------//

  procedure FillBuffer;
  var
   f,k   : Integer;
   shift : Integer;
  begin

   for f := 0 to nFrames - 1 do
   begin
    shift := f * nChannels;

    for k := 0 to nChannels - 1 do
    buffer[shift + k] := channel[k][f];

   end;

  end;

//----------------------------------------------------------------------------//

  procedure FillSink;
  var
   i : Integer;
  begin

   for i := 0 to nSamples - 1 do
   begin
    sinkSample^ := round(buffer[i]);

    Inc(sinkSample);
   end;

  end;

//----------------------------------------------------------------------------//

begin

 Result := -1;

 if (ims = nil) then exit;

  SetSilent;

 if Route.FReload then
  Route.Update;

  UpdateVoices;

  SetVariables;

  GetVoices;

  if Route.UseFileMapping then
   FillChannelFileMapping
  else
   FillChannelRouting;

  FillBuffer;

  FillSink;

 Result := S_OK;

end;


function TMWavePlayerPin.VoiceCheck(index: Integer): PBCVoice;
var
 v: PBCVoice;
begin
 if index >= VoiceList.Count then
  VoiceList.Count := index + 1;

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

  for i := 0 to VoiceList.Count - 1 do
  if i <> index then
  begin
    if assigned(VoiceList.Items[i]) then
    begin
      vv := VoiceList.Items[i];
      if vv.FFileName = v.FFileName then exit;
    end;
  end;

  if v <> nil then  
  v.Reset; //

end;

//TMVoice---------------------------------------------------------------------//
constructor TMVoice.Create;
var
   i : integer;
begin
   FFilename       := '';
   FSize           := 0;
   FData           := nil;
   FSourceFrames   := 0;
   FPlay           := false;
   FSync           := false;
   FGain           := 1;
   FPan            := 0.5;
   FPosition       := 0;
   FPhase          := 0;
   FPitch          := 1;
   FStartTime      := 0;
   FFStartTime     := 0;
   FEndTime        := 0;
   FFEndTime       := 0;
   FSeekPosition   := 0;
   FDoSeek         := false;
   FDuration       := 0;
   FFading         := 0;
   FLoop           := false;
   FFrameFraction  := 1;
   FReload         := false;
   FPhaseShift     := 0;
   FPhaseShiftPrev := 0;
   FChannelSize    := 0;
   FInitialized    := false;

   FFData          := nil;
   FFSize          := 0;
   FFSourceFrames  := 0;
   FFFrameFraction := 1;
   FFChannelCount  := NUMCHANNELS;
   FFDuration      := 1;

   for i := 0 to MAXCHANNELS - 1 do
   FFChannelMap [i] := 0;

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

function TMVoice.ReadTheFile(AFileName: String): Boolean;
var
  _Mem: PByte;
  hmm : HMMIO;
  mmiParent : MMCKINFO;
  mmiSub : MMCKINFO;
  mmres : MMRESULT;
  ires, i : longint;
  fcc : FOURCC;
  count : Integer;
  tmpMask : Cardinal;
  tmpCode : Array [0..17] of integer;
  upCount : integer;
begin
  Result  := FALSE;

  // Open the file for reading with buffered I/O. Let windows use its default internal buffer
  hmm := mmioOpen(pchar(AFileName), nil, MMIO_READ + MMIO_ALLOCBUF);
  if (hmm = NULL) then
  begin
    OutputDebugString('waveplayer open error');
    MMIOClose(hmm, 0);
    Exit;
  end;

  // Locate a "RIFF" chunk with a "WAVE" form type to make sure the file is a waveform-audio file.
  fcc := MAKEFOURCC('W', 'A', 'V', 'E');
  mmres := mmioDescend(hmm, @mmiParent, nil, 0);
  if mmres <> MMSYSERR_NOERROR then
  begin
    OutputDebugString('waveplayer error 1');
    MMIOClose(hmm, 0);
    Exit;
  end;
  if mmiParent.ckid <> FOURCC_RIFF then
  begin
    OutputDebugString('waveplayer error 2');
    MMIOClose(hmm, 0);
    Exit;
  end;
  if mmiParent.fccType <> fcc then
  begin
    OutputDebugString('waveplayer error 3');
    MMIOClose(hmm, 0);
    Exit;
  end;

  mmiSub.ckid := MAKEFOURCC('f', 'm', 't', ' ');
  mmres := mmioDescend(hmm, @mmiSub, @mmiParent, MMIO_FINDCHUNK);

  if mmres <> MMSYSERR_NOERROR then
  begin
    OutputDebugString('waveplayer error 4');
    MMIOClose(hmm, 0);
    Exit;
  end;

  if mmiSub.cksize < sizeof(PCMWAVEFORMAT) then
  begin
   OutputDebugString('waveplayer error 5');
   MMIOClose(hmm, 0);
   Exit;
  end
  else if mmiSub.cksize = sizeof(PCMWAVEFORMAT) then
  begin
   mmioRead(hmm, @FFWaveFormat, sizeof(PCMWAVEFORMAT));

   if FFWaveFormat.Format.wFormatTag <> WAVE_FORMAT_PCM then
   begin
     OutputDebugString('WavePlayer: unknown format');
     MMIOClose(hmm, 0);
     Exit;
   end;
  end
  else
  begin
   mmioRead(hmm, @FFWaveFormat, sizeof(WAVEFORMATEXTENSIBLE));

   //seems even if cksize > sizeof(PCMWAVEFORMAT)
   //wFormatTag may still be WAVE_FORMAT_PCM for some files
   if (FFWaveFormat.Format.wFormatTag <> WAVE_FORMAT_EXTENSIBLE)
   and (FFWaveFormat.Format.wFormatTag <> WAVE_FORMAT_PCM) then
   begin
     OutputDebugString('WavePlayer: unknown format');
     MMIOClose(hmm, 0);
     Exit;
   end;
  end;

  mmres := mmioAscend(hmm, @mmiSub, 0);
  if mmres <> MMSYSERR_NOERROR then
  begin
    OutputDebugString('waveplayer error 8');
    MMIOClose(hmm, 0);
    Exit;
  end;

  mmioSeek(hmm, mmiParent.dwDataOffset + sizeof(FOURCC), SEEK_SET);
  //if mmres < 0 then begin OutputDebugString('waveplayer error 9'); Exit; end;

  mmiSub.ckid := MAKEFOURCC('d', 'a', 't', 'a');
  mmres := mmioDescend(hmm, @mmiSub, @mmiParent, MMIO_FINDCHUNK);
  if mmres <> MMSYSERR_NOERROR then
  begin
    OutputDebugString('error 10');
    MMIOClose(hmm, 0);
    Exit;
  end;

  _Mem := CoTaskMemAlloc(mmiSub.ckSize);
  if (_Mem = nil) then
  begin
    OutputDebugString('waveplayer: out of memory');
    MMIOClose(hmm, 0);
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
  FFilename        := AFileName;
  FFData           := _Mem;
  FFSize           := mmiSub.cksize;
  FFSourceFrames   := trunc(FFSize / (FFWaveFormat.Format.wBitsPerSample div BITSPERBYTE) / FFWaveFormat.Format.nChannels);
  FFDuration       := FFSourceFrames / FFWaveFormat.Format.nSamplesPerSec;
  FFFrameFraction  := 1.0 / FFSourceFrames;
  FFChannelCount   := FFWaveFormat.Format.nChannels;


  for i := 0 to MAXCHANNELS - 1 do
  FFChannelMap[i] := 0;

  if FFWaveFormat.Format.wFormatTag = WAVE_FORMAT_PCM then
  begin

   if FFChannelCount = 1 then
    FFChannelMap[0] := 0;

   if FFChannelCount = 2 then
   begin
    FFChannelMap[0] := 0;
    FFChannelMap[1] := 1;
   end;

  end;

  if FFWaveFormat.Format.wFormatTag = WAVE_FORMAT_EXTENSIBLE then
  begin
   count := 0;

   tmpMask := FFWaveFormat.dwChannelMask;

   for i:= 0 to 17 do
   begin
    FFChannelMap[i] := 0;
    tmpCode     [i] := 0;
   end;

   if FFWaveFormat.dwChannelMask <> 0 then
   begin

     for i := MAXCHANNELS - 1 downto 0 do
     begin

       if tmpMask >= CHANNELCODE[i] then
       begin
        tmpCode[count] := i;
        tmpMask := tmpMask - CHANNELCODE[i];
        Inc(count);
       end;

     end;

     upCount := 0;

     for i := count-1 downto 0 do
     begin
       FFChannelMap[upCount] := tmpCode[i];
       Inc(upCount);
     end;

   end;

   if (FFWaveFormat.dwChannelMask = 0) and (FFWaveFormat.Format.nChannels = 6) then
   begin
    FFWaveFormat.dwChannelMask := CHANNELMASK5POINT1;

    FFChannelMap[0] := 0;
    FFChannelMap[1] := 1;
    FFChannelMap[2] := 2;
    FFChannelMap[3] := 3;
    FFChannelMap[4] := 4;
    FFChannelMap[5] := 5;

   end;

   if (FFWaveFormat.dwChannelMask = 0) and (FFWaveFormat.Format.nChannels = 8) then
   begin
    FFWaveFormat.dwChannelMask := CHANNELMASK7POINT1;

    FFChannelMap[0] := 0;
    FFChannelMap[1] := 1;
    FFChannelMap[2] := 2;
    FFChannelMap[3] := 3;
    FFChannelMap[4] := 4;
    FFChannelMap[5] := 5;
    FFChannelMap[6] := 9;
    FFChannelMap[7] :=10;

   end;

  end;

  //Close the file----
  MMIOClose(hmm, 0);

  Result       := true;
  FInitialized := true;
  FReload      := true;
end;

procedure TMVoice.Clone(other: PBCVoice);
var
 k : integer;
begin

     FFilename        := other.FFilename;
     FFSize           := other.FFSize;
     FFData           := other.FFData;
     FFSourceFrames   := other.FFSourceFrames;
     FPlay            := other.FPlay;
     FSync            := other.FSync;
     FGain            := other.FGain;
     FPan             := other.FPan;
     FPosition        := other.FPosition;
     FPhase           := other.FPhase;
     FPitch           := other.FPitch;
     FStartTime       := other.FStartTime;
     FFStartTime      := other.FFStartTime;
     FEndTime         := other.FEndTime;
     FFEndTime        := other.FFEndTime;
     FSeekPosition    := other.FSeekPosition;
     FDoSeek          := other.FDoSeek;
     FFDuration       := other.FFDuration;
     FFading          := other.FFading;
     FLoop            := other.FLoop;
     FFFrameFraction  := other.FFFrameFraction;
     FPhaseShift      := other.FPhaseShift;
     FPhaseShiftPrev  := other.FPhaseShiftPrev;
     FFChannelCount   := other.FFChannelCount;
     FChannelSize     := other.FChannelSize;

     for k := 0 to MAXCHANNELS - 1 do
     FFChannelMap[k] := other.FChannelMap[k];

     FFWaveFormat.Samples                := other.FFWaveFormat.Samples;
     FFWaveFormat.dwChannelMask          := other.FFWaveFormat.dwChannelMask;
     FFWaveFormat.Format.wFormatTag      := other.FFWaveFormat.Format.wFormatTag;
     FFWaveFormat.Format.nChannels       := other.FFWaveFormat.Format.wFormatTag;
     FFWaveFormat.Format.nSamplesPerSec  := other.FFWaveFormat.Format.wFormatTag;
     FFWaveFormat.Format.nAvgBytesPerSec := other.FFWaveFormat.Format.wFormatTag;
     FFWaveFormat.Format.nBlockAlign     := other.FFWaveFormat.Format.wFormatTag;
     FFWaveFormat.Format.wBitsPerSample  := other.FFWaveFormat.Format.wFormatTag;
     FFWaveFormat.Format.cbSize          := other.FFWaveFormat.Format.wFormatTag;

     FReload      := true;

     if FFData <> nil then
     FInitialized := true;

end;


procedure TMVoice.Reset;
begin

  if assigned(FData) then
   CoTaskMemFree(FData);

  if assigned(FFData) then
   CoTaskMemFree(FFData);

  FData        := nil;
  FFData       := nil;
  FInitialized := false;
  FReload      := false;
  FFileName    := '';

end;

procedure TMVoice.Update;
var
   i : integer;
begin

   FSize          := FFSize;
   FData          := FFData;
   FSourceFrames  := FFSourceFrames;
   FChannelCount  := FFChannelCount;
   FFrameFraction := FFFrameFraction;
   FDuration      := FFDuration;

   FWaveFormat.Samples                := FFWaveFormat.Samples;
   FWaveFormat.dwChannelMask          := FFWaveFormat.dwChannelMask;
   FWaveFormat.Format.wFormatTag      := FFWaveFormat.Format.wFormatTag;
   FWaveFormat.Format.nChannels       := FFWaveFormat.Format.nChannels;
   FWaveFormat.Format.nSamplesPerSec  := FFWaveFormat.Format.nSamplesPerSec;
   FWaveFormat.Format.nAvgBytesPerSec := FFWaveFormat.Format.nAvgBytesPerSec;
   FWaveFormat.Format.nBlockAlign     := FFWaveFormat.Format.nBlockAlign;
   FWaveFormat.Format.wBitsPerSample  := FFWaveFormat.Format.wBitsPerSample;
   FWaveFormat.Format.cbSize          := FFWaveFormat.Format.cbSize;

   if FChannelCount > 0 then
    FChannelCount := FFChannelCount;

   if FChannelCount < 1 then
    FChannelCount := NUMCHANNELS;

   For i := 0 to MAXCHANNELS - 1 do
     FChannelMap[i] := FFChannelMap[i];

   FReload := false;

end;

function TMVoice.FillBuffer(size: integer) : HRESULT;
var
  nFrames     : Integer;
  wheel       : Double;
  loop        : Boolean;
  phaseStart  : Double;
  phaseEnd    : Double;
  phaseSeek   : Double;
  interval    : Double;
  gain        : Double;
  pitch       : Double;
  nChannels   : Integer;
  fadingGain  : Double;
  frameIndex  : Integer;
  sourceFrame : TSourceFrame;


  function TimeToPhase(value : Double) : Double;
  begin
   Result := value / FDuration ;

  end;           

  procedure SetInterval;
  begin

   if (not FSync) then
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

  procedure SetVariables;
  var
   i,k : Integer;

  begin
   loop       := FLoop;
   nChannels  := FChannelCount;
   nFrames    := size;
   gain       := FGain; //Vorerst
   pitch      := FPitch;
   fadingGain := 1;

   SetInterval;

   for i := 0 to MAXCHANNELS - 1 do
   sourceFrame[i] := 0;

   FChannelSize := nFrames;

   for k := 0 to nChannels - 1 do
   for i := 0 to FChannelSize - 1 do
     FChannel[k][i] := 0;

  end;

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

   FPhase  := phaseSeek;
   FDoSeek := false;

   FStartTime := FFStartTime; 
   FEndTime   := FFEndTime;

   //Outputdebugstring(pchar(format('FENDTIME: %f',[FEndTime])));

  end;

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

   if FSync then
   if (FPhase <= phaseStart) or (FPhase >= phaseEnd) then
   begin
    FStartTime := FFStartTime;
    FEndTime   := FFEndTime;
   end;

   Result := true;

  end;

  procedure SetSource;
  var
   byteIndex        : Integer;
   source           : PByte;
   sourceFrameIndex : Integer;
   k                : Integer;
  begin

   source           := FData;
   sourceFrameIndex := round(wheel);

   if sourceFrameIndex >= FSourceFrames then  //security
   sourceFrameIndex := FSourceFrames - 1;

   if sourceFrameIndex < 0 then
   sourceFrameIndex := 0;

   byteIndex := sourceFrameIndex * (BYTESPERSAMPLE * nChannels);

   Inc(source,byteIndex);

   for k := 0 to nChannels - 1 do
   begin
    sourceFrame[k] := PAudioSample(source)^;

    Inc(source,BYTESPERSAMPLE);
   end;

  end;

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

  procedure FillChannel;
  var
   k : Integer;
  begin

   for k := 0 to nChannels - 1 do
   begin
    sourceFrame[k] := sourceFrame[k] * (gain * fadingGain);

    FChannel[k][frameIndex] := sourceFrame[k]; 
   end;

  end;

  procedure SetPosition;
  begin
    FPosition := FDuration * FPhase;

  end;

begin

  Result := ERROR;

  if not FInitialized then exit;

   SetVariables;

  if FPlay then
  for frameIndex := 0 to nFrames - 1 do//-----------//
  begin

   if FDoSeek then
   Seek;

   if (FPhaseShift <> FPhaseShiftPrev) and loop then
   ShiftPhase;

   if not SetWheel then
   Continue;

   if loop then
   SetGain;

   SetSource;

   FillChannel;

  end;//end for frameIndex-------------------------//

  SetPosition;

  Result := S_OK;

end;

//****************************************************************************//

initialization

 TBCClassFactory.CreateFilter( TMWavePlayer,
                               FilterName,
                               CLSID_WavePlayer,
                               CLSID_LegacyAmFilterCategory,
                               MERIT_DO_NOT_USE,
                               1,
                               @sudOutputPin );

end.






