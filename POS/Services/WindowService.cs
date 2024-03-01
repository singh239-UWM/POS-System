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
        bool OpenDeptConfigWindow<T>() where T : ViewModel;
        bool CloseDeptConfigWindow();
        bool OpenMNMConfigWindow<T>() where T : ViewModel;
        bool CloseMNMConfigWindow();

        event EventHandler ProdConfigWindowClosedEvent;
        event EventHandler DeptConfigWindowClosedEvent;
        event EventHandler MNMConfigWindowClosedEvent;
    }
    public class WindowService : ObservableObject, IWindowService
    {
        #region private variables
        private Window _payWindow;
        private Window _prodConfigWindow;
        private Window _deptConfigWindow;
        private Window _MNMConfigWindow;
        private Func<Type, ViewModel> _viewModelFactory;
        #endregion
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
        public Window DeptConfigWindow
        {
            get { return _deptConfigWindow; }
            set
            {
                _deptConfigWindow = value;
                OnPropertyChanged();
            }
        }
        public Window MNMConfigWindow
        {
            get { return _MNMConfigWindow; }
            set
            {
                _MNMConfigWindow = value;
                OnPropertyChanged();
            }
        }
        #region event handler def
        public event EventHandler ProdConfigWindowClosedEvent;
        public event EventHandler DeptConfigWindowClosedEvent;
        public event EventHandler MNMConfigWindowClosedEvent;
        #endregion 

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

            #region Close if any opened window
            if (ProdConfigWindow != null)
            {
                CloseProdConfigWindow();
            }

            if (PayWindow != null)
            {
                ClosePayWindow();
            }
            #endregion

            var win = new Window();
            win.Title = "Inventory Management";
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

        public bool OpenDeptConfigWindow<TViewModle>() where TViewModle : ViewModel
        {
            ViewModel viewModel = _viewModelFactory.Invoke(typeof(TViewModle));

            if (viewModel.GetType() != typeof(DeptConfigViewModel))
            {
                Debug.WriteLine("Not Supported ViewModel");
                return false;
            }

            #region Close if any opened window
            if (DeptConfigWindow != null) { CloseDeptConfigWindow(); }
            if (ProdConfigWindow != null) { CloseProdConfigWindow(); }
            if (PayWindow != null) { ClosePayWindow(); }
            #endregion

            #region openwindow
            var win = new Window();
            win.Title = "Department Management";
            DeptConfigWindow = win;
            win.Owner = App.Current.MainWindow;
            App.Current.MainWindow.IsHitTestVisible = false;
            win.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            win.SizeToContent = SizeToContent.WidthAndHeight;
            win.WindowStyle = WindowStyle.None;
            win.ResizeMode = ResizeMode.NoResize;
            win.Content = viewModel;

            win.Closed += DeptConfigWindow_Closed;

            win.Show();
            #endregion

            return true;
        }
        private void DeptConfigWindow_Closed(object sender, EventArgs e)
        {
            DeptConfigWindow.Closed -= DeptConfigWindow_Closed;
            DeptConfigWindow.Close();
            App.Current.MainWindow.IsHitTestVisible = true;
            DeptConfigWindow = null;
            DeptConfigWindowClosedEvent?.Invoke(this, EventArgs.Empty);
        }
        public bool CloseDeptConfigWindow()
        {
            if (DeptConfigWindow != null)
            {
                DeptConfigWindow.Closed -= DeptConfigWindow_Closed;
                DeptConfigWindow.Close();
                App.Current.MainWindow.IsHitTestVisible = true;
                DeptConfigWindow = null;
                return true;
            }
            return false;
        }

        public bool OpenMNMConfigWindow<TViewModle>() where TViewModle : ViewModel
        {
            ViewModel viewModel = _viewModelFactory.Invoke(typeof(TViewModle));

            if (viewModel.GetType() != typeof(MixnMatchConfigViewModel))
            {
                Debug.WriteLine("Not Supported ViewModel");
                return false;
            }

            #region Close if any opened window
            if (MNMConfigWindow != null) { CloseMNMConfigWindow(); }
            if (ProdConfigWindow != null) { CloseProdConfigWindow(); }
            if (DeptConfigWindow != null) { CloseDeptConfigWindow(); }
            if (PayWindow != null) { ClosePayWindow(); }
            #endregion

            #region openwindow
            var win = new Window();
            win.Title = "Mix-n-Match Management";
            MNMConfigWindow = win;
            win.Owner = App.Current.MainWindow;
            App.Current.MainWindow.IsHitTestVisible = false;
            win.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            win.SizeToContent = SizeToContent.WidthAndHeight;
            win.WindowStyle = WindowStyle.None;
            win.ResizeMode = ResizeMode.NoResize;
            win.Content = viewModel;

            win.Closed += MNMConfigWindow_Closed;

            win.Show();
            #endregion

            return true;
        }
        private void MNMConfigWindow_Closed(object sender, EventArgs e)
        {
            MNMConfigWindow.Closed -= MNMConfigWindow_Closed;
            MNMConfigWindow.Close();
            App.Current.MainWindow.IsHitTestVisible = true;
            MNMConfigWindow = null;
            MNMConfigWindowClosedEvent?.Invoke(this, EventArgs.Empty);
        }
        public bool CloseMNMConfigWindow()
        {
            if (MNMConfigWindow != null)
            {
                MNMConfigWindow.Closed -= MNMConfigWindow_Closed;
                MNMConfigWindow.Close();
                App.Current.MainWindow.IsHitTestVisible = true;
                MNMConfigWindow = null;
                return true;
            }
            return false;
        }
    }
}
