/* HOMYLOGIC DEVICE X LIST
 * 
 * Obsahuje zoznam načítaných spúšťačov.
 * 
 * 
 */
using System;
using System.Linq;
using System.Text;
using X.Data;
using X.Data.Factory;
using X.Data.Management;
using X.Homylogic.Models.Objects.Triggers;

namespace X.Homylogic.Models.Objects
{
    public sealed class TriggerXList : Factory.ObjectXList
    {

        #region --- DATA LIST ---

        public static void CreateTable(DBClient dbClient)
        {
            SqlStringBuilder sql = new SqlStringBuilder(dbClient.ClientType);
            sql.CreateTable(TriggerX.TABLE_NAME);
            sql.Append("(");
            sql.AddPrimaryKey();
            sql.Chars("name");
            sql.Text("notice");
            sql.Int01("showOnHome");
            sql.Int32("triggerType");
            sql.Int64("deviceID");
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
            sql.Int01("disabled", appendComma:false);
            sql.Append(")");
            dbClient.ExecuteNonQuery(sql.ToString());
            sql.Clear();
            // ! Sqlite require unique index names per database !
            sql.CreateIndex("deviceID_trigger", "deviceID", TriggerX.TABLE_NAME);
            dbClient.ExecuteNonQuery(sql.ToString());
        }
        public override DataRecord GetInitializedDataRecord(DBReader dbReader = null) 
        {
            if (dbReader != null)
            {
                return GetInitializedTrigger((TriggerX.TriggerTypes)dbReader.GetInt32("triggerType"));
            }
            return new TriggerX() { ParentDataList = this }; 
        }
        public TriggerX GetInitializedTrigger(TriggerX.TriggerTypes triggerType) 
        {
            if (triggerType == TriggerX.TriggerTypes.Device) return new Triggers.DeviceTriggerX() { ParentDataList = this };
            if (triggerType == TriggerX.TriggerTypes.OutputBuffer) return new Triggers.OutputBufferTriggerX() { ParentDataList = this };
            return new TriggerX() { ParentDataList = this };
        }
        public void LoadData()
        {
            base.LoadData("triggerType, name");
        }

        #endregion

        /// <summary>
        /// Aktualizuje relačný názov zariadenia, napr. po zmene názvu zariadenia.
        /// </summary>
        public void RelationDeviceUpdateNames(DeviceX device)
        {
            // Aktualizovanie údajov v databáze.
            string target = DeviceXList.GetRelationDeviceName(device);
            string sql = $"UPDATE {TriggerX.TABLE_NAME} SET name = '{target}' WHERE deviceID = {device.ID}";
            device.DBClient.Open();
            device.DBClient.ExecuteNonQuery(sql);

            // Aktualizovanie údajov v načítanom zozname List.
            try
            {
                for (int i = 0; i < this.List.Count; i++)
                {
                    DeviceTriggerX deviceTrigger = (DeviceTriggerX)this.List[i];
                    if (deviceTrigger.DeviceID == device.ID)
                        deviceTrigger.NameSet(target);
                }
            }
            catch (Exception ex)
            {
                Body.Environment.Logs.Error($"Problem updating device relation names.", ex, this.GetType().Name);
            }
        }
        /// <summary>
        /// Vymaže relačné záznamy, ktoré odkazujú na zadané zariadenie.
        /// </summary>
        public void RelationDeviceDelete(Int64 deviceID, DBClient dbClient)
        {
            // Aktualizovanie údajov v databáze.
            string sql = $"DELETE FROM {TriggerX.TABLE_NAME} WHERE deviceID = {deviceID}";
            dbClient.Open();
            dbClient.ExecuteNonQuery(sql);

            // Aktualizovanie údajov v načítanom zozname List.
            try
            {
                for (int i = this.List.Count - 1; i >= 0; i--)
                {
                    TriggerX triggerItem = (TriggerX)this.List[i];
                    if (triggerItem.TriggerType == TriggerX.TriggerTypes.Device)
                    {
                        DeviceTriggerX deviceTrigger = (DeviceTriggerX)triggerItem;
                        _list.Remove(deviceTrigger);
                    }
                }
            }
            catch (Exception ex)
            {
                Body.Environment.Logs.Error($"Problem removing device relations.", ex, this.GetType().Name);
            }
        }

    }
}
