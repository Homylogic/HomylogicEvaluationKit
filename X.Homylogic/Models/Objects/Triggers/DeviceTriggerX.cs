/* HOMYLOGIC DEVICE TRIGGER X
 * 
 * Umožňuje spúštanie zariadení, napr otváranie a zatváranie zariadení.
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
using System.Collections.Generic;

namespace X.Homylogic.Models.Objects.Triggers
{
    public sealed class DeviceTriggerX : TriggerX
    {
        const string TITLE = "Device trigger";

        #region --- DATA PROPERTIES ---

        public Int64 DeviceID { get; set; } = 0;
        public enum ActionTypes : Int32 
        { 
            [Description("Open")]
            Open = 1,
            [Description("Close")]
            Close = 2 
        }
        public ActionTypes ActionType { get; set; } = ActionTypes.Open;
        public override string Settings => $"{ActionType} device";
        public override int ImageNumber 
        {
            get 
            {
                return (this.ActionType switch
                {
                    ActionTypes.Open => 101,
                    ActionTypes.Close => 102,
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
        }
        public override string SqlInsert(Data.Management.SqlConvert q, List<string> tags)
        {
            string fields = "deviceID, setInt01";
            StringBuilder values = new StringBuilder();
            values.AppendFormat("{0}, ", (Int64)this.DeviceID);
            values.AppendFormat("{0}", (Int32)this.ActionType);
            return string.Format(base.SqlInsert(q, tags), fields, values);
        }
        public override string SqlUpdate(Data.Management.SqlConvert q, List<string> tags)
        {
            StringBuilder values = new StringBuilder();
            values.AppendFormat("deviceID = {0}, ", (Int32)this.DeviceID); ;
            values.AppendFormat("setInt01 = {0}", (Int32)this.ActionType);
            return string.Format(base.SqlUpdate(q, tags), values);
        }
        public override XException Validate()
        {
            if (this.DeviceID <= 0) return new XException("Invalid device.", 10001);
            return new XException(); // Vráti result OK.
        }
        public override void Save()
        {
            // Nastaviť relačný názov zaridenia podľa zadaného identifikátor.
            this.Name = DeviceXList.GetRelationDeviceName(this.DeviceID);
            base.Save();
        }

        #endregion

        public DeviceTriggerX() { base.TriggerType = TriggerTypes.Device; }

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
                    case ActionTypes.Open:
                        try
                        {
                            device.Open();
                        }
                        catch (Exception ex)
                        {
                            Body.Environment.Logs.Error($"Trigger can't open device.", ex, $"{TITLE} : {this.Name}");
                        }
                        break;

                    case ActionTypes.Close:
                        try
                        {
                            device.Close();
                        }
                        catch (Exception ex)
                        {
                            Body.Environment.Logs.Error($"Trigger can't close device.", ex, $"{TITLE} : {this.Name}");
                        }
                        break;
                }
            }

            base.Stop(); // Nastaviť príznak IsStarted.
        }


    }


}
