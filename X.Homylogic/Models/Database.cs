/* HOMYLOGIC DATABASE
 * 
 * DB Client conection always stay open, because of thread concurency (when open/close). Close connection only when application quits.
 * 
 * 
 * Umožňuje pracovať s databázou v programe Homylogic.
 * Obsahuje transformáciu databázy a objekt SQL clienta. 
 * 
 */
using System;
using System.Configuration;
using X.Data;
using X.App;
using X.Homylogic.Models.Objects;
using X.App.Logs;
using X.App.Settings;

namespace X.Homylogic.Models
{
    /// <summary>
    /// Údržba databázy programu Homylogic a nastavene pripojenie klienta pre prácu s databázov.
    /// </summary>
    public sealed class Database
    {
        /// <summary>
        /// CryptoKey for store mysql connnection configuration in settings file.
        /// </summary>
        public const string CRYPTO_KEY = "#š! _56sdr áťťťťťť123";

        /// <summary>
        /// Aktuaálne verzia databázy.
        /// </summary>
        public const int VERSION = 100;

        /// <summary>
        /// Názov databázového súboru pre konfiguráciu programu.
        /// </summary>
        const string DB_FILE_NAME = "homylogic.db";
        /// <summary>
        /// Názov databázového súboru pre hisoriu údajov a logovanie.
        /// </summary>
        const string DB_FILE_NAME_LOGS = "datalogs.db";

        // Prihlasovacie údaje na lokálny MySQL server.
        string DB_SERVER = "localhost";
        string DB_USER = "homylogic";
        string DB_PASSWORD = "EvaluationKit1000";
        const string DB_NAME = "homylogic";
        // Databáza pre históriu a logovanie na MySQl serveri.
        const string DB_NAME_LOGS = "homylogiclogs";

        readonly DBClient _dbClient;
        /// <summary>
        /// Univerzálny databázový klient pre konfiguráciu programu.
        /// </summary>
        public DBClient DBClient => _dbClient;

        readonly DBClient _dbClientLogs;
        /// <summary>
        /// Univerzálny databázový klient pre históriu údajov a logovanie.
        /// </summary>
        public DBClient DBClientLogs => _dbClientLogs;


        /// <summary>
        /// Inicializácia univerzálneho databázového klienta podľa zadaného typu.
        /// </summary>
        public Database(X.Data.DBClient.ClientTypes databaseType) 
        {
            switch (databaseType) 
            {
                case DBClient.ClientTypes.Sqlite: 
                    _dbClient = new DBClient(DB_FILE_NAME);
                    _dbClientLogs = new DBClient(DB_FILE_NAME_LOGS);
                    break;

                case DBClient.ClientTypes.MySql:
                    // Read configuration file.
                    try
                    {
                        string crypted = X.App.Settings.ConfigFile.Read("database-mysql");
                        if (crypted != null) 
                        { 
                            string settingsValue = X.Basic.Text.Crypto.Decrypt(crypted, CRYPTO_KEY);
                            string[] arrValues = settingsValue.Split(";");
                            foreach (string val in arrValues)
                            {
                                if (val.StartsWith("host")) DB_SERVER = val.Substring(val.IndexOf("=") + 1).Trim();
                                if (val.StartsWith("user")) DB_USER = val.Substring(val.IndexOf("=") + 1).Trim();
                                if (val.StartsWith("pass")) DB_PASSWORD = val.Substring(val.IndexOf("=") + 1).Trim();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Problem reading MySql connection configuration.");
                        Console.WriteLine(ex.Message);
                    }
                    _dbClient = new DBClient(DB_SERVER, DB_USER, DB_PASSWORD, DB_NAME);
                    _dbClientLogs = new DBClient(DB_SERVER, DB_USER, DB_PASSWORD, DB_NAME_LOGS);
                    break;

                default: throw new NotImplementedException();
            }
        }
        /// <summary>
        /// Vytvorí novú databázu ak ešte neexistuje, alebo vykoná transformáciu databázy ak je staršej verzie.
        /// </summary>
        public void CreateOrTransform() 
        {
            // Vytvorenie novej databázy, ak neexistuje.
            // Sqlite - ak DB neexistuje bude vytvorený nový prázdny súbor pri otvorení pripojenia.
            if (_dbClient.ClientType == DBClient.ClientTypes.MySql)
            {
                X.Data.Management.MySql.CreateDatabase(DB_SERVER, DB_USER, DB_PASSWORD, DB_NAME);
                X.Data.Management.MySql.CreateDatabase(DB_SERVER, DB_USER, DB_PASSWORD, DB_NAME_LOGS);
            }

            // Otvor pripojenie k databáze a zisti verziu databázy.
            bool createTables = false;

            _dbClient.Open();

            if (_dbClient.IsTableExist("main"))
            {
                // Over aktuálnu verziu.            


            }
            else {
                // Vytvor štruktúru pre novú databázu.
                CreateTables();
                createTables = true;
            }

            // Vytvorenie databázy pre logovanie a historiu údajov.
            // Pridružená databáza pre zber údajov používa testovanie verzie podľa hlavnej databázy.
            _dbClientLogs.Open();

            if (createTables) 
            {
                if (!_dbClientLogs.IsTableExist(LogRecord.TABLE_NAME))
                {
                    // -- Environment --
                    // Logs
                    LogList.CreateTable(_dbClientLogs);
                }
            }

        }


        /// <summary>
        /// Vytvorí nové databázové tabuľky.
        /// </summary>
        void CreateTables() 
        {
            // Main
            _dbClient.ExecuteNonQuery("CREATE TABLE main (version INTEGER)");
            _dbClient.ExecuteNonQuery($"INSERT INTO main (version) VALUES ({VERSION})");

            // -- Environment --
            // Settings
            SettingsList.CreateTable(_dbClient);

            // -- RUNTIME --
            // Triggers
            TriggerXList.CreateTable(_dbClient);
            // Buffers (input/output)
            BufferXList.CreateTables(_dbClient);
            // Devices
            DeviceXList.CreateTable(_dbClient);
            // Schedule
            Schedule.ScheduleList.CreateTable(_dbClient);

        }








    }
}
