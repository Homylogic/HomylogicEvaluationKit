/* HOMYLOGIC OUTPUT BUFFER TRIGGER X
 * 
 * Umožňuje prácu so zásobníko pre odosielanie údajov, napr. zapisovani údajov pre odosielanie do zariadení.
 * 
 * 
 */
using System;
using System.Text;
using System.IO.Ports;
using System.Threading;
using X.Basic;
using X.Data;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using X.Homylogic.Models.Objects.Buffers;
using System.Collections.Generic;

namespace X.Homylogic.Models.Objects.Triggers
{
    public sealed class OutputBufferTriggerX : TriggerX
    {
        const string TITLE = "Output buffer trigger";

        #region --- DATA PROPERTIES ---

        public Int64 DeviceID { get; set; } = 0;
        public enum ActionTypes : Int32 
        { 
            [Description("Write")]
            Write = 1,
        }
        public ActionTypes ActionType { get; set; } = ActionTypes.Write;
        public string Data { get; set; }
        public override string Settings 
        {
            get 
            {
                return (this.ActionType switch
                {
                    ActionTypes.Write => "Write to output buffer",
                    _ => null
                });
            }
        }
        public override int ImageNumber
        {
            get
            {
                return (this.ActionType switch
                {
                    ActionTypes.Write => 201,
                    _ => base.ImageNumber
                });
            }
        }

        #endregion

        #region --- DATA RECORD ---

        public override void SetRecordValues(DBReader dbReader, List<string> tags)
        {
            base.SetRecordValues(dbReader, tags);
            this.DeviceID = dbReader.GetInt64("deviceID");
            this.ActionType = (ActionTypes)dbReader.GetInt32("setInt01");
            this.Data = dbReader.GetString("setStr01");
            // Poznámka sa používa len pre čítanie ako info v UI DataGride.
            this.Notice = $"Data: {this.Data}";
        }
        public override string SqlInsert(Data.Management.SqlConvert q, List<string> tags)
        {
            string fields = "deviceID, setInt01, setStr01";
            StringBuilder values = new StringBuilder();
            values.AppendFormat("{0}, ", (Int64)this.DeviceID);
            values.AppendFormat("{0}, ", (Int32)this.ActionType);
            values.AppendFormat("{0}", q.Str(this.Data));
            return string.Format(base.SqlInsert(q, tags), fields, values);
        }
        public override string SqlUpdate(Data.Management.SqlConvert q, List<string> tags)
        {
            StringBuilder values = new StringBuilder();
            values.AppendFormat("deviceID = {0}, ", (Int32)this.DeviceID); ;
            values.AppendFormat("setInt01 = {0}, ", (Int32)this.ActionType);
            values.AppendFormat("setStr01 = {0}", q.Str(this.Data));
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
            // Poznámka sa používa len pre čítanie ako info v UI DataGride.
            this.Notice = $"Data: {this.Data}";
            // Nastaviť relačný názov zaridenia podľa zadaného identifikátor.
            this.Name = DeviceXList.GetRelationDeviceName(this.DeviceID);
            base.Save();
        }

        #endregion

        public OutputBufferTriggerX() { base.TriggerType = TriggerTypes.OutputBuffer; }

        /// <summary>
        /// Otvorí alebo zatvorí zariadenie (podľa nastavení akcie spúšťača).
        /// </summary>
        public override void Start()
        {
            if (this.IsStarted) return;
            base.Start(); // Skontrolovať stav zariadenia a nastaviť príznak IsStarted.

            DeviceX device = (DeviceX)Body.Runtime.Devices.FindDataRecord(this.DeviceID);

            if (device == null)
            {
                Body.Environment.Logs.Error($"Trigger can't found device.", new Exception($"DeviceID:{this.DeviceID}") , $"{TITLE} : {this.Name}");
            }
            else {
                switch (this.ActionType) 
                {
                    case ActionTypes.Write:
                        try
                        {
                            OutputBufferX outputBuffer = (OutputBufferX)Body.Runtime.OutputBuffers.GetInitializedDataRecord();
                            outputBuffer.DeviceID = device.ID;
                            outputBuffer.ProcessTime = DateTime.Now;
                            outputBuffer.Data = this.Data;
                            outputBuffer.Save();
                        }
                        catch (Exception ex)
                        {
                            Body.Environment.Logs.Error($"Trigger can't open device.", ex, $"{TITLE} : {this.Name}");
                        }
                        break;
                }
            }

            base.Stop(); // Nastaviť príznak IsStarted.
        }


    }


}
