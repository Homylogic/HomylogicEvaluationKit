/* HOMYLOGIC BUFFER X LIST
 * 
 * Obsahuje zoznam údajov pre odoslanie, napr. do zariadenia.
 * 
 * 
 */
using System;
using System.Text;
using System.Linq;
using X.Data;
using X.Data.Factory;
using System.Threading;
using System.Collections.Generic;

namespace X.Homylogic.Models.Objects.Buffers
{
    public sealed class OutputBufferXList : BufferXList
    {
        const string TITLE = "Output buffers";
        Thread _threadProcessing;
        bool _isStopped;

        #region --- DATA LIST --- 

        public override DataRecord GetInitializedDataRecord(DBReader dbReader = null) 
        {
            return new Buffers.OutputBufferX() { ParentDataList = this };
        }

        #endregion

        public OutputBufferXList() { base.BufferType = BufferX.BufferTypes.Output; }

        /// <summary>
        /// Spustí spracovávanie položiek buffera, napr. zapisovanie údajov do zariadenia.
        /// </summary>
        public void Start() 
        {
            _isStopped = false;
            if (_threadProcessing == null || !_threadProcessing.IsAlive) _threadProcessing = new Thread(ThreadProcessing) { Name = this.GetType().Name, IsBackground = true };
            _threadProcessing.Start();
        }
        /// <summary>
        /// Zastaví spracovávanie položiek.
        /// </summary>
        public void Stop() 
        {
            _isStopped = true;
        }
        /// <summary>
        /// Vlákno ktoré vykonáva spracovávanie položiek.
        /// </summary>
        [MTAThread]
        void ThreadProcessing() 
        {
g_start:
            try
            {
                while (!_isStopped)
                {
                    // Nájdi všetky ďalšie položky pre spracovanie.
                    List<Int64> bufferItemsID = new List<Int64>();
                    try
                    {
                        for (int i = 0; i < this.List.Count; i++)
                        {
                            OutputBufferX bufferItem = (OutputBufferX)this.List[i];
                            if (!bufferItem.IsProcessed && bufferItem.ProcessTime <= DateTime.Now)
                                bufferItemsID.Add(bufferItem.ID);
                        }
                    }
                    catch (Exception ex)
                    {
                        Body.Environment.Logs.Error($"Problem reading buffer items for next processing.", ex, TITLE);
                    }
                    if (_isStopped) goto g_exit;

                    // Vykonaj spracovanie všetkých položiek, ktoré ešte neboli spracované.
                    foreach (Int64 bufferItemID in bufferItemsID) 
                    {
                        try
                        {
                            OutputBufferX bufferItem = (OutputBufferX)Body.Runtime.OutputBuffers.FindDataRecord(bufferItemID);
                            if (bufferItem != null) 
                            {
                                bufferItem.Process();
                            }
                            // Malá pauza, aby posielanie údajov neprebiahalo príliš rýchlo, aby boli odosielané packety postupne.
                            Thread.Sleep(100);
                        }
                        catch (Exception ex)
                        {
                            Body.Environment.Logs.Error("Can't process output buffer item.", ex, TITLE);
                            throw;
                        }
                    }
                    Thread.Sleep(500);

                    // Vymazanie spracovaných záznamov ktorých je viac ako určitý počet.
                    if (_list.Count > 100)
                    {
                        try
                        {
                            for (int i = 100; i < this.List.Count; i++)
                            {
                                OutputBufferX eBufferItem = (OutputBufferX)this.List[i];
                                if (eBufferItem.IsProcessed) 
                                    eBufferItem.Delete();
                            }
                        }
                        catch (Exception ex)
                        {
                            Body.Environment.Logs.Error($"Problem deleting old processed output buffer items.", ex, TITLE);
                        }
                    }
                    Thread.Sleep(1000);
                }
            }
            catch (Exception)
            {
                Thread.Sleep(5000);
                if (_isStopped) goto g_exit;
                goto g_start;
            }
g_exit:;
        }
        /// <summary>
        /// Aktualizuje relačný názov cieľového zariadenia, napr. po zmene názvu zariadenia.
        /// </summary>
        public void RelationDeviceUpdateNames(DeviceX device)
        {
            // Aktualizovanie údajov v databáze.
            string destination = DeviceXList.GetRelationDeviceName(device);
            string sql = $"UPDATE {OutputBufferX.TABLE_NAME} SET name = '{destination}' WHERE deviceID = {device.ID}";
            device.DBClient.Open();
            device.DBClient.ExecuteNonQuery(sql);

            // Aktualizovanie údajov v načítanom zozname List.
            try
            {
                for (int i = 0; i < this.List.Count; i++)
                {
                    OutputBufferX bufferItem = (OutputBufferX)this.List[i];
                    if (bufferItem.DeviceID == device.ID)
                        bufferItem.NameSet(destination);
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

            // Aktualizovanie údajov v načítanom zozname List.
            try
            {
                for (int i = this.List.Count - 1; i >= 0; i--)
                {
                    OutputBufferX bufferItem = (OutputBufferX)this.List[i];
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
