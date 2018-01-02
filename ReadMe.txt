List of .NET standards and implementation frameworks currently supported by Rollbar.NET v1:
===========================================================================================

Moniker                             Preprocessor Symbol
--------------------------------------------------------
netstandard2.0						NETSTANDARD2_0
netcoreapp2.0						NETCOREAPP2_0
net45								NET45
net471								NET471
net47								NET47
net462								NET462
net461								NET461
net46                               NET46
net452                              NET452
net451                              NET451
net45                               NET45

RESOURCES:
======================

Complete list of Target Framework Monikers and corresponding preprocessor symbols:
https://docs.microsoft.com/en-us/dotnet/standard/frameworks

Also see:
https://blogs.msdn.microsoft.com/dotnet/2017/08/14/announcing-net-standard-2-0/
regarding UWP support via netstandard2.0 later this year.


We dropped usage of the StyleCop NuGet package since it is not compatible 
with .NetCoreApp2.0 platform and causes some build warnings.
Instead, use SyleCop Visual Studio Extension:
https://marketplace.visualstudio.com/items?itemName=ChrisDahlberg.StyleCop
