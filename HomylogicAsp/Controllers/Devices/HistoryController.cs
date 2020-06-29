using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HomylogicAsp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using X.Homylogic;
using X.Homylogic.Models.Objects;
using X.Basic;
using X.Homylogic.Models.Objects.Devices;
using HomylogicAsp.Models.Devices;
using System.Text;
using X.Homylogic.Models.Objects.Devices.Homyoko;
using System.Globalization;
using Newtonsoft.Json;
using X.Data;
using System.Threading;
using System.Runtime.CompilerServices;
using MySqlX.XDevAPI.Relational;
using HomylogicAsp.Commands;

namespace HomylogicAsp.Controllers.Devices
{
    public class HistoryController : Controller
    {
        public const string SELECTABLE_RANGE_DEFALT_KEY = "[today]";
        public const string SELECTABLE_RANGE_DEFALT_CAPTION = "Today";


        /// <summary>
        /// Loads and returns all data range selectable options for filtering history data on page UI.
        /// </summary>
        /// <param name="id">Device ID.</param>
        public string GetSelectablesDataRange(int id) 
        {
            StringBuilder result = new StringBuilder();
            try
            {
                // Adds constant selectable ranges.
                result.AppendFormat("{0}:{1};", SELECTABLE_RANGE_DEFALT_KEY, SELECTABLE_RANGE_DEFALT_CAPTION);
                result.Append("[yesterday]:Yesterday;");
                result.Append("[days-03]:Last 3 days;");
                result.Append("[days-07]:Last 7 days;");
                result.Append("[days-30]:Last 30 days;");
                // Reads all available months for all years in history.
                string tableName = $"deviceHistory_{id}";
                DBClient dbClient = Body.Database.DBClientLogs;
                StringBuilder sql = new StringBuilder();
                sql.AppendFormat("SELECT logTime FROM {0} ", tableName);
                if (dbClient.ClientType == DBClient.ClientTypes.Sqlite) {
                    // Sqlite.
                    sql.Append("GROUP BY strftime('%Y-%m', logTime) ");
                }
                else {
                    // MySql.
                    sql.Append("GROUP BY DATE_FORMAT(logTime, '%Y-%m') ");
                }
                sql.Append("ORDER BY logTime DESC");
                CultureInfo ci = (System.Globalization.CultureInfo)CultureInfo.CurrentCulture.Clone();
                ci.NumberFormat.NumberDecimalSeparator = ".";
                ci.NumberFormat.NegativeSign = "-";
                ci.NumberFormat.NumberGroupSeparator = "";
                List<int> yearsAdded = new List<int>();
                dbClient.Open();
                using DBReader dbReader = dbClient.ExecuteReader(sql.ToString());
                while (dbReader.Read())
                {
                    DateTime time = dbReader.GetDateTime("logTime");
                    // Add whole year.
                    int year = time.Year;
                    if (!yearsAdded.Contains(year)) {
                        result.AppendFormat("[year-{0}]:All year {0};", year);
                        yearsAdded.Add(year);
                    }
                    // Add month.
                    string month = time.Month.ToString("0#"); // Use leading zero for easier reading.
                    result.AppendFormat("[month-{0}*{1}]:{2} - {0};", year, month, time.ToString("MMMM", CultureInfo.CreateSpecificCulture("en")));
                }
            }
            catch (Exception ex)
            {
                X.Homylogic.Body.Environment.Logs.Error("Can't load selectable data range options.", ex, this.GetType().Name);
            }
            return result.ToString();
        }





