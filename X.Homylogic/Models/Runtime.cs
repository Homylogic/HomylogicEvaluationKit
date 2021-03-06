﻿/* OBJECTS RUNTIME
 * 
 * Tu sa nachádazjú aktívne inštancie objektov vytvorených podľa databázy.
 * 
 * 
 * 
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net.WebSockets;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics.CodeAnalysis;
using X.Homylogic.Models.Objects;
using System.Linq;
using X.Homylogic.Models.Objects.Devices;

namespace X.Homylogic.Models
{
    public sealed class Runtime
    {
        Thread _workerThread;
        bool _isWorking = true;

        public TriggerXList Triggers { get; private set; }
        public VariableXList Variables { get; private set; }
        public Objects.Buffers.InputBufferXList InputBuffers { get; private set; }
        public Objects.Buffers.OutputBufferXList OutputBuffers { get; private set; }
        public DeviceXList Devices { get; private set; }
        public Schedule.ScheduleWorker ScheduleWorker { get; private set; }

        public Runtime()
        {
            this.Triggers = new TriggerXList();
            this.Variables = new VariableXList();
            this.InputBuffers = new Objects.Buffers.InputBufferXList();
            this.OutputBuffers = new Objects.Buffers.OutputBufferXList();
            this.Devices = new DeviceXList();
            this.ScheduleWorker = new Schedule.ScheduleWorker();
        }
        /// <summary>
        /// Načíta zoznam všetkých objektov podľa databázy.
        /// </summary>
        public void Load()
        {
            this.Triggers.LoadData();
            this.Variables.LoadData();
            this.InputBuffers.LoadData(recordsLimit:500); // Načítaj maximálne N záznamov, viac nemá význam načítavať (posledných N prijatých údajov je dostačujúce).
            this.OutputBuffers.LoadData();
            this.Devices.LoadData();
        }
        /// <summary>
        /// Spustí vykonávanie všetkých objektov.
        /// </summary>
        public void Start() 
        {
            this.OutputBuffers.Start();
            this.Devices.Start();
            this.ScheduleWorker.Start();
            if (_workerThread == null || !_workerThread.IsAlive) _workerThread = new Thread(ThreadWorker) { Name = this.GetType().Name, IsBackground = true };
            _workerThread.Start();
        }
        /// <summary>
        /// Zastaví vykonávanie všetkých objektov.
        /// </summary>
        public void Stop()
        {
            this.OutputBuffers.Stop();
            this.Devices.Stop();
            this.ScheduleWorker.Stop();
            _isWorking = false;
        }
        /// <summary>
        /// Vykonáva úlohy s objektami, napr. vymazávanie starých údajov.
        /// </summary>
        [MTAThread]
        void ThreadWorker()
        {
            DateTime lastDataClear = DateTime.MinValue;

g_start:    try
            {
                while (_isWorking)
                {
                    // Time-out vlákna, robí sa na začiatku vlákna, aby vlákno pri prvom spustení programu nezačalo pracovať okamžite.
                    for (int i = 0; i < 60; i++)
                    {
                        if (!_isWorking) goto g_exit;
                        Thread.Sleep(1000);
                    }

                    // Vymazávnie údajov, vykonáva sa vždy jeden krát v noci alebo pri prvom spustení programu.
                    if (lastDataClear == DateTime.MinValue || (DateTime.Now.Hour == 1 && lastDataClear < DateTime.Now.Date)) 
                    {
                        // Vymazanie starých logov.
                        try
                        {
                            Body.Environment.Logs.DeleteOld();
                        }
                        catch (Exception ex)
                        {
                            Body.Environment.Logs.Error("Problem deleting old logs.", ex, this.GetType().Name);
                        }

                        // Vymazanie starej histórie zariadení.
                        try
                        {
                            for (int i = 0; i < this.Devices.List.Count; i++)
                            {
                                DeviceX device = (DeviceX)this.Devices.List[i];
                                if (device is IHistoryDataLogs historyDataLogs)
                                {
                                    try
                                    {
                                        historyDataLogs.DeleteLogHistory();
                                    }
                                    catch (Exception ex)
                                    {
                                        Body.Environment.Logs.Error($"Problem deleting old history data of device {device.Name}.", ex, this.GetType().Name);
                                    }
                                }
                            }
                            lastDataClear = DateTime.Now.Date;
                        }
                        catch (Exception ex)
                        {
                            Body.Environment.Logs.Error($"Problem deleting old history data of devices.", ex, this.GetType().Name);
                        }
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



    } // End class Runtime


}
