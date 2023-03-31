using POS.Core;
using POS.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS.MVVM.ViewModel
{
    public class MainViewModel : Core.ViewModel
    {
        public LoginViewModel LoginVM { get; set; }
        public DateTimeViewModel DateTimeVM { get; set; }

        private INavigationService _navigation;

        public INavigationService Navigation
        {
            get => _navigation;
            set 
            {
                _navigation = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand NavToLoginViewComm { get; set; }

        public MainViewModel(INavigationService navService)
        {
            DateTimeVM = new DateTimeViewModel();

            Navigation = navService;
            NavToLoginViewComm = new RelayCommand(o => { Navigation.NavigateTo<LoginViewModel>(); }, canExecute: o => true);

            _navigation.NavigateTo<LoginViewModel>();
        }

    }
}
