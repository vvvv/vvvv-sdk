REM Usage: post-build-plugin.bat $(TargetPath) $(SolutionDir) $(ProjectDir)

set TargetDir=%~dp1
set TargetName=%~n1
set TargetPath=%TargetDir%%TargetName%
set OutputDir=%2..\lib\nodes\plugins
set DependenciesDir=%3Dependencies

copy /y %TargetPath%.* %OutputDir%
IF EXIST %DependenciesDir% copy /y %DependenciesDir%\* %OutputDir%
