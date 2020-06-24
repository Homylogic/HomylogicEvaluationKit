/* UNIVERSLA DATABASE CLIENT 
 * 
 * Používa sa pre vytvorenie inštancie objektu s nastaveným pripojením k databáze.
 * Obsahuje základné funkcie pre prácu s databázov.
 * 
 */
using MySql.Data.MySqlClient;
using System;
using System.Data.Common;
using X.Data.Providers;

namespace X.Data
{
    /// <summary>
    /// Inštancia databázového objektu 'Sqlite' pre prístup k SQL údajom.
    /// </summary>
    public sealed class DBClient : 
        IDisposable
    {
        readonly IDBClient _clientProvider;
        
        public enum ClientTypes { Sqlite, MySql }
        /// <summary>
        /// Typ providera používaného databázového klienta.
        /// </summary>
        public ClientTypes ClientType { get; private set; }

        /// <summary>
        /// Inicializuje databázového klienta typu SQLite Server.
        /// </summary>
        /// <param name="dbName">Umiestnenie alebo názov databázy, napr. './MyDatabase.db' (databáza sa bude nachádzať pri programe).</param>
        public DBClient(string dbName) {
            this.ClientType = ClientTypes.Sqlite;
            _clientProvider = new SqliteClient(dbName);
        }

        /// <summary>
        /// Inicializuje databázového klienta typu Maria MySql Server.
        /// </summary>
        /// <param name="server">Názov alebo IP adresa a port servera MySql</param>
        /// <param name="user">Prihlasovacie meno používateľa.</param>
        /// <param name="dbName">Názov databázy (databáza musí už existovať).</param>
        public DBClient(string server, string user, string password, string dbName)
        {
            this.ClientType = ClientTypes.MySql;
            _clientProvider = new MySqlClient(server, user, password, dbName);
        }

        /// <summary>
        /// Vykoná pripojenie k databáze.
        /// </summary>
        public void Open() { _clientProvider.Open(); }

        /// <summary>
        /// Zatvorí pripojenie k databáze.
        /// </summary>
        public void Close() { _clientProvider.Close(); }

        /// <summary>
        /// Spustí dotaz na databáze.
        /// </summary>
        /// <param name="sql">SQL dotaz.</param>
        public void ExecuteNonQuery(string sql) { _clientProvider.ExecuteNonQuery(sql); }

        /// <summary>
        /// Spustí dotaz na databáze a vráti výsledok.
        /// </summary>
        /// <param name="sql">SQL dotaz.</param>
        public object ExecuteScalar(string sql) { return _clientProvider.ExecuteScalar(sql); }

        /// <summary>
        /// Vytvorí DataReader pre načítanie záznamov.
        /// </summary>
        /// <param name="sql">SQL dotaz.</param>
        public DBReader ExecuteReader(string sql) { return _clientProvider.ExecuteReader(sql); }

        /// <summary>
        /// Vráti identifikátor posledne pridaného záznamu.
        /// </summary>
        public Int64 GetLastInsertedRowID() { return _clientProvider.GetLastInsertedRowID(); }

        /// <summary>
        /// Overí či v databáze existuje uvedená tabuľka.
        /// </summary>
        /// <param name="tableName">Názov tabuľky.</param>
        public bool IsTableExist(string tableName) { return _clientProvider.IsTableExist(tableName); }

        /// <summary>
        /// Returns new DBClient object, with new connection object.
        /// </summary>
        public DBClient Clone() 
        {
            switch (this.ClientType) 
            {
                case ClientTypes.Sqlite:
                    return new DBClient(((SqliteClient)_clientProvider).ConnectionStringBuilder.DataSource);

                case ClientTypes.MySql:
                    MySqlConnectionStringBuilder connectionStringBuilder = ((MySqlClient)_clientProvider).ConnectionStringBuilder;
                    return new DBClient(connectionStringBuilder.Server, connectionStringBuilder.UserID, connectionStringBuilder.Password, connectionStringBuilder.Database);
            }
            throw new InvalidOperationException("Unknown database provider.");
        }

        /// <summary>
        /// Zatvorenie objektu.
        /// </summary>
        public void Dispose() { if (_clientProvider != null) _clientProvider.Dispose(); }

    }

}
