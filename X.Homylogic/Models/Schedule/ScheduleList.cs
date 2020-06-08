/* SCHEDULE DATA LIST
 * 
 * Obsahuje zoznam plánovaných položiek per vykonanie v určitom čase.
 * Používa sa v objektoch ObjectX, takže každý objekt podporuje plánované vykonanie.
 * 
 * 
 * 
 */
using System;
using System.Text;
using X.Data;
using X.Data.Factory;

namespace X.Homylogic.Models.Schedule
{
    public sealed class ScheduleList : DataList
    {
        readonly DBClient _dbClient;
        /// <summary>
        /// Identifikátor parent objektu ObjectX, ku ktorému patrí tento zoznam položiek plánovača.
        /// </summary>
        public Int64 OwnerObjectID { get; set; } 

        #region --- DATA LIST ---

        public override DBClient DBClient => _dbClient;
        public override DataRecord GetInitializedDataRecord(DBReader dbReader = null)
        {
            return new ScheduleRecord(_dbClient, this.OwnerObjectID) { ParentDataList = this };
        }
        public static void CreateTable(DBClient dbClient)
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendFormat("CREATE TABLE {0} (", ScheduleRecord.TABLE_NAME);
            if (dbClient.ClientType == DBClient.ClientTypes.Sqlite)
                sql.Append("id INTEGER PRIMARY KEY, ");
            else
                sql.Append("id INTEGER PRIMARY KEY AUTO_INCREMENT, ");
            sql.Append("objectID INTEGER, ");
            sql.Append("scheduleTime DATETIME, ");
            sql.Append("dayMonday INTEGER, ");
            sql.Append("dayTuesday INTEGER, ");
            sql.Append("dayWednesday INTEGER, ");
            sql.Append("dayThursday INTEGER, ");
            sql.Append("dayFriday INTEGER, ");
            sql.Append("daySaturday INTEGER, ");
            sql.Append("daySunday INTEGER, ");
            sql.Append("action INTEGER, ");
            sql.Append("methodName TEXT, ");
            sql.Append("actionSettings TEXT ");
            sql.Append(")");
            dbClient.ExecuteNonQuery(sql.ToString());
            // Indexy.
            sql.Clear();
            sql.AppendFormat("CREATE INDEX idxObjectID ON {0} (objectID)", ScheduleRecord.TABLE_NAME);
            dbClient.ExecuteNonQuery(sql.ToString());
        }
        public void LoadData()
        {
            if (this.OwnerObjectID == 0) throw new InvalidOperationException("Invalid schedule list owner ID.");
            base.FilterCondition = $"objectID = {this.OwnerObjectID}";
            base.LoadData("scheduleTime DESC");
        }

        #endregion

        public ScheduleList(DBClient dbClient) {_dbClient = dbClient; }


    }
}