        // GET: WeatherStation/?
        public ActionResult WeatherStation(int id)
        {
            return View("/Views/Devices/Homyoko/HistoryWeatherStation.cshtml", new Models.Devices.Homyoko.EditWeatherStationViewModel() { ID = id });
        }
        public enum DataGroupTypes : Int32 
        {  
            /// <summary>No groups, returns all logged data.</summary>
            None = 0,
            /// <summary>Group by hours with average values pre hour.</summary>
            AvgHour = 1
        }
        /// <summary>
        /// Returns log history data values in CSV format.
        /// </summary>
        /// <param name="id">Weather station device ID.</param>
        /// <param name="rangeKey">Range key defines filter range for loakding data.</param>
        /// <param name="dataGroupType">Can group data for less results with average values (long range keys are autmatically grouped by hour, etc. year).</param>
        public string GetHistoryHomyokoWeatherStation(int id, string rangeKey, DataGroupTypes dataGroupType)
        {
            if (string.IsNullOrEmpty(rangeKey)) rangeKey = SELECTABLE_RANGE_DEFALT_KEY;
            StringBuilder result = new StringBuilder();
            try
            {
                string tableName = $"deviceHistory_{id}";
                StringBuilder sql = new StringBuilder();
                DBClient dbClient = Body.Database.DBClientLogs;
                string sqlWhere = this.GetSQLWhereRange(rangeKey, tableName, new X.Data.Management.SqlConvert(dbClient.ClientType));
                if (rangeKey.StartsWith("[year-") || rangeKey.StartsWith("[month-") || dataGroupType == DataGroupTypes.AvgHour) {
                    // Group values every hour, if selected history range for default is month or year.
                    sql.AppendFormat("SELECT logTime, AVG(temperature1) AS temperature1, AVG(temperature2) AS temperature2, AVG(windspeed) AS windspeed, AVG(windspeedAvg) AS windspeedAvg, AVG(sunshine) AS sunshine FROM {0} ", tableName);
                    sql.Append(sqlWhere);
                    if (dbClient.ClientType == DBClient.ClientTypes.Sqlite) {
                        // Sqlite.
                        sql.Append("GROUP BY strftime('%Y-%m-%d %H', logTime) ");
                    }
                    else {
                        // MySql.
                        sql.Append("GROUP BY DATE_FORMAT(logTime, '%Y-%m-%d %H') ");
                    }
                }
                else { 
                    // Load all history values, which are saved every 12min.
                    sql.AppendFormat("SELECT logTime, temperature1, temperature2, windspeed, windspeedAvg, sunshine FROM {0} ", tableName);
                    sql.Append(sqlWhere);
                }
                sql.Append("ORDER BY logTime DESC");
                CultureInfo ci = (System.Globalization.CultureInfo)CultureInfo.CurrentCulture.Clone();
                ci.NumberFormat.NumberDecimalSeparator = ".";
                ci.NumberFormat.NegativeSign = "-";
                ci.NumberFormat.NumberGroupSeparator = "";
                dbClient.Open();
                using DBReader dbReader = dbClient.ExecuteReader(sql.ToString());
                int count = 0;
                while (dbReader.Read())
                {
                    string time = dbReader.GetDateTime("logTime").ToString("yyyy-MM-dd HH:mm:ss");
                    string temp1 = dbReader.GetFloat("temperature1").ToString(ci);
                    string temp2 = dbReader.GetFloat("temperature2").ToString(ci);
                    string wind = dbReader.GetFloat("windspeed").ToString(ci);
                    string windA = dbReader.GetFloat("windspeedAvg").ToString(ci);
                    int shineVal = dbReader.GetInt32("sunshine");
                    string shine = Math.Round(shineVal * X.Homylogic.Models.Objects.Devices.Homyoko.WeatherStation.SUN_SHINE_PERCENT_COEF, 2).ToString(ci);
                    result.AppendFormat("{0},{1},{2},{3},{4},{5};", time, temp1, temp2, wind, windA, shine);
                    count++;
                }
            }
            catch (Exception ex)
            {
                X.Homylogic.Body.Environment.Logs.Error("Problem loading weather station history data.", ex, this.GetType().Name);
            }
            return result.ToString();
        }


        // GET: IVTController/?
        public ActionResult IVTController(int id)
        {
            return View("/Views/Devices/Homyoko/HistoryIVTController.cshtml", new Models.Devices.Homyoko.EditIVTControllerViewModel() { ID = id });
        }
        /// <summary>
        /// Returns log history data values in CSV format.
        /// </summary>
        /// <param name="id">IVT Controller device ID.</param>
        /// <param name="rangeKey">Range key defines filter range for loakding data.</param>
        /// <param name="dataGroupType">Can group data for less results with average values (long range keys are autmatically grouped by hour, etc. year).</param>
        public string GetHistoryHomyokoIVTController(int id, string rangeKey, DataGroupTypes dataGroupType)
        {
            if (string.IsNullOrEmpty(rangeKey)) rangeKey = SELECTABLE_RANGE_DEFALT_KEY;
            StringBuilder result = new StringBuilder();
            string tableName = $"deviceHistory_{id}";
            StringBuilder sql = new StringBuilder();
            DBClient dbClient = Body.Database.DBClientLogs;
            string sqlWhere = this.GetSQLWhereRange(rangeKey, tableName, new X.Data.Management.SqlConvert(dbClient.ClientType));
            if (rangeKey.StartsWith("[year-") || rangeKey.StartsWith("[month-") || dataGroupType == DataGroupTypes.AvgHour)
            {
                // Group values every hour, if selected history range for default is month or year.
                sql.AppendFormat("SELECT logTime, AVG(temperatureFloor) AS temperatureFloor FROM {0} ", tableName);
                sql.Append(sqlWhere);
                if (dbClient.ClientType == DBClient.ClientTypes.Sqlite)
                {
                    // Sqlite.
                    sql.Append("GROUP BY strftime('%Y-%m-%d %H', logTime) ");
                }
                else
                {
                    // MySql.
                    sql.Append("GROUP BY DATE_FORMAT(logTime, '%Y-%m-%d %H') ");
                }
            }
            else
            {
                // Load all history values, which are saved every 12min.
                sql.AppendFormat("SELECT logTime, temperatureFloor FROM {0} ", tableName);
                sql.Append(sqlWhere);
            }
            sql.Append("ORDER BY logTime DESC");
            CultureInfo ci = (System.Globalization.CultureInfo)CultureInfo.CurrentCulture.Clone();
            ci.NumberFormat.NumberDecimalSeparator = ".";
            ci.NumberFormat.NegativeSign = "-";
            ci.NumberFormat.NumberGroupSeparator = "";
            try
            {
                dbClient.Open();
                using DBReader dbReader = dbClient.ExecuteReader(sql.ToString());
                int count = 0;
                while (dbReader.Read())
                {
                    string time = dbReader.GetDateTime("logTime").ToString("yyyy-MM-dd HH:mm:ss");
                    string temp = dbReader.GetFloat("temperatureFloor").ToString(ci);
                    result.AppendFormat("{0},{1};", time, temp);
                    count++;
                    if (count > 300) break;
                }
            }
            catch (Exception ex)
            {
                X.Homylogic.Body.Environment.Logs.Error("Problem loading IVT Controller history data.", ex, this.GetType().Name);
            }
            return result.ToString();
        }


        


