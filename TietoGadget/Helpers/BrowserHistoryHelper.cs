using Java.Net;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Web;


namespace TietoGadget.Helpers
{
    public class HistoryItem
    {
        public string Url { get; set; }

        public string Title { get; set; }

        public DateTime VisitedTime { get; set; }
    }
    public class BrowserHistoryHelper
    {
        public List<HistoryItem> GetBrowserHistory()
        {
            List<HistoryItem> lstBrowserHistory = new List<HistoryItem>();

            lstBrowserHistory = GetFirefoxhistory(lstBrowserHistory);
            lstBrowserHistory = GetChromeHistory(lstBrowserHistory);

            return lstBrowserHistory;
        }
        public List<HistoryItem> GetChromeHistory(List<HistoryItem> lstHistoryItem)
        {
            try
            {
                string chromeHistoryFile = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\Google\Chrome\User Data\Default\History";
                chromeHistoryFile = @"C:\Users\Administrator.SP15DEV\AppData\Local\Google\Chrome\User Data\Default\History";                                
                if (File.Exists(chromeHistoryFile))
                {
                    SQLiteConnection connection = new SQLiteConnection("Data Source=" + chromeHistoryFile + ";Version=3;New=False;Compress=True;");

                    connection.Open();

                    DataSet dataset = new DataSet();

                    SQLiteDataAdapter adapter = new SQLiteDataAdapter("select * from urls order by last_visit_time desc", connection);
                    adapter.Fill(dataset);
                    if (dataset != null && dataset.Tables.Count > 0 & dataset.Tables[0] != null)
                    {
                        DataTable dt = dataset.Tables[0];

                        foreach (DataRow historyRow in dt.Rows)
                        {
                            if (Convert.ToString(historyRow["title"]).Contains("Google Search") || Convert.ToString(historyRow["title"]).Contains("Facebook Search")
                                    || Convert.ToString(historyRow["title"]).Contains("Wikipedia, the free encyclopedia"))
                            {
                                HistoryItem historyItem = new HistoryItem();
                                historyItem.Url = Convert.ToString(historyRow["url"]);
                                historyItem.Title = Convert.ToString(historyRow["title"]);

                                // Chrome stores time elapsed since Jan 1, 1601 (UTC format) in microseconds
                                long utcMicroSeconds = Convert.ToInt64(historyRow["last_visit_time"]);
                                // Windows file time UTC is in nanoseconds, so multiplying by 10
                                DateTime gmtTime = DateTime.FromFileTimeUtc(10 * utcMicroSeconds);
                                // Converting to local time
                                DateTime localTime = TimeZoneInfo.ConvertTimeFromUtc(gmtTime, TimeZoneInfo.Local);
                                historyItem.VisitedTime = localTime;

                                lstHistoryItem.Add(historyItem);
                            }
                        }
                    }
                }
                return lstHistoryItem;
            }
            catch (Exception Ex)
            {
                return lstHistoryItem;
            }
        }

        public List<HistoryItem> GetFirefoxhistory(List<HistoryItem> lstHistoryItem)
        {
            try
            {
                string chromeHistoryFile = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\Google\Chrome\User Data\Default\History";
                string documentsFolder = @"C:\Users\Administrator.SP15DEV\AppData\Roaming\Mozilla\Firefox\Profiles";
                // Check if directory exists
                if (Directory.Exists(documentsFolder))
                {
                    // Loop each Firefox Profile
                    foreach (string folder in Directory.GetDirectories(documentsFolder))
                    {
                        // Fetch Profile History
                        DataTable historyDT = ExtractFromTable("moz_places", folder);
                        // Get visit Time/Data info
                        DataTable visitsDT = ExtractFromTable("moz_historyvisits", folder);
                        // Loop each history entry
                        foreach (DataRow row in historyDT.Rows)
                        {
                            // Select entry Date from visits
                            var entryDate = (from dates in visitsDT.AsEnumerable()
                                             where dates["place_id"].ToString() == row["id"].ToString()
                                             select dates).LastOrDefault();
                            // If history entry has date
                            if (entryDate != null)
                            {
                                if (row["title"].ToString().Contains("Google Search") || row["title"].ToString().Contains("Facebook Search")
                                    || row["title"].ToString().Contains("Wikipedia, the free encyclopedia"))
                                {
                                    // Obtain URL and Title strings
                                    HistoryItem historyItem = new HistoryItem();
                                    historyItem.Url = row["Url"].ToString();
                                    historyItem.Title = row["title"].ToString();
                                    lstHistoryItem.Add(historyItem);
                                }

                            }
                        }
                    }
                }
                return lstHistoryItem;
            }
            catch (Exception ex)
            {
                return lstHistoryItem;
            }
        }
        DataTable ExtractFromTable(string table, string folder)
        {
            SQLiteConnection sql_con;
            SQLiteCommand sql_cmd;
            SQLiteDataAdapter DB;
            DataTable DT = new DataTable();


            // FireFox database file
            string dbPath = folder + "\\places.sqlite";

            // If file exists
            if (File.Exists(dbPath))
            {
                // Data connection
                sql_con = new SQLiteConnection("Data Source=" + dbPath +";Version=3;New=False;Compress=True;");
                // Open the Connection
                sql_con.Open();
                sql_cmd = sql_con.CreateCommand();

                // Select Query
                string CommandText = "select * from " + table;

                // Populate Data Table
                DB = new SQLiteDataAdapter(CommandText, sql_con);
                DB.Fill(DT);

                // Clean up
                sql_con.Close();
            }
            return DT;
        }
    }

