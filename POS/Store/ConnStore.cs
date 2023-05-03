using MySql.Data.MySqlClient;

namespace POS.Store
{
    public class ConnStore
    {
        private MySqlConnection _currentConn;
        private string _userID;

        public MySqlConnection CurrentCon 
        { 
            get { return _currentConn; }
            set
            {
                _currentConn = value;
                MySqlConnectionStringBuilder builder = new MySqlConnectionStringBuilder(_currentConn?.ConnectionString);
                UserID = builder?.UserID;
            }
        }

        public string UserID
        {
            get { return _userID; }
            set { _userID = value; }
        }
    }
}
