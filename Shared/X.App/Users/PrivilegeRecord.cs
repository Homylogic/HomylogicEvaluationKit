/* USER PRIVILEGE DATA RECORD
 * 
 * Contains user privilege record.
 * 
 * 
 */
using System;
using System.Collections.Generic;
using System.Text;
using X.Data;
using X.Data.Factory;

namespace X.App.Users
{
    public sealed class PrivilegeRecord : DataRecord
    {
        readonly DBClient _dbClient;

        #region --- DATA PROPERTIES ---

        /// <summary>
        /// User owner ID (user which owns this privilege).
        /// </summary>
        public Int64 UserID { get; set; }
        /// <summary>
        /// Unique privilege key (identifies privilege by key).
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// Privilege permission value.
        /// </summary>
        public Int32 Permission { get; set; }

        #endregion

        #region --- DATA RECORD ---

        public override DBClient DBClient => _dbClient;
        public const string TABLE_NAME = "usersPrivileges";
        public override string TableName => TABLE_NAME;

        public override void SetRecordValues(DBReader dbReader, List<string> tags)
        {
            base.SetRecordValues(dbReader, tags);
            this.UserID = dbReader.GetInt64("userID");
            this.Key = dbReader.GetString("keey");
            this.Permission = dbReader.GetInt32("permission");
        }
        public override string SqlInsert(Data.Management.SqlConvert q, List<string> tags)
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendFormat("INSERT INTO {0} (", this.TableName);
            sql.Append("userID, keey, permission) VALUES (");
            sql.AppendFormat("{0}, ", this.UserID);
            sql.AppendFormat("{0}, ", q.Str(this.Key));
            sql.AppendFormat("{0})", (Int32)this.Permission);
            return sql.ToString();
        }
        public override string SqlUpdate(Data.Management.SqlConvert q, List<string> tags)
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendFormat("UPDATE {0} SET ", this.TableName);
            sql.AppendFormat("userID = {0}, ", this.UserID);
            sql.AppendFormat("keey = {0}, ", q.Str(this.Key));
            sql.AppendFormat("permission = {0}", this.Permission);
            return sql.ToString();
        }

        #endregion

        public PrivilegeRecord(DBClient dbClient) { _dbClient = dbClient; }

    }
}
