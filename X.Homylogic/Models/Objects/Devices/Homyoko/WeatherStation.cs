/* HOMYOKO WEATHER STATION
 * 
 * Meteostanica vykraftená vo fokovej dielne.
 * 
 * ip : 192.168.16.242
 * tcp port : 5242
 * dotaz : xxsens5242xx
 * odpoved :  SENS106610550030100604
 * 
 * prvy snimac teploty v stupnoch C
 * 1 - znamienko , 1 kladna, 9 zaporna
 * 0 - desiatky
 * 6 - jednotky
 * 6 - desatiny 
 * druhy snimac teploty v stupnoch C
 * 1 - znamienko, 1 kladna, 9 zaporna 
 * 0 - desiatky
 * 5 - jednotky
 * 5 - desatiny 
 * rychlost vetra aktualna (10 sekundovy interval) v m/s
 * 0 - desiatky
 * 0 - jednotky
 * 3 - desatiny
 * rychlost vetra priemerna (30 minutovy priemer) v m/s
 * 0 - desiatky
 * 1 - jednotky
 * 0 - desatiny
 * uroven vonkajsieho jasu (0-1024)
 * 0 - tisicky
 * 6 - stovky
 * 0 - desiatky
 * 4 - jednotky
 *
 * 
 * 
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Threading;
using X.Data;
using X.Data.Management;

namespace X.Homylogic.Models.Objects.Devices.Homyoko
{
    public class WeatherStation : TCPDeviceX,
        IAutoDataUpdate,
        IHistoryDataLogs
    {
        const string TITLE = "Weather station";
        public const float SUN_SHINE_PERCENT_COEF = 0.0976F; // = 100 / 1024 

        bool _isClosed = true;
        bool _isDataReceived;
        DateTime _edgeValuesReset;

        /// <summary>
        /// Dátum a čas posledného merania údajov zo vzdialenej metostanice.
        /// </summary>
        public DateTime MeasureTime { get; private set; }
        /// <summary>
        /// Templota snímača 1 - °C.
        /// </summary>
        public float Temperature1 { get; private set; }
        /// <summary>
        /// Templota snímača 2 - °C.
        /// </summary>
        public float Temperature2 { get; private set; }
        /// <summary>
        /// Rýchlosť vetra - m/s.
        /// </summary>
        public float Windspeed { get; private set; }
        /// <summary>
        /// Priemerná rýchlosť vetra za posledných 30 sekúnd- m/s.
        /// </summary>
        public float WindspeedAvg { get; private set; }
        /// <summary>
        /// Úroveň svetelného jasu 0-1024.
        /// </summary>
        public float Sunshine { get; private set; }
        /// <summary>
        /// Úroveň svetelného jasu v percentách.
        /// </summary>
        public float SunshinePercent => (float)Math.Round(this.Sunshine * SUN_SHINE_PERCENT_COEF, 2);
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
        public CustomsTemperatureValues CustomsTemperature1 { get; set; }
        /// <summary>
        /// Temperature 2 customs settings.
        /// </summary>
        public CustomsTemperatureValues CustomsTemperature2 { get; set; }
        /// <summary>
        /// Customs settings values for temperatures.
        /// </summary>
        public class CustomsWindspeedValues
        {
            /// <summary>
            /// 1. stage of wind speed.
            /// </summary>
            public float LightAir { get; set; }
            /// <summary>
            /// 2. stage of wind speed.
            /// </summary>
            public float GentleBreeze { get; set; }
            /// <summary>
            /// 3. stage of wind speed.
            /// </summary>
            public float StrongBreeze { get; set; }
        }
        /// <summary>
        /// Customs settings values for windspeed.
        /// </summary>
        public CustomsWindspeedValues CustomsWindspeed { get; set; }
        /// <summary>
        /// Customs settings for sunshine.
        /// </summary>
        public class CustomsSunshineValues
        {
            /// <summary>
            /// Day is larged value than this defined percent value.
            /// </summary>
            public int Day { get; set; }
        }
        /// <summary>
        /// Customs settings values for sunshine.
        /// </summary>
        public CustomsSunshineValues CustomsSunshine { get; set; }
        /// <summary>
        /// Min/Max temperature 1 for current day.
        /// </summary>
        public EdgeValues EdgeTemperature1 { get; private set; }
        /// <summary>
        /// Min/Max temperature 2 for current day.
        /// </summary>
        public EdgeValues EdgeTemperature2 { get; private set; }
        /// <summary>
        /// Min/Max current windspeed for current day.
        /// </summary>
        public EdgeValues EdgeWindspeed { get; private set; }
        /// <summary>
        /// Min/Max average windspeed for current day.
        /// </summary>
        public EdgeValues EdgeWindspeedAvg { get; private set; }
        /// <summary>
        /// Min/Max sunshine for current day.
        /// </summary>
        public EdgeValues EdgeSunshinePercent { get; private set; }

        #region --- DATA PROPERTIES ---

        public enum PacketTypes : Int32
        {
            [Description("Version 1.")]
            Version_1 = 1,
            [Description("Version 2.")]
            Version_2 = 2
        }
        /// <summary>
        /// Určuje verziu komunikácie so zariadením (formát používaných paketov).
        /// </summary>
        public PacketTypes PacketType { get; set; } = PacketTypes.Version_1;
        public override string Settings => $"IP: {this.IPAddress}:{this.PortNumber}";

        #endregion

        #region --- DATA RECORD ---

        public override void SetRecordValues(DBReader dbReader, List<string> tags)
        {
            tags.Add("ignore-tcp-device"); // Pretože tieto nastavenia sa používajú namiesto nastavení base TCPDevice.
            base.SetRecordValues(dbReader, tags);
            this.IPAddress = dbReader.GetString("setStr01");
            this.PortNumber = dbReader.GetInt32("setInt01");
            this.PacketType = (PacketTypes)dbReader.GetInt32("setInt02");
            this.CustomsTemperature1.Caption = dbReader.GetString("setStr05");
            this.CustomsTemperature1.Minimum = dbReader.GetFloat("setDec01");
            this.CustomsTemperature1.Maximum = dbReader.GetFloat("setDec02");
            this.CustomsTemperature2.Caption = dbReader.GetString("setStr06");
            this.CustomsTemperature2.Minimum = dbReader.GetFloat("setDec03");
            this.CustomsTemperature2.Maximum = dbReader.GetFloat("setDec04");
            this.CustomsWindspeed.LightAir = dbReader.GetFloat("setDec05");
            this.CustomsWindspeed.GentleBreeze = dbReader.GetFloat("setDec06");
            this.CustomsWindspeed.StrongBreeze = dbReader.GetFloat("setDec07");
            this.CustomsSunshine.Day = dbReader.GetInt32("setInt05");
        }
        public override string SqlInsert(Data.Management.SqlConvert q, List<string> tags)
        {
            string fields = "setStr01, setInt01, setInt02, setStr05, setStr06, setDec01, setDec02, setDec03, setDec04, setDec05, setDec06, setDec07, setInt05";
            StringBuilder values = new StringBuilder();
            values.AppendFormat("{0}, ", q.Str(this.IPAddress));
            values.AppendFormat("{0}, ", (Int32)(this.PortNumber));
            values.AppendFormat("{0}, ", (Int32)(this.PacketType));
            values.AppendFormat("{0}, ", q.Str(this.CustomsTemperature1.Caption));
            values.AppendFormat("{0}, ", q.Str(this.CustomsTemperature2.Caption));
            values.AppendFormat("{0}, ", q.Float(this.CustomsTemperature1.Minimum));
            values.AppendFormat("{0}, ", q.Float(this.CustomsTemperature1.Maximum));
            values.AppendFormat("{0}, ", q.Float(this.CustomsTemperature2.Minimum));
            values.AppendFormat("{0}, ", q.Float(this.CustomsTemperature2.Maximum));
            values.AppendFormat("{0}, ", q.Float(this.CustomsWindspeed.LightAir));
            values.AppendFormat("{0}, ", q.Float(this.CustomsWindspeed.GentleBreeze));
            values.AppendFormat("{0}, ", q.Float(this.CustomsWindspeed.StrongBreeze));
            values.AppendFormat("{0} ", (Int32)(this.CustomsSunshine.Day));
            tags.Add("ignore-tcp-device"); // Pretože tieto nastavenia sa používajú namiesto nastavení base TCPDevice.
            return string.Format(base.SqlInsert(q, tags), fields, values);
        }
        public override string SqlUpdate(Data.Management.SqlConvert q, List<string> tags)
        {
            StringBuilder values = new StringBuilder();
            values.AppendFormat("setStr01 = {0}, ", q.Str(this.IPAddress));
            values.AppendFormat("setInt01 = {0}, ", (Int32)(this.PortNumber));
            values.AppendFormat("setInt02 = {0}, ", (Int32)this.PacketType);
            values.AppendFormat("setStr05 = {0}, ", q.Str(this.CustomsTemperature1.Caption));
            values.AppendFormat("setStr06 = {0}, ", q.Str(this.CustomsTemperature2.Caption));
            values.AppendFormat("setDec01 = {0}, ", q.Float(this.CustomsTemperature1.Minimum));
            values.AppendFormat("setDec02 = {0}, ", q.Float(this.CustomsTemperature1.Maximum));
            values.AppendFormat("setDec03 = {0}, ", q.Float(this.CustomsTemperature2.Minimum));
            values.AppendFormat("setDec04 = {0}, ", q.Float(this.CustomsTemperature2.Maximum));
            values.AppendFormat("setDec05 = {0}, ", q.Float(this.CustomsWindspeed.LightAir));
            values.AppendFormat("setDec06 = {0}, ", q.Float(this.CustomsWindspeed.GentleBreeze));
            values.AppendFormat("setDec07 = {0}, ", q.Float(this.CustomsWindspeed.StrongBreeze));
            values.AppendFormat("setInt05 = {0} ", (Int32)(this.CustomsSunshine.Day));
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

        public WeatherStation() 
        { 
            base.DeviceType = DeviceTypes.HomyokoWeatherStation;
            this.PortNumber = 5242;
            this.SocketType = SocketTypes.Client;
            this.PacketEndChar = @"\r\n";
            this.CustomsTemperature1 = new CustomsTemperatureValues() { Maximum = 40F };
            this.CustomsTemperature2 = new CustomsTemperatureValues() { Maximum = 40F };
            this.CustomsWindspeed = new CustomsWindspeedValues() { LightAir = 2, GentleBreeze = 7, StrongBreeze = 12 };
            this.CustomsSunshine = new CustomsSunshineValues() { Day = 30};
            this.EdgeTemperature1 = new EdgeValues();
            this.EdgeTemperature2 = new EdgeValues();
            this.EdgeWindspeed = new EdgeValues();
            this.EdgeWindspeedAvg = new EdgeValues();
            this.EdgeSunshinePercent = new EdgeValues();
        }
        /// <summary>
        /// Otvorí komunikáciu na krátku dobu max. 1 sekunda a odošle príkaz pre načítanie údajov z meteostanice. 
        /// </summary>
        public override void Open() 
        {
            if (!_isClosed) return;
            _isClosed = false;
            _isDataReceived = false;
            switch (this.PacketType)
            {
                case PacketTypes.Version_1:
                    //if (_threadComVer1 == null || !_threadComVer1.IsAlive) _threadComVer1 = new Thread(ThreadComVer1) { Name = this.GetType().Name, IsBackground = true };
                    //_threadComVer1.Start();
                    // Rýchlo zanikajúce vlákno je lepšie spustiť cez (ThreadPool), pretože systém lepšie manžuje resurces pre vlákno s krátkou životnosťou.
                    ThreadPool.QueueUserWorkItem(ThreadComVer1);
                    break;
            }
        }
        /// <summary>
        /// Zatvorí komunikáciu (nie je potrebné používať, pretože zariadenie sa automatciky odpojí samo).
        /// </summary>
        public override void Close()
        {
            _isClosed = true;
            base.Close();
        }
        /// <summary>
        /// Vlákno ktoré sa spustí po otvorení komunikácie, používa sa pre odoslanie packetu pre vykonanie príkazu pre načítanie údajov zo stanice verzia 1 (používa sa u Pala na stožiari).
        /// </summary>
        [MTAThread]
        void ThreadComVer1(Object stateInfo)
        {
            const int NUMBER_OF_TRY = 6;
            int numOfTry = NUMBER_OF_TRY;
g_again:
            try
            {
                numOfTry -= 1;

                base.Open();

                // Čakaj na na otvorenie zariadenia a vytvorenia komunikácie.
                while (this.IsOpen) 
                {
                    if (_isClosed || this.Disabled) return;
                    if (this.CanWrite) 
                    {
                        // Odošli príkazy viac krát po sebe aby zariadenie 100% odpovedalo.
                        this.Write("xxsens5242xx");
                        break;
                    }
                    Thread.Sleep(60);
                }
                if (_isClosed || this.Disabled) return;

                // Spracovať prijaté údaje alebo opakovať otvorenie zariadneia a znova odoslať packet.
                if (numOfTry > 0 && _isDataReceived == false) 
                {
                    // Zatiaľ sa nepodarilo prijať údaje opakuj akciu.
                    Thread.Sleep(60);
                    goto g_again;
                }
            }
            catch (Exception ex)
            {
                Body.Environment.Logs.Error($"Problem sending initialization packet.", ex, $"{TITLE} : {this.Name}");
            }
            _isClosed = true;
        }
        /// <summary>
        /// Logs error when device can't open connection.
        /// </summary>
        protected override void OnCantOpenSocket(Exception ex)
        {
            Body.Environment.Logs.Error($"Can't open weather station device {this.IPAddress}:{this.PortNumber}.", ex, $"{TITLE} : {this.Name}");
        }
        /// <summary>
        /// Spracovanie prijatých údajov.
        /// </summary>
        protected override void OnDataRecived(string data, string packetEndChar)
        {
            CultureInfo ci = (System.Globalization.CultureInfo)CultureInfo.CurrentCulture.Clone();
            ci.NumberFormat.NumberDecimalSeparator = ".";
            ci.NumberFormat.NegativeSign = "-";

            // Spracuj prijaté údaje a nastav premenné.
            switch (this.PacketType)
            {
                case PacketTypes.Version_1:
                    if (!string.IsNullOrEmpty(data) && data.StartsWith("SENS") && data.Length > 22)
                    {
                        // Bol prijatý korektný packet.
                        _isDataReceived = true;
                    } else {
                        // Bol prijatý nekorektný packet.    
                        return;  
                    }
                    // Spracuj korektne prijatý packet a nastav premenné.
                    string strTemp1 = data.Substring(4, 4);
                    string strTemp2 = data.Substring(8, 4);
                    string strWind = data.Substring(12, 3);
                    string strWindA = data.Substring(15, 3);
                    string strShine = data.Substring(18, 4);

                    // Uprava hodnôt na čísla.
                    if (strTemp1.StartsWith("9"))
                        strTemp1 = $"-{strTemp1.Substring(1)}";
                    else
                        strTemp1 = strTemp1.Substring(1);
                    strTemp1 = strTemp1.Insert(strTemp1.Length - 1, ".");
                    if (strTemp2.StartsWith("9"))
                        strTemp2 = $"-{strTemp2.Substring(1)}";
                    else
                        strTemp2 = strTemp2.Substring(1);
                    strTemp2 = strTemp2.Insert(strTemp2.Length - 1, ".");
                    strWind = strWind.Insert(strWind.Length - 1, ".");
                    strWindA = strWindA.Insert(strWindA.Length - 1, ".");

                    // Previesť hodnoty na čísla a nataviť premenné zariadenia.
                    try
                    {
                        this.MeasureTime = DateTime.Now;
                        this.Temperature1 = float.Parse(strTemp1, ci);
                        this.Temperature2 = float.Parse(strTemp2, ci);
                        this.Windspeed = float.Parse(strWind, ci);
                        this.WindspeedAvg = float.Parse(strWindA, ci);
                        this.Sunshine = float.Parse(strShine);

                        // Update edge min/max values
                        if (DateTime.Now.Hour == 0 && _edgeValuesReset.Day != DateTime.Now.Day) {
                            // Reset values at new day after midnight.
                            _edgeValuesReset = DateTime.Now;
                            this.EdgeTemperature1.Reset();
                            this.EdgeTemperature2.Reset();
                            this.EdgeWindspeed.Reset();
                            this.EdgeWindspeedAvg.Reset();
                            this.EdgeSunshinePercent.Reset();
                        }
                        else { 
                            this.EdgeTemperature1.Update(this.Temperature1);
                            this.EdgeTemperature2.Update(this.Temperature2);
                            this.EdgeWindspeed.Update(this.Windspeed);
                            this.EdgeWindspeedAvg.Update(this.WindspeedAvg);
                            this.EdgeSunshinePercent.Update(this.SunshinePercent);
                        }
                    }
                    catch (Exception ex)
                    {
                        Body.Environment.Logs.Error($"Problem parsing received data '{data}'.", ex, $"{TITLE} : {this.Name}");
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
            this.Open();
        }

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
                sql.Float("temperature1");
                sql.Float("temperature2");
                sql.Float("windspeed");
                sql.Float("windspeedAvg");
                sql.Float("sunshine", appendComma:false);
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
            sql.AppendFormat("INSERT INTO {0} (logTime, temperature1, temperature2, windspeed, windspeedAvg, sunshine) VALUES (", $"deviceHistory_{this.ID}");
            sql.AppendFormat("{0}, ", q.DTime(this.MeasureTime));
            sql.AppendFormat("{0}, ", q.Float(this.Temperature1));
            sql.AppendFormat("{0}, ", q.Float(this.Temperature2));
            sql.AppendFormat("{0}, ", q.Float(this.Windspeed));
            sql.AppendFormat("{0}, ", q.Float(this.WindspeedAvg));
            sql.AppendFormat("{0})", (int)(this.Sunshine));
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
        /// <summary>
        /// Nastaví hodnoty (vlastnosti) histórie, podľa údajov z databázy.
        /// </summary>
        public void SetDataHistory()
        {
            // TODO: Dorobiť históriu - napr. max/min teplota ...
            throw new NotImplementedException("Zatiaľ nedokončené ...");
        }

        #endregion

    }
}
