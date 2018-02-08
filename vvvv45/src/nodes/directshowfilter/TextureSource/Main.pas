//////project name
//TextureSource

//////description
//directshow source filter.
//lets you pump a DirextX9 texture into a directshow graph.

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

unit Main;

interface

uses
  Windows, ActiveX,
  BaseClass, DirectShow9, DXSUtil;

const
  CLSID_TextureSource: TGUID = '{E446D455-7C13-492A-9C96-38F948687E8B}';
  FilterName: String = 'TextureSource';

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

  ITextureSource = interface(IUnknown)['{E446D455-7C13-492A-9C96-38F948687E8B}']
    function Pass(buf: PByte; size: longint): HResult; stdcall;
  end;

  TMTextureSourcePushPin = class(TBCSourceStream)
  protected
    // To track where we are in the file
    FFramesWritten: Integer;
    // Do we need to clear the buffer?
    FZeroMemory: Boolean;
    // The time stamp for each sample
    FSampleTime: TRefTime;
    // Pointer to the bitmap header
    FBitmapInfo: PBitmapInfo;
    // Size of the bitmap header
    FBitmapInfoSize: DWord;
    // Points to beginning of file buffer
    FFileBuffer: PByte;
    // Points to pixel bits
    FImage: PByte;
    // How many frames have been displayed
    FFrameNumber: Integer;
    // How many frames have been passed
    FPassFrame: Integer;
    FLastFrame: Integer;
    // Duration of one frame
    FFrameLength: TReferenceTime;

    FSize: longint;
  public
   constructor Create(out hr: HResult; Filter: TBCSource);
    destructor Destroy; override;
    // Override the version that offers exactly one media type
    function GetMediaType(MediaType: PAMMediaType): HResult; override;
    function DecideBufferSize(Allocator: IMemAllocator; Properties: PAllocatorProperties): HRESULT; override;
    function FillBuffer(Sample: IMediaSample): HResult; override;
    procedure ParseBuf(Bitmap: PByte; Size: longint);
    function Notify(Filter: IBaseFilter; q: TQuality): HRESULT; override; stdcall;
  end;

  TMTextureSourcePushFilter = class(TBCSource, ITextureSource, IPersist)
  private
    FPin: TMTextureSourcePushPin;

  public
    constructor Create(ObjName: string; Unk: IUnKnown; out hr: HRESULT);
    constructor CreateFromFactory(Factory: TBCClassFactory; const Controller: IUnknown); override;
    destructor Destroy; override;
    function NonDelegatingQueryInterface(const IID: TGUID; out Obj): HResult; override;
    function Pass(buf: PByte; size: longint): HResult; stdcall;
  end;


implementation

constructor TMTextureSourcePushPin.Create(out hr: HResult; Filter: TBCSource);
begin
  inherited Create(FilterName, hr, Filter, 'Out');

  FFramesWritten := 0;
  FZeroMemory := False;
  FBitmapInfo := nil;
  FBitmapInfoSize := 0;
  FFileBuffer := nil;
  FImage := nil;
  FFrameNumber := 0;
  FPassFrame := 0;
  FLastFrame := 0;
  FFrameLength := DefaultFrameLength;
end;

procedure TMTextureSourcePushPin.ParseBuf(Bitmap: PByte; Size: longint);
var
  bmpFileHeaderSize: Integer;
  bmpFileHeader: PBitmapFileHeader;
  pb: PByte;
begin
  if FFileBuffer = nil then
    FFileBuffer := CoTaskMemAlloc(Size);
  if (FFileBuffer = nil) then
    Exit;

  CopyMemory(FFileBuffer, Bitmap, Size);
  bmpFileHeaderSize := SizeOf(TBitmapFileHeader);
  bmpFileHeader := PBitmapFileHeader(FFileBuffer);

  //Store the Size of the BITMAPINFO
  FBitmapInfoSize := bmpFileHeader.bfOffBits - bmpFileHeaderSize;

  //Store a pointer to the BITMAPINFO
  pb := PByte(FFileBuffer);
  Inc(pb, bmpFileHeaderSize);
  FBitmapInfo := PBitmapInfo(pb);

  //Store a pointer to the starting address of the pixel bits
  Inc(pb, FBitmapInfoSize);
  FImage := pb;
end;

destructor TMTextureSourcePushPin.Destroy;
begin
  if Assigned(FFileBuffer) then
  begin
    CoTaskMemFree(FFileBuffer);
    FFileBuffer := nil;
  end;

  inherited;
end;

function TMTextureSourcePushPin.GetMediaType(MediaType: PAMMediaType): HResult;
var
  pvi: PVideoInfoHeader;
