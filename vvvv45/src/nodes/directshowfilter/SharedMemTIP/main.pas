//////project name
//SharedMemTIP

//////description
//directshow tranform-inplace-filter filter.
//writes mediasamples bytes to a shared memory file.

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
//joreg -> joreg@vvvv.org

//////edited by
//your name here

unit Main;

interface
uses
  BaseClass, ActiveX, DirectShow9, Windows;

const
  CLSID_SharedMemTIP        : TGUID = '{C8724592-854D-49B3-A610-69951BAC7754}';
  IID_SharedMemParameters: TGUID = '{34A9C25E-9427-4963-BB3D-294C4A5947C3}';

  MyPinTypes: TRegPinTypes =
    (clsMajorType: @MEDIATYPE_Video;
     clsMinorType: @MEDIASUBTYPE_NULL);

  MyPins : array[0..1] of TRegFilterPins =
    ((strName: 'Input'; bRendered: FALSE; bOutput: FALSE; bZero: FALSE; bMany: FALSE; oFilter: nil; strConnectsToPin: nil; nMediaTypes: 1; lpMediaType: @MyPinTypes),
     (strName: 'Output'; bRendered: FALSE; bOutput: TRUE; bZero: FALSE; bMany: FALSE; oFilter: nil; strConnectsToPin: nil; nMediaTypes: 1; lpMediaType: @MyPinTypes));

type
  ISharedMemParameters = interface
  ['{34A9C25E-9427-4963-BB3D-294C4A5947C3}']
    function SetShareName(ShareName: PChar): HRESULT; stdcall;
  end;

  TMSharedMemTIP = class(TBCTransInPlaceFilter, ISharedMemParameters, IPersist)
  private
    FActualDataLength: DWord;
    FDataPointer: Pointer;
    FMapHandle: THandle;
    FFilename: String;
    FShareName: String;
  public
    function CheckInputType(mtIn: PAMMediaType): HRESULT; override;
    constructor Create(ObjName: string; unk: IUnKnown; out hr: HRESULT);
    constructor CreateFromFactory(Factory: TBCClassFactory; const Controller: IUnknown); override;
    destructor Destroy; override;

    // Overrides the PURE virtual Transform of CTransInPlaceFilter base class
    // This is where the "real work" is done.
    function Transform(Sample: IMediaSample): HRESULT; override;

    //ISharedMemParameters implementation
    function SetShareName(ShareName: PChar): HRESULT; stdcall;
  end;

implementation

uses SharedMemoryUtils;

constructor TMSharedMemTIP.Create(ObjName: string; unk: IUnKnown;
  out hr: HRESULT);
begin
  inherited Create(ObjName, unk, CLSID_SharedMemTIP, hr);
  FFilename := '#vvvv';
  FShareName := '';
end;

constructor TMSharedMemTIP.CreateFromFactory(Factory: TBCClassFactory;
  const Controller: IUnKnown);
var hr: HRESULT;
begin
  Create(Factory.Name, Controller, hr);
end;

destructor TMSharedMemTIP.Destroy;
begin
  inherited;
end;

function TMSharedMemTIP.SetSharename(Sharename: PChar): HRESULT;
begin
  FFilename := Sharename;
  Result := S_OK;
end;

function TMSharedMemTIP.CheckInputType(mtIn: PAMMediaType): HRESULT;
begin
  if IsEqualGUID(mtIn.majortype, MEDIATYPE_Video)
  and (IsEqualGUID(mtIn.subtype, MEDIASUBTYPE_RGB8)
    or IsEqualGUID(mtIn.subtype, MEDIASUBTYPE_RGB555)
    or IsEqualGUID(mtIn.subtype, MEDIASUBTYPE_RGB565)
    or IsEqualGUID(mtIn.subtype, MEDIASUBTYPE_RGB24)
    or IsEqualGUID(mtIn.subtype, MEDIASUBTYPE_RGB8)) then
    Result := S_OK
  else
    Result := E_FAIL;
end;

function TMSharedMemTIP.Transform(Sample: IMediaSample): HRESULT;
var
  pbData: PBYTE;
begin
  if (FActualDataLength <> Sample.GetActualDataLength)
  or (FFilename <> FShareName) then
  begin
    FShareName := FFilename;
    FActualDataLength := Sample.GetActualDataLength;
    CloseMap(FMapHandle, FDataPointer);

    OpenMap(FMapHandle, FDataPointer, FShareName, FActualDataLength);
  end;

  //write to shared memory
  try
    LockMap(FShareName);
    Sample.GetPointer(pbData);
    Move(pbData^, FDataPointer^, FActualDataLength);
  finally
    UnlockMap;
  end;

  result := S_OK;
end;


initialization
  TBCClassFactory.CreateFilter(TMSharedMemTIP, 'SharedMemTIP', CLSID_SharedMemTIP,
    CLSID_LegacyAmFilterCategory, MERIT_DO_NOT_USE, 2, @MyPins);
end.
