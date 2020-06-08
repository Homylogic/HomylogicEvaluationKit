/* LOG DATA RECORD
 * 
 * Obsahuje a edituje údaje databázového záznamu logov.
 * 
 * 
 */
using System;
using System.Text;
using System.Drawing;
using X.Data;
using X.Data.Factory;
using System.Collections.Generic;

namespace X.App.Logs
{
    public sealed class LogRecord : DataRecord
    {
        readonly DBClient _dbClient;

        #region --- DATA PROPERTIES ---

        public enum LogTypes : Int32
        {
            Unknown = 0,
            Info = 1,
            Warning = 2,
            Error = 3
        }
        /// <summary>
        /// Určuje typ zariadenia.
        /// </summary>
        public LogTypes LogType { get; set; }
        /// <summary>
        /// Dátum a čas pridania log záznamu.
        /// </summary>
        public DateTime LogTime { get; set; }
        /// <summary>
        /// Príznak ktorý určuje či sa log záznam vymaže, napr. pri hromadnom vymazaní starších logov.
        /// </summary>
        public bool DontDelete { get; set; }
        /// <summary>
        /// Hlavná správa logu.
        /// </summary>
        public string Text { get; set; }
        /// <summary>
        /// Popis ku hlavnej správe, napr. popis chyby.
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Zdroj odkial bol log zapísaný, napr. názov classu.
        /// </summary>
        public string Source { get; set; }

        #endregion

        #region --- DATA RECORD ---

        public override DBClient DBClient => _dbClient;
        public const string TABLE_NAME = "logs";
        public override string TableName => TABLE_NAME;

        public override void SetRecordValues(DBReader dbReader, List<string> tags)
        {
            base.SetRecordValues(dbReader, tags);
            this.LogType = (LogTypes)dbReader.GetInt32("logType");
            this.LogTime = dbReader.GetDateTime("logTime");
            this.DontDelete = dbReader.GetBool("dontDelete");
            this.Text = dbReader.GetString("text");
            this.Description = dbReader.GetString("description");
            this.Source = dbReader.GetString("source");
        }
        public override string SqlInsert(Data.Management.SqlConvert q, List<string> tags)
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendFormat("INSERT INTO {0} (", this.TableName);
            sql.Append("logType, logTime, dontDelete, text, description, source) VALUES (");
            sql.AppendFormat("{0}, ", (Int32)this.LogType);
            sql.AppendFormat("{0}, ", q.DTime(this.LogTime));
            sql.AppendFormat("{0}, ", q.Innt32(this.DontDelete));
            sql.AppendFormat("{0}, ", q.Str(this.Text));
            sql.AppendFormat("{0}, ", q.Str(this.Description));
            sql.AppendFormat("{0})", q.Str(this.Source));
            return sql.ToString();
        }
        public override string SqlUpdate(Data.Management.SqlConvert q, List<string> tags)
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendFormat("UPDATE {0} SET ", this.TableName);
            sql.AppendFormat("logType = {0}, ", (Int32)this.LogType);
            sql.AppendFormat("logTime = {0}, ", q.DTime(this.LogTime));
            sql.AppendFormat("dontDelete = {0}, ", q.Innt32(this.DontDelete));
            sql.AppendFormat("text = {0}, ", q.Str(this.Text));
            sql.AppendFormat("description = {0}, ", q.Str(this.Description));
            sql.AppendFormat("Source = {0})", q.Str(this.Source));
            return sql.ToString();
        }

        #endregion

        public LogRecord(DBClient dbClient) 
        {
            _dbClient = dbClient;
            this.AddToListType = AddToListTypes.InsertFirst;
        }


    }
}
