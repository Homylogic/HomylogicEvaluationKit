/* HOMYLOGIC TRIGGER X
 * 
 * Obsahuje vlastnosti a metódy spoločné pre všetky spúšťače. 
 * 
 * 
 * 
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using X.Basic;
using X.Data;

namespace X.Homylogic.Models.Objects
{
    public class TriggerX : Factory.ObjectX
    {
        bool _isStarted;
        /// <summary>
        /// Či je trigger spustený.
        /// </summary>
        public bool IsStarted
        {
            get { return _isStarted; }
            protected set 
            {
                _isStarted = value;
                this.OnPropertyChanged("IsStarted");
            }    
        }

        #region --- DATA PROPERTIES ---

        public enum TriggerTypes : Int32
        {
            Unknown = 0,
            [Description("Device")]
            Device = 1,
            [Description("Output buffer")]
            OutputBuffer = 2
        }
        /// <summary>
        /// Určuje typ zariadenia.
        /// </summary>
        public TriggerTypes TriggerType { get; protected set; } = TriggerTypes.Unknown;
        /// <summary>
        /// Vráti číslo obrázka podľa typu a akcie triggera.
        /// </summary>
        public virtual int ImageNumber => 0;

        #endregion

        #region --- DATA RECORD ---

        public const string TABLE_NAME = "triggers";
        public override string TableName => TABLE_NAME;

        public override void SetRecordValues(DBReader dbReader, List<string> tags)
        {
            base.SetRecordValues(dbReader, tags);
            this.TriggerType = (TriggerTypes)dbReader.GetInt32("triggerType");
        }
        public override string SqlInsert(Data.Management.SqlConvert q, List<string> tags)
        {
            string fields = "triggerType, {0}";
            StringBuilder values = new StringBuilder();
            values.AppendFormat("{0}, ", (Int32)this.TriggerType);
            values.Append(@"{1}");
            return string.Format(base.SqlInsert(q, tags), fields, values);
        }
        public override string SqlUpdate(Data.Management.SqlConvert q, List<string> tags)
        {
            StringBuilder values = new StringBuilder();
            values.AppendFormat("triggerType = {0}, ", (Int32)this.TriggerType);
            values.Append(@"{0}");
            return string.Format(base.SqlUpdate(q, tags), values);
        }
        public override XException Validate()
        {
            if (this.TriggerType == TriggerTypes.Unknown) return new XException("Invalid trigger type.", 1001);
            return base.Validate();
        }
        public override void Save()
        {
            // Zatvoriť spustený trigger pred uložením nových zmien záznamu.
            this.Stop();
            base.Save();
        }
        public override void Delete(Int64 id)
        {
            // Zatvoriť spustený trigger pred vymazaním záznamu.
            this.Stop();
            base.Delete(id);
        }

        #endregion

        public TriggerX() : base(true) {}

        // Metódy typu abstract, určené hlavne pre vyšší objekt v dedení. Avšak aby sa dala vytvoriť nová inštancia základného objektu triggra TriggerX tak sú metódy typu virtual.
        public virtual void Start() 
        {
            if (this.Disabled) throw new InvalidOperationException("Can't start disabled trigger.");
            this.IsStarted = true; 
        }
        public virtual void Stop() { this.IsStarted = false; }

    }
}
