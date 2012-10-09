# Microsoft Developer Studio Project File - Name="CalibFilter" - Package Owner=<4>
# Microsoft Developer Studio Generated Build File, Format Version 6.00
# ** DO NOT EDIT **

# TARGTYPE "Win32 (x86) Dynamic-Link Library" 0x0102

CFG=CalibFilter - Win32 Debug
!MESSAGE This is not a valid makefile. To build this project using NMAKE,
!MESSAGE use the Export Makefile command and run
!MESSAGE 
!MESSAGE NMAKE /f "CalibFilter.mak".
!MESSAGE 
!MESSAGE You can specify a configuration when running NMAKE
!MESSAGE by defining the macro CFG on the command line. For example:
!MESSAGE 
!MESSAGE NMAKE /f "CalibFilter.mak" CFG="CalibFilter - Win32 Debug"
!MESSAGE 
!MESSAGE Possible choices for configuration are:
!MESSAGE 
!MESSAGE "CalibFilter - Win32 Release" (based on "Win32 (x86) Dynamic-Link Library")
!MESSAGE "CalibFilter - Win32 Debug" (based on "Win32 (x86) Dynamic-Link Library")
!MESSAGE 

# Begin Project
# PROP AllowPerConfigDependencies 0
# PROP Scc_ProjName ""
# PROP Scc_LocalPath ""
CPP=xicl6.exe
MTL=midl.exe
RSC=rc.exe

!IF  "$(CFG)" == "CalibFilter - Win32 Release"

# PROP BASE Use_MFC 2
# PROP BASE Use_Debug_Libraries 0
# PROP BASE Output_Dir "Release"
# PROP BASE Intermediate_Dir "Release"
# PROP BASE Target_Dir ""
# PROP Use_MFC 0
# PROP Use_Debug_Libraries 0
# PROP Output_Dir "..\..\_temp\CalibFilter_Rls"
# PROP Intermediate_Dir "..\..\_temp\CalibFilter_Rls"
# PROP Ignore_Export_Lib 1
# PROP Target_Dir ""
LIB32=link.exe -lib
# ADD BASE CPP /nologo /MD /W3 /GX /O2 /D "WIN32" /D "NDEBUG" /D "_WINDOWS" /D "_WINDLL" /D "_AFXDLL" /YX /FD /c
# ADD CPP /nologo /MD /W4 /GX /Zi /O2 /I "..\..\cxcore\include" /I "..\..\cv\include" /D "WIN32" /D "NDEBUG" /D "_WINDOWS" /D "_MBCS" /YX /FD /c
# ADD BASE MTL /nologo /D "NDEBUG" /mktyplib203 /win32
# ADD MTL /nologo /D "NDEBUG" /mktyplib203 /win32
# ADD BASE RSC /l 0x419 /d "NDEBUG" /d "_AFXDLL"
# ADD RSC /l 0x419 /i "." /d "NDEBUG"
BSC32=bscmake.exe
# ADD BASE BSC32 /nologo
# ADD BSC32 /nologo
LINK32=xilink6.exe
# ADD BASE LINK32 /nologo /subsystem:windows /dll /machine:I386
# ADD LINK32 cxcore.lib cv.lib quartz.lib STRMBASE.LIB comctl32.lib winmm.lib kernel32.lib user32.lib gdi32.lib winspool.lib comdlg32.lib advapi32.lib shell32.lib ole32.lib oleaut32.lib uuid.lib odbc32.lib odbccp32.lib opengl32.lib glu32.lib /nologo /entry:"DllEntryPoint@12" /subsystem:windows /dll /machine:I386 /nodefaultlib:"libmmd.lib" /out:"..\..\Bin\CalibFilter.ax" /libpath:"..\..\lib"

!ELSEIF  "$(CFG)" == "CalibFilter - Win32 Debug"

