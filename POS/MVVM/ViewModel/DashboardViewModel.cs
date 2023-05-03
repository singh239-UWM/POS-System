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
using System.Windows;

namespace POS.MVVM.ViewModel
{
    public class DashboardViewModel : Core.ViewModel
    {
        Timer _timer;

        

        private ConnStore _connStore;
        private IWindowService _windowService;
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


        #region Commands
        public RelayCommand OpenProdConfigComm { get; set; }
        public RelayCommand NavigateToManualEntryView { get; set; }
        public RelayCommand NavigateToOtherView { get; set; }
        #endregion


        #region Constructor
        public DashboardViewModel(ConnStore conn, INavigationService navService, IWindowService windowService)
        {
            _connStore = conn;
            Navigation = navService; 
            _windowService = windowService;

            #region Commands Def assigning
            OpenProdConfigComm = new RelayCommand(o => { OpenProdConfig(); }, canExecute: o => true);
            NavigateToManualEntryView = new RelayCommand(o => { Navigation.NavigatDashBoardTabTo<ManualEntryViewModel>(); }, canExecute: o => true);
            NavigateToOtherView = new RelayCommand(o => { Navigation.NavigatDashBoardTabTo<CartViewModel>(); }, canExecute: o => true);
            #endregion


            //NavToCartViewComm = new RelayCommand(o => {
            //    Navigation.ViewCartView<CartViewModel>();
            //}, canExecute: o => true);

            _navigation.ViewCartView<CartViewModel>();

            _timer = new Timer(new TimerCallback((s) => DatabasePing(_connStore.CurrentCon)),
                               null, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(5));
            
        }
        #endregion

        #region Commands Def
        private void OpenProdConfig()
        {
            bool isOpend = _windowService.OpenProdConfigWindow<ProductConfigViewModel>();
            if (isOpend)
            {
                return;
            }
            else
            {
                MessageBox.Show("Could Not open Prod config window");
            }

        }
        #endregion

        private void DatabasePing(MySqlConnection conn)
        {
            //Debug.WriteLine(conn.ConnectionString);
            try
            {
                if (conn.Ping() == false)
                {
                    conn.Close();
                    if (Navigation.CurrentView.GetType() != typeof(LoginViewModel))
                    {
                        Navigation.NavigateTo<LoginViewModel>();
                    }

                }
            }
            catch (Exception ex)
            {
                
            }
        }
    }
}
