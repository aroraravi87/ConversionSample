using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;


namespace AVSToJVSConversion.DLL
{
    class DbConnection:IDisposable
    {
        private static SqlConnection _connection = null;
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static SqlConnection GetSqlConnection()
        {
            if (_connection != null)
            {
                if (_connection.State == ConnectionState.Closed)
                {
                    _connection.Open();

                }
                return _connection;
            }
            else
            {

                _connection = new SqlConnection(Convert.ToString(ConfigurationManager.ConnectionStrings["dbconnection"].ConnectionString));
                _connection.Open();
            }
            return _connection;
        }
        /// <summary>
        /// 
        /// </summary>
        public static void CloseSqlConnection(SqlConnection _connection)
        {
            if (_connection.State == ConnectionState.Open)
            {
                _connection.Close();

            }
        }


        public void Dispose()
        {
         // Dispose();
        }
    }
}
