In order to be able to run these tests you need to edit the following file of your SD installation:

C:\Program Files (x86)\SharpDevelop\4.1\bin\Tools\NUnit\nunit-console-x86.exe.config

<startup useLegacyV2RuntimeActivationPolicy="true">

This needs to be done because SlimDX is a mixed-mode assembly targeting the v2.0 of the CLR.
Once SlimDX releases a version targeting v4.0 this step is not necessary anymore.