/* LOG DATA LIST
 * 
 * Obsahuje zoznam objektov LogRecord a umožňuje zapisovanie logov.
 * 
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using X.Data;
using X.Data.Factory;

namespace X.App.Logs
{
    public sealed class LogList : DataList
    {
        readonly DBClient _dbClient;
        Hashtable _latestLogValues = new Hashtable();

        #region --- DATA LIST ---

        public override DBClient DBClient => _dbClient;
        public override DataRecord GetInitializedDataRecord(DBReader dbReader = null)
        {
            return new LogRecord(_dbClient) { ParentDataList = this };
        }
        public static void CreateTable(DBClient dbClient)
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendFormat("CREATE TABLE {0} (", LogRecord.TABLE_NAME);
            if (dbClient.ClientType == DBClient.ClientTypes.Sqlite)
                sql.Append("id INTEGER PRIMARY KEY, ");
            else
                sql.Append("id INTEGER PRIMARY KEY AUTO_INCREMENT, ");
            sql.Append("logType INTEGER, ");
            sql.Append("logTime DATETIME, ");
            sql.Append("dontDelete INTEGER, ");
            sql.Append("text TEXT, ");
            sql.Append("description TEXT, ");
            sql.Append("source TEXT");
            sql.Append(")");
            dbClient.ExecuteNonQuery(sql.ToString());
        }
        public void LoadData(int recordsLimit)
        {
            base.LoadData("logTime DESC", recordsLimit);
        }

        #endregion

        public LogList(DBClient dbClient)
        {
            _dbClient = dbClient; 
        }

        /// <summary>
        /// Writes info log record.
        /// </summary>
        /// <param name="text">Main log text.</param>
        /// <param name="description">Some info description.</param>
        /// <param name="source">Can be used as key, for filter fast repetitive key when is unique, etc. Title and VariableName.</param>
        public void Info(string text, string description = null, string source = null) 
        {
            try
            {
                string keyLatest = $"Info:{source}";
                string logValues = $"{text}-{description}";

                if (this.CompareToLatestLog(keyLatest, logValues)) {
                    // Current log values are same like latest log record values,
                    // find latest log record and update DateTime.
                    KeyValuePair<long, string> latestValues = (KeyValuePair<long, string>)_latestLogValues[keyLatest];
                    LogRecord log = (LogRecord)this.FindDataRecord(latestValues.Key);
                    log.LogTime = DateTime.Now;
                    log.Save();
                } else { 
                    // Log values are not same like latest log values, 
                    // add new log recrod.
                    LogRecord log = (LogRecord)this.GetInitializedDataRecord();
                    log.LogType = LogRecord.LogTypes.Info;
                    log.LogTime = DateTime.Now;
                    log.Text = text;
                    log.Description = description;
                    log.Source = source;
                    log.Save();

                    // Write latest info values for current log.
                    if (_latestLogValues.ContainsKey(keyLatest))
                        _latestLogValues[keyLatest] = new KeyValuePair<long, string>(log.ID, logValues);
                    else
                        _latestLogValues.Add(keyLatest, new KeyValuePair<long, string>(log.ID, logValues));
                }
            }
            catch (Exception)
            {
            }
        }
        /// <summary>
        /// Writes warning log record.
        /// </summary>
        /// <param name="text">Main log text.</param>
        /// <param name="description">Some info description.</param>
        /// <param name="source">Can be used as key, for filter fast repetitive key when is unique, etc. Title and VariableName.</param>
        public void Warning(string text, string description = null, string source = null)
        {
            try
            {
                string keyLatest = $"Warning:{source}";
                string logValues = $"{text}-{description}";

                if (this.CompareToLatestLog(keyLatest, logValues)) {
                    // Current log values are same like latest log record values,
                    // find latest log record and update DateTime.
                    KeyValuePair<long, string> latestValues = (KeyValuePair<long, string>)_latestLogValues[keyLatest];
                    LogRecord log = (LogRecord)this.FindDataRecord(latestValues.Key);
                    log.LogTime = DateTime.Now;
                    log.Save();
                }
                else
                {
                    // Log values are not same like latest log values, 
                    // add new log recrod.
                    LogRecord log = (LogRecord)this.GetInitializedDataRecord();
                    log.LogType = LogRecord.LogTypes.Warning;
                    log.LogTime = DateTime.Now;
                    log.Text = text;
                    log.Description = description;
                    log.Source = source;
                    log.Save();

                    // Write latest info values for current log.
                    if (_latestLogValues.ContainsKey(keyLatest))
                        _latestLogValues[keyLatest] = new KeyValuePair<long, string>(log.ID, logValues);
                    else
                        _latestLogValues.Add(keyLatest, new KeyValuePair<long, string>(log.ID, logValues));
                }
            }
            catch (Exception)
            {
            }
        }
        /// <summary>
        /// Writes error log record.
        /// </summary>
        /// <param name="text">Main log text.</param>
        /// <param name="description">Some info description.</param>
        /// <param name="source">Can be used as key, for filter fast repetitive key when is unique, etc. Title and VariableName.</param>
        public void Error(string text, Exception ex, string source = null)
        {
            try
            {
                string keyLatest = $"Error:{source}";
                string logValues = $"{text}-{ex.Message}";

                if (this.CompareToLatestLog(keyLatest, logValues)) {
                    // Current log values are same like latest log record values,
                    // find latest log record and update DateTime.
                    KeyValuePair<long, string> latestValues = (KeyValuePair<long, string>)_latestLogValues[keyLatest];
                    LogRecord log = (LogRecord)this.FindDataRecord(latestValues.Key);
                    log.LogTime = DateTime.Now;
                    log.Save();
                }
                else
                {
                    // Log values are not same like latest log values, 
                    // add new log recrod.
                    LogRecord log = (LogRecord)this.GetInitializedDataRecord();
                    log.LogType = LogRecord.LogTypes.Error;
                    log.LogTime = DateTime.Now;
                    log.Text = text;
                    log.Description = ex.Message;
                    log.Source = source;
                    log.Save();

                    // Write latest info values for current log.
                    if (_latestLogValues.ContainsKey(keyLatest))
                        _latestLogValues[keyLatest] = new KeyValuePair<long, string>(log.ID, logValues);
                    else
                        _latestLogValues.Add(keyLatest, new KeyValuePair<long, string>(log.ID, logValues));
                }
            }
            catch (Exception)
            {
            }
        }
        /// <summary>
        /// Vymazanie príliš starých log údajov v databáze (neexistujú v kolekcii).
        /// </summary>
        public void DeleteOld()
        {
            // Vymazanie všetkých záznamov starších ako posledný log záznam v zozname list.
            if (this.List.Count < this.RecordsLimit) return;
            LogRecord log = (LogRecord)this.List[^1];
            Data.Management.SqlConvert q = new Data.Management.SqlConvert(this.DBClient.ClientType);
            this.DBClient.Open();
            this.DBClient.ExecuteNonQuery($"DELETE FROM {LogRecord.TABLE_NAME} WHERE logTime < {q.DTime(log.LogTime)}");
        }
        /// <summary>
        /// Compare specified (usually current) logs data with latest logs values.
        /// Returns TRUE if specified values are same like latest log.
        /// </summary>
        private bool CompareToLatestLog(string keyLatest, string logValues)
        {                               
            if (this.List.Count == 0) return false;

            // Find lates log values from hash table.
            if (!_latestLogValues.ContainsKey(keyLatest)) return false;
            KeyValuePair<long, string> latestValues = (KeyValuePair<long, string>)_latestLogValues[keyLatest];

            // Compare latest log values with current specified log values.
            if (logValues == latestValues.Value)
                return true;
            else
                return false;
        }
    }
}
