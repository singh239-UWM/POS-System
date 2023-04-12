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
using Wpf.Ui.Extensions;
using Wpf.Ui.Mvvm.Interfaces;

namespace POS.Services
{
    public interface IWindowService
    {
        Window PayWindow { get; }
        bool OpenPayWindow<T>() where T : ViewModel;
        bool ClosePayWindow();
    }
    public class WindowService : ObservableObject, IWindowService
    {
        private Window _currentWindow;
        private bool _isOpen;
        private Func<Type, ViewModel> _viewModelFactory;
        public Window PayWindow
        {
            get { return _currentWindow; }
            set
            { 
                _currentWindow = value;
                OnPropertyChanged();
            }
        }

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
            win.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            win.SizeToContent = SizeToContent.WidthAndHeight;
            win.WindowStyle = WindowStyle.None;
            win.ResizeMode = ResizeMode.NoResize;
            win.Content = viewModel;
            win.Show();
            return true;
        }

        public bool ClosePayWindow()
        {
            if(PayWindow != null)
            {
                PayWindow.Close();
                PayWindow = null;
                return true;
            }
            return false;
        }
    }
}
