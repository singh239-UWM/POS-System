using Microsoft.Extensions.DependencyInjection;
using MySql.Data.MySqlClient;
using POS.Core;
using POS.MVVM.ViewModel;
using POS.MVVM.View;
using POS.Services;
using POS.Store;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace POS
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private ServiceProvider _serviceProvider;

        public App()
        {
            IServiceCollection services = new ServiceCollection();

            services.AddSingleton<MainWindow>(provider => new MainWindow
            {
                DataContext = provider.GetRequiredService<MainViewModel>()
            });

            services.AddSingleton<CustomerFaceWindow>(provider => new CustomerFaceWindow
            {
                DataContext = provider.GetRequiredService<CustFaceViewModel>()
            });

            services.AddSingleton<MainViewModel>();
            services.AddSingleton<CustFaceViewModel>();
            services.AddSingleton<LoginViewModel>();
            services.AddSingleton<DashboardViewModel>();
            services.AddSingleton<CartViewModel>();
            services.AddSingleton<PayViewModel>();
            services.AddSingleton<CustFaceTotalViewModel>();
            services.AddSingleton<CustFaceDueViewModel>();
            services.AddSingleton<ManualEntryViewModel>();
            services.AddSingleton<ProductConfigViewModel>();
            services.AddSingleton<ConnStore>();
            services.AddSingleton<ReceiptAmountStore>();
            services.AddSingleton<ReceiptItemsStore>();
            services.AddSingleton<ReceiptAmountDueStore>();
            services.AddSingleton<SelectedItemForConfigStore>();

            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<IWindowService, WindowService>();

            services.AddSingleton<Func<Type, ViewModel>>( serviceProvider => viewModelType => (ViewModel)serviceProvider.GetRequiredService(viewModelType));

            _serviceProvider = services.BuildServiceProvider();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            var customerWindow = _serviceProvider.GetRequiredService<CustomerFaceWindow>();
            mainWindow.Show();
            customerWindow.Show();
            base.OnStartup(e);
        }
    }
}
