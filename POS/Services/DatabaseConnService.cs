using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS.Services
{
    public class DatabaseConnService
    {
        private bool _isConnected;

        public bool  IsConnected
        {
            get { return _isConnected; }
            set { _isConnected = value; }
        }

        public DatabaseConnService(MySqlConnection conn)
        {
            _isConnected = LaunchSQL(conn);
        }
        private bool LaunchSQL(MySqlConnection conn)
        {

            bool isConnected;

            try
            {
                conn.OpenAsync().Wait();


                //conn.Close();
                isConnected = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                conn.Close();
                isConnected = false;
            }
            //Debug.WriteLine(conn.ConnectionString.ToString());
            // Perform database operations

            return isConnected;
        }
    }
}
