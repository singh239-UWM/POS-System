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
        ViewModel CartVM { get; }
        void NavigateTo<T>() where T : ViewModel;
        void ViewCartView<T>() where T : ViewModel;
    }

    public class NavigationService : ObservableObject, INavigationService
    {
        private ViewModel _currentView;
        private ViewModel _cartVM;
        private Func<Type, ViewModel> _viewModelFactory;

        public ViewModel CurrentView
        {
            get { return _currentView; }
            private set
            {
                _currentView = value;
                OnPropertyChanged();
            }
        }

        public ViewModel CartVM
        {
            get { return _cartVM; }
            private set
            {
                _cartVM = value;
                OnPropertyChanged();
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

        public void ViewCartView<TViewModel>() where TViewModel : ViewModel
        {
            ViewModel viewModel = _viewModelFactory.Invoke(typeof(TViewModel));
            CartVM = viewModel;
        }
    }
}
