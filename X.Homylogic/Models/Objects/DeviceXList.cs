/* HOMYLOGIC DEVICE X LIST
 * 
 * Obsahuje zoznam načítaných zariadení.
 * 
 * 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using X.Data;
using X.Data.Factory;
using X.Data.Management;
using X.Homylogic.Models.Objects.Devices;

namespace X.Homylogic.Models.Objects
{
    public sealed class DeviceXList : Factory.ObjectXList
    {
        Thread _threadDataUpdate;
        bool _isStopped;

        #region --- DATA LIST ---

        public static void CreateTable(DBClient dbClient)
        {
            SqlStringBuilder sql = new SqlStringBuilder(dbClient.ClientType);
            sql.CreateTable(DeviceX.TABLE_NAME);
            sql.Append("(");
            sql.AddPrimaryKey();
            sql.Chars("name", appendComma:false); sql.UniqueNotNull();
            sql.Text("notice");
            sql.Int01("showOnHome");
            sql.Int32("deviceType");
            sql.Int01("writeToBuffer");
            sql.Int01("autoDataUpdate");
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
        }
        public override DataRecord GetInitializedDataRecord(DBReader dbReader = null) 
        {
            if (dbReader != null)
            {
                return GetInitializedDevice((DeviceX.DeviceTypes)dbReader.GetInt32("deviceType"));
            }
            return new DeviceX() { ParentDataList = this }; 
        }
        public DeviceX GetInitializedDevice(DeviceX.DeviceTypes deviceType) 
        {
            if (deviceType == DeviceX.DeviceTypes.Serial) return new SerialDeviceX() { ParentDataList = this };
            if (deviceType == DeviceX.DeviceTypes.TCPSocket) return new TCPDeviceX() { ParentDataList = this };
            if (deviceType == DeviceX.DeviceTypes.HomyokoWeatherStation) return new Devices.Homyoko.WeatherStation() { ParentDataList = this };
            if (deviceType == DeviceX.DeviceTypes.HomyokoIVTController) return new Devices.Homyoko.IVTController() { ParentDataList = this };
            return new DeviceX() { ParentDataList = this };
        }
        public void LoadData()
        {
            base.LoadData("deviceType, name");
        }

        #endregion

        /// <summary>
        /// Spustí automatické aktualizovanie načítaných údajov zariadení, tiež vykonáva logovanie údajov zariadení.
        /// Vykonáva automatickú synchronizáciu údajov zariadenia (napr. vykoná pripojenie a stiahne údaje).
        /// </summary>
        public void Start()
        {
            _isStopped = false;
            if (_threadDataUpdate == null || !_threadDataUpdate.IsAlive) _threadDataUpdate = new Thread(ThreadDataUpdate) { Name = this.GetType().Name, IsBackground = true };
            _threadDataUpdate.Start();
        }
        /// <summary>
        /// Zastaví spracovávanie položiek.
        /// </summary>
        public void Stop()
        {
            _isStopped = true;
        }
        /// <summary>
        /// Vlákno ktoré vykonáva aktualizovanie údajov zariadení (to znamená, načítanie údajov po pripojení a uloženie v premenných).
        /// </summary>
        [MTAThread]
        void ThreadDataUpdate()
        {
            const int HISTORY_LOG_TIMEOUT = 720; // Logovať históriu každých 12 minút (za jeden deň vytvorí 120 záznamov).
            DateTime lastHistoryLog = DateTime.Now.AddSeconds(-HISTORY_LOG_TIMEOUT);
g_start:
            try
            {
                while (!_isStopped)
                {
                    // Nájdi všetky zariadniea ktoré umožňujú aktualizovanie údajov a logovanie údajov.
                    List<Int64> autoDataUpdatesID = new List<Int64>();

                    try
                    {
                        for (int i = 0; i < this.List.Count; i++)
                        {
                            DeviceX device = (DeviceX)this.List[i];
                            if (device.Disabled) continue;
                            if (!device.CanAutoDataUpdate) continue;
                            if (device is IAutoDataUpdate)
                                autoDataUpdatesID.Add(device.ID);
                        }
                    }
                    catch (Exception ex)
                    {
                        Body.Environment.Logs.Error($"Problem getting devices for auto data update.", ex, this.GetType().Name);
                    }

                    if (_isStopped) goto g_exit;

                    // Vykonaj aktualizovanie údajov zariadení.
                    bool ishistoryLogged = false;

                    foreach (Int64 dataUpdateID in autoDataUpdatesID)
                    {
                        DeviceX device = (DeviceX)Body.Runtime.Devices.FindDataRecord(dataUpdateID);
                        if (device != null)
                        {
                            try
                            {
                                // Načítanie údajov so zariadenie a nastavenie premenných.
                                IAutoDataUpdate autoDataUpdate = (IAutoDataUpdate)device;
                                autoDataUpdate.AutoDataUpdate();
                            }
                            catch (Exception ex)
                            {
                                Body.Environment.Logs.Error($"Problem updating device data (ID: {dataUpdateID}).", ex, this.GetType().Name);
                            }
                            try
                            {
                                // Vykonaj zápis do histórie log údajov.
                                if (device is IHistoryDataLogs historyDataLogs)
                                {
                                    if ((DateTime.Now - lastHistoryLog).TotalSeconds > HISTORY_LOG_TIMEOUT)
                                    {
                                        historyDataLogs.WriteLogHistory();
                                        ishistoryLogged = true;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Body.Environment.Logs.Error($"Problem updating device history (ID: {dataUpdateID}).", ex, this.GetType().Name);
                            }
                        }                                
                    }

                    if (ishistoryLogged)
                        lastHistoryLog = DateTime.Now;

                    // Time-out vlákna.
                    for (int i = 0; i < 6; i++)
                    {
                        if (_isStopped) goto g_exit;
                        Thread.Sleep(1000);
                    }

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
        /// Vráti zoznam zariadený vo formáte EnnumValues, vhodné pre pridanie do zoznamu v UI controle napr. ComboBox.
        /// </summary>
        public List<X.Basic.CodeDom.Ennum.EnnumValues> GetEnnumValues()
        {
            List<X.Basic.CodeDom.Ennum.EnnumValues> list = new List<X.Basic.CodeDom.Ennum.EnnumValues>();
            foreach (DeviceX device in _list)
            {
                string deviceName = (device.DeviceType switch
                {
                    DeviceX.DeviceTypes.Serial => $"{device.Name} (Serial port)",
                    DeviceX.DeviceTypes.TCPSocket => $"{device.Name} (TCP socket)",
                    DeviceX.DeviceTypes.HomyokoWeatherStation => $"{device.Name} (Homyoko weather)",
                    DeviceX.DeviceTypes.HomyokoIVTController => $"{device.Name} (Homyoko IVT)",
                    _ => null
                });
                if (device == null) continue;
                list.Add(new X.Basic.CodeDom.Ennum.EnnumValues
                {
                    Description = deviceName,
                    Value = device.ID
                }); 
            }
            return list;
        }
        /// <summary>
        /// Vráti relačný názov zariadenia.
        /// </summary>
        public static string GetRelationDeviceName(DeviceX device)
        {
            string source = null;
            switch (device.DeviceType)
            {
                case DeviceX.DeviceTypes.Serial:
                    source = $"Serial port: {device.Name}";
                    break;
                case DeviceX.DeviceTypes.TCPSocket:
                    source = $"TCP socket: {device.Name}";
                    break;
                case DeviceX.DeviceTypes.HomyokoWeatherStation:
                    source = $"Homyoko weather: {device.Name}";
                    break;
                case DeviceX.DeviceTypes.HomyokoIVTController:
                    source = $"Homyoko IVT: {device.Name}";
                    break;
            }
            return source;
        }
        /// <summary>
        /// Vráti relačný názov zariadenia.
        /// </summary>
        public static string GetRelationDeviceName(Int64 deviceID)
        {
            DeviceX device = (DeviceX)Body.Runtime.Devices.FindDataRecord(deviceID);
            if (device != null)
                return GetRelationDeviceName(device);
            return null;
        }

    }
}
