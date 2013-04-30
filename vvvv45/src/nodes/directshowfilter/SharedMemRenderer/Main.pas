//////project name
//SharedMemRenderer

//////description
//directshow renderer filter.
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
  BaseClass, DirectShow9, ActiveX, Windows;

const
  CLSID_SharedMemRenderer: TGUID = '{0453D075-0992-4D17-8E08-9C9B72E29D47}';
  IID_ISharedMemParameters: TGUID = '{34A9C25E-9427-4963-BB3D-294C4A5947C3}';

  MyPinType: TRegPinTypes =
    (clsMajorType: @MEDIATYPE_NULL;
     clsMinorType: @MEDIASUBTYPE_NULL);

  MyPins : array[0..0] of TRegFilterPins =
    ((strName: 'Input'; bRendered: FALSE; bOutput: FALSE; bZero: FALSE; bMany: FALSE; oFilter: nil; strConnectsToPin: nil; nMediaTypes: 1; lpMediaType: @MyPinType));

Type
  ISharedMemParameters = interface
  ['{34A9C25E-9427-4963-BB3D-294C4A5947C3}']
    function SetShareName(ShareName: PChar): HRESULT; stdcall;
  end;

  TMPin = class(TBCRenderedInputPin)
  private
    FActualDataLength: DWord;
    FDataPointer: Pointer;
    FMapHandle: THandle;
    FFilename: String;
    FShareName: string;
  public
    constructor Create(ObjectName: string; pUnk: IUnKnown; Filter: TBCBaseFilter;
      Lock: TBCCritSec; out hr: HRESULT;  Name: WideString);
    destructor Destroy;
    function CheckMediaType(mt: PAMMediaType): HRESULT; override;
    function Receive(pSample: IMediaSample): HRESULT; override; stdcall;
  end;

  TMSharedMemRenderer = class(TBCBaseFilter, ISharedMemParameters, IPersist)
  private
    xxx: integer;
    FPin: TMPin;
  protected
  public
    function GetPin(n: Integer): TBCBasePin; override;
    constructor Create(Name: string; Unk : IUnKnown; Lock: TBCCritSec; const clsid: TGUID);

    function GetPinCount: integer; override;

    //ISharedMemParameters implementation
    function SetShareName(ShareName: PChar): HRESULT; stdcall;
  end;

implementation

uses
  SysUtils, DXSUtil,
  SharedMemoryUtils;

constructor TMPin.Create(ObjectName: string;pUnk: IUnKnown; Filter: TBCBaseFilter;
      Lock: TBCCritSec; out hr: HRESULT; Name: WideString);
begin
  inherited Create(ObjectName, Filter, Lock, hr, Name);
  FFilename := '#vvvv';
  FShareName := '';
end;

destructor TMPin.Destroy;
begin
  CloseMap(FMapHandle, FDataPointer);
  inherited;
end;

function TMPin.Receive(pSample: IMediaSample): HRESULT;
var
  pbData: PBYTE;
begin
  if (FActualDataLength <> pSample.GetActualDataLength)
  or (FFilename <> FShareName) then
  begin
    FShareName := FFilename;
    FActualDataLength := pSample.GetActualDataLength;
    CloseMap(FMapHandle, FDataPointer);

    OpenMap(FMapHandle, FDataPointer, FShareName, FActualDataLength);
  end;

  //write to shared memory
  try
    LockMap(FShareName);
    pSample.GetPointer(pbData);
    try
      Move(pbData^, FDataPointer^, FActualDataLength);
    except on EAccessViolation do
      ///just not quit
    end;
  finally
    UnLockMap;
  end;

  result := S_OK;
end;

function TMPin.CheckMediaType(mt: PAMMediaType): HRESULT;
begin
  result := S_OK;
end;

function TMSharedMemRenderer.GetPinCount: integer;
begin
  result := 1;
end;

constructor TMSharedMemRenderer.Create(Name: string; Unk : IUnKnown; Lock: TBCCritSec; const clsid: TGUID);
begin
  inherited create(Name, Unk, Lock, CLSID_SharedMemRenderer);
end;

function TMSharedMemRenderer.GetPin(n: Integer): TBCBasePin;
var
  hr: HRESULT;
begin
  if (xxx = 0) then
  begin
    xxx := 1;
    FPin := TMPin.Create('Input pin', GetOwner, self, TBCCritSec.Create, hr, 'Input');
  end;
  result := FPin;
end;

function TMSharedMemRenderer.SetSharename(Sharename: PChar): HRESULT;
begin
  if Assigned(FPin) then
    FPin.FFilename := Sharename;
  Result := S_OK;
end;





initialization
  TBCClassFactory.CreateFilter(TMSharedMemRenderer, 'SharedMemRenderer', CLSID_SharedMemRenderer,
    CLSID_LegacyAmFilterCategory, MERIT_DO_NOT_USE, 1, @MyPins);
end.
