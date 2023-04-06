using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS.Store
{
    public class ConnStore
    {
        private MySqlConnection _currentConn;

        public MySqlConnection CurrentCon 
        { 
            get { return _currentConn; }
            set
            {
                _currentConn = value;
            }
        }
    }
}
