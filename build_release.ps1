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

Write-Host "Deleting /obj and /bin folders of the SDK projects/modules..."
Get-ChildItem -Path "." -Directory |
Where-Object {$_.Name -match '^Rollbar*'} |
ForEach-Object {
    Write-Host "  SDK module: " $_.FullName
    $path = $_.FullName + '/obj'
    if (Test-Path $path) {
    Remove-Item -Path $path -Recurse #-WhatIf
    }
    $path = $_.FullName + '/bin'
    if (Test-Path $path) {
        Remove-Item -Path $path -Recurse #-WhatIf
    }
}

Write-Host "Restoring the SDK dependencies..."
dotnet restore rollbar.sln

Write-Host "Building all the SDK build configurations..."
$buildConfigurations = @(
    "Instrumented",
    "Debug",
    "Release"
)
foreach($buildConfiguration in $buildConfigurations) {
    Write-Host "  Building the SDK build configuration:" + $buildConfiguration + ...
    # clean the configuration:
    Write-Host "    - cleaning..."
    dotnet clean rollbar.sln --configuration $buildConfiguration
    # build the configurations:
    Write-Host "    - building..."
    dotnet build rollbar.sln --configuration $buildConfiguration
    # unit-test the Release configuration: 
    #Write-Host "    - unit-testing..."
    #dotnet test rollbar.sln --configuration $buildConfiguration
}
# unit-test the Release configuration: 
Write-Host "    - unit-testing Debug build..."
dotnet test rollbar.sln --configuration Debug

# make sure all the samples are buildable:
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
