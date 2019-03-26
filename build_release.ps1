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

# restore all the dependencies:
dotnet restore rollbar.sln

$buildConfigurations = @(
    "Instrumented",
    "Debug",
    "Release"
)

foreach($buildConfiguration in $buildConfigurations) {
    # clear the ground:
    dotnet clean rollbar.sln --configuration $buildConfiguration
    # build the configurations:
    dotnet build rollbar.sln --configuration $buildConfiguration

    # unit-test the Release configuration: 
    #dotnet test rollbar.sln --configuration $buildConfiguration
}
# unit-test the Release configuration: 
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
