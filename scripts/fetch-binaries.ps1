# Retrieve arguments
Param(
    [string][Parameter(Mandatory=$true)][ValidateSet("x86", "x64")]$platform,
    [int]$maxCount = 100
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

$localCommits = git log --format=%H --max-count=$maxCount
$i = 0
$found = $false
foreach ($c in $localCommits)
{
    $buildsForTagUri = "$tcHost/guestAuth/app/rest/builds/?locator=branch:(name:$currentBranch,default:any),tags:$c"
    $response = Invoke-RestMethod -Uri $buildsForTagUri
    $build = $response.builds.ChildNodes | select -first 1
    Write-Progress -Activity "Looking for matching binaries (Branch: $currentBranch, Platform: $platform)" -Status "Commit $c" -PercentComplete ($i / $maxCount * 100)
    $i++
    if ($build)
    {
        $found = $true
        Write-Output "Using build $($build.webUrl)"
        $semVerUri = "$tcHost$($build.href)/resulting-properties/system.SemVer"
        $packageVersion = Invoke-RestMethod -Uri $semVerUri
        Write-Output "Package version is $packageVersion"
        & $nugetExe install $packageName -Version $packageVersion -OutputDirectory "$vvvvRootPath"
        break
    }
}

if ($found -eq $false)
{
    Write-Error "Couldn't find any matching binaries. Either increase the maxCount parameter or look manually at http://vvvv.org:8111"
}