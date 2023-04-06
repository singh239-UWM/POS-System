using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows.Data;
using System.Threading;
using System.Reflection;
using MySql.Data.MySqlClient;
using POS.Store;
using POS.Services;
using Wpf.Ui.Controls.Interfaces;
using POS.Core;

namespace POS.MVVM.ViewModel
{
    public class DashboardViewModel : Core.ViewModel
    {
        

        Timer _timer;

        

        private ConnStore _connStore;

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

        //public RelayCommand NavToCartViewComm { get; set; }

        #region Constructor
        public DashboardViewModel(ConnStore conn, INavigationService navService)
        {
            _connStore = conn;
            Navigation = navService;


            //NavToCartViewComm = new RelayCommand(o => {
            //    Navigation.ViewCartView<CartViewModel>();
            //}, canExecute: o => true);

            _navigation.ViewCartView<CartViewModel>();

            _timer = new Timer(new TimerCallback((s) => DatabasePing(_connStore.CurrentCon)),
                               null, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(5));
        }
        #endregion

        private void DatabasePing(MySqlConnection conn)
        {
            Debug.WriteLine(conn.ConnectionString);
            if (conn.Ping() == false)
            {
                conn.Close();
                if(Navigation.CurrentView.GetType() != typeof(LoginViewModel))
                {
                    Navigation.NavigateTo<LoginViewModel>();
                }
                
            }
        }
    }
}
