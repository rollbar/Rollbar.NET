using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;

namespace Sample.Xamarin.Forms
{
	public partial class App : Application
	{
		public App ()
		{
			InitializeComponent();

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
