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
using X.Homylogic.Models.Objects.Triggers;

namespace X.Homylogic.Models.Objects
{
    public sealed class TriggerXList : Factory.ObjectXList
    {

        #region --- DATA LIST ---

        public static void CreateTable(DBClient dbClient)
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendFormat("CREATE TABLE {0} (", TriggerX.TABLE_NAME);
            if (dbClient.ClientType == DBClient.ClientTypes.Sqlite)
                sql.Append("id INTEGER PRIMARY KEY, ");
            else
                sql.Append("id INTEGER PRIMARY KEY AUTO_INCREMENT, ");
            sql.Append("name TEXT, ");
            sql.Append("notice TEXT, ");
            sql.Append("disabled INTEGER, ");
            sql.Append("showOnHome INTEGER, ");
            sql.Append("triggerType INTEGER, ");
            sql.Append("deviceID INTEGER, ");
            sql.Append("setInt01 INTEGER, ");
            sql.Append("setInt02 INTEGER, ");
            sql.Append("setInt03 INTEGER, ");
            sql.Append("setInt04 INTEGER, ");
            sql.Append("setInt05 INTEGER, ");
            sql.Append("setInt06 INTEGER, ");
            if (dbClient.ClientType == DBClient.ClientTypes.Sqlite)
            {
                sql.Append("setDec01 INTEGER, ");
                sql.Append("setDec02 INTEGER, ");
                sql.Append("setDec03 INTEGER, ");
                sql.Append("setDec04 INTEGER, ");
                sql.Append("setDec05 INTEGER, ");
                sql.Append("setDec06 INTEGER, ");
            }
            else {
                sql.Append("setDec01 FLOAT, ");
                sql.Append("setDec02 FLOAT, ");
                sql.Append("setDec03 FLOAT, ");
                sql.Append("setDec04 FLOAT, ");
                sql.Append("setDec05 FLOAT, ");
                sql.Append("setDec06 FLOAT, ");
            }
            sql.Append("setStr01 TEXT, ");
            sql.Append("setStr02 TEXT, ");
            sql.Append("setStr03 TEXT, ");
            sql.Append("setStr04 TEXT, ");
            sql.Append("setStr05 TEXT, ");
            sql.Append("setStr06 TEXT");
            sql.Append(")");
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
