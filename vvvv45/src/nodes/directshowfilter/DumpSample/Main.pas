//////project name
//DumpSample

//////description
//directshow renderer filter.
//hands you over the mediasamples bytes as string.

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
  CLSID_DumpSample: TGUID = '{A6F6F26A-6E48-4B14-99EA-60B3B2F8400A}';
  IID_IDumpString: TGUID = '{028ECF40-14E0-4DEA-A8CC-0E4E7DAFA81C}';

  MyPinType: TRegPinTypes =
    (clsMajorType: @MEDIATYPE_NULL;
     clsMinorType: @MEDIASUBTYPE_NULL);

  MyPins : array[0..0] of TRegFilterPins =
    ((strName: 'Input'; bRendered: FALSE; bOutput: FALSE; bZero: FALSE; bMany: FALSE; oFilter: nil; strConnectsToPin: nil; nMediaTypes: 1; lpMediaType: @MyPinType));

Type
  IDumpString = interface
  ['{028ECF40-14E0-4DEA-A8CC-0E4E7DAFA81C}']
    function GetDumpString(out DumpPointer: PChar): HRESULT; stdcall;
    function GetActualDataLength(out DataLength: DWord): HRESULT; stdcall;
  end;

  TMPin = class(TBCRenderedInputPin)
  private
    FActualDataLength: DWord;
    FDumpString: PChar;
  public
    mF:string;
    constructor Create(ObjectName: string; pUnk: IUnKnown; Filter: TBCBaseFilter;
      Lock: TBCCritSec; out hr: HRESULT;  Name: WideString);
    function CheckMediaType(mt: PAMMediaType): HRESULT; override;
    function Receive(pSample: IMediaSample): HRESULT; override; stdcall;
  end;

  TMDump = class(TBCBaseFilter, IDumpString)
  private
    xxx: integer;
    FPin: TMPin;
  protected
  public
    function GetPin(n: Integer): TBCBasePin; override;
    constructor Create(Name: string; Unk : IUnKnown; Lock: TBCCritSec; const clsid: TGUID);
    function GetPinCount: integer; override;

    //IDumpString implementation
    function GetDumpString(out DumpPointer: PChar): HRESULT; stdcall;
    function GetActualDataLength(out DataLength: DWord): HRESULT; stdcall;
  end;

implementation

constructor TMPin.Create(ObjectName: string;pUnk: IUnKnown; Filter: TBCBaseFilter;
      Lock: TBCCritSec; out hr: HRESULT; Name: WideString);
begin
  inherited Create(ObjectName, Filter, Lock, hr, Name);
end;

function TMPin.Receive(pSample: IMediaSample): HRESULT;
var
  pbData: PBYTE;
begin
  pSample.GetPointer(pbData);
  FDumpString := PChar(pbData);
  FActualDataLength := pSample.GetActualDataLength;
  result := S_OK;
end;

function TMPin.CheckMediaType(mt: PAMMediaType): HRESULT;
begin
  result := S_OK;
end;

function TMDump.GetPinCount: integer;
begin
  result := 1;
end;

constructor TMDump.Create(Name: string; Unk : IUnKnown; Lock: TBCCritSec; const clsid: TGUID);
begin
  Lock := TBCCritSec.Create;
  inherited create(Name, Unk, Lock, CLSID_DumpSample);
end;

function TMDump.GetPin(n: Integer): TBCBasePin;
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

function TMDump.GetActualDataLength(out DataLength: DWord): HRESULT;
begin
  DataLength := FPin.FActualDataLength;
  Result := S_OK;
end;

function TMDump.GetDumpString(out DumpPointer: PChar): HRESULT;
begin
  DumpPointer := PChar(FPin.FDumpString);
  Result := S_OK;
end;


initialization
  TBCClassFactory.CreateFilter(TMDump, 'DumpSample', CLSID_DumpSample,
    CLSID_LegacyAmFilterCategory, MERIT_DO_NOT_USE, 1, @MyPins);
end.
