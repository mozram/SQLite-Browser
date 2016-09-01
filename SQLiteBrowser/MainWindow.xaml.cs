using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using System.Data.SQLite;
using Microsoft.Win32;
using System.Data;

namespace SQLiteBrowser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DataSet ds;
        DB db;
        private string currentCell { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            btnCloseDB.IsEnabled = false;
            btnDeleteRow.IsEnabled = false;
            btnExecuteSql.IsEnabled = false;
            cboxTable.IsEnabled = false;
        }

        private void openDB_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog OpenDB = new OpenFileDialog();
            OpenDB.Filter = "DB files (*.db)|*.db";
            if (OpenDB.ShowDialog() == true)
            {
                DB.db_path = OpenDB.FileName;
                cboxTable.Items.Clear();
            }
            else return;

            DB._DBconnectionString = "Data Source=" + DB.db_path + ";Version=3;";
            if(DB.dbVerify("select name from sqlite_master limit 1").ToString() == "-1")
            {
                InputDialog input = new InputDialog("Password for database", "Enter this database password: ");
                if (input.ShowDialog() == false)
                    return;
                DB._DBconnectionString = "Data Source=" + DB.db_path + ";Password=" + input.Text + ";Version=3;";
                if (DB.dbVerify("select name from sqlite_master limit 1").ToString() == "-1")
                {
                    MessageBox.Show("Wrong password");
                    return;
                }
            }

            refreshTable();   

            this.Title = "SQLite Browser - " + DB.db_path;

            btnopenDB.IsEnabled = false;
            btnopenDB.Content = "Opened";
            btnCloseDB.IsEnabled = true;
            btnDeleteRow.IsEnabled = true;
            btnExecuteSql.IsEnabled = true;
            cboxTable.IsEnabled = true;
        }

        private void refreshTable()
        {
            cboxTable.Items.Clear();
            db = new DB();
            SQLiteDataReader rd = db.QueryResult("select name from sqlite_master");
            while (rd.Read())
            {
                if (!rd["name"].ToString().Contains("sqlite"))
                {
                    cboxTable.Items.Add(rd["name"].ToString());
                }
            }
        }

        private void cboxTable_DropDownClosed(object sender, EventArgs e)
        {
            if (cboxTable.SelectedIndex == -1) return;
            string sql = "select * from " + cboxTable.SelectedItem.ToString();
            ds = DB.QueryDataset(sql);
            lstDB.ItemsSource = ds.Tables[0].DefaultView;
        }

        private void lstDB_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            var element = e.EditingElement as TextBox;
            var text = element.Text;
            string selectedHeader = e.Column.Header.ToString();
            string header = (string)lstDB.SelectedCells[0].Column.Header;

            DataRowView row = (DataRowView)lstDB.SelectedItems[0];
            string firstColumn = row[0].ToString();

            //apply changes to database
            string query = "update " + cboxTable.Text + " set `" + selectedHeader + "`='" + text + "' where `" + header + "`='" + firstColumn + "' and `" + selectedHeader + "`='" + currentCell + "'";
            DB.Query(query);
        }

        private void lstDB_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            string header = e.Column.Header.ToString();

            DataRowView row = (DataRowView)lstDB.SelectedItems[0];
            currentCell = row[header].ToString();
        }

        private void btnDeleteRow_Click(object sender, RoutedEventArgs e)
        {
            int items = lstDB.SelectedItems.Count;

            //get first two header
            string header1 = (string)lstDB.SelectedCells[0].Column.Header;
            string header2 = (string)lstDB.SelectedCells[1].Column.Header;

            for(int i = 0; i < items; i++)
            {
                //get content of first two row
                DataRowView row = (DataRowView)lstDB.SelectedItems[i];
                string column1 = row[0].ToString();
                string column2 = row[1].ToString();

                string sql = "delete from " + cboxTable.Text + " where " + header1.Replace("__","_") + "='" + column1 + "' and " + header2.Replace("__", "_") + "='" + column2 + "'";
                DB.Query(sql);
            }
            

            //refresh grid
            lstDB.ItemsSource = null;
            cboxTable_DropDownClosed(sender, e);
        }

        private void btnExecuteSql_Click(object sender, RoutedEventArgs e)
        {
            InputDialog input = new InputDialog("Custom SQLite Command", "Enter SQLite command: ");
            if (input.ShowDialog() == false)
                return;
            string sql = input.Text.ToLower();
            if (sql.Substring(0, 6).ToLower() == "select")
            {
                ds = DB.QueryDataset(sql);
                if (ds != null)
                    lstDB.ItemsSource = ds.Tables[0].DefaultView;
            }
            else DB.Query(sql);

            refreshTable();
        }

        private void lstDB_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            string header = e.Column.Header.ToString();

            e.Column.Header = header.Replace("_", "__");
        }

        private void btnCloseDB_Click(object sender, RoutedEventArgs e)
        {
            btnCloseDB.IsEnabled = false;
            btnDeleteRow.IsEnabled = false;
            btnExecuteSql.IsEnabled = false;
            cboxTable.IsEnabled = false;
            lstDB.ItemsSource = null;

            btnopenDB.IsEnabled = true;
            btnopenDB.Content = "Open DB";
        }
    }
}
