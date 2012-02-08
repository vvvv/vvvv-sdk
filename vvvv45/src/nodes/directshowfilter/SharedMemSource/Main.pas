//////project name
//SharedMemSource

//////description
//directshow source filter.
//lets you pump sharedmemory into a directshow graph.

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
//joreg -> joreg@vvvv.org

//////edited by
//your name here

unit Main;

interface

uses
  Windows, ActiveX,
  BaseClass, DirectShow9, DSUtil;

const
  CLSID_SharedMemSource: TGUID = '{59F06E43-D8F9-4EB0-BAF7-DBADA343CDB0}';
  FilterName: String = 'SharedMemSource';

  // Setup information
  sudPinTypes: TRegPinTypes =
  (
    // video stream connection
    clsMajorType: @MEDIATYPE_Video;
    // all available
    clsMinorType: @MEDIASUBTYPE_NULL
  );

  // pins info
  sudOutputPins: array[0..0] of TRegFilterPins =
  (
    (
    strName: 'Output'; // Pins string name
    bRendered: FALSE; // Is it rendered
    bOutput: TRUE; // Is it an output
    bZero: FALSE; // Are we allowed none
    bMany: FALSE; // And allowed many
    oFilter: nil; // Connects to filter
    strConnectsToPin: nil; // Connects to pin
    nMediaTypes: 1; // Number of types
    lpMediaType: @sudPinTypes // Pin information
    )
    );

  UNITS = 10000000;
  FPS_30 = UNITS div 30;
  FPS_25 = UNITS div 25;
  FPS_20 = UNITS div 20;
  FPS_10 = UNITS div 10;

  DefaultFrameLength: TReferenceTime = FPS_25;


type

  ISharedMemSource = interface(IUnknown)['{A8BB543C-AC95-4B5D-97BB-535117B502DC}']
    function SetShareName(Name: WideString): HResult; stdcall;
    function SetImageFormat(Width: Integer; Height: Integer; Depth: Integer): HResult; stdcall;
    function SetUseSync(UseSync: Boolean): HResult; stdcall;
  end;

  TMSharedMemSourcePushPin = class(TBCSourceStream)
  private
    FBitmapInfoHeader: TBitmapInfoHeader;
    FFlipped: Boolean;
    FImageSize: Integer;
    FMapHandle: THandle;
    FSharedData: Pointer;
    FShareName: WideString;
    FUseSync: Boolean;
    procedure Reinitialize;
  protected
    // To track where we are in the file
    FFramesWritten: Integer;
    // The time stamp for each sample
    FSampleTime: TRefTime;
    // Pointer to the bitmap header
    FBitmapInfo: PBitmapInfo;
    // Size of the bitmap header
    FBitmapInfoSize: DWord;
    // Points to pixel bits
    FRawImageData: PByte;
    // How many frames have been displayed
    FFrameNumber: Integer;
    // How many frames have been passed
    FPassFrame: Integer;
    FLastFrame: Integer;
    // Duration of one frame
    FFrameLength: TReferenceTime;

  public
   constructor Create(out hr: HResult; Filter: TBCSource);
    destructor Destroy; override;
    // Override the version that offers exactly one media type
    function GetMediaType(MediaType: PAMMediaType): HResult; override;
    function DecideBufferSize(Allocator: IMemAllocator; Properties: PAllocatorProperties): HRESULT; override;
    function FillBuffer(Sample: IMediaSample): HResult; override;
    procedure SetShareName(ShareName: WideString);
    function Notify(Filter: IBaseFilter; q: TQuality): HRESULT; override; stdcall;
    procedure SetImageFormat(Width, Height, Depth: Integer);
    procedure SetUseSync(UseSync: Boolean); stdcall;
  end;

  TMSharedMemSourcePushFilter = class(TBCSource, ISharedMemSource, IPersist)
  private
    FPin: TMSharedMemSourcePushPin;
  public
    constructor Create(ObjName: string; Unk: IUnKnown; out hr: HRESULT);
    constructor CreateFromFactory(Factory: TBCClassFactory; const Controller: IUnknown); override;
    destructor Destroy; override;
    function NonDelegatingQueryInterface(const IID: TGUID; out Obj): HResult; override;
    function SetImageFormat(Width, Height, Depth: Integer): HResult; stdcall;
    function SetShareName(ShareName: WideString): HResult; stdcall;
    function SetUseSync(UseSync: Boolean): HResult; stdcall;
  end;


implementation

uses
  SharedMemoryUtils;

constructor TMSharedMemSourcePushPin.Create(out hr: HResult; Filter: TBCSource);
begin
  inherited Create(FilterName, hr, Filter, 'Out');

  FFramesWritten := 0;
  FRawImageData := nil;
  FFrameNumber := 0;
  FPassFrame := 0;
  FLastFrame := 0;
  FImageSize := 0;
  FFrameLength := DefaultFrameLength;

  FCanReconnectWhenActive := true;