begin
  FFilter.StateLock.Lock;
  try
    if (MediaType = nil) then
    begin
      Result := E_POINTER;
      Exit;
    end;

    if (FFileBuffer = nil) then
    begin
      Result := E_FAIL;
      Exit;
    end;
    //Allocate enough room for the VIDEOINFOHEADER and the color tables
    MediaType.cbFormat := SIZE_PREHEADER + FBitmapInfoSize;
    pvi := CoTaskMemAlloc(MediaType.cbFormat);
    if (pvi = nil) then
    begin
      Result := E_OUTOFMEMORY;
      Exit;
    end;

    ZeroMemory(pvi, MediaType.cbFormat);
    pvi.AvgTimePerFrame := FFrameLength;

    //Copy the header info
    CopyMemory(@pvi.bmiHeader, FBitmapInfo, FBitmapInfoSize);

    //Set image size for use in FillBuffer
    pvi.bmiHeader.biSizeImage := GetBitmapSize(@pvi.bmiHeader);

    //Clear source and target rectangles
    //we want the whole image area rendered
    SetRectEmpty(pvi.rcSource);
    //no particular destination rectangle
    SetRectEmpty(pvi.rcTarget);

    MediaType.majortype := MEDIATYPE_Video;
    MediaType.formattype := FORMAT_VideoInfo;
    // Work out the GUID for the subtype from the header info.
    MediaType.subtype := GetBitmapSubtype(@pvi.bmiHeader);

    MediaType.bTemporalCompression := False;
    MediaType.bFixedSizeSamples := True;
    MediaType.pbFormat := pvi;
    MediaType.lSampleSize := pvi.bmiHeader.biSizeImage;

    Result := S_OK;
  finally
    FFilter.StateLock.UnLock;
  end;
end;

function TMTextureSourcePushPin.DecideBufferSize(Allocator: IMemAllocator;
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
    if (FFileBuffer = nil) then
    begin
      Result := E_FAIL;
      Exit;
    end;

    pvi := AMMediaType.pbFormat;

    // Ensure a minimum number of buffers
    if (Properties.cBuffers = 0) then
      Properties.cBuffers := 2;
    Properties.cbBuffer := pvi.bmiHeader.biSizeImage;

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

function TMTextureSourcePushPin.FillBuffer(Sample: IMediaSample): HResult;
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

  // If the bitmap file was not loaded, just fail here.
  if (FImage = nil) then
  begin
    Result := E_FAIL;
    Exit;
  end;

  // Access the sample's data buffer
  Sample.GetPointer(pData);
  cbData := Sample.GetSize;

  // Check that we're still using video
  Assert(IsEqualGUID(AMMediaType.formattype, FORMAT_VideoInfo));

  pvi := AMMediaType.pbFormat;

    // If we want to change the contents of our source buffer (FImage)
    // at some interval or based on some condition, this is where to do it.
    // Remember that the new data has the same format that we specified in GetMediaType.
    // For example:
    // if(FFrameNumber > SomeValue)
    //    LoadNewBitsIntoBuffer(FImage)

    // Copy the DIB bits over into our filter's output buffer.
    // Since sample size may be larger than the image size, bound the copy size.

  CopyMemory(pData, FImage, min(pvi.bmiHeader.biSizeImage, cbData));

    // Set the timestamps that will govern playback frame rate.
    // If this file is getting written out as an AVI,
    // then you'll also need to configure the AVI Mux filter to
    // set the Average Time Per Frame for the AVI Header.
    // The current time is the sample's start

  Start := FFrameNumber * FFrameLength;
  Stop := Start + FFrameLength;
  //outputdebugstring(pchar(inttostr(fframenumber)));
  Sample.SetTime(@Start, @Stop);
  Inc(FFrameNumber);

    // Set TRUE on every sample for uncompressed frames

  Sample.SetSyncPoint(True);

  Result := S_OK;
end;

function TMTextureSourcePushPin.Notify(Filter: IBaseFilter; q: TQuality): HRESULT;
begin
  Result := E_FAIL;
end;


function TMTextureSourcePushFilter.NonDelegatingQueryInterface(const IID: TGUID;
  out Obj): HResult;
begin
  if IsEqualGUID(IID, CLSID_TextureSource) then
    if GetInterface(CLSID_TextureSource, Obj) then
      Result := S_OK
    else
      Result := E_FAIL
  else
    Result := Inherited NonDelegatingQueryInterface(IID, Obj);
end;

function TMTextureSourcePushFilter.Pass(buf: PByte; size: longint) : HResult; stdcall;
begin
  Fpin.ParseBuf(buf,size);
  result:=S_OK;
end;


constructor TMTextureSourcePushFilter.Create(ObjName: string; Unk: IUnKnown; out hr: 
    HRESULT);
begin
  inherited Create(ObjName, Unk, CLSID_TextureSource);
  // The pin magically adds itself to our pin array.
  FPin := TMTextureSourcePushPin.Create(hr, Self);

  if (hr <> S_OK) then
    if (FPin = nil) then
      hr := E_OUTOFMEMORY;
end;

constructor TMTextureSourcePushFilter.CreateFromFactory(Factory: TBCClassFactory;
  const Controller: IUnknown);
var
  hr: HRESULT;
begin
  Create(Factory.Name, Controller, hr);
end;

destructor TMTextureSourcePushFilter.Destroy;
begin
  FreeAndNil(FPin);
  inherited;
end;

initialization
  // provide entries in the CFactoryTemplate array
  TBCClassFactory.CreateFilter(TMTextureSourcePushFilter, FilterName,
    CLSID_TextureSource, CLSID_LegacyAmFilterCategory,
    MERIT_DO_NOT_USE, 1, @sudOutputPins
    );

end.

