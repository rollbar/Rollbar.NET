using Rollbar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Sample.Xamarin.Forms
{
	public partial class MainPage 
        : ContentPage
	{
		public MainPage()
		{
			InitializeComponent();
		}

        private void HandledExceptionButton_Clicked(object sender,EventArgs e)
        {
            try
            {
                throw new ApplicationException("HANDLED exception!");
            }
            catch(System.Exception ex)
            {
                RollbarLocator.RollbarInstance.AsBlockingLogger(TimeSpan.FromSeconds(10)).Critical(ex);
            }
        }

        private void UnhandledExceptionButton_Clicked(object sender,EventArgs e)
        {
            throw new ApplicationException("UNHANDLED exception!");
        }
    }
}
