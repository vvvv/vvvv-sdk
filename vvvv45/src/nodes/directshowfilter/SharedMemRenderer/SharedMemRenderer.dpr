library SharedMemRenderer;

uses
  BaseClass,
  Main in 'Main.pas',
  SharedMemoryUtils in 'SharedMemoryUtils.pas';

{$E ax}

exports
  DllGetClassObject,
  DllCanUnloadNow,
  DllRegisterServer,
  DllUnregisterServer;
begin

end.
