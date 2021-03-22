# That is the location where this script run results will be placed:
$releasesRoot = "../Rollbar.NET-Releases"

$ErrorActionPreference="Stop"
$ProgressPreference="SilentlyContinue"

# $LocalDotnet is the path to the locally-installed SDK to ensure the
#   correct version of the tools are executed.
$LocalDotnet=""
# $InstallDir and $CliVersion variables can come from options to the
#   script.
$InstallDir = "./cli-tools"
$CliVersion = "latest" # = "1.0.1"

# Test the path provided by $InstallDir to confirm it exists. If it
#   does, it's removed. This is not strictly required, but it's a
#   good way to reset the environment.
if (Test-Path $InstallDir)
{
    Remove-Item -Recurse $InstallDir
}
New-Item -Type "directory" -Path $InstallDir

Write-Host "Downloading the CLI installer..."

# Use the Invoke-WebRequest PowerShell cmdlet to obtain the
#   installation script and save it into the installation directory.
Invoke-WebRequest `
    -Uri "https://dot.net/v1/dotnet-install.ps1" `
    -OutFile "$InstallDir/dotnet-install.ps1"

Write-Host "Installing the CLI requested version ($CliVersion) ..."

# Install the SDK of the version specified in $CliVersion into the
#   specified location ($InstallDir).
& $InstallDir/dotnet-install.ps1 -Version $CliVersion `
    -InstallDir $InstallDir

Write-Host "Downloading and installation of the SDK is complete."

# $LocalDotnet holds the path to dotnet.exe for future use by the
#   script.
$LocalDotnet = "$InstallDir/dotnet"

# Run the build process now. Implement your build script here:
#=============================================================

Write-Host "Harvesting all the SDK module projects..."
$sdkProjects = 
    Get-ChildItem -Path "." -Directory | 
    Where-Object {$_.Name -match '^Rollbar*'} |
    Where-Object {!($_.Name -match '^Rollbar.Benchmarker')} # we are not interested it this one...
Write-Host "  " $sdkProjects

Write-Host "Deleting /obj and /bin folders of the SDK projects/modules..."
foreach($project in $sdkProjects) {
    Write-Host "  SDK module:" $project.FullName
    $path = $project.FullName + '/obj'
    if (Test-Path $path) {
    Remove-Item -Path $path -Recurse #-WhatIf
    }
    $path = $project.FullName + '/bin'
    if (Test-Path $path) {
        Remove-Item -Path $path -Recurse #-WhatIf
    }
}

Write-Host "Restoring the SDK dependencies..."
#!!! dotnet restore rollbar.sln

Write-Host "Building all the SDK build configurations..."
$buildConfigurations = @(
    "Instrumented",
    "Debug",
    "Release"
)
foreach($buildConfiguration in $buildConfigurations) {
    Write-Host "  Building the SDK build configuration:" $buildConfiguration "..."
    # clean the configuration:
    <#
    Write-Host "    - cleaning..."
    dotnet clean rollbar.sln --configuration $buildConfiguration
    #>
    # build the configurations:
    Write-Host "    - building..."
    dotnet build rollbar.sln --configuration $buildConfiguration
    # unit-test the Release configuration: 
    #Write-Host "    - unit-testing..."
    #dotnet test rollbar.sln --configuration $buildConfiguration
}
# unit-test the Release configuration: 
#!!! Write-Host "    - unit-testing Debug build..."
#!!! dotnet test rollbar.sln --configuration Debug

Write-Host "Consolidating SDK build results..."
# define location for releases:
$releasesPath = $releasesRoot
if(!(Test-Path $releasesPath)) {
    New-Item -Path $releasesPath -ItemType Directory #-WhatIf
}
Write-Host "Releases folder: "$releasesPath

# define location for old releases archive:
$releasesArchivePath = Join-Path -Path $releasesPath -ChildPath "ARCHIVE" 
if(!(Test-Path $releasesArchivePath)) {
    New-Item -Path $releasesArchivePath -ItemType Directory #-WhatIf
}
Write-Host "Releases Archive folder: "$releasesArchivePath
# archive last release:
$releasesToArchive = @(
    Get-ChildItem -Path $releasesPath -Directory |
    Where-Object {$_.Name -match '^v*'} |
    Where-Object {!($_.Name -match '^ARCHIVE')} # we are not interested it this one...    
)
Write-Host "Archiving: "$releasesToArchive
foreach($release in $releasesToArchive) {
    Write-Host "  Moving "$release.FullName"..."
    Move-Item -Path ($release.FullName) -Destination $releasesArchivePath -Force #-WhatIf
}

# collect current release:
Add-Type -assembly "system.io.compression.filesystem"

foreach($project in $sdkProjects) {
    # project source path:
    $projectBin = $project.FullName + '/bin'

    # release bin name based on the NuGet package name prefix:
    $nugetPackage = (Get-ChildItem -Path $projectBin/Release -Filter *.nupkg | Select-Object -First 1).FullName
    $nugetPackage = [System.IO.Path]::GetFileNameWithoutExtension($nugetPackage)
    Write-Host "Package:" $nugetPackage
    $nugetPackageNameComponents =  $nugetPackage.Split('.')
    $releaseBinName = ""
    for($i=0; $i -lt ($nugetPackageNameComponents.Count - 3); $i++) {
        $releaseBinName = $releaseBinName + $nugetPackageNameComponents[$i]
        if ($i -lt ($nugetPackageNameComponents.Count - 3 - 1)) {
            $releaseBinName = $releaseBinName + "."
        }
    }
    Write-Host "New release bin name:" $releaseBinName

    # release bin version based on the NuGet package version suffix:
    [array]::Reverse($nugetPackageNameComponents)
    Write-Host "Package components:" $nugetPackageNameComponents

    if($nugetPackageNameComponents[$nugetPackageNameComponents.Count - 1] -eq "LTS") {
        Write-Host "Dealing with an LTS realease..."
        $patchAndSuffix = $nugetPackageNameComponents[0]
        $patchAndSuffixComponents = $patchAndSuffix.Split('-')
        Write-Host "   Patch and Suffix components:" $patchAndSuffixComponents
        if ($patchAndSuffixComponents.Count -gt 1) {
            $releaseBinVersion = 
            'v' + $nugetPackageNameComponents[2] + '.' + $nugetPackageNameComponents[1] + '.' + $patchAndSuffixComponents[0] + '-LTS'
            Write-Host "   Release bin version (in progress):" $releaseBinVersion
            for($i=1; $i -lt $patchAndSuffixComponents.Count; $i++) {
                $releaseBinVersion = $releaseBinVersion + '-' + $patchAndSuffixComponents[$i]
                Write-Host "   Release bin version (in progress):" $releaseBinVersion
            }
        }
        else {
            Write-Host "Dealing with a non-LTS realease..."
            $releaseBinVersion = 
            'v' + $nugetPackageNameComponents[2] + '.' + $nugetPackageNameComponents[1] + '.' + $nugetPackageNameComponents[0] + "-LTS"
        }
    }
    else {
        $releaseBinVersion = 
            'v' + $nugetPackageNameComponents[2] + '.' + $nugetPackageNameComponents[1] + '.' + $nugetPackageNameComponents[0]
    }
    Write-Host "Release bin version:" $releaseBinVersion

    # release folder path:
    $releaseFolder = Join-Path -Path $releasesPath  -ChildPath $releaseBinVersion 
    if(!(Test-Path $releaseFolder)) {
        New-Item -Path $releaseFolder -ItemType Directory #-WhatIf
    }

    # release bin folder path:
    $releaseBin = Join-Path -Path $releaseFolder -ChildPath ('bin-' + $releaseBinVersion + '-' + $releaseBinName)
    if(!(Test-Path $releaseBin)) {
        New-Item -Path $releaseBin -ItemType Directory #-WhatIf
    }

    # copy release bin content from the project source path:
    Copy-Item -Path ($projectBin+'/*') -Destination $releaseBin -recurse -Force #-Verbose

    # stash away NuGet packages of interest:
    $nugetBuilds = @("Release", "Debug")
    foreach($nugetBuild in $nugetBuilds) {
        $releaseNuGetPackagesPath = Join-Path -Path $releaseFolder -ChildPath ("NuGet-"+$nugetBuild)
        if(!(Test-Path $releaseNuGetPackagesPath)) {
            New-Item -Path $releaseNuGetPackagesPath -ItemType Directory #-WhatIf
        }
        Copy-Item -Path ($releaseBin+'/'+$nugetBuild+'/*.nupkg') -Destination $releaseNuGetPackagesPath -Recurse -Force #-Verbose
    }

    # zip release bins folders
    # https://devblogs.microsoft.com/scripting/use-powershell-to-create-zip-archive-of-folder/
    $zipPath = Join-Path -Path $releaseFolder -ChildPath ((Split-Path $releaseBin -Leaf) + '.zip') 
    [io.compression.zipfile]::CreateFromDirectory($releaseBin, $zipPath) 

    # delete release bins folders
    Remove-Item -Path $releaseBin -Recurse -Force #-Verbose
}




# make sure all the samples are buildable:
<# 
Get-ChildItem "./Samples" -Filter *.sln | 
Foreach-Object {

    # clear all the ground:
    dotnet clean $_.FullName --configuration Debug
    dotnet clean $_.FullName --configuration Release

    # restore all the dependencies:
    dotnet restore $_.FullName | Where-Object {$_.FullName -match 'Xamarin'}

    # build the Release configurations:
    dotnet build $_.FullName --configuration Release | Where-Object {$_.FullName -match 'Xamarin'}
} 
#>
