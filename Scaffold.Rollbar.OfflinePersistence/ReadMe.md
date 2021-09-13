
# Critical Notes
Make sure both projects' (Rollbar.OfflinePersistence and Scaffold.Rollbar.OfflinePersistence) dependencies on EF Core related packages are of the same EF Core version!
Currently, we are using Microsoft.EntityFrameworkCore.* version is 3.1.8 - the last one compatible with netstandard2.0 that is current FrameworkTarget of the Rollbar.OfflinePersistence project.

# Useful References related to the Project:

https://docs.microsoft.com/en-us/ef/core/cli/dotnet#other-target-frameworks
https://docs.microsoft.com/en-us/dotnet/core/tools/global-tools#install-a-local-tool
https://docs.microsoft.com/en-us/aspnet/core/data/ef-rp/intro?view=aspnetcore-5.0&tabs=visual-studio

# Relevant CLI Commands:

dotnet tool install --global dotnet-ef
dotnet tool update --global dotnet-ef
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet ef

dotnet new tool-manifest
dotnet tool install dotnetsay
dotnet tool install dotnet-doc
dotnet tool install dotnet-ef
dotnet tool restore
dotnet tool list

dotnet tool update dotnetsay
dotnet tool update dotnet-doc
dotnet tool update dotnet-ef
dotnet tool restore
dotnet tool list

dotnet tool uninstall dotnetsay
dotnet tool uninstall dotnet-doc
dotnet tool uninstall dotnet-ef
dotnet tool restore
dotnet tool list

# EF Core Migrations Related Commands:
dotnet ef migrations list -p "..\Rollbar.OfflinePersistence"
dotnet ef migrations add <MIGRATION_NAME> -p "..\Rollbar.OfflinePersistence"
