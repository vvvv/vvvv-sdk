//////project name
//Cheater

//////description
//directshow transform filter.
//the main purpose of this filter is to convert mediasamples 
//of type VideoInfoHeader to type VideoInfoHeader2 so that 
//they can be deinterlaced via VMR9.

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
//joreg -> joreg@gmx.at

//////edited by
//your name here

unit Main;

interface
uses
  BaseClass, DirectShow9, ActiveX, Windows;

const
  CLSID_Cheater : TGUID = '{4B131422-187C-42C0-BD71-895C6147D959}';
  IID_CheaterParameters: TGUID = '{75915F42-231C-4247-9FB4-D0314407BC68}';

  MyPinTypes: TRegPinTypes =
    (clsMajorType: @MEDIATYPE_Video;
     clsMinorType: @MEDIASUBTYPE_YUY2);

  MyPins : array[0..1] of TRegFilterPins =
    ((strName: 'Input'; bRendered: FALSE; bOutput: FALSE; bZero: FALSE; bMany: FALSE; oFilter: nil; strConnectsToPin: nil; nMediaTypes: 1; lpMediaType: @MyPinTypes),
     (strName: 'Output'; bRendered: FALSE; bOutput: TRUE; bZero: FALSE; bMany: FALSE; oFilter: nil; strConnectsToPin: nil; nMediaTypes: 1; lpMediaType: @MyPinTypes));

type
  ICheaterParameters = interface
  ['{75915F42-231C-4247-9FB4-D0314407BC68}']
    function FirstFieldFirst(First: boolean): HResult; stdcall;
    function ClearSampleTimes(Clear: boolean): HResult; stdcall;
    function DropSample(Drop: boolean): HResult; stdcall;
    function GetTimes(out Time1, Time2: int64): HResult; stdcall;
  end;

  TMCheater = class(TBCTransformFilter, ICheaterParameters, IPersist)
  private
    FCritSec: TBCCritSec;
    FClearSampleTimes: Boolean;
    FFirstFieldFirst: Boolean;
    FStreamTime: TReferenceTime;
    FSampleStartTime: TReferenceTime;
    FDrop: Boolean;
    FOutputWidth: LongWord;
    FOutputHeight: LongWord;
  public
    constructor Create(ObjName: String; Unk: IUnKnown; out hr: HRESULT);
    destructor Destroy; override;
    constructor CreateFromFactory(Factory: TBCClassFactory; const Controller: IUnknown); override;
    function Transform(pIn, pOut: IMediaSample): HRESULT; overload; override;
    function Copy(Source, dest: IMediaSample): HRESULT;
    function CheckInputType(mtIn: PAMMediaType): HRESULT; override;
    function CheckTransform(mtIn, mtOut: PAMMediaType): HRESULT; override;
    function GetMediaType(Position: integer; out MediaType: PAMMediaType): HRESULT; override;
    function DecideBufferSize(Alloc: IMemAllocator; Properties: PAllocatorProperties): HRESULT; override;

    //IID_CheaterParameters implementation
    function FirstFieldFirst(First: boolean): HResult; stdcall;
    function ClearSampleTimes(Clear: boolean): HResult; stdcall;
    function DropSample(Drop: boolean): HResult; stdcall;
    function GetTimes(out Time1, Time2: int64): HResult; stdcall;
  end;

function ConvertVideoInfoToVideoInfo2(pmt: PAMMediaType): HRESULT;
procedure CopyMediaType(pmtTarget: PAMMEDIATYPE; pmtSource: PAMMEDIATYPE);

implementation


function ConvertVideoInfoToVideoInfo2(pmt: PAMMediaType): HRESULT;
var
  VIH: PVideoInfoHeader;
  VIH2: PVideoInfoHeader2;

  PaletteDataLength : cardinal;
begin
  if IsEqualGUID(pmt.formattype, FORMAT_VideoInfo) then
  begin
    VIH := PVideoInfoHeader(pmt.pbFormat);

    // make sure to allocate extra space after VideoInfoHeader to store color palette
    PaletteDataLength := pmt.cbFormat - SizeOf(VideoInfoHeader);
    VIH2 := PVideoInfoHeader2(CoTaskMemAlloc(SizeOf(VideoInfoHeader2) + PaletteDataLength));

    fillchar(VIH2^, SizeOf(VideoInfoHeader2) + PaletteDataLength, 0);

    if not assigned(VIH2) then
    begin
      Result := E_OUTOFMEMORY;
      exit;
    end;

    VIH2.rcSource := VIH.rcSource;
    VIH2.rcTarget := VIH.rcTarget;
    VIH2.dwBitRate := VIH.dwBitRate;
    VIH2.dwBitErrorRate := VIH.dwBitErrorRate;
    VIH2.AvgTimePerFrame := VIH.AvgTimePerFrame;

    VIH2.dwInterlaceFlags := AMINTERLACE_IsInterlaced + AMINTERLACE_DisplayModeBobOrWeave;
    VIH2.dwCopyProtectFlags := 0;

    CopyMemory(@VIH2.bmiHeader, @VIH.bmiHeader, SizeOf(BitmapInfoHeader));

    VIH2.dwPictAspectRatioX := DWORD(VIH2.bmiHeader.biWidth);
    VIH2.dwPictAspectRatioY := DWORD(VIH2.bmiHeader.biHeight);

    //VIH2.dw .ControlFlags := 0;
    //VIH2.dwReserved2 := 0;

    pmt.formattype := FORMAT_VideoInfo2;

    CoTaskMemFree(pmt.pbFormat);

    pmt.pbFormat := PBYTE(VIH2);
    pmt.cbFormat := sizeof(VIDEOINFOHEADER2) + PaletteDataLength;
  end
  else
  begin
    VIH2 := PVideoInfoHeader2(pmt.pbFormat);
    VIH2.dwInterlaceFlags := AMINTERLACE_IsInterlaced + AMINTERLACE_DisplayModeBobOrWeave;
  end;

  Result := S_OK;