end;

procedure TMSharedMemSourcePushPin.SetShareName(ShareName: WideString);
begin
  if ShareName <> FShareName then
  begin
    FShareName := ShareName;
    Reinitialize;
  end;
end;

destructor TMSharedMemSourcePushPin.Destroy;
begin
  FRawImageData := 0;
  CloseMap(FMapHandle, FSharedData);

  inherited;
end;

function TMSharedMemSourcePushPin.GetMediaType(MediaType: PAMMediaType): HResult;
var
  pvi: PVIDEOINFOHEADER;       
begin
  FFilter.StateLock.Lock;
  try
    if (MediaType = nil) then
    begin
      Result := E_POINTER;
      Exit;
    end;

    // Allocate enough room for the VIDEOINFOHEADER and the color tables
    MediaType.cbFormat := SIZE_PREHEADER + SizeOf(TBitmapInfoHeader);
    pvi := CoTaskMemAlloc(MediaType.cbFormat);
    if (pvi = nil) then
    begin
      Result := E_OUTOFMEMORY;
      Exit;
    end;

    ZeroMemory(pvi, MediaType.cbFormat);
    pvi.AvgTimePerFrame := FFrameLength;

    // Copy the header info
    CopyMemory(@pvi.bmiHeader, @FBitmapInfoHeader, SizeOf(TBitmapInfoHeader));

    // Set image size for use in FillBuffer
    pvi.bmiHeader.biSizeImage := GetBitmapSize(@pvi.bmiHeader);

    // Clear source and target rectangles
    // we want the whole image area rendered
    SetRectEmpty(pvi.rcSource);
    // no particular destination rectangle
    SetRectEmpty(pvi.rcTarget);

    MediaType.majortype := MEDIATYPE_Video;
    MediaType.formattype := FORMAT_VideoInfo;
    // Work out the GUID for the subtype from the header info.
    MediaType.subtype := GetBitmapSubtype(@pvi.bmiHeader);
    //OutputDebugString(pchar(GuidToString(MediaType.subtype)));
    MediaType.bTemporalCompression := False;
    MediaType.bFixedSizeSamples := True;
    MediaType.pbFormat := pvi;
    MediaType.lSampleSize := pvi.bmiHeader.biSizeImage;

    Result := S_OK;
  finally
    FFilter.StateLock.UnLock;
  end;
end;

function TMSharedMemSourcePushPin.DecideBufferSize(Allocator: IMemAllocator;
  Properties: PAllocatorProperties): HRESULT;
var
  pvi: PVIDEOINFOHEADER;
  Actual: ALLOCATOR_PROPERTIES;

begin
  if (Allocator = nil) or (Properties = nil) then
  begin
    Result := E_POINTER;
    Exit;
  end;

  FFrameNumber := 0;
  FFilter.StateLock.Lock;
  try
    pvi := AMMediaType.pbFormat;

    // Ensure a minimum number of buffers
    if (Properties.cBuffers = 0) then
      Properties.cBuffers := 2;
    Properties.cbBuffer := FImageSize;

    Result := Allocator.SetProperties(Properties^, Actual);
    if Failed(Result) then
      Exit;

    // Is this allocator unsuitable?
    if (Actual.cbBuffer < Properties.cbBuffer) then
      Result := E_FAIL
    else
      Result := S_OK;

  finally
    FFilter.StateLock.UnLock;
  end;
end;

// This is where we insert the DIB bits into the video stream.
// FillBuffer is called once for every sample in the stream.
function TMSharedMemSourcePushPin.FillBuffer(Sample: IMediaSample): HResult;
var
  pData: PByte;
  cbData: Longint;
  pvi: PVIDEOINFOHEADER;
  Start, Stop: REFERENCE_TIME;

  function min(v1, v2: DWord): DWord;
  begin
    if v1 <= v2 then
      Result := v1
    else
      Result := v2;
  end;

