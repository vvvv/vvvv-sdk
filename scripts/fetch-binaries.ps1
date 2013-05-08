# Retrieve arguments
Param(
    [string][Parameter(Mandatory=$true)][ValidateSet("x86", "x64")]$platform
)

$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Definition
$vvvvRootPath = Join-Path $scriptPath "..\vvvv45"
$solutionPath = Join-Path $scriptPath "..\vvvv45\src"
$nugetExe = Join-Path $solutionPath  ".nuget\nuget.exe"
$packageName = "VVVV.Binaries.$platform"
$packageVersion = ""

$tcHost = "http://vvvv.org:8111"
$tagsUri = "$tcHost/guestAuth/app/rest/builds/id:$buildId/tags/"
$currentBranch = git rev-parse --abbrev-ref HEAD

Write-Host "Looking for last split commit in branch $currentBranch ..."

$localCommits = git log --format=%H
foreach ($c in $localCommits)
{
    $buildsForTagUri = "$tcHost/guestAuth/app/rest/builds/?locator=branch:(name:$currentBranch,default:any),tags:$c"
    $response = Invoke-RestMethod -Uri $buildsForTagUri
    $build = $response.builds.ChildNodes | select -first 1
    if ($build)
    {
        Write-Host "Using build $($build.webUrl)"
        $semVerUri = "$tcHost$($build.href)/resulting-properties/system.SemVer"
        $packageVersion = Invoke-RestMethod -Uri $semVerUri
        Write-Host "Package version is $packageVersion"
        & $nugetExe install $packageName -Version $packageVersion -OutputDirectory "$vvvvRootPath"
        break
    }
}