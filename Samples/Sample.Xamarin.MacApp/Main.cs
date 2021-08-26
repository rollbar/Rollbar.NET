namespace Sample.Xamarin.MacApp
{
    using System.Collections.Generic;
    using AppKit;
    using Rollbar;

    static class MainClass
    {
        static void Main(string[] args)
        {
            NSApplication.Init();

            // Setup Rollbar SDK after the applicaion runtime is initialized above:
            RollbarHelper.ConfigureRollbar();
            RollbarHelper.RegisterForGlobalExceptionHandling();

            // First Rollbar SDK test usage:
            RollbarLocator.RollbarInstance
                .Info("Sample.Xamarin.MacApp: Hello world! Xamarin is here @App.xaml.cs ...");
            Dictionary<string, object> customFields = new Dictionary<string, object>();
            customFields.Add("Hebrew", "אספירין");
            customFields.Add("Hindi", "एस्पिरि");
            customFields.Add("Chinese", "阿司匹林");
            customFields.Add("Japanese", "アセチルサリチル酸");
            customFields.Add("path1", "d:\\Work\\\ufffd\ufffd\ufffd\ufffd\ufffd\ufffd\\branches\\v2\\...");
            customFields.Add("path2", @"d:\Work\אספירין\branches\v2\...");
            RollbarLocator.RollbarInstance.Info("Sample.Xamarin.MacApp: Basic info log example.", customFields);
            RollbarLocator.RollbarInstance.Debug("Sample.Xamarin.MacApp: First debug log.");

            NSApplication.Main(args);
        }
    }
}
