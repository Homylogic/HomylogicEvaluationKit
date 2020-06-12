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
using X.Data.Management;

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
            SqlStringBuilder sql = new SqlStringBuilder(dbClient.ClientType);
            sql.CreateTable(ScheduleRecord.TABLE_NAME);
            sql.Append("(");
            sql.AddPrimaryKey();
            sql.Int64("objectID");
            sql.DateTime("scheduleTime");
            sql.Int01("dayMonday");
            sql.Int01("dayTuesday");
            sql.Int01("dayWednesday");
            sql.Int01("dayThursday");
            sql.Int01("dayFriday");
            sql.Int01("daySaturday");
            sql.Int01("daySunday");
            sql.Int32("action");
            sql.Text("methodName");
            for (int i = 1; i < 12; i++)
            {
                sql.Int32($"setInt{string.Format("{0:D2}", i)}");
            }
            for (int i = 1; i < 12; i++)
            {
                sql.Float($"setDec{string.Format("{0:D2}", i)}");
            }
            for (int i = 1; i < 12; i++)
            {
                sql.Text($"setStr{string.Format("{0:D2}", i)}");
            }
            sql.Text("actionSettings", appendComma:false);
            sql.Append(")");
            dbClient.ExecuteNonQuery(sql.ToString());
            sql.Clear();
            // ! Sqlite require unique index names per database !
            sql.CreateIndex("objectID_scheduler", "objectID", ScheduleRecord.TABLE_NAME);
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
