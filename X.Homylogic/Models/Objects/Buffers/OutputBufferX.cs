/* HOMYLOGIC OUTPUT BUFFER X
 * 
 * Obsahuje údaje ktoré je potrebné odoslať napr. do zariadenia.
 * 
 * 
 */
using System;
using System.Collections.Generic;
using System.Text;
using X.Basic;
using X.Data;

namespace X.Homylogic.Models.Objects.Buffers
{
    public sealed class OutputBufferX : BufferX
    {
        const string TITLE = "Output buffer";

        #region --- DATA PROPERTIES ---

        public bool IsProcessed { get; private set; } = false;

        #endregion

        #region --- DATA RECORD ---

        public const string TABLE_NAME = "buffersOutput";
        public override void SetRecordValues(DBReader dbReader, List<string> tags)
        {
            base.SetRecordValues(dbReader,tags );
            this.IsProcessed = dbReader.GetBool("isProcessed");
        }
        public override string SqlInsert(Data.Management.SqlConvert q, List<string> tags)
        {
            string fields = "isProcessed";
            StringBuilder values = new StringBuilder();
            values.AppendFormat("{0}", q.Innt32(this.IsProcessed));
            return string.Format(base.SqlInsert(q, tags), fields, values);
        }
        public override string SqlUpdate(Data.Management.SqlConvert q, List<string> tags)
        {
            StringBuilder values = new StringBuilder();
            values.AppendFormat("isProcessed = {0}", q.Innt32(this.IsProcessed));
            return string.Format(base.SqlUpdate(q, tags), values);
        }
        public override XException Validate()
        {
            if (this.DeviceID <= 0) return new XException("Invalid device.", 10001);
            if (string.IsNullOrEmpty(this.Data)) return new XException("Data is required.", 10002);
            return new XException(); // Vráti result OK.
        }
        public override void Save()
        {
            // Nastaviť relačný názov zaridenia podľa zadaného identifikátor.
            this.Name = DeviceXList.GetRelationDeviceName(this.DeviceID);
            base.Save();
        }

        #endregion

        public OutputBufferX() { base.BufferType = BufferTypes.Output; this.AddToListType = AddToListTypes.InsertFirst; }

        /// <summary>
        /// Vykoná spracovanie položky podľa nastaveních údajov, napr. odošle údaje do zariadenia.
        /// </summary>
        public void Process() 
        {
            bool isProcessed = false;

            // Odoslanie údajov do zariadenia.                            
            if (this.DeviceID > 0)
            {
                DeviceX device = (DeviceX)Body.Runtime.Devices.FindDataRecord(this.DeviceID);
                if (device.CanWrite) 
                { 
                    device.Write(this.Data);
                    isProcessed = true;
                }
            }

            // Zmeň stav položky na spracovaná.
            if (isProcessed) 
            {
                this.IsProcessed = true;
                this.Save();
            }

        }


    }


}
