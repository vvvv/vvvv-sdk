//////project name
//CheaterIP

//////description
//directshow tranform-inplace-filter filter.
//lets you set the firstfield property of mediasamples 
//of type VideoInfoHeader2 to the desired value so that 
//VMR9 deinterlaces correctly. useful e.g. in connection with //http://btwincap.sourceforge.net driver that outputs 
//VIH2 samples but doesn't let you set the correct field order.

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
  BaseClass, ActiveX, DirectShow9, Windows;

const
  CLSID_CheaterInPlace        : TGUID = '{D157DCEB-404D-40C9-8FBE-F48C1F2BDB02}';
  IID_CheaterParameters       : TGUID = '{75915F42-231C-4247-9FB4-D0314407BC68}';

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

  TMCheaterInPlace = class(TBCTransInPlaceFilter, ICheaterParameters, IPersist)
  private
    FClearSampleTimes: Boolean;
    FDrop: Boolean;
    FFirstFieldFirst: Boolean;
    FSampleStartTime: TReferenceTime;
    FStreamTime: TReferenceTime;
  public
    function CheckInputType(mtIn: PAMMediaType): HRESULT; override;
    constructor Create(ObjName: string; unk: IUnKnown; out hr: HRESULT);
    constructor CreateFromFactory(Factory: TBCClassFactory; const Controller: IUnknown); override;
    destructor Destroy; override;

    // Overrides the PURE virtual Transform of CTransInPlaceFilter base class
    // This is where the "real work" is done.
    function Transform(Sample: IMediaSample): HRESULT; override;

    //ICheaterParameters
    function ClearSampleTimes(Clear: boolean): HResult; stdcall;
    function DropSample(Drop: boolean): HResult; stdcall;
    function FirstFieldFirst(First: boolean): HResult; stdcall;
    function GetTimes(out Time1, Time2: int64): HResult; stdcall;
  end;

implementation

function TMCheaterInPlace.CheckInputType(mtIn: PAMMediaType): HRESULT;
begin
  if IsEqualGUID(mtIn.formattype, FORMAT_VideoInfo2) then
    Result := S_OK
  else
    Result := E_FAIL;
end;

constructor TMCheaterInPlace.Create(ObjName: string; unk: IUnKnown;
  out hr: HRESULT);
begin
  inherited Create(ObjName, unk, CLSID_CheaterInPlace, hr);
end;

constructor TMCheaterInPlace.CreateFromFactory(Factory: TBCClassFactory;
  const Controller: IUnKnown);
var hr: HRESULT;
begin
  Create(Factory.Name, Controller, hr);
end;

destructor TMCheaterInPlace.Destroy;
begin
  inherited;
end;

function TMCheaterInPlace.Transform(Sample: IMediaSample): HRESULT;
var
  sp: TAMSAMPLE2PROPERTIES;
  TimeStart, TimeEnd: TReferenceTime;
begin
  if FClearSampleTimes then
    Sample.SetTime(nil, nil);

  Self.StreamTime(FStreamTime);

  if (NOERROR = Sample.GetTime(TimeStart, TimeEnd)) then
    FSampleStartTime := TimeStart;

  if FDrop then
  begin
    Sample := nil;
    Result := S_FALSE;
    exit;
  end;

  if FFirstFieldFirst then
  begin
    sp.cbData := 8;
    sp.dwTypeSpecificFlags := AM_VIDEO_FLAG_FIELD1FIRST;
    (Sample as IMediaSample2).SetProperties(8, sp);
  end;

  result := S_OK;
end;

function TMCheaterInPlace.ClearSampleTimes(Clear: boolean): HResult;
begin
  FClearSampleTimes := Clear;
  Result := S_OK;
end;

function TMCheaterInPlace.DropSample(Drop: boolean): HResult;
begin
  FDrop := Drop;
  Result := S_OK;
end;

function TMCheaterInPlace.FirstFieldFirst(First: boolean): HResult;
begin
  FFirstFieldFirst := First;
  Result := S_OK;
end;

function TMCheaterInPlace.GetTimes(out Time1, Time2: int64): HResult;
begin
  Time1 := FStreamTime;
  Time2 := FSampleStartTime;
  Result := S_OK;
end;


initialization
  TBCClassFactory.CreateFilter(TMCheaterInPlace, 'CheaterInPlace', CLSID_CheaterInPlace,
    CLSID_LegacyAmFilterCategory, MERIT_DO_NOT_USE, 2, @MyPins);
end.
