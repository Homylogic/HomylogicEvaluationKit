/* LOG DATA LIST
 * 
 * Obsahuje zoznam objektov LogRecord a umožňuje zapisovanie logov.
 * 
 */
using System;
using System.Linq;
using System.Text;
using X.Data;
using X.Data.Factory;

namespace X.App.Logs
{
    public sealed class LogList : DataList
    {
        readonly DBClient _dbClient;
        object _dbSyncObject;
        int _recordsLimit;

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
            _recordsLimit = recordsLimit;
            base.LoadData("logTime DESC", _recordsLimit);
        }

        #endregion

        public LogList(DBClient dbClient, object dbSyncObject = null)
        {
            _dbClient = dbClient; 
            _dbSyncObject = dbSyncObject;
            if (_dbSyncObject == null)
                _dbSyncObject = new object(); 
        }

        /// <summary>
        /// Zapísanie informácie.
        /// </summary>
        public void Info(string text, string description, string source = null) 
        {
            try
            {
                lock (_dbSyncObject) 
                { 
                    LogRecord log = (LogRecord)this.GetInitializedDataRecord();
                    log.LogType = LogRecord.LogTypes.Info;
                    log.LogTime = DateTime.Now;
                    log.Text = text;
                    log.Description = description;
                    log.Source = source;
                    log.Save();

                    // Vymazať najstarší záznam (nový log je pridaný vždy na začiatok listu).
                    if (this.List.Count > _recordsLimit)
                        this.List.RemoveAt(this.List.Count - 1);
                }
            }
            catch (Exception)
            {
            }
        }
        /// <summary>
        /// Zapísanie warovania.
        /// </summary>
        public void Warning(string text, string description = null, string source = null)
        {
            try
            {
                lock (_dbSyncObject) 
                { 
                    LogRecord log = (LogRecord)this.GetInitializedDataRecord();
                    log.LogType = LogRecord.LogTypes.Warning;
                    log.LogTime = DateTime.Now;
                    log.Text = text;
                    log.Description = description;
                    log.Source = source;
                    log.Save();

                    // Vymazať najstarší záznam (nový log je pridaný vždy na začiatok listu).
                    if (this.List.Count > _recordsLimit)
                        this.List.RemoveAt(this.List.Count - 1);
                }
            }
            catch (Exception)
            {
            }
        }
        /// <summary>
        /// Zapísanie chyby.
        /// </summary>
        public void Error(string text, Exception ex, string source = null)
        {
            try
            {
                lock (_dbSyncObject) 
                { 
                    LogRecord log = (LogRecord)this.GetInitializedDataRecord();
                    log.LogType = LogRecord.LogTypes.Error;
                    log.LogTime = DateTime.Now;
                    log.Text = text;
                    log.Description = ex.Message;
                    log.Source = source;
                    log.Save();

                    // Vymazať najstarší záznam (nový log je pridaný vždy na začiatok listu).
                    if (this.List.Count > _recordsLimit)
                        this.List.RemoveAt(this.List.Count - 1);
                }
            }
            catch (Exception)
            {
            }
        }
        /// <summary>
        /// Vymazanie starých log údajov.
        /// </summary>
        public void DeleteOld()
        {
            lock (_dbSyncObject) 
            { 
                // Vymazanie všetkých záznamov starších ako posledný log záznam v zozname list.
                LogRecord log = (LogRecord)this.List[^1];
                Data.Management.SqlConvert q = new Data.Management.SqlConvert(this.DBClient.ClientType);
                this.DBClient.Open();
                this.DBClient.ExecuteNonQuery($"DELETE FROM {LogRecord.TABLE_NAME} WHERE logTime < {q.DTime(log.LogTime)}");
                this.DBClient.Close();
            }
        }
    }
}
