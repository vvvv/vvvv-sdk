library WavePlayer;
{$IFOPT D-}{$WEAKLINKRTTI ON}{$ENDIF}
{$RTTI EXPLICIT METHODS([]) PROPERTIES([]) FIELDS([])}

uses
  BaseClass,
  Main in 'Main.pas';

{$E ax}

exports
  DllGetClassObject,
  DllCanUnloadNow,
  DllRegisterServer,
  DllUnregisterServer;

begin
end.
