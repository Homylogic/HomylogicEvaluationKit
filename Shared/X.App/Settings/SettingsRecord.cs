/* SETTIGS DATA RECORD
 * 
 * Obsahuje jeden záznam nastavení.
 * 
 */
using System;
using System.Collections.Generic;
using System.Text;
using X.Data;
using X.Data.Factory;

namespace X.App.Settings
{
    public class SettingsRecord : DataRecord
    {
        readonly DBClient _dbClient;

        #region --- DATA PROPERTIES ---

        /// <summary>
        /// Určuje hlavnú sekciu (napr. definuje určitú skupinu nsatavení).
        /// </summary>
        public string Section { get; set; }
        /// <summary>
        /// Názov nastavenia. 
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// Nejaký príznak podľa ktorého je možné nastavenia ďalej členiť (nemusí byť nastavené).
        /// </summary>
        public string Flag { get; set; }
        /// <summary>
        /// Hodnota nastavenia.
        /// </summary>
        public SettingsValue Value { get; set; }

        #endregion

        #region --- DATA RECORD ---

        public override DBClient DBClient => _dbClient;
        public const string TABLE_NAME = "settings";
        public override string TableName => TABLE_NAME;

        public override void SetRecordValues(DBReader dbReader, List<string> tags)
        {
            base.SetRecordValues(dbReader, tags);
            this.Section = dbReader.GetString("section");
            this.Key = dbReader.GetString("key");
            this.Flag = dbReader.GetString("flag");
            this.Value.SetValue(dbReader.GetString("value"));
        }
        public override string SqlInsert(Data.Management.SqlConvert q, List<string> tags)
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendFormat("INSERT INTO {0} (", this.TableName);
            sql.Append("section, key, flag, value) VALUES (");
            sql.AppendFormat("{0}, ", q.Str(this.Section));
            sql.AppendFormat("{0}, ", q.Str(this.Key));
            sql.AppendFormat("{0}, ", q.Str(this.Flag));
            sql.AppendFormat("{0})", q.Str(this.Value.Text));
            return sql.ToString();
        }
        public override string SqlUpdate(Data.Management.SqlConvert q, List<string> tags)
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendFormat("UPDATE {0} SET ", this.TableName);
            sql.AppendFormat("section = {0}, ", q.Str(this.Section));
            sql.AppendFormat("key = {0}, ", q.Str(this.Key));
            sql.AppendFormat("flag = {0}, ", q.Str(this.Flag));
            sql.AppendFormat("value = {0}", q.Str(this.Value.Text));
            return sql.ToString();
        }

        #endregion

        public SettingsRecord(DBClient dbClient) 
        { 
            _dbClient = dbClient;
            this.Value = new SettingsValue();
        }

    }
}
