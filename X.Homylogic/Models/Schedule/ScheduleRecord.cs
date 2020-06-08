/* SCHEDULE DATA RECORD
 * 
 * Obsahuje a edituje údaje databázového záznamu plánovania.
 * 
 * 
 */
using System;
using System.Text;
using X.Data.Factory;
using System.Collections.Generic;
using X.Data;

namespace X.Homylogic.Models.Schedule
{
    public sealed class ScheduleRecord : DataRecord
    {
        readonly DBClient _dbClient;

        /// <summary>
        /// Či bola už položka dnes spracovaná.
        /// </summary>
        public bool IsProcessedToday;

        #region --- DATA PROPERTIES ---

        /// <summary>
        /// Identifikátor objektu ObjectX ktorému patrí tanto objekt DataRecord.
        /// </summary>
        public Int64 ObjectID { get; private set; }
        /// <summary>
        /// Čas vykonania plánovania úlohy.
        /// </summary>
        public DateTime ScheduleTime { get; set; }
        /// <summary>
        /// Deň v ktorý sa vykoná plánovanie.
        /// </summary>
        public bool DayMonday { get; set; } = true;
        /// <summary>
        /// Deň v ktorý sa vykoná plánovanie.
        /// </summary>
        public bool DayTuesday { get; set; } = true;
        /// <summary>
        /// Deň v ktorý sa vykoná plánovanie.
        /// </summary>
        public bool DayWednesday { get; set; } = true;
        /// <summary>
        /// Deň v ktorý sa vykoná plánovanie.
        /// </summary>
        public bool DayThursday { get; set; } = true;
        /// <summary>
        /// Deň v ktorý sa vykoná plánovanie.
        /// </summary>
        public bool DayFriday { get; set; } = true;
        /// <summary>
        /// Deň v ktorý sa vykoná plánovanie.
        /// </summary>
        public bool DaySaturday { get; set; } = true;
        /// <summary>
        /// Deň v ktorý sa vykoná plánovanie.
        /// </summary>
        public bool DaySunday { get; set; } = true;
        public enum ActionTypes : Int32
        {
            CallMethod = 1
        }
        /// <summary>
        /// Určuje čo sa má vykonať.
        /// </summary>
        public ActionTypes Action { get; set; } = ActionTypes.CallMethod;
        /// <summary>
        /// Názov metódy, ktorú má plánovač vyvolať.
        /// </summary>
        public string MethodName { get; set; }
        /// <summary>
        /// Používa sa len pre zobrazenie nastavení pre užívateľa v zozname (DataGrid-e), nastavuje pri uložení záznamu.
        /// </summary>
        public string ActionSettings { get; set; }

        #endregion

        #region --- DATA RECORD ---

        public override DBClient DBClient => _dbClient;
        public const string TABLE_NAME = "schedule";
        public override string TableName => TABLE_NAME;

        public override void SetRecordValues(DBReader dbReader, List<string> tags)
        {
            base.SetRecordValues(dbReader, tags);
            this.ObjectID = dbReader.GetInt64("objectID");
            this.ScheduleTime = dbReader.GetDateTime("scheduleTime");
            this.DayMonday = dbReader.GetBool("dayMonday");
            this.DayTuesday = dbReader.GetBool("dayTuesday");
            this.DayWednesday = dbReader.GetBool("dayWednesday");
            this.DayThursday = dbReader.GetBool("dayThursday");
            this.DayFriday = dbReader.GetBool("dayFriday");
            this.DaySaturday = dbReader.GetBool("daySaturday");
            this.DaySunday = dbReader.GetBool("daySunday");
            this.Action = (ActionTypes)dbReader.GetInt32("action");
            this.MethodName = dbReader.GetString("methodName");
            this.ActionSettings = dbReader.GetString("actionSettings");
        }
        public override string SqlInsert(Data.Management.SqlConvert q, List<string> tags)
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendFormat("INSERT INTO {0} (", this.TableName);
            sql.Append("objectID, scheduleTime, dayMonday, dayTuesday, dayWednesday, dayThursday, dayFriday, daySaturday, daySunday, action, methodName, actionSettings) VALUES (");
            sql.AppendFormat("{0}, ", (Int64)this.ObjectID);
            sql.AppendFormat("{0}, ", q.DTime(this.ScheduleTime));
            sql.AppendFormat("{0}, ", q.Innt32(this.DayMonday));
            sql.AppendFormat("{0}, ", q.Innt32(this.DayTuesday));
            sql.AppendFormat("{0}, ", q.Innt32(this.DayWednesday));
            sql.AppendFormat("{0}, ", q.Innt32(this.DayThursday));
            sql.AppendFormat("{0}, ", q.Innt32(this.DayFriday));
            sql.AppendFormat("{0}, ", q.Innt32(this.DaySaturday));
            sql.AppendFormat("{0}, ", q.Innt32(this.DaySunday));
            sql.AppendFormat("{0}, ", (Int32)this.Action);
            sql.AppendFormat("{0}, ", q.Str(this.MethodName));
            sql.AppendFormat("{0})", q.Str(this.ActionSettings));
            return sql.ToString();
        }
        public override string SqlUpdate(Data.Management.SqlConvert q, List<string> tags)
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendFormat("UPDATE {0} SET ", this.TableName);
            sql.AppendFormat("objectID = {0}, ", (Int64)this.ObjectID);
            sql.AppendFormat("scheduleTime = {0}, ", q.DTime(this.ScheduleTime));
            sql.AppendFormat("dayMonday = {0}, ", q.Innt32(this.DayMonday));
            sql.AppendFormat("dayTuesday = {0}, ", q.Innt32(this.DayTuesday));
            sql.AppendFormat("dayWednesday = {0}, ", q.Innt32(this.DayWednesday));
            sql.AppendFormat("dayThursday = {0}, ", q.Innt32(this.DayThursday));
            sql.AppendFormat("dayFriday = {0}, ", q.Innt32(this.DayFriday));
            sql.AppendFormat("daySaturday = {0}, ", q.Innt32(this.DaySaturday));
            sql.AppendFormat("daySunday = {0}, ", q.Innt32(this.DaySunday));
            sql.AppendFormat("action = {0}, ", (Int32)this.Action);
            sql.AppendFormat("methodName = {0}, ", q.Str(this.MethodName));
            sql.AppendFormat("actionSettings = {0}", q.Str(this.ActionSettings));
            return sql.ToString();
        }

        #endregion

        public ScheduleRecord(DBClient dbClient, Int64 ownerObjectID) 
        { 
            _dbClient = dbClient;
            this.ObjectID = ownerObjectID;
        }

    }
}
