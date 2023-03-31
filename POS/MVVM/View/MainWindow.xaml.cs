using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace POS
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MySqlConnection conn = new MySqlConnection( "server=localhost;" + 
                                                            "user=ping;" + 
                                                            "database=POS_1;" + 
                                                            "port=3306;" + 
                                                            "password=DatabasePing1");
        
        private Timer _timer;

        public MainWindow()
        {
            InitializeComponent();

            InitializeDatabase();

            //this method will ping database every 10 seconds
            _timer = new Timer(new TimerCallback((s) => DatabasePing(conn)),
                               null, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));
        }

        private async void InitializeDatabase()
        {


            this.isConnected_Button.Icon = Wpf.Ui.Common.SymbolRegular.DatabaseSearch20;
            this.databaseFlyout.Content = "Database Connecting...";


            Task<bool> connectedTask = LaunchSQL(conn);

            bool connected = await connectedTask;

            if (connected == true)
            {
                this.isConnected_Button.Icon = Wpf.Ui.Common.SymbolRegular.Database32;
                this.databaseFlyout.Content = "Database Connected";
            }
            else
            {
                this.isConnected_Button.Icon = Wpf.Ui.Common.SymbolRegular.DatabaseWarning20;
                this.databaseFlyout.Content = "Database Not Connected";
            }
        }

        private static async Task<bool> LaunchSQL(MySqlConnection conn)
        {
            bool isConnected = await Task.Run(() =>
            {
                try
                {
                    conn.Open();


                    //conn.Close();
                    return true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    conn.Close();
                    return false;
                }
            });
            //Debug.WriteLine(conn.ConnectionString.ToString());
            // Perform database operations


            return isConnected;
        }

        private void DatabasePing(MySqlConnection e)
        {
            if (e.Ping() == false)
            {
                this.Dispatcher.Invoke(() =>
                {
                    InitializeDatabase();
                });
            }
        }

        private void IsConnected_Click(object sender, RoutedEventArgs e)
        {
            this.databaseFlyout.Show();

        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            App.Current.Shutdown();
        }
    }
}
