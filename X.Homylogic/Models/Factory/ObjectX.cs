/* HOMYLOGIC OBJECT X
 * 
 * Obsahuje vlastnosti a metódy spoločné pre všetky objekty.
 * 
 * 
 * 
 */
using System;
using System.Collections.Generic;
using System.Text;
using X.Basic;
using X.Data;
using X.Homylogic.Models.Schedule;

namespace X.Homylogic.Models.Factory
{
    public abstract class ObjectX : X.Data.Factory.DataRecord
    {
        private string NameOriginal;
        /// <summary>
        /// Umožňuje detekovať zmenu názvu počas uloženia záznamu.
        /// </summary>
        public bool IsNameChanged => NameOriginal != Name;
        public void NameSet(string name) { this.Name = name; this.NameOriginal = name; }
        /// <summary>
        /// Umožňuje vypnúť logovanie.
        /// Používa len pre nastavenie z vyšších objektov v dedení.
        /// </summary>
        internal bool WriteToLogs { get; set; } = true;
        /// <summary>
        /// Plánovač uloh, umožňuje vykonanie akcií podľa zadaného času.
        /// </summary>
        public ScheduleList Scheduler { get; private set; }

        #region --- DATA PROPERTIES ---

        /// <summary>
        /// Názov objektu.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Nejaká poznámka.
        /// </summary>
        public string Notice { get; set; }
        /// <summary>
        /// Či je objekt aktívny alebo zakázaný.
        /// </summary>
        public bool Disabled { get; set; }
        /// <summary>
        /// Či sa má objekt zobrazovať na domovskej obrazovke.
        /// </summary>
        public bool ShowOnHome { get; set; }
        /// <summary>
        /// Vráti nastavenia objektu podľa najvyššieho typ v dední.
        /// </summary>
        public virtual string Settings => string.Empty;

        #endregion

        #region --- DATA RECORD ---

        public override DBClient DBClient => Body.Database.DBClient;
        public override string TableName => throw new NotImplementedException();

        public override void SetRecordValues(DBReader dbReader, List<string> tags)
        {
            base.SetRecordValues(dbReader, tags);
            this.NameSet(dbReader.GetString("name"));
            this.Notice = dbReader.GetString("notice");
            this.Disabled = dbReader.GetBool("disabled");
            this.ShowOnHome = dbReader.GetBool("showOnHome");
            if (this.Scheduler != null)
                this.Scheduler.OwnerObjectID = _id;
        }
        public override string SqlInsert(Data.Management.SqlConvert q, List<string> tags)
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendFormat("INSERT INTO {0} (", this.TableName);
            sql.AppendFormat("name, notice, disabled, showOnHome, {0}) VALUES (", @"{0}");
            sql.AppendFormat("{0}, ", q.Str(this.Name));
            sql.AppendFormat("{0}, ", q.Str(this.Notice));
            sql.AppendFormat("{0}, ", q.Innt32(this.Disabled));
            sql.AppendFormat("{0}, ", q.Innt32(this.ShowOnHome));
            sql.AppendFormat("{0})", @"{1}");
            return sql.ToString();
        }
        public override string SqlUpdate(Data.Management.SqlConvert q, List<string> tags)
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendFormat("UPDATE {0} SET ", this.TableName);
            sql.AppendFormat("name = {0}, ", q.Str(this.Name));
            sql.AppendFormat("notice = {0}, ", q.Str(this.Notice));
            sql.AppendFormat("disabled = {0}, ", q.Innt32(this.Disabled));
            sql.AppendFormat("showOnHome = {0}, ", q.Innt32(this.ShowOnHome));
            sql.Append(@"{0}");
            return sql.ToString();
        }
        public override XException Validate()
        {
            if (string.IsNullOrEmpty(this.Name)) return new XException("Name is required.", 101);
            return base.Validate();
        }
        public override void Load(long id)
        {
            base.Load(id);
            if (this.Scheduler != null)
                this.Scheduler.OwnerObjectID = _id;
        }
        public override void Save()
        {
            base.Save();
            this.NameOriginal = this.Name;
            if (this.Scheduler != null)
                this.Scheduler.OwnerObjectID = _id;
        }
        public override void Delete(long id)
        {
            base.Delete(id);
            if (this.Scheduler != null)
                this.Scheduler.DeleteAll();
        }

        #endregion

        public ObjectX(bool canSchedule) 
        {
            if (canSchedule)
                this.Scheduler = new ScheduleList(this.DBClient);
        }
    }
}
