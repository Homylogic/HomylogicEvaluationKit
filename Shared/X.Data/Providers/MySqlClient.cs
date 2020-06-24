/* CLIENT FOR MARIA MYSQL SERVER
 * 
 * Používa sa pre vytvorenie inštancie objektu s nastaveným pripojením k databáze.
 * Obsahuje základné funkcie pre prácu s databázov.
 * 
 */
using System;
using System.Data.Common;
using MySql.Data.MySqlClient;

namespace X.Data.Providers
{
    public sealed class MySqlClient : 
        IDBClient
    {
        readonly MySqlConnectionStringBuilder _connectionStringBuilder;
        readonly MySqlConnection _connection;

        public MySqlConnectionStringBuilder ConnectionStringBuilder => _connectionStringBuilder;

        /// <summary>
        /// Inicializuje databázového klienta typu MySQl Server.
        /// </summary>
        /// <param name="server">Názov alebo IP adresa a port servera MySql</param>
        /// <param name="user">Prihlasovacie meno používateľa.</param>
        /// <param name="dbName">Názov databázy (databáza musí už existovať).</param>
        public MySqlClient(string server, string user, string password, string dbName) {
            _connectionStringBuilder = new MySqlConnectionStringBuilder {Server = server, UserID = user, Password = password, Database = dbName};
            _connection = new MySqlConnection(_connectionStringBuilder.ConnectionString);
        }

        public void Open() {
            // Check connection state, and open or reopen connection.
            if (_connection.State == System.Data.ConnectionState.Open || 
                _connection.State == System.Data.ConnectionState.Connecting ||
                _connection.State == System.Data.ConnectionState.Executing ||
                _connection.State == System.Data.ConnectionState.Fetching) return;
            if (_connection.State == System.Data.ConnectionState.Broken)
                _connection.Close();
            _connection.Open();
        }
        public void Close() { 
            _connection.Close();
        }

        public void ExecuteNonQuery(string sql) {
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();
        }

        public object ExecuteScalar(string sql)
        {
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = sql;
            return cmd.ExecuteScalar();
        }

        public DBReader ExecuteReader(string sql) 
        {
            // USING SA NEPOUŽÍVA, pretože by zatvorilo DataReader (stačí zatvoriť DataReader, on by mal zatvoriť aj command).
            var cmd = _connection.CreateCommand();
            cmd.CommandText = sql;
            DbDataReader providerReader = cmd.ExecuteReader();
            return new DBReader(providerReader, DBClient.ClientTypes.MySql);
        }

        public Int64 GetLastInsertedRowID() 
        {
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = "select last_insert_id()";
            UInt64 idU64 = (UInt64)cmd.ExecuteScalar();
            Int64 id = Int64.Parse(idU64.ToString());
            return id;
        }

        public bool IsTableExist(string tableName)
        {
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = $"SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = '{_connectionStringBuilder.Database}' AND table_name = '{tableName}' LIMIT 1";
            Int64 count = (Int64)cmd.ExecuteScalar();
            return (count > 0);
        }

        public void Dispose() {
            if (_connection != null) _connection.Dispose();
        }

    }

}
