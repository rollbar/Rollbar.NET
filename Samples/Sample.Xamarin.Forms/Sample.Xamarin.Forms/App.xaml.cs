using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms;
using Rollbar;
using Rollbar.DTOs;
using System.Threading.Tasks;

namespace Sample.Xamarin.Forms
{

    public partial class App : Application
    {
        public App ()
        {

            RollbarLocator.RollbarInstance
                .Info("Xamarin.Forms sample: Hello world! Xamarin is here @App.xaml.cs ...");

            InitializeComponent();

            Dictionary<string, object> customFields = new Dictionary<string, object>();
            customFields.Add("Hebrew", "אספירין");
            customFields.Add("Hindi", "एस्पिरि");
            customFields.Add("Chinese", "阿司匹林");
            customFields.Add("Japanese", "アセチルサリチル酸");
            customFields.Add("path1", "d:\\Work\\\ufffd\ufffd\ufffd\ufffd\ufffd\ufffd\\branches\\v2\\...");
            customFields.Add("path2", @"d:\Work\אספירין\branches\v2\...");

            RollbarLocator.RollbarInstance.Info("Xamarin.Forms sample: Basic info log example.", customFields);
            RollbarLocator.RollbarInstance.Debug("Xamarin.Forms sample: First debug log.");

            MainPage = new Sample.Xamarin.Forms.MainPage();
        }

        protected override void OnStart ()
        {
            // Handle when your app starts
        }

        protected override void OnSleep ()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume ()
        {
            // Handle when your app resumes
        }

    }
}
