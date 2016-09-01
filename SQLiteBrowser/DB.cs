using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SQLiteBrowser
{
    class DB
    {
        //Global variable for absolute path of db
        static string _db_path;
        public static string db_path
        {
            get { return _db_path; }
            set { _db_path = value; }
        }

        static string __DBconnectionString;
        public static string _DBconnectionString
        {
            get { return __DBconnectionString; }
            set { __DBconnectionString = value; }
        }

        //private static string _DBconnectionString = "Data Source=pnewels.db;Version=3;";// Password=lol;";

        //non-static members
        private SQLiteConnection __conn = null;

        public DB()
        {
            __conn = new SQLiteConnection(_DBconnectionString);
            try
            {
                __conn.Open();//_conn.ChangePassword("lol");
            }
            catch (SQLiteException ex)
            {
                MessageBox.Show("Unable to connect to database. Exception output: " + ex.ToString());
                __conn = null;
            }
        }

        public static bool isDbOK()
        {

            bool _dbok = false;

            using (SQLiteConnection _conn = new SQLiteConnection(_DBconnectionString))
            {
                try
                {
                    _conn.Open();//_conn.ChangePassword("lol");
                    _dbok = true;

                }
                catch (SQLiteException) { }
            }
            return _dbok;
        }


        /// <summary>
        /// Execute SQL query returning number of rows affected. Only for INSERT, UPDATE and DELETE
        /// </summary>
        /// <param name="sql">the sql query to be executed</param>
        /// <returns>int - number of rows affected</returns>
        public static int Query(string sql)
        {
            int retval = -1;

            using (SQLiteConnection _conn = new SQLiteConnection(_DBconnectionString))
            {
                try
                {
                    _conn.Open();
                    using (SQLiteCommand cmd = new SQLiteCommand(sql, _conn))
                    {
                        retval = cmd.ExecuteNonQuery();
                    }
                }
                catch (SQLiteException ex)
                {
                    System.Windows.MessageBox.Show(ex.Message, "Query error!", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    //L.og("ERROR: " + ex.Message);
                }
            }

            return retval;
        }


        /// <summary>
        /// Execute SQL reader returning number of rows affected. Only for SELECT
        /// </summary>
        /// <param name="sql">the sql query to be executed</param>
        /// <returns>int - number of rows affected</returns>
        public static int Reader(string sql)
        {
            int retval = 0;

            using (SQLiteConnection _conn = new SQLiteConnection(_DBconnectionString))
            {
                try
                {
                    _conn.Open();
                    using (SQLiteCommand cmd = new SQLiteCommand(sql, _conn))
                    {
                        SQLiteDataReader rdr = cmd.ExecuteReader();
                        while (rdr.Read())
                        {
                            retval++;
                        }
                    }
                }
                catch (SQLiteException ex)
                {
                    System.Windows.MessageBox.Show(ex.Message, "DB error reading", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    //L.og("ERROR: " + ex.Message);
                }
            }

            return retval;
        }

        /// <summary>
        /// Check whether the db is protected or not
        /// </summary>
        /// <param name="sql">the query to be executed</param>
        /// <returns>reader -1 on error or integer</returns>
        public static object dbVerify(string sql)
        {
            int retval = -1;

            using (SQLiteConnection _conn = new SQLiteConnection(_DBconnectionString))
            {
                try
                {
                    _conn.Open();
                    using (SQLiteCommand cmd = new SQLiteCommand(sql, _conn))
                    {
                        object x = cmd.ExecuteScalar();

                        if (x != null)
                        {
                            return x;
                        }
                    }
                }
                catch (SQLiteException)
                {
                    //System.Windows.MessageBox.Show(ex.Message, "DB error Scalar", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    //L.og("ERROR: " + ex.Message);
                }
            }

            return retval;
        }

        /// <summary>
        /// Execute SQL SELECT returning first row, first column
        /// </summary>
        /// <param name="sql">the query to be executed</param>
        /// <returns>reader -1 on error or integer</returns>
        public static object QueryScalar(string sql)
        {
            int retval = -1;

            using (SQLiteConnection _conn = new SQLiteConnection(_DBconnectionString))
            {
                try
                {
                    _conn.Open();
                    using (SQLiteCommand cmd = new SQLiteCommand(sql, _conn))
                    {
                        object x = cmd.ExecuteScalar();

                        if (x != null)
                        {
                            return x;
                        }
                    }
                }
                catch (SQLiteException ex)
                {
                    System.Windows.MessageBox.Show(ex.Message, "DB error Scalar", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    //L.og("ERROR: " + ex.Message);
                }
            }

            return retval;
        }


        /// <summary>
        /// Execute SQL query returning records available
        /// e.g.
        /// rdr = .queryresult(...)
        /// while (rdr.Read())
        /// {
        ///     Console.WriteLine(rdr[0]+" -- "+rdr[1]);
        /// }
        /// rdr.Close();
        /// </summary>
        /// <param name="sql">the query to be executed</param>
        /// <returns>reader object to .read() results and to .close() once done</returns>
        public SQLiteDataReader QueryResult(string sql)
        {
            if (__conn == null)
                return null;

            SQLiteCommand cmd = new SQLiteCommand(sql, __conn);
            return cmd.ExecuteReader();
        }


        public static DataSet QueryDataset(string sql)
        {
            DataSet ds = new DataSet();

            using (SQLiteConnection _conn = new SQLiteConnection(_DBconnectionString))
            {
                try
                {
                    _conn.Open();
                    using (SQLiteDataAdapter fda = new SQLiteDataAdapter(sql, _conn))
                    {
                        fda.Fill(ds);
                    }
                }
                catch (SQLiteException ex)
                {
                    MessageBox.Show(ex.Message.ToString(), "SQL Syntax Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }
            }

            return ds;
        }
    }
}
