using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Rollbar;
using Sample.Xamarin.VSMac;
//using Xamarin.Essentials;

namespace Sample.Xamarinoid.VSMac.Droid
{
    [Activity(Label = "Sample.Xamarin.VSMac", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            RollbarHelper.ConfigureRollbar();

            // First informational log via Rollbar:
            RollbarLocator.RollbarInstance
                .Info("Xamarin.Forms sample: Hello world! Xamarin is here @MainActivity.OnCreate(...) ...");

            //START: Let's subscribe to all known unhandled exception events application-wide...
            RollbarHelper.RegisterForGlobalExceptionHandling();
            AndroidEnvironment.UnhandledExceptionRaiser += AndroidEnvironment_UnhandledExceptionRaiser;
            //END.

            //TabLayoutResource = global::Xamarin.Essentials.Resource.Layout.Tabbar;
            //ToolbarResource = global::Xamarin.Essentials.Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            global::Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App());
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            global::Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        private void AndroidEnvironment_UnhandledExceptionRaiser(object sender, RaiseThrowableEventArgs e)
        {
            var newExc = new System.Exception("UnhandledExceptionRaiser", e.Exception);
            RollbarLocator.RollbarInstance.AsBlockingLogger(RollbarHelper.RollbarTimeout).Critical(newExc);
        }


    }
}