using POS.Core;
using POS.MVVM.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;
using Wpf.Ui.Extensions;
using Wpf.Ui.Mvvm.Interfaces;

namespace POS.Services
{
    public interface IWindowService
    {
        Window PayWindow { get; }
        Window ProdConfigWindow { get; }
        bool OpenPayWindow<T>() where T : ViewModel;
        bool ClosePayWindow();
        bool OpenProdConfigWindow<T>() where T : ViewModel;
        bool CloseProdConfigWindow();

        event EventHandler ProdConfigWindowClosedEvent;
    }
    public class WindowService : ObservableObject, IWindowService
    {
        private Window _payWindow;
        private Window _prodConfigWindow;
        private Func<Type, ViewModel> _viewModelFactory;
        public Window PayWindow
        {
            get { return _payWindow; }
            set
            {
                _payWindow = value;
                OnPropertyChanged();
            }
        }
        public Window ProdConfigWindow
        {
            get { return _prodConfigWindow; }
            set
            {
                _prodConfigWindow = value;
                OnPropertyChanged();
            }
        }
        
        public event EventHandler ProdConfigWindowClosedEvent;
        public WindowService(Func<Type, ViewModel> viewModelFactory)
        {
            _viewModelFactory = viewModelFactory;
        }

        public bool OpenPayWindow<TViewModle>() where TViewModle : ViewModel
        {
            ViewModel viewModel = _viewModelFactory.Invoke(typeof(TViewModle));

            if(viewModel.GetType() != typeof(PayViewModel))
            {
                Debug.WriteLine("Not Supported ViewModel");
                return false;
            }

            if (PayWindow != null)
            {
                ClosePayWindow();
            }

            var win = new Window();
            PayWindow = win;
            win.Owner = App.Current.MainWindow;
            App.Current.MainWindow.IsHitTestVisible = false;
            win.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            win.SizeToContent = SizeToContent.WidthAndHeight;
            win.WindowStyle = WindowStyle.None;
            win.ResizeMode = ResizeMode.NoResize;
            win.Content = viewModel;

            win.Closed += PayWin_Closed;

            win.Show();
            return true;
        }

        private void PayWin_Closed(object sender, EventArgs e)
        {
            PayWindow.Closed -= PayWin_Closed;
            PayWindow.Close();
            App.Current.MainWindow.IsHitTestVisible = true;
            PayWindow = null;
        }

        public bool ClosePayWindow()
        {
            if(PayWindow != null)
            {
                PayWindow.Closed -= PayWin_Closed;
                PayWindow.Close();
                App.Current.MainWindow.IsHitTestVisible = true;
                PayWindow = null;
                return true;
            }
            return false;
        }

        public bool OpenProdConfigWindow<TViewModle>() where TViewModle : ViewModel
        {
            ViewModel viewModel = _viewModelFactory.Invoke(typeof(TViewModle));

            if (viewModel.GetType() != typeof(ProductConfigViewModel))
            {
                Debug.WriteLine("Not Supported ViewModel");
                return false;
            }

            if (ProdConfigWindow != null)
            {
                CloseProdConfigWindow();
            }

            if (PayWindow != null)
            {
                ClosePayWindow();
            }

            var win = new Window();
            ProdConfigWindow = win;
            win.Owner = App.Current.MainWindow;
            App.Current.MainWindow.IsHitTestVisible = false;
            win.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            win.SizeToContent = SizeToContent.WidthAndHeight;
            win.WindowStyle = WindowStyle.None;
            win.ResizeMode = ResizeMode.NoResize;
            win.Content = viewModel;

            win.Closed += ProdConfigWindow_Closed;

            win.Show();
            return true;
        }

        private void ProdConfigWindow_Closed(object sender, EventArgs e)
        {
            ProdConfigWindow.Closed -= ProdConfigWindow_Closed;
            ProdConfigWindow.Close();
            App.Current.MainWindow.IsHitTestVisible = true;
            ProdConfigWindow = null;
            ProdConfigWindowClosedEvent?.Invoke(this, EventArgs.Empty);
        }

        public bool CloseProdConfigWindow()
        {
            if (ProdConfigWindow != null)
            {
                ProdConfigWindow.Closed -= ProdConfigWindow_Closed;
                ProdConfigWindow.Close();
                App.Current.MainWindow.IsHitTestVisible = true;
                ProdConfigWindow = null;
                return true;
            }
            return false;
        }
    }
}
