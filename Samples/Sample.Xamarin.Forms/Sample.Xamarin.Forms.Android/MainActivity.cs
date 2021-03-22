using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Rollbar;
using Rollbar.DTOs;
using System.Threading.Tasks;

namespace Sample.Xamarin.Forms.Droid
{
    [Activity(
        Label = "Sample.Xamarin.Forms", 
        Icon = "@drawable/icon", 
        Theme = "@style/MainTheme", 
        MainLauncher = true, 
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation
        )]
    public class MainActivity 
        : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            RollbarHelper.ConfigureRollbarSingleton();

            // First informational log via Rollbar:
            RollbarLocator.RollbarInstance
                .Info("Xamarin.Forms sample: Hello world! Xamarin is here @MainActivity.OnCreate(...) ...");

            //START: Let's subscribe to all known unhandled exception events application-wide...
            RollbarHelper.RegisterForGlobalExceptionHandling();
            AndroidEnvironment.UnhandledExceptionRaiser += AndroidEnvironment_UnhandledExceptionRaiser;
            //END.

            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);
            LoadApplication(new App());
        }

        private void AndroidEnvironment_UnhandledExceptionRaiser(object sender, RaiseThrowableEventArgs e)
        {
            var newExc = new System.Exception("UnhandledExceptionRaiser", e.Exception);
            RollbarLocator.RollbarInstance.AsBlockingLogger(RollbarHelper.RollbarTimeout).Critical(newExc);
        }


    }
}