        /// <summary>
        /// Returns SQL where condition for filter history data according to range key option.
        /// </summary>
        private string GetSQLWhereRange(string rangeKey, string tableName, X.Data.Management.SqlConvert q)
        {
            StringBuilder sql = new StringBuilder();
            if (rangeKey.StartsWith("[today]"))
            {
                DateTime now = DateTime.Now;
                string sqlNow = q.DTime(new DateTime(now.Year, now.Month, now.Day, 0, 0, 1));
                sql.AppendFormat("WHERE logTime > {0}", sqlNow);
            }
            if (rangeKey.StartsWith("[yesterday]"))
            {
                DateTime now = DateTime.Now.AddDays(-1);
                string sqlNowDown = q.DTime(new DateTime(now.Year, now.Month, now.Day, 0, 0, 1));
                string sqlNowUp = q.DTime(new DateTime(now.Year, now.Month, now.Day, 23, 59, 59));
                sql.AppendFormat("WHERE logTime > {0} AND logTime < {1}", sqlNowDown, sqlNowUp);
            }
            if (rangeKey.StartsWith("[days-"))
            {
                int daysCount = int.Parse(rangeKey.Substring(rangeKey.IndexOf("-") + 1, 2));
                DateTime dateDays = DateTime.Now.AddDays(daysCount * -1);
                string sqlDays = q.DTime(new DateTime(dateDays.Year, dateDays.Month, dateDays.Day, 0, 0, 1));
                sql.AppendFormat("WHERE logTime > {0}", sqlDays);
            }
            if (rangeKey.StartsWith("[year-"))
            {
                int year = int.Parse(rangeKey.Substring(rangeKey.IndexOf("-") + 1, 4));
                string sqlYearStart = q.DTime(new DateTime(year, 1, 1, 0, 0, 1));
                string sqlYearFinish = q.DTime(new DateTime(year, 12, 31, 23, 59, 59));
                sql.AppendFormat("WHERE logTime > {0} AND logTime < {1}", sqlYearStart, sqlYearFinish);
            }
            if (rangeKey.StartsWith("[month-"))
            {
                int year = int.Parse(rangeKey.Substring(rangeKey.IndexOf("-") + 1, 4));
                int month = int.Parse(rangeKey.Substring(rangeKey.IndexOf("*") + 1, 2));
                string sqlYearMonthStart = q.DTime(new DateTime(year, month, 1, 0, 0, 1));
                string sqlYearMonthFinish = q.DTime(new DateTime(year, 12, 31, 23, 59, 59));
                sql.AppendFormat("WHERE logTime > {0} AND logTime < {1}", sqlYearMonthStart, sqlYearMonthFinish);
            }

            // Default TODAY, when rangeKey is null or unknown.
            if (sql.Length == 0) { 
                DateTime now = DateTime.Now;
                string sqlNow = q.DTime(new DateTime(now.Year, now.Month, now.Day, 0, 0, 1));
                sql.AppendFormat("WHERE logTime > {0}", sqlNow);
            }

            return sql.ToString();
        }



    }
}