begin
  if (Sample = nil) then
  begin
    Result := E_POINTER;
    Exit;
  end;

  //If the bitmap file was not loaded, just fail here.
  if (FRawImageData = nil) then
  begin
    Result := E_FAIL;
    Exit;
  end;

  //Access the sample's data buffer
  Sample.GetPointer(pData);
  cbData := Sample.GetSize;

  //Check that we're still using video
  Assert(IsEqualGUID(AMMediaType.formattype, FORMAT_VideoInfo));

  pvi := AMMediaType.pbFormat;

    //If we want to change the contents of our source buffer (FRawImageData)
    //at some interval or based on some condition, this is where to do it.
    //Remember that the new data has the same format that we specified in GetMediaType.
    //For example:
    //if(FFrameNumber > SomeValue)
    //   LoadNewBitsIntoBuffer(FRawImageData)

    //Copy the DIB bits over into our filter's output buffer.
    //Since sample size may be larger than the image size, bound the copy size.

  if FUseSync then
    LockMap(FSharename);
  CopyMemory(pData, FRawImageData, min(FImageSize, cbData));
  if FUseSync then
    UnLockMap;

    //Set the timestamps that will govern playback frame rate.
    //If this file is getting written out as an AVI,
    //then you'll also need to configure the AVI Mux filter to
    //set the Average Time Per Frame for the AVI Header.
    //The current time is the sample's start

  Start := FFrameNumber * FFrameLength;
  Stop := Start + FFrameLength;
  //outputdebugstring(pchar(inttostr(fframenumber)));
  Sample.SetTime(@Start, @Stop);
  Inc(FFrameNumber);

  //Set TRUE on every sample for uncompressed frames

  Sample.SetSyncPoint(True);

  Result := S_OK;
end;

function TMSharedMemSourcePushPin.Notify(Filter: IBaseFilter; q: TQuality): HRESULT;
begin
  Result := E_FAIL;
end;

procedure TMSharedMemSourcePushPin.Reinitialize;
begin
  CloseMap(FMapHandle, FSharedData);

  if FImageSize > 0 then
  begin
    OpenMap(FMapHandle, FSharedData, FShareName, FImageSize);
    FRawImageData := PByte(FSharedData);
  end;
end;

procedure TMSharedMemSourcePushPin.SetImageFormat(Width, Height, Depth:
    Integer);
var
  imageSize: Integer;
  flipped: boolean;
begin
  flipped := Height < 0;
  imageSize := Width * Height * Depth;

  if (imageSize <> FImageSize)
  or (flipped <> FFlipped) then
  begin
    FImageSize := abs(imageSize);
    FFlipped := flipped;

    FBitmapInfoHeader.biSize := SizeOf(TBitmapInfoHeader);
    FBitmapInfoHeader.biWidth := Width;
    FBitmapInfoHeader.biHeight := Height;
    FBitmapInfoHeader.biPlanes := 1;
    FBitmapInfoHeader.biBitCount := Depth * 8;
    FBitmapInfoHeader.biCompression := BI_RGB;

    Reinitialize;
  end;
end;

procedure TMSharedMemSourcePushPin.SetUseSync(UseSync: Boolean);
begin
  FUseSync := UseSync;
end;        

function TMSharedMemSourcePushFilter.NonDelegatingQueryInterface(const IID: TGUID;
  out Obj): HResult;
begin
  if IsEqualGUID(IID, CLSID_SharedMemSource) then
    if GetInterface(CLSID_SharedMemSource, Obj) then
      Result := S_OK
    else
      Result := E_FAIL
  else
    Result := Inherited NonDelegatingQueryInterface(IID, Obj);
end;

constructor TMSharedMemSourcePushFilter.Create(ObjName: string; Unk: IUnKnown; out hr: 
    HRESULT);
begin
  inherited Create(ObjName, Unk, CLSID_SharedMemSource);
  // The pin magically adds itself to our pin array.
  FPin := TMSharedMemSourcePushPin.Create(hr, Self);

  if (hr <> S_OK) then
  begin
    if (FPin = nil) then
      hr := E_OUTOFMEMORY;
  end
  else
  begin
    //set some defaults
    FPin.SetImageFormat(320, 240, 3);
    FPin.SetShareName('#vvvv');
  end;                               
end;

constructor TMSharedMemSourcePushFilter.CreateFromFactory(Factory: TBCClassFactory;
  const Controller: IUnknown);
var
  hr: HRESULT;
begin
  Create(Factory.Name, Controller, hr);
end;

destructor TMSharedMemSourcePushFilter.Destroy;
begin
  FreeAndNil(FPin);
  inherited;
end;

function TMSharedMemSourcePushFilter.SetShareName(ShareName: WideString):
    HResult;
begin
  FPin.SetShareName(ShareName);
end;

function TMSharedMemSourcePushFilter.SetImageFormat(Width, Height, Depth:
    Integer): HResult;
begin
  FPin.SetImageFormat(Width, Height, Depth);
end;

function TMSharedMemSourcePushFilter.SetUseSync(UseSync: Boolean): HResult;
begin
  FPin.SetUseSync(UseSync);
  result := S_OK;
end;


initialization
  // provide entries in the CFactoryTemplate array
  TBCClassFactory.CreateFilter(TMSharedMemSourcePushFilter, FilterName,
    CLSID_SharedMemSource, CLSID_LegacyAmFilterCategory,
    MERIT_DO_NOT_USE, 1, @sudOutputPins
    );

end.

