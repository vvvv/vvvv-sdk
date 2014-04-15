# Retrieve arguments
Param(
    [string][ValidateSet("x86", "x64")]$platform = "x86",
    [int]$maxCount = 100,
    [string]$branch = "refs/heads/develop"
)

$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Definition
$packagesPath = Join-Path $scriptPath "..\packages"
$vvvvRootPath = Join-Path $scriptPath "..\vvvv45"
$solutionPath = Join-Path $scriptPath "..\vvvv45\src"
$nugetExe = Join-Path $solutionPath  ".nuget\nuget.exe"
$packageName = "VVVV.Binaries.$platform"
$packageVersion = ""

$tcHost = "http://vvvv.org:8111"
$tagsUri = "$tcHost/guestAuth/app/rest/builds/id:$buildId/tags/"

$localCommits = git log --format=%H --max-count=$maxCount
$i = 0
$found = $false
foreach ($c in $localCommits)
{
    Write-Progress -Activity "Looking for matching binaries (Branch: $branch, Platform: $platform)" -Status "Commit $c" -PercentComplete ($i++ / $maxCount * 100)
    
    $buildsForTagUri = "$tcHost/guestAuth/app/rest/builds/?locator=branch:(name:$branch,default:any),tags:$c"
    $response = Invoke-RestMethod -Uri $buildsForTagUri
    $build = $response.builds.ChildNodes | select -first 1
    if ($build)
    {
        $found = $true
        Write-Progress -Activity "Found matching binaries ($($build.webUrl))" -Status "Commit $c" -PercentComplete 100
        
        $response = Invoke-RestMethod -Uri "$tcHost$($build.href)"
        $response = Invoke-RestMethod -Uri "$tcHost$($response.build.artifacts.href)"
        $file = $response.ChildNodes.file | select -first 1
        # $file.name like VVVV.NAME.31.3.2-develop-42.nupkg
        $p = "VVVV\.[^\.]*\.([0-9]+)\.([0-9]+)\.([0-9]+)([^\.]*)\.nupkg"
        $major, $minor, $patch, $preRelease = ([regex]$p).Match($file.name).Groups | select -skip 1 | %{$_.Value}
        $packageVersion = "$major.$minor.$patch$preRelease"
        & $nugetExe install $packageName -Version $packageVersion -OutputDirectory "$packagesPath"
        $contentFolder = Join-Path $packagesPath (Join-Path "VVVV.Binaries.$platform.$packageVersion" "content")
        Copy-Item (Join-Path $contentFolder "\*") -Destination $vvvvRootPath -Recurse -Force
        break
    }
}

if ($found -eq $false)
{
    Write-Error "Couldn't find any matching binaries. Either increase the maxCount parameter or look manually at http://vvvv.org:8111"
}