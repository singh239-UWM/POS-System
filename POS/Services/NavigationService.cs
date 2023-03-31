using MySql.Data.MySqlClient;
using POS.Core;
using POS.MVVM.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wpf.Ui.Mvvm.Interfaces;

namespace POS.Services
{
    public interface INavigationService
    {
        ViewModel CurrentView { get; }
        void NavigateTo<T>() where T : ViewModel;
        void NavigateTo<T>(MySqlConnection conn) where T : ViewModel;
    }

    public class NavigationService : ObservableObject, INavigationService
    {
        private ViewModel _currentView;
        private Func<Type, ViewModel> _viewModelFactory;
        private MySqlConnection _conn;

        Timer _timer;

        public ViewModel CurrentView
        {
            get { return _currentView; }
            private set
            {
                _currentView = value;
                OnPropertyChanged();
            }
        }

        private void DatabasePing(MySqlConnection conn)
        {
            if( conn.Ping() == false )
            {
                conn.Close();
                NavigateTo<LoginViewModel>();
            }
        }

        public NavigationService(Func<Type, ViewModel> viewModelFactory)
        {
            _viewModelFactory = viewModelFactory;

        }

        public void NavigateTo<TViewModel>() where TViewModel : ViewModel
        {
            ViewModel viewModel = _viewModelFactory.Invoke(typeof(TViewModel));
            CurrentView = viewModel;
        }

        public void NavigateTo<TViewModel>(MySqlConnection conn) where TViewModel : ViewModel
        {
            ViewModel viewModel = _viewModelFactory.Invoke(typeof(TViewModel));
            CurrentView = viewModel;
            _conn = conn;

            _timer = new Timer(new TimerCallback((s) => DatabasePing(_conn)),
                               null, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(5));
        }
    }
}
