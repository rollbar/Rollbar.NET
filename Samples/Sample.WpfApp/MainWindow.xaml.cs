namespace Sample.WpfApp
{
    using Rollbar;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Navigation;
    using System.Windows.Shapes;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void _btnGnerateUnhandledException_Click(object sender, RoutedEventArgs e)
        {
            throw new ApplicationException("WpfApp Sample: Simulated unhandled exception!");
        }

        private void _btnGnerateHandledException_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                throw new Exception("WpfApp Sample: Simulated handled exception!");
            }
            catch (Exception ex)
            {
                ApplicationException appExceptionWrapper = new ApplicationException("WpfApp Sample exception wrapper", ex);
                // Let's log this:
                RollbarLocator.RollbarInstance.Critical(appExceptionWrapper);
            }
        }
    }
}
