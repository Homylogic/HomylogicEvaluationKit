/* SCHEDULE WORKER
 * 
 * Vykonáva naplánované činnosti.
 * 
 * 
 */
using Org.BouncyCastle.Asn1.Cms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using X.Homylogic.Models.Objects;
using X.Homylogic.Models.Objects.Devices.Homyoko;

namespace X.Homylogic.Models.Schedule
{
    public sealed class ScheduleWorker
    {
        const string TITLE = "Scheduler";

        Thread _schedulerThread;
        Thread _workerThread;
        bool _isWorking = false;
        object _syncObject = new object();
        List<KeyValuePair<Int64, Int64>> _deviceIDs = new List<KeyValuePair<Int64, Int64>>();

        public void Start()
        {
            if (_isWorking) return;
            _isWorking = true;
            if (_schedulerThread == null || !_schedulerThread.IsAlive) _schedulerThread = new Thread(ThreadScheduler) { Name = this.GetType().Name, IsBackground = true };
            _schedulerThread.Start();
            if (_workerThread == null || !_workerThread.IsAlive) _workerThread = new Thread(ThreadWorker) { Name = this.GetType().Name, IsBackground = true };
            _workerThread.Start();
        }
        public void Stop()
        {
            _isWorking = false;
        }
        /// <summary>
        /// Zbiera položky pre vykonanie.
        /// </summary>
        [MTAThread]
        void ThreadScheduler()
        {
g_start:    try
            {
                while (_isWorking)
                {
                    DayOfWeek day = DateTime.Now.DayOfWeek;

                    // Devices.
                    try
                    {
                        for (int i = 0; i < Body.Runtime.Devices.List.Count; i++)
                        {
                            DeviceX device = (DeviceX)Body.Runtime.Devices.List[i];
                            if (device.Disabled) continue;
                            if (device is IVTController) 
                            {
                                IVTController ivtController = (IVTController)device;
                                if (ivtController.Scheduler != null) 
                                {
                                    if (!ivtController.Scheduler.IsDataLoaded)
                                        ivtController.Scheduler.LoadData();

                                    for (int n = 0; n < ivtController.Scheduler.List.Count; n++)
                                    {
                                        ScheduleRecord schedule = (ScheduleRecord)ivtController.Scheduler.List[n];
                                        if (schedule.IsProcessedToday) continue;
                                        bool isDay = false;
                                        if (schedule.DayMonday && day == DayOfWeek.Monday) isDay = true;
                                        if (schedule.DayTuesday && day == DayOfWeek.Tuesday) isDay = true;
                                        if (schedule.DayWednesday && day == DayOfWeek.Wednesday) isDay = true;
                                        if (schedule.DayThursday && day == DayOfWeek.Thursday) isDay = true;
                                        if (schedule.DayFriday && day == DayOfWeek.Friday) isDay = true;
                                        if (schedule.DaySaturday && day == DayOfWeek.Saturday) isDay = true;
                                        if (schedule.DaySunday && day == DayOfWeek.Sunday) isDay = true;
                                        if (!isDay) continue;

                                        // Pridaj položku staršiu ako 2 minúty.
                                        double secoudsDiff = (DateTime.Now.TimeOfDay - schedule.ScheduleTime.TimeOfDay).TotalSeconds;
                                        if (secoudsDiff > 0 && secoudsDiff < 120) 
                                        { 
                                            lock (_syncObject) 
                                            {
                                                KeyValuePair<Int64, Int64> IDs = new KeyValuePair<Int64, Int64>(device.ID, schedule.ID);    
                                                if (!_deviceIDs.Contains(IDs))
                                                    _deviceIDs.Add(IDs); 
                                            }
                                        }
                                    }
                                }
                            }
                        } // end of for - devices
                    }
                    catch (Exception ex)
                    {
                        Body.Environment.Logs.Error($"Problem while scheduler processing items for start.", ex, TITLE);
                    }

                    // Time-out vlákna.
                    for (int i = 0; i < 12; i++)
                    {
                        if (!_isWorking) goto g_exit;
                        Thread.Sleep(1000);
                    }
                }
            }
            catch (Exception)
            {
                Thread.Sleep(5000);
                if (!_isWorking) goto g_exit;
                goto g_start;
            }
g_exit:;
        }
        /// <summary>
        /// Vykonáva položky pre spracovanie.
        /// </summary>
        [MTAThread]
        void ThreadWorker()
        {
g_start:    try
            {
                while (_isWorking)
                {
                    KeyValuePair<Int64, Int64> curIDs = new KeyValuePair<Int64, Int64>(0, 0);
                    lock (_syncObject)
                    {
                        if (_deviceIDs.Count > 0)
                            curIDs = _deviceIDs[0];
                    }
                    if (curIDs.Key != 0) 
                    {
                        DeviceX device = (DeviceX)Body.Runtime.Devices.FindDataRecord(curIDs.Key);
                        if (device != null) 
                        {
                            ScheduleRecord schedule = (ScheduleRecord)device.Scheduler.FindDataRecord(curIDs.Value);
                            if (schedule != null) 
                            {
                                bool closeDevice = false;
                                try
                                {
                                    // Otvor komunikáciu.
                                    if (!device.IsOpen) 
                                    {
                                        closeDevice = true;
                                        Stopwatch stopwatch = new Stopwatch();
                                        device.Open();
                                        stopwatch.Start();
                                        while (!device.CanWrite) 
                                        {
                                            if (stopwatch.ElapsedMilliseconds > 15000) break;
                                        }
                                    }
                                    MethodInfo method = device.GetType().GetMethod(schedule.MethodName);
                                    method.Invoke(device, null);
                                    Body.Environment.Logs.Info($"Scheduler starts - {schedule.ActionSettings}.", $"Device: {device.Name}", TITLE);
                                }
                                catch (Exception ex)
                                {
                                    // Lepšia chyba je v inner exception.
                                    if (ex.InnerException != null)
                                        ex = ex.InnerException;
                                    Body.Environment.Logs.Error($"Problem while scheduler starts {device.Name}-{schedule.ActionSettings}.", ex, TITLE);
                                }
                                if (closeDevice)
                                    device.Close();
                                schedule.IsProcessedToday = true;
                                lock (_syncObject)
                                {
                                    _deviceIDs.Remove(curIDs);
                                }
                            }
                        }
                    }

                    // Time-out vlákna.
                    for (int i = 0; i < 12; i++)
                    {
                        if (!_isWorking) goto g_exit;
                        Thread.Sleep(1000);
                    }
                }
            }
            catch (Exception)
            {
                Thread.Sleep(5000);
                if (!_isWorking) goto g_exit;
                goto g_start;
            }
g_exit:;
        }
    }
}
