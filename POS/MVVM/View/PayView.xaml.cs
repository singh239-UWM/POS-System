using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace POS.MVVM.View
{
    /// <summary>
    /// Interaction logic for PayView.xaml
    /// </summary>
    public partial class PayView : UserControl
    {
        public PayView()
        {
            InitializeComponent();
        }

        private void Num1_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine((sender as Button).Content.ToString());
        }
    }
}
