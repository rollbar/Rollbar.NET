"C:\Program Files (x86)\MSBuild\14.0\Bin\MsBuild.exe" Rollbar.sln /p:configuration=Release-Net45
"C:\Program Files (x86)\MSBuild\14.0\Bin\MsBuild.exe" Rollbar.sln /p:configuration=Release-Net40
"C:\Program Files (x86)\MSBuild\14.0\Bin\MsBuild.exe" Rollbar.sln /p:configuration=Release-Net35
nuget pack Rollbar\Rollbar.csproj -Prop Configuration=Release