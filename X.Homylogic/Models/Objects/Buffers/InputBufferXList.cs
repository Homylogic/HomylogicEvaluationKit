/* HOMYLOGIC INPUT BUFFER X LIST
 * 
 * Obsahuje zoznam načítaných údajov, napr. zo zariadenia.
 * 
 * 
 */
using System;
using System.Linq;
using System.Text;
using X.Data;
using X.Data.Factory;

namespace X.Homylogic.Models.Objects.Buffers
{
    public sealed class InputBufferXList : BufferXList
    {
        #region --- DATA LIST --- 

        public override DataRecord GetInitializedDataRecord(DBReader dbReader = null) 
        {
            return new Buffers.InputBufferX() { ParentDataList = this };
        }

        #endregion

        public InputBufferXList() { base.BufferType = BufferX.BufferTypes.Input; }

        /// <summary>
        /// Zapísanie prijatých údajov zo zariadenia.
        /// </summary>
        public void Enqueue(DeviceX.DataReceivedEventArgs e)
        {
            lock (Body.Database.SyncObject) 
            { 
                InputBufferX bufferItem = (InputBufferX)this.GetInitializedDataRecord();
                bufferItem.DeviceID = e.Device.ID;
                bufferItem.ProcessTime = DateTime.Now;
                bufferItem.Data = e.Data;
                bufferItem.Save();

                // Vymazanie záznamov ktorých je viac ako určitý počet.
                if (_list.Count > 500) 
                {
                    _list[^1].Delete();
                }
            }
        }
        /// <summary>
        /// Aktualizuje relačný názov zdrojového zariadenia, napr. po zmene názvu zariadenia.
        /// </summary>
        public void RelationDeviceUpdateNames(DeviceX device) 
        {
            // Aktualizovanie údajov v databáze.
            string source = DeviceXList.GetRelationDeviceName(device);
            string sql = $"UPDATE {InputBufferX.TABLE_NAME} SET name = '{source}' WHERE deviceID = {device.ID}";
            device.DBClient.Open();
            device.DBClient.ExecuteNonQuery(sql);
            device.DBClient.Close();

            // Aktualizovanie údajov v načítanom zozname List.
            try
            {
                for (int i = 0; i < this.List.Count; i++)
                {
                    InputBufferX bufferItem = (InputBufferX)this.List[i];
                    if (bufferItem.DeviceID == device.ID)
                        bufferItem.NameSet(source);
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
            string sql = $"DELETE FROM {InputBufferX.TABLE_NAME} WHERE deviceID = {deviceID}";
            dbClient.Open();
            dbClient.ExecuteNonQuery(sql);
            dbClient.Close();

            // Aktualizovanie údajov v načítanom zozname List.
            try
            {
                for (int i = this.List.Count - 1; i >= 0; i--)
                {
                    InputBufferX bufferItem = (InputBufferX)this.List[i];
                    if (bufferItem.DeviceID == deviceID)
                        _list.RemoveAt(i);
                }
            }
            catch (Exception ex)
            {
                Body.Environment.Logs.Error($"Problem removing device relations.", ex, this.GetType().Name);
            }
        }
    }
}
