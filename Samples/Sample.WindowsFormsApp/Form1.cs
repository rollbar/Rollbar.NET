namespace Sample.WindowsFormsApp
{
    using Rollbar;
    using System;
    using System.Windows.Forms;

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void _btnLogHandledException_Click(object sender, EventArgs e)
        {
            try
            {
                throw new ApplicationException("WindowsFormsApp sample: A handled exception just happened!");
            }
            catch (Exception ex)
            {
                RollbarLocator.RollbarInstance.Critical(ex);
            }

        }

        private void _btnLogUnhandledException_Click(object sender, EventArgs e)
        {
            throw new ApplicationException("WindowsFormsApp sample: An unhandled exception just happened!");
        }
    }
}