# PROP BASE Use_MFC 2
# PROP BASE Use_Debug_Libraries 1
# PROP BASE Output_Dir "Debug"
# PROP BASE Intermediate_Dir "Debug"
# PROP BASE Target_Dir ""
# PROP Use_MFC 0
# PROP Use_Debug_Libraries 1
# PROP Output_Dir "..\..\_temp\CalibFilter_Dbg"
# PROP Intermediate_Dir "..\..\_temp\CalibFilter_Dbg"
# PROP Ignore_Export_Lib 1
# PROP Target_Dir ""
LIB32=link.exe -lib
# ADD BASE CPP /nologo /MDd /W3 /Gm /GX /ZI /Od /D "WIN32" /D "_DEBUG" /D "_WINDOWS" /D "_WINDLL" /D "_AFXDLL" /YX /FD /GZ /c
# ADD CPP /nologo /MDd /W4 /Gm /GX /Zi /Od /I "..\..\cxcore\include" /I "..\..\cv\include" /D "WIN32" /D "_DEBUG" /D "DEBUG" /D "_WINDOWS" /D "_MBCS" /FR /YX /FD /GZ /c
# ADD BASE MTL /nologo /D "_DEBUG" /mktyplib203 /win32
# ADD MTL /nologo /D "_DEBUG" /mktyplib203 /win32
# ADD BASE RSC /l 0x419 /d "_DEBUG" /d "_AFXDLL"
# ADD RSC /l 0x419 /i "." /d "_DEBUG"
BSC32=bscmake.exe
# ADD BASE BSC32 /nologo
# ADD BSC32 /nologo
LINK32=xilink6.exe
# ADD BASE LINK32 /nologo /subsystem:windows /dll /debug /machine:I386 /pdbtype:sept
# ADD LINK32 cxcored.lib cvd.lib quartz.lib STRMBASD.LIB comctl32.lib winmm.lib kernel32.lib user32.lib gdi32.lib winspool.lib comdlg32.lib advapi32.lib shell32.lib ole32.lib oleaut32.lib uuid.lib odbc32.lib odbccp32.lib opengl32.lib glu32.lib /nologo /entry:"DllEntryPoint@12" /subsystem:windows /dll /debug /machine:I386 /nodefaultlib:"libmmdd.lib" /out:"..\..\Bin\CalibFilterd.ax" /pdbtype:sept /libpath:"..\..\lib"

!ENDIF 

# Begin Target

# Name "CalibFilter - Win32 Release"
# Name "CalibFilter - Win32 Debug"
# Begin Group "Src"

# PROP Default_Filter "cpp;c;cxx;rc;def;r;odl;idl;hpj;bat"
# Begin Source File

SOURCE=.\Calib3DWindow.cpp
# End Source File
# Begin Source File

SOURCE=.\CalibFilter.cpp
# End Source File
# Begin Source File

SOURCE=.\CalibFilter.def

!IF  "$(CFG)" == "CalibFilter - Win32 Release"

!ELSEIF  "$(CFG)" == "CalibFilter - Win32 Debug"

# PROP Exclude_From_Build 1

!ENDIF 

# End Source File
# Begin Source File

SOURCE=.\CalibFilterprop.cpp
# End Source File
# Begin Source File

SOURCE=.\CalibFilterprop.rc
# End Source File
# End Group
# Begin Group "Include"

# PROP Default_Filter "h;hpp;hxx;hm;inl"
# Begin Source File

SOURCE=.\Calib3DWindow.h
# End Source File
# Begin Source File

SOURCE=.\CalibFilter.h
# End Source File
# Begin Source File

SOURCE=.\CalibFilterprop.h
# End Source File
# Begin Source File

SOURCE=.\CalibFilteruids.h
# End Source File
# Begin Source File

SOURCE=.\iCalibFilter.h
# End Source File
# Begin Source File

SOURCE=.\resource.h
# End Source File
# End Group
# Begin Group "Resource"

# PROP Default_Filter "ico;cur;bmp;dlg;rc2;rct;bin;rgs;gif;jpg;jpeg;jpe"
# End Group
# End Target
# End Project
