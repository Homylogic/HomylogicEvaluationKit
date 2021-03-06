﻿/* HOMYOKO IVT CONTROLLER STATION
 * 
 * IVT Controller (tepelné čerpadlo) umožňuje ovládanie kúrenia a čerpadiel.
 * 
 * ip : 192.168.16.165
 * tcp port : 5000
 * 
 * -- Poznkamy od Foka:
 * 
 * prikaz 65 z homy = zakur :
 * zapni cerpadla, posli spravu do homy "cerpadla zapnute"
 * pockaj 10 sekund
 * skontroluj prietok-stav alarm rele, ak ok posli "prietok ok"
 * ak nieje prietok vypni cerpadla, pockaj 10 sekund, zapni cerpadla, posli upozornenie do homy "nieje prietok"
 * ak nieje pritok opakuj 5x
 * ak nieje prietok, posli oznam do homy "5 pokusov a netece"
 * ak prietok ok, pockaj 2 sekundy, prestan blokovat chladnicku, posli spravu do homy "IVT zapnute"
 * sleduj primarny prietok a teplotu do podlahy, pokial klesne prietok posli spravu "nieje prietok", alebo teplota presiahne 40stupnov posli spravu " vysoka teplota v podlahe". zablokuj chladnicku,  posli spravu "IVT vypnute" , pockaj 10 sekund, vypni cerpadla, posli "cerpadla vypnute"
 * pocas kurenia odosielaj aktualnu teplotu vody do podlahy v intervaloch 1 minuta
 * pocas kurenia sleduj prikaz na vypnutie od homy
 * 
 * prikaz 66 = nekur
 * posli spravu "IVT vypnute" , pockaj 10 sekund, vypni cerpadla, posli "cerpadla vypnute"
 * prikaz 67=posli teplotu
 * "Podlaha:  " xx " °C"
 * "Vonku:  " xx " °C"
 * prikaz 68= spust cerpadla
 * prikaz 69= vypni cerpadla (funguje len ak sa nekuri)
 * prikaz 70= zisti prietok
 * mozne odpovede:
 * "nie je prietok - cerpadla su vypnute"
 * "nie je prietok - cerpadla su zapnute"
 * "prietok ok"
 * posielas znaky A - F, v ascii to reprezentuje 65-70
 * 
 * -- Prikazy HEX (A až F):
 * 
 * A - Zapnut kurenie
 * B - Vypnut kurenie
 * C - Pošli teploty
 *     Response: Podlaha: 23.1 �C Vonku: -0.0 �C
 *     MinLength: 26 - '\r\nPodlaha: �C\r\nVonku: �C\r\n' 
 *     ! Dlzka odpovede sa moze menit podla dlzky hodnot
 * D - Spusti cerpadla
 * E - Vypni cerpadla
 * F - Pošli prietoky
 *     Response: nie je prietok - cerpadla su vypnute, nie je prietok - cerpadla su zapnute, prietok ok
 *     MinLegth: 10 - prietok ok 
 * 
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading;
using X.Basic;
using X.Data;
using X.Data.Management;

namespace X.Homylogic.Models.Objects.Devices.Homyoko
{
    public class IVTController : TCPDeviceX,
        IAutoDataUpdate,
        IHistoryDataLogs
    {
        const string TITLE = "IVT Controller";

        Thread _threadHoldComOpen;
        bool _isOpen = false;

        /// <summary>
        /// Dátum a čas posledného merania údajov zo vzdialenej metostanice.
        /// </summary>
        public DateTime MeasureTime { get; private set; }
        /// <summary>
        /// Teplota podlahy (vykurovacia teplota).
        /// </summary>
        public float TemperatureFloor { get; private set; }
        /// <summary>
        /// Teplota v tepelnom cerpadle (zatial sa nepoužíva).
        /// </summary>
        public float TemperatureOut { get; private set; }
        public enum WaterFlowTypes
        {
            /// <summary>
            /// Nenastavená (nenačítaná so zariadenia) hodnota.
            /// </summary>
            [Description("Not detected")]
            Unknown,
            /// <summary>
            /// Bez prietoku s vypnutými cerpadlami.
            /// </summary>
            [Description("Pump off")]
            NoFlow_PumpOff,
            /// <summary>
            /// Bez prietku so zapnutými cerpadlami.
            /// POZOR: Tentot stav je porucha systemu.
            /// </summary>
            [Description("! No flow but pump on")]
            NoFlow_PumpOn,
            /// <summary>
            /// Cerpadla zapnute, prietok vporiadku.
            /// </summary>
            [Description("Water flow on")]
            FlowIsOK
        }
        /// <summary>
        /// Teplota v tepelnom cerpadle (zatial sa nepoužíva).
        /// </summary>
        public WaterFlowTypes WaterFlow { get; private set; } = WaterFlowTypes.Unknown;
        public enum StatusTypes { None, TurnedOff, TurnedOn }
        /// <summary>
        /// Stav IVT po odoslaní príkazu zapnúť/vypnúť kúrenie.
        /// </summary>
        public StatusTypes IVTStatus { get; private set; } = StatusTypes.None;
        /// <summary>
        /// Stav čerpadiel po odoslaní príkazu zapnúť/vypnúť čerpadlá.
        /// </summary>
        public StatusTypes PumpStatus { get; private set; } = StatusTypes.None;
        /// <summary>
        /// Problem IVT zariadenia - Nie je prietok.
        /// Nastavuje as automaticky pri prijatí chybného stavu IVT zariadenia.
        /// </summary>
        public bool IsProblemNoWaterFlow { get; private set; } = false;
        /// <summary>
        /// Problem IVT zariadenia - Vysoká teplota.
        /// Nastavuje as automaticky pri prijatí chybného stavu IVT zariadenia.
        /// </summary>
        public bool IsProblemHighTemperature { get; private set; } = false;

        #region --- DATA PROPERTIES ---

        public enum PacketTypes : Int32
        {
            [Description("Version 1.")]
            Version_1 = 1
            /* Verzia 2 sa zatiaľ neplánuje, ale pre istotu ...
            [Description("Version 2.")]
            Version_2 = 2
            */
        }
        /// <summary>
        /// Určuje verziu komunikácie so zariadením (formát používaných paketov).
        /// </summary>
        public PacketTypes PacketType { get; set; } = PacketTypes.Version_1;
        public override string Settings => $"IP: {this.IPAddress}:{this.PortNumber}";
        /// <summary>
        /// Customs settings values for temperatures.
        /// </summary>
        public class CustomsTemperatureValues
        {
            /// <summary>
            /// Caption for temperature, etc. shown on home screen.
            /// </summary>
            public string Caption { get; set; }
            /// <summary>
            /// Minimum temperature for cold temperature default 0°C.
            /// </summary>
            public float Minimum { get; set; }
            /// <summary>
            /// Maximum temperature for hot temperature default 40°C.
            /// </summary>
            public float Maximum { get; set; }
        }
        /// <summary>
        /// Temperature 1 customs settings.
        /// </summary>
        public CustomsTemperatureValues CustomsTemperature { get; set; }

        #endregion

        #region --- DATA RECORD ---

        public override void SetRecordValues(DBReader dbReader, List<string> tags)
        {
            tags.Add("ignore-tcp-device"); // Pretože tieto nastavenia sa používajú namiesto nastavení base TCPDevice.
            base.SetRecordValues(dbReader, tags);
            this.IPAddress = dbReader.GetString("setStr01");
            this.PortNumber = dbReader.GetInt32("setInt01");
            this.PacketType = (PacketTypes)dbReader.GetInt32("setInt02");
            this.CustomsTemperature.Caption = dbReader.GetString("setStr02");
            this.CustomsTemperature.Minimum = dbReader.GetFloat("setDec01");
            this.CustomsTemperature.Maximum = dbReader.GetFloat("setDec02");
        }
        public override string SqlInsert(Data.Management.SqlConvert q, List<string> tags)
        {
            string fields = "setStr01, setInt01, setInt02, setStr02, setDec01, setDec02";
            StringBuilder values = new StringBuilder();
            values.AppendFormat("{0}, ", q.Str(this.IPAddress));
            values.AppendFormat("{0}, ", (Int32)(this.PortNumber));
            values.AppendFormat("{0}, ", (Int32)(this.PacketType));
            values.AppendFormat("{0}, ", q.Str(this.CustomsTemperature.Caption));
            values.AppendFormat("{0}, ", q.Float(this.CustomsTemperature.Minimum));
            values.AppendFormat("{0} ", q.Float(this.CustomsTemperature.Maximum));
            tags.Add("ignore-tcp-device"); // Pretože tieto nastavenia sa používajú namiesto nastavení base TCPDevice.
            return string.Format(base.SqlInsert(q, tags), fields, values);
        }
        public override string SqlUpdate(Data.Management.SqlConvert q, List<string> tags)
        {
            StringBuilder values = new StringBuilder();
            values.AppendFormat("setStr01 = {0}, ", q.Str(this.IPAddress));
            values.AppendFormat("setInt01 = {0}, ", (Int32)(this.PortNumber));
            values.AppendFormat("setInt02 = {0}, ", (Int32)this.PacketType);
            values.AppendFormat("setStr02 = {0}, ", q.Str(this.CustomsTemperature.Caption));
            values.AppendFormat("setDec01 = {0}, ", q.Float(this.CustomsTemperature.Minimum));
            values.AppendFormat("setDec02 = {0} ", q.Float(this.CustomsTemperature.Maximum));
            tags.Add("ignore-tcp-device"); // Pretože tieto nastavenia sa používajú namiesto nastavení base TCPDevice.
            return string.Format(base.SqlUpdate(q, tags), values);
        }
        public override void Save()
        {
            base.Save();

            // Vytvoriť tabuľku pre logovanie histórie údajov, ak tabuľka neexistuje.
            CreateTableHistory();
        }
        public override void Delete(long id)
        {
            // Vymazať tabuľku pre logovanie histórie údajov.
            this.DropTableHistory(id);

            base.Delete(id);
        }

        #endregion

        public IVTController() 
        { 
            base.DeviceType = DeviceTypes.HomyokoIVTController;
            this.PortNumber = 5000;
            this.SocketType = SocketTypes.Client;
            this.PacketEndChar = @"\r\n";
            this.CustomsTemperature = new CustomsTemperatureValues() { Minimum = 20F, Maximum = 30F };
        }
        /// <summary>
        /// Otvorí komunikáciu a snaží sa udržiavať pripojenie (napr. keď nastane prerušenie).
        /// </summary>
        public override void Open()
        {
            if (_isOpen) return;
            _isOpen = true;

            // Nastavenie default príznakov.
            this.IsProblemNoWaterFlow = false;
            this.IsProblemHighTemperature = false;

            // Spusti vlákno ktoré sa snaží udržiavať stále otvorené pripojenie.
            if (_threadHoldComOpen == null || !_threadHoldComOpen.IsAlive) _threadHoldComOpen = new Thread(ThreadHoldComOpen) { Name = this.GetType().Name, IsBackground = true };
            _threadHoldComOpen.Start();
        }
        /// <summary>
        /// Zatvorí komunikáciu (nie je potrebné používať, pretože zariadenie sa automatciky odpojí samo).
        /// </summary>
        public override void Close()
        {
            _isOpen = false;
            base.Close();
        }
        /// <summary>
        /// Vlákno ktoré sa spustí po otvorení komunikácie, používa sa pre odoslanie packetu pre vykonanie príkazu pre načítanie údajov zo stanice verzia 1 (používa sa u Pala na stožiari).
        /// </summary>
        [MTAThread]
        void ThreadHoldComOpen()
        {
g_again:
            try
            {
                base.Open();

                // Testuj či je zariadenie stále otvorené.
                while (this.IsOpen)
                {
                    if (!_isOpen || this.Disabled) return;
                    Thread.Sleep(6000);
                }

                if (_isOpen)
                {
                    goto g_again;
                }
                else {
                    // Zariadenie bolo zatvorené používatelom.
                    return;
                }
            }
            catch (Exception ex)
            {
                Body.Environment.Logs.Error($"Can't open connection.", ex, $"{TITLE} : {this.Name}");
                goto g_again;
            }
        }
        /// <summary>
        /// Logs error when device can't open connection.
        /// </summary>
        protected override void OnCantOpenSocket(Exception ex)
        {
            Body.Environment.Logs.Error($"Can't open IVT controller device {this.IPAddress}:{this.PortNumber}.", ex, $"{TITLE} : {this.Name}");
        }
        /// <summary>
        /// Spracovanie prijatých údajov.
        /// </summary>
        protected override void OnDataRecived(string data, string packetEndChar)
        {
            // Spracuj prijaté údaje a nastav premenné.
            switch (this.PacketType)
            {
                case PacketTypes.Version_1:
                    // Teploty.
                    if (!string.IsNullOrEmpty(data) && data.StartsWith("\r\nPodlaha:") && data.Length >= 26)
                    {
                        // Bol prijatý korektný packet na odpoveď C - Posli teploty .
                        // Nacítaj a nastav prijate teploty.
                        string[] arrTemps = data.Split("�C"); // � = ? = ASCI(63)
                        if (arrTemps.Length > 2)
                        {
                            try
                            {
                                string strPodlaha = arrTemps[0].Replace("\r\n", string.Empty).Replace("Podlaha: ", string.Empty).Trim();
                                string strVon = arrTemps[1].Replace("\r\n", string.Empty).Replace("Vonku: ", string.Empty).Trim();

                                this.MeasureTime = DateTime.Now;
                                this.TemperatureFloor = float.Parse(strPodlaha, XCommon.CSVNumberCulture);
                                this.TemperatureOut = float.Parse(strVon, XCommon.CSVNumberCulture);
                            }
                            catch (Exception ex)
                            {
                                Body.Environment.Logs.Error($"Problem parsing received data '{data}'.", ex, $"{TITLE} : {this.Name}");
                            }
                        }
                    }
                    else {
                        // Ine ako teploty.
                        // Controll status (nastane po odoslaní príkazu).
                        if (!string.IsNullOrEmpty(data)) 
                        {
                            if (data.StartsWith("IVT zapnute")) 
                            { 
                                this.IVTStatus = StatusTypes.TurnedOn;
                                break;
                            }
                            if (data.StartsWith("IVT vypnute")) 
                            { 
                                this.IVTStatus = StatusTypes.TurnedOff;
                                break;
                            }
                            if (data.StartsWith("cerpadla zapnute"))
                            {
                                this.PumpStatus = StatusTypes.TurnedOn;
                                break;
                            }
                            if (data.StartsWith("cerpadla vypnute"))
                            {
                                this.PumpStatus = StatusTypes.TurnedOff;
                                break;
                            }
                            if (data.StartsWith("nieje prietok"))
                            {
                                this.IsProblemNoWaterFlow = true;
                                break;
                            }
                            if (data.StartsWith("vysoka teplota v podlahe"))
                            {
                                this.IsProblemHighTemperature = true;
                                break;
                            }
                        }
                        // Cerpadla.
                        if (!string.IsNullOrEmpty(data) && data.Length >= 10)
                        {
                            if (data.StartsWith("nie je prietok - cerpadla su vypnute")) 
                                this.WaterFlow = WaterFlowTypes.NoFlow_PumpOff;
                            if (data.StartsWith("nie je prietok - cerpadla su zapnute"))
                            { 
                                this.WaterFlow = WaterFlowTypes.NoFlow_PumpOn;
                                Body.Environment.Logs.Warning($"Pump has problem, no water flow but pumping.", source:$"{TITLE} : {this.Name}");
                            }
                            if (data.StartsWith("prietok ok"))
                            {
                                this.WaterFlow = WaterFlowTypes.FlowIsOK;
                                this.IsProblemHighTemperature = false;
                                this.IsProblemNoWaterFlow = false;
                            }
                        }
                    }
                    break;
            }
            base.OnDataRecived(data, packetEndChar);
        }
        /// <summary>
        /// Automatické aktualizovanie údajov premenných so vzdialeného zariadenia (krátke otvorenie komunikácie a načítanie údajov).
        /// </summary>
        public void AutoDataUpdate()
        {
            // Zariadenie by malo zostávať otvorené.
            this.Open();

            if (!this.CanWrite) return;

            // Odošli príkazy pre získanie stavou.
            switch (this.PacketType)
            {
                case PacketTypes.Version_1 :
                    // Odosiela dva príkazy C a F.
                    this.Write("CF");
                    break;
            }
        }

        #region --- CONTROLS METHODS ---

        /// <summary>
        /// Zapne kúrenie (odošle príkaz pre zapntie kúrenia).
        /// </summary>
        [Description("Heat on")]
        public void HeatOn() 
        {
            this.IVTStatus = StatusTypes.None;
            this.Write("A");
        }
        /// <summary>
        /// Vypne kúrenie (odošle príkaz pre vypnutie kúrenia).
        /// </summary>
        [Description("Heat off")]
        public void HeatOff()
        {
            this.IVTStatus = StatusTypes.None;
            this.Write("B");
        }
        /// <summary>
        /// Zapne čerpadlá (odošle príkaz pre zapntie čerpadiel).
        /// </summary>
        [Description("Pump on")]
        public void PumpOn()
        {
            this.PumpStatus = StatusTypes.None;
            this.Write("D");
        }
        /// <summary>
        /// Vypne čerpadlá (odošle príkaz pre vypnutie čerpadiel).
        /// </summary>
        [Description("Pump off")]
        public void PumpOff()
        {
            this.PumpStatus = StatusTypes.None;
            this.Write("E");
        }

        #endregion

        #region --- HISTORY DATA LOGS --- 

        /// <summary>
        /// Vytvorí databázovú tabuľku pre logovanie histórie v databáze pre zber údajov.
        /// </summary>
        public void CreateTableHistory()
        {
            Body.Database.DBClientLogs.Open();
            string tableName = $"deviceHistory_{this.ID}";
            if (!Body.Database.DBClientLogs.IsTableExist(tableName)) 
            {
                SqlStringBuilder sql = new SqlStringBuilder(Body.Database.DBClientLogs.ClientType);
                sql.CreateTable(tableName);
                sql.Append("(");
                sql.AddPrimaryKey();
                sql.DateTime("logTime");
                sql.Float("temperatureFloor");
                sql.Float("temperatureOut", appendComma:false);
                sql.Append(")");
                sql.EngineMyISAM();
                Body.Database.DBClientLogs.ExecuteNonQuery(sql.ToString());
            }
        }
        public void DropTableHistory(long id) 
        {
            string tableName = $"deviceHistory_{id}";
            Body.Database.DBClientLogs.Open();
            Body.Database.DBClientLogs.ExecuteNonQuery($"DROP TABLE {tableName}");
        }
        /// <summary>
        /// Zapíše aktuálne údaje do log tabuľky.
        /// </summary>
        public void WriteLogHistory() 
        {
            if (this.MeasureTime.Year == 1) return; // Ignorovať logovanie, ak údaje ešte neboli aktualizované.

            Data.Management.SqlConvert q = new Data.Management.SqlConvert(Body.Database.DBClientLogs.ClientType);
            StringBuilder sql = new StringBuilder();
            sql.AppendFormat("INSERT INTO {0} (logTime, temperatureFloor, temperatureOut) VALUES (", $"deviceHistory_{this.ID}");
            sql.AppendFormat("{0}, ", q.DTime(this.MeasureTime));
            sql.AppendFormat("{0}, ", q.Float(this.TemperatureFloor));
            sql.AppendFormat("{0})", q.Float(this.TemperatureOut));
            Body.Database.DBClientLogs.Open();
            Body.Database.DBClientLogs.ExecuteNonQuery(sql.ToString());
        }
        /// <summary>
        /// Vymaže staré záznamy logov.
        /// </summary>
        public void DeleteLogHistory() 
        {
            // Vymaž posledných 30k záznamov, ak je počet záznamov viac ako 100k (2,2 roka).
            Body.Database.DBClientLogs.Open();
            long count = (long)Body.Database.DBClientLogs.ExecuteScalar($"SELECT COUNT(*) FROM deviceHistory_{this.ID}");
            if (count >= 100000)
            {
                long lastID = (long)Body.Database.DBClientLogs.ExecuteScalar($"SELECT ID FROM deviceHistory_{this.ID} ORDER BY ID DESC LIMIT 30000, 1");
                Body.Database.DBClientLogs.ExecuteNonQuery($"DELETE FROM deviceHistory_{this.ID} WHERE ID < {lastID}");
            }
        }

        #endregion

    }
}
