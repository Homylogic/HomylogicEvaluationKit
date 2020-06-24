/* CLIENT FOR SQLITE SERVER
 * 
 * Používa sa pre vytvorenie inštancie objektu s nastaveným pripojením k databáze.
 * Obsahuje základné funkcie pre prácu s databázov.
 * 
 */
using System;
using System.Data.Common;
using Microsoft.Data.Sqlite;

namespace X.Data.Providers
{
    public sealed class SqliteClient :
        IDBClient
    {
        readonly SqliteConnectionStringBuilder _connectionStringBuilder;
        readonly SqliteConnection _connection;

        public SqliteConnectionStringBuilder ConnectionStringBuilder => _connectionStringBuilder;

        /// <summary>
        /// Inicializuje databázového klienta typu SQLite Server.
        /// </summary>
        /// <param name="dbName">Umiestnenie alebo názov databázy, napr. './MyDatabase.db' (databáza sa bude nachádzať pri programe) Súbor nemusí existovať, vytvorí sa automaticky pri otvorení prvého pripojenia.</param>
        public SqliteClient(string dbName) {
            _connectionStringBuilder = new SqliteConnectionStringBuilder { DataSource = dbName };
            _connection = new SqliteConnection(_connectionStringBuilder.ConnectionString);
        }

        public void Open() { _connection.Open(); }
        public void Close() { _connection.Close(); }

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
            return new DBReader(providerReader, DBClient.ClientTypes.Sqlite);
        }
        public Int64 GetLastInsertedRowID() 
        {
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = "select last_insert_rowid()";
            return (Int64)cmd.ExecuteScalar();
        }

        public bool IsTableExist(string tableName)
        {
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = $"SELECT count(*) FROM sqlite_master WHERE type = 'table' AND name = '{tableName}'";
            Int64 count = (Int64)cmd.ExecuteScalar();
            return (count > 0);
        }

        public void Dispose() { if (_connection != null) _connection.Dispose(); }
    }
}
