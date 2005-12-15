// *****************************************************************************
// Author: komarov andrey.
// Email: komar@elecard.net.ru
// This filter is a useful debugging tool. For example, you can verify,
// bit by bit, the results of a transform filter. You can build a graph
// manually by using GraphEdit, and connect the Dump filter to the output
// of a transform filter or any other output pin.
// *****************************************************************************

library DumpSample;

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
