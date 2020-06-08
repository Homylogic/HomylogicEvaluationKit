/* HOMYLOGIC BUFFER X
 * 
 * Obsahuje vlastnosti a metódy spoločné pre všetky buffre (input/output). 
 * 
 * 
 * 
 */
using System;
using System.Collections.Generic;
using System.Text;
using X.Data;

namespace X.Homylogic.Models.Objects
{
    public class BufferX : Factory.ObjectX
    {

        #region --- DATA PROPERTIES ---

        public enum BufferTypes : Int32 { Output = 1, Input = 2 }
        /// <summary>
        /// Typ zásobníka, či sa používa pre čítanie alebo zapisovanie do/zo zariadenia.
        /// </summary>
        public BufferTypes BufferType { get; protected set; }
        /// <summary>
        /// Jedinečný identifikátor zariadenia z ktorého pochádzajú údaje zásobníka alebo do kotrého sa majú zapísať údaje zo zásobníka.
        /// </summary>
        public Int64 DeviceID { get; set; }
        /// <summary>
        /// Dátum a čas kedy sa má spracovať alebo bola spracovaná položka zásobníka, napr. čas prijatia údajov.
        /// </summary>
        public DateTime ProcessTime { get; set; }
        /// <summary>
        /// Prijaté údaje (napr. zo zariadenia) alebo údaje pre odoslanie (napr. do zariadenia).
        /// </summary>
        public string Data { get; set; }

        #endregion

        #region --- DATA RECORD ---

        public override string TableName
        {
            get
            {
                if (this.BufferType == BufferTypes.Input) return Buffers.InputBufferX.TABLE_NAME;
                if (this.BufferType == BufferTypes.Output) return Buffers.OutputBufferX.TABLE_NAME;
                throw new InvalidOperationException("Buffer type is not valid.");
            }
        }
        public override void SetRecordValues(DBReader dbReader, List<string> tags)
        {
            base.SetRecordValues(dbReader, tags);
            this.DeviceID = dbReader.GetInt64("deviceID");
            this.ProcessTime = dbReader.GetDateTime("processTime");
            this.Data = dbReader.GetString("data");
        }
        public override string SqlInsert(Data.Management.SqlConvert q, List<string> tags)
        {
            string fields = "deviceID, processTime, data, {0}";
            StringBuilder values = new StringBuilder();
            values.AppendFormat("{0}, ", (Int64)this.DeviceID);
            values.AppendFormat("{0}, ", q.DTime(this.ProcessTime));
            values.AppendFormat("{0}, ", q.Str(this.Data));
            values.Append(@"{1}");
            return string.Format(base.SqlInsert(q, tags), fields, values);
        }
        public override string SqlUpdate(Data.Management.SqlConvert q, List<string> tags)
        {
            StringBuilder values = new StringBuilder();
            values.AppendFormat("deviceID = {0}, ", (Int64)this.DeviceID);
            values.AppendFormat("processTime = {0}, ", q.DTime(this.ProcessTime));
            values.AppendFormat("data = {0}, ", q.Str(this.Data));
            values.Append(@"{0}");
            return string.Format(base.SqlUpdate(q, tags), values);
        }

        #endregion

        public BufferX() : base(false) {}
    }
}
