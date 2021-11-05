using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// In SDK-style projects such as this one, several assembly attributes that were historically
// defined in this file are now automatically added during build and populated with
// values defined in project properties. For details of which attributes are included
// and how to customize this process see: https://aka.ms/assembly-info-properties


// Setting ComVisible to false makes the types in this assembly not visible to COM
// components.  If you need to access a type in this assembly from COM, set the ComVisible
// attribute to true on that type.

[assembly: ComVisible(true)]

[assembly: CLSCompliant(true)]

// The following GUID is for the ID of the typelib if this project is exposed to COM.

[assembly: Guid("40140cb3-aab4-4259-8b9f-2cfb3e782fdb")]

[assembly: InternalsVisibleTo("Rollbar.AppSettings.Json")]
[assembly: InternalsVisibleTo("Rollbar.App.Config")]
[assembly: InternalsVisibleTo("Rollbar.OfflinePersistence")]
[assembly: InternalsVisibleTo("Rollbar.NetPlatformExtensions")]
[assembly: InternalsVisibleTo("Rollbar.NetCore.AspNet")]

[assembly: InternalsVisibleTo("UnitTest.Rollbar")]
[assembly: InternalsVisibleTo("UnitTest.Rollbar.PlugIns.Log4net")]
[assembly: InternalsVisibleTo("UnitTest.Rollbar.PlugIns.MSEnterpriseLibrary")]
[assembly: InternalsVisibleTo("UnitTest.Rollbar.PlugIns.NLog")]
[assembly: InternalsVisibleTo("UnitTest.Rollbar.PlugIns.Serilog")]