    public class Firefox
    {
        public List<URL> URLs { get; set; }
        public IEnumerable<URL> GetHistory()
        {
            // Get Current Users App Data
            string documentsFolder = Environment.GetFolderPath
                             (Environment.SpecialFolder.ApplicationData);

            // Move to Firefox Data
            documentsFolder += "\\Mozilla\\Firefox\\Profiles\\";
            documentsFolder = @"C:\Users\Administrator.SP15DEV\AppData\Roaming\Mozilla\Firefox\Profiles";

            // Check if directory exists
            if (Directory.Exists(documentsFolder))
            {
                // Loop each Firefox Profile
                foreach (string folder in Directory.GetDirectories
                                                   (documentsFolder))
                {
                    // Fetch Profile History
                    return ExtractUserHistory(folder);
                }
            }
            return null;
        }

        IEnumerable<URL> ExtractUserHistory(string folder)
        {
            // Get User history info
            DataTable historyDT = ExtractFromTable("moz_places", folder);

            // Get visit Time/Data info
            DataTable visitsDT = ExtractFromTable("moz_historyvisits", folder);
            // Loop each history entry
            foreach (DataRow row in historyDT.Rows)
            {
                // Select entry Date from visits
                var entryDate = (from dates in visitsDT.AsEnumerable()
                                 where dates["place_id"].ToString() == row["id"].ToString()
                                 select dates).LastOrDefault();
                // If history entry has date
                if (entryDate != null)
                {
                    // Obtain URL and Title strings
                    string url = row["Url"].ToString();
                    string title = row["title"].ToString();

                    // Create new Entry
                    //URL u = new URL(url.Replace('\'', ' '),
                    //                title.Replace('\'', ' '),
                    //                "Mozilla Firefox");


                    // Add entry to list
                    //URLs.Add(u);
                }
            }
            // Clear URL History
            //DeleteFromTable("moz_places", folder);
            //DeleteFromTable("moz_historyvisits", folder);


            return URLs;
        }
        /*
        void DeleteFromTable(string table, string folder)
        {
            SQLiteConnection sql_con;
            SQLiteCommand sql_cmd;


            // FireFox database file
            string dbPath = folder + "\\places.sqlite";


            // If file exists
            if (File.Exists(dbPath))
            {
                // Data connection
                sql_con = new SQLiteConnection("Data Source=" + dbPath +

                                    ";Version=3;New=False;Compress=True;");


                // Open the Conn
                sql_con.Open();


                // Delete Query
                string CommandText = "delete from " + table;


                // Create command
                sql_cmd = new SQLiteCommand(CommandText, sql_con);


                sql_cmd.ExecuteNonQuery();


                // Clean up
                sql_con.Close();
            }
        }
        */

        DataTable ExtractFromTable(string table, string folder)
        {
            SQLiteConnection sql_con;
            SQLiteCommand sql_cmd;
            SQLiteDataAdapter DB;
            DataTable DT = new DataTable();


            // FireFox database file
            string dbPath = folder + "\\places.sqlite";


            // If file exists
            if (File.Exists(dbPath))
            {
                // Data connection
                sql_con = new SQLiteConnection("Data Source=" + dbPath +

                                    ";Version=3;New=False;Compress=True;");


                // Open the Connection
                sql_con.Open();
                sql_cmd = sql_con.CreateCommand();


                // Select Query
                string CommandText = "select * from " + table;


                // Populate Data Table
                DB = new SQLiteDataAdapter(CommandText, sql_con);
                DB.Fill(DT);


                // Clean up
                sql_con.Close();
            }
            return DT;
        }

        
    }
}