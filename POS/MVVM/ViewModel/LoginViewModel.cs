using MySql.Data.MySqlClient;
using POS.Core;
using POS.Services;
using POS.Store;
using System.Diagnostics;
using System.Security;
using System.Threading.Tasks;

namespace POS.MVVM.ViewModel
{
    public class LoginViewModel : Core.ViewModel
    {

        private ConnStore _connStore;

        private string _loginMessage = "";

        private string _userID = "";
        private string _password = "";

        public string UserID
        {
            get
            { 
                return _userID; 
            }
            set 
            {
                _userID = value;
                OnPropertyChanged();
            }
        }

        public string Password
        {
            get
            {
                return _password;
            }
            set
            {
                _password = value;
                OnPropertyChanged();
            }
        }

        public string LoginMessage
        {
            get
            {
                return this._loginMessage;
            }
            set
            {
                this._loginMessage = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand NavToCashRegViewComm { get; set; }

        public LoginViewModel(INavigationService navService, ConnStore connStore)
        {
            _connStore = connStore;

            NavToCashRegViewComm = new RelayCommand( o => 
            {

                MySqlConnection conn;

                conn = new MySqlConnection("server=virk-s;" +
                                            "user=" + UserID.ToString() + ";" +
                                            "database=POS_1;" +
                                            "port=3306;" +
                                            "password=" + Password.ToString() + "");

                //Debug.WriteLine(UserID + " | " + Password.ToString());

                DatabaseConnService ds = new DatabaseConnService(conn);

                if (ds.IsConnected == true)
                {
                    _connStore.CurrentCon = conn;

                    navService.NavigateTo<DashboardViewModel>();
                    UserID = "";
                    LoginMessage = "";

                }

                LoginMessage = "Login Failed";
                Password = "";

            }, canExecute: o => true);

        }
    }
}