end;

  procedure CopyMediaType(pmtTarget: PAMMEDIATYPE; pmtSource: PAMMEDIATYPE);
  begin
    //  We'll leak if we copy onto one that already exists - there's one
    //  case we can check like that - copying to itself.
    ASSERT(pmtSource <> pmtTarget);
    pmtTarget^ := pmtSource^;
    if (pmtSource.cbFormat <> 0) then
    begin
      ASSERT(pmtSource.pbFormat <> nil);
      pmtTarget.pbFormat := CoTaskMemAlloc(pmtSource.cbFormat);
      if (pmtTarget.pbFormat = nil) then
        pmtTarget.cbFormat := 0
      else
        CopyMemory(pmtTarget.pbFormat, pmtSource.pbFormat, pmtTarget.cbFormat);
    end;
    if (pmtTarget.pUnk <> nil) then  pmtTarget.pUnk._AddRef;
  end;

constructor TMCheater.Create(ObjName: String; Unk: IUnKnown; out hr: HRESULT);
begin
  inherited Create(ObjName, Unk, CLSID_Cheater);

  FCritSec := TBCCritSec.Create;
  FFirstFieldFirst := true;
  FClearSampleTimes := false;

  ASSERT(FOutput = nil, 'TMCheater');
end;

destructor TMCheater.Destroy;
begin
  FCritSec.Free;
  inherited;
end;

constructor TMCheater.CreateFromFactory(Factory: TBCClassFactory; const Controller: IUnknown);
var hr: HRESULT;
begin
  Create(Factory.Name, Controller, hr);
end;

function TMCheater.Transform(pIn, pOut: IMediaSample): HRESULT;
begin
  FCritSec.Lock;
  try
    result := Copy(pIn, pOut);
  finally
    FCritSec.UnLock;
  end;
end;

function TMCheater.Copy(Source, dest: IMediaSample): HRESULT;
var
  SourceBuffer, DestBuffer: PBYTE;
  TimeStart, TimeEnd: TReferenceTime;
  MediaStart, MediaEnd: int64;
  sp: TAMSAMPLE2PROPERTIES;
  pSVIH: PVideoInfoHeader;
  pDVIH2, pSVIH2: PVideoInfoHeader2;
  srcpitch, dstpitch, lines: Integer;
begin
  // Copy the sample data
  Source.GetPointer(SourceBuffer);
  Dest.GetPointer(DestBuffer);

  srcpitch := 0;
  lines := 0;
  if IsEqualGUID(FInput.CurrentMediaType.MediaType.formattype, FORMAT_VideoInfo) then
  begin
    pSVIH := PVideoInfoHeader(FInput.CurrentMediaType.MediaType.pbFormat);
    srcpitch := pSVIH.bmiHeader.biWidth * pSVIH.bmiHeader.biBitCount div 8;
    lines := pSVIH.bmiHeader.biHeight;
  end
  else if IsEqualGUID(FInput.CurrentMediaType.MediaType.formattype, FORMAT_VideoInfo2) then
  begin
    pSVIH2 := PVideoInfoHeader2(FInput.CurrentMediaType.MediaType.pbFormat);
    srcpitch := pSVIH2.bmiHeader.biWidth * pSVIH2.bmiHeader.biBitCount div 8;
    lines := pSVIH2.bmiHeader.biHeight;
  end;

  pDVIH2 := PVideoInfoHeader2(FOutput.CurrentMediaType.MediaType.pbFormat);
  dstpitch := FOutputWidth * pDVIH2.bmiHeader.biBitCount div 8;


  while lines > 0 do
  begin
    CopyMemory(DestBuffer, SourceBuffer, srcpitch);
    Inc(DestBuffer, dstpitch);
    Inc(SourceBuffer, srcpitch);
    Dec(lines);
  end;

  if FClearSampleTimes then
    Dest.SetTime(nil, nil)
  else
  begin
    // Copy the sample times
    if (NOERROR = Source.GetTime(TimeStart, TimeEnd)) then
      Dest.SetTime(@TimeStart, @TimeEnd);

    Self.StreamTime(FStreamTime);

    FSampleStartTime := TimeStart;
    if FDrop then
    begin
      Source := nil;
      Result := S_FALSE;
      exit;
    end;
  end;

  if (Source.GetMediaTime(MediaStart,MediaEnd) = NOERROR) then
    Dest.SetMediaTime(@MediaStart, @MediaEnd);

  if FFirstFieldFirst then
  begin
    sp.cbData := 8;
    sp.dwTypeSpecificFlags := AM_VIDEO_FLAG_FIELD1FIRST;
    (Dest as IMediaSample2).SetProperties(8, sp);
  end;

  result := NOERROR;
