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
using System.Windows.Shapes;

namespace ANetEmvDesktopSdk.Sample
{
    /// <summary>
    /// Interaction logic for ErrorScreen.xaml
    /// </summary>
    public partial class ErrorScreen : Window
    {
        public ErrorScreen()
        {
            InitializeComponent();
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        private void OnOkClicked(object context, RoutedEventArgs state)
        {
            this.Hide();
        }
    }
}
