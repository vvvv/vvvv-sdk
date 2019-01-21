SET version=38.2.0-prerelease

msbuild vvvv45\src\core\Hosting\Hosting.csproj /p:Configuration=NuGet /v:m
msbuild vvvv45\src\core\GenericNodes\GenericNodes.csproj /p:Configuration=NuGet /v:m

nuget pack common\src\core\Core\Core.nuspec -properties Configuration=NuGet;Version=%version%
nuget pack common\src\core\Utils\Utils.nuspec -properties Configuration=NuGet;Version=%version%
nuget pack common\src\core\UtilsIL\UtilsIL.nuspec -properties Configuration=NuGet;Version=%version%
nuget pack vvvv45\src\core\PluginInterfaces\PluginInterfaces.nuspec -properties Configuration=NuGet;Version=%version%
nuget pack vvvv45\src\core\Hosting\Hosting.nuspec -properties Configuration=NuGet;Version=%version%
nuget pack vvvv45\src\core\GenericNodes\GenericNodes.nuspec -properties Configuration=NuGet;Version=%version%
nuget pack vvvv45\src\core\Utils3rdParty\Utils3rdParty.nuspec -properties Configuration=NuGet;Version=%version%