end;

function TMCheater.GetTimes(out Time1, Time2: int64): HResult;
begin
  Time1 := FStreamTime;
  Time2 := FSampleStartTime;
  Result := S_OK;
end;

function TMCheater.CheckInputType(mtIn: PAMMediaType): HRESULT;
begin
  //The Input.CheckMediaType member function is implemented to call the CheckInputType member function of the derived filter class
  if (IsEqualGUID(mtIn.subtype, MEDIASUBTYPE_YUY2)   //any packed yuv source
  or IsEqualGUID(mtIn.subtype, MEDIASUBTYPE_UYVY)
  or IsEqualGUID(mtIn.subtype, MEDIASUBTYPE_AYUV)
  or IsEqualGUID(mtIn.subtype, MEDIASUBTYPE_UYVY)
  or IsEqualGUID(mtIn.subtype, MEDIASUBTYPE_Y411)
  or IsEqualGUID(mtIn.subtype, MEDIASUBTYPE_Y41P)
  or IsEqualGUID(mtIn.subtype, MEDIASUBTYPE_Y211)
  or IsEqualGUID(mtIn.subtype, MEDIASUBTYPE_YUY2)
  or IsEqualGUID(mtIn.subtype, MEDIASUBTYPE_YVYU)
  or IsEqualGUID(mtIn.subtype, MEDIASUBTYPE_YUYV))
  and IsEqualGUID(mtIn.formattype, FORMAT_VideoInfo) then   //todo: update for packed yuv sources
    Result := S_OK
  else
    Result := VFW_E_TYPE_NOT_ACCEPTED;
end;

function TMCheater.CheckTransform(mtIn,
  mtOut: PAMMediaType): HRESULT;
var
  pDVIH: PVideoInfoHeader2;
begin
  //called when
  //    Output is already connected during connection of input
  //    Output.CheckMediaType is called

  pDVIH := PVideoInfoHeader2(mtOut.pbFormat);
  FOutputWidth := pDVIH.bmiHeader.biWidth;
  FOutputHeight := pDVIH.bmiHeader.biHeight;

  if IsEqualGUID(mtIn.subtype, mtOut.subtype) then
    Result := S_OK
  else
    Result := VFW_E_TYPE_NOT_ACCEPTED;
end;

function TMCheater.GetMediaType(Position: integer;
  out MediaType: PAMMediaType): HRESULT;
begin
  Result := S_OK;

  if not FInput.IsConnected then
    Result := E_UNEXPECTED;

  if Position < 0 then
    Result := E_INVALIDARG;

  if Position > 0 then
    Result :=  VFW_S_NO_MORE_ITEMS;

  if Position = 0 then
  begin
    CopyMediaType(MediaType, FInput.CurrentMediaType.MediaType);
    ConvertVideoInfoToVideoInfo2(MediaType);
  end;
end;

function TMCheater.DecideBufferSize(Alloc: IMemAllocator;
  Properties: PAllocatorProperties): HRESULT;
var
  Actual: TAllocatorProperties;
  pDVIH: PVideoInfoHeader2;
begin
 // Is the input pin connected
  if not FInput.IsConnected then
    begin
      result := E_UNEXPECTED;
      exit;
    end;

  pDVIH := PVideoInfoHeader2(FOutput.CurrentMediaType.MediaType.pbFormat);

  Properties.cBuffers := 1;
  Properties.cbBuffer := pDVIH.bmiHeader.biSizeImage;
  Properties.cbAlign := 1;
  Properties.cbPrefix := 0;

  // Ask the allocator to reserve us some sample memory, NOTE the function
  // can succeed (that is return NOERROR) but still not have allocated the
  // memory that we requested, so we must check we got whatever we wanted

  result := Alloc.SetProperties(Properties^, Actual);
  if FAILED(result) then exit;

  if (Properties.cBuffers > Actual.cBuffers)
  or (Properties.cbBuffer > Actual.cbBuffer) then
    result := E_FAIL
  else
    result := NOERROR;
end;


function TMCheater.ClearSampleTimes(Clear: boolean): HResult;
begin
  FClearSampleTimes := Clear;
  Result := S_OK;
end;

function TMCheater.FirstFieldFirst(First: boolean): HResult;
begin
  FFirstFieldFirst := First;
  Result := S_OK;
end;

function TMCheater.DropSample(Drop: boolean): HResult;
begin
  FDrop := Drop;
  Result := S_OK;
end;

initialization
  TBCClassFactory.CreateFilter(TMCheater, 'Cheater', CLSID_Cheater,
    CLSID_LegacyAmFilterCategory, MERIT_DO_NOT_USE, 2, @MyPins);
end.

