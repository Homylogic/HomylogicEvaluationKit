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
                    foreach (OutputBufferX eBufferItem in _list.ToArray())
                    {
                        if (!eBufferItem.IsProcessed && eBufferItem.ProcessTime <= DateTime.Now)
                            bufferItemsID.Add(eBufferItem.ID);
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
                                if (this.SynchronizationContext != null)
                                    this.SynchronizationContext.Send(o => bufferItem.Process(), null);
                                else
                                    lock (Body.Database.SyncObject)
                                        bufferItem.Process();
                            }
                            // Malá pauza, aby posielanie údajov neprebiahalo príliš rýchlo, aby boli odosielané packety postupne.
                            Thread.Sleep(100);
                        }
                        catch (Exception ex)
                        {
                            Body.Environment.Logs.Error("Can't process output buffer item.", ex, this.GetType().Name);
                            throw;
                        }
                    }
                    Thread.Sleep(500);

                    // Vymazanie spracovaných záznamov ktorých je viac ako určitý počet.
                    if (_list.Count > 100)
                    {
                        int index = 0;
                        foreach (OutputBufferX eBufferItem in _list.ToArray())
                        {
                            if (eBufferItem.IsProcessed && index > 100) 
                            {
                                if (this.SynchronizationContext != null)
                                    this.SynchronizationContext.Send(o => eBufferItem.Delete(), null);
                                else
                                    lock (Body.Database.SyncObject)
                                        eBufferItem.Delete();
                            }
                            index++;
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
            device.DBClient.Close();

            // Aktualizovanie údajov v načítanom zozname List.
            foreach (OutputBufferX bufferItem in _list.ToArray())
            {
                if (bufferItem.DeviceID == device.ID)
                    bufferItem.NameSet(destination);
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
            foreach (OutputBufferX bufferItem in _list.ToArray())
            {
                if (bufferItem.DeviceID == deviceID)
                {
                    if (this.SynchronizationContext != null)
                        this.SynchronizationContext.Send(o => _list.Remove(bufferItem), null);
                    else
                        _list.Remove(bufferItem);
                }
            }
        }

    }
}
