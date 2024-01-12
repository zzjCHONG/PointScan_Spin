using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace Simscop.Spindisk.WPF.Views
{
    /// <summary>
    /// Interaction logic for ExampleView.xaml
    /// </summary>
    public partial class ExampleView : Window
    {
        public ExampleView()
        {
            InitializeComponent();
        }
        protected override void OnClosing(CancelEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
        }

    }
}
