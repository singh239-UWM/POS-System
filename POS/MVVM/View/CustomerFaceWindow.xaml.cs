using System;
using System.Windows;
using System.Windows.Controls;

namespace POS.MVVM.View
{
    /// <summary>
    /// Interaction logic for window2.xaml
    /// </summary>
    public partial class CustomerFaceWindow : Window
    {
        public CustomerFaceWindow()
        {
            InitializeComponent();
        }

        private void ReceiptListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            receiptListView.ScrollIntoView(receiptListView.SelectedValue);
        }
    }
}
