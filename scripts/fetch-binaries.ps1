# Retrieve arguments
Param(
    [string][Parameter(Mandatory=$true)][ValidateSet("x86", "x64")]$platform
)

$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Definition
$solutionPath = Join-Path $scriptPath "..\vvvv45\src"
$nugetExe = Join-Path $solutionPath  ".nuget\nuget.exe"

$tagsUri = "http://vvvv.org:8111/guestAuth/app/rest/builds/id:$buildId/tags/"
$buildsForTagUri = "http://vvvv.org:8111/guestAuth/app/rest/builds/?locator=branch:%28default:any%29,tags:test"

#$git = bash -c "git log -1 | grep URI | sed -e 's/.* //g'"
$commitId = git log | where {$_ -match "git-subtree-split"} | select -first 1 | %{$_ -split " "} | select -last 1
$version = $commitId.Substring(0,8) + "-$platform"

& $nugetExe pack $nuspecFile -o $packageOutputDir -version $version -p Version=$version

Write-Host $commitId