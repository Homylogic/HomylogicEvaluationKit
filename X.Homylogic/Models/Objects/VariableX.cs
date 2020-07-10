/* HOMYLOGIC VARIABLE X
 * 
 * Contains properties and methods for all variables which inherit from this class.
 * Variable values are stored only in memory (not in database) as C# string (default value is null, when not set).
 * Null is special variable value and determines value as not set (not usable in conditions).
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
    public class VariableX : Factory.ObjectX
    {
        /// <summary>
        /// Contains in memory stored variable value as string.
        /// </summary>
        public string Value { get; set; } = null;

        #region --- DATA PROPERTIES ---

        public enum VariableTypes : Int32
        {
            Unknown = 0,
            [Description("Text")]
            Text = 1,
            [Description("Integer number")]
            Integer = 5,
            [Description("Decimal number")]
            Decimal = 6,
            /*
             * DATE, TIME AND DATE-AND-TIME NOT SUPPORTED YET - 11,12,13
             */
            [Description("Yes or No")]
            YesNo = 21
        }
        /// <summary>
        /// Defines variable value type.
        /// </summary>
        public VariableTypes VariableType { get; protected set; } = VariableTypes.Unknown;
        /// <summary>
        /// Default value which is stored in database as string.
        /// </summary>
        public string DefaultValue { get; set; } = null;

        #endregion

        #region --- DATA RECORD ---

        public const string TABLE_NAME = "variables";
        public override string TableName => TABLE_NAME;

        public override void SetRecordValues(DBReader dbReader, List<string> tags)
        {
            base.SetRecordValues(dbReader, tags);
            this.VariableType = (VariableTypes)dbReader.GetInt32("variableType");
            this.DefaultValue = dbReader.GetString("defaultValue");

            // Sets current value according to default value when loaded from database.
            if (this.Value == null && !string.IsNullOrEmpty(this.DefaultValue))
                this.Value = this.DefaultValue;
        }
        public override string SqlInsert(Data.Management.SqlConvert q, List<string> tags)
        {
            string fields = "variableType, defaultValue";
            StringBuilder values = new StringBuilder();
            values.AppendFormat("{0}, ", (Int32)this.VariableType);
            values.AppendFormat("{0} ", q.Str(this.DefaultValue));
            return string.Format(base.SqlInsert(q, tags), fields, values);
        }
        public override string SqlUpdate(Data.Management.SqlConvert q, List<string> tags)
        {
            StringBuilder values = new StringBuilder();
            values.AppendFormat("variableType = {0}, ", (Int32)this.VariableType);
            values.AppendFormat("defaultValue = {0} ", q.Str(this.DefaultValue));
            return string.Format(base.SqlUpdate(q, tags), values);
        }
        public override XException Validate()
        {
            if (this.VariableType == VariableTypes.Unknown) return new XException("Invalid variable type.", 1001);
            return base.Validate();
        }

        #endregion

        public VariableX() : base(false) { }

        public override string ToString() 
        {
            return this.Value;
        }
    }
}
