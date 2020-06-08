/* DATABASE CLIENT INTERFACE 
 * 
 * Používa sa pre implementáciu jednotlivých databázových providerov.
 * 
 */
using System;

namespace X.Data.Providers
{
    interface IDBClient : IDisposable
    {
        public void Open();
        public void Close();
        public void ExecuteNonQuery(string sql);
        public object ExecuteScalar(string sql);
        public DBReader ExecuteReader(string sql);
        public Int64 GetLastInsertedRowID();
        public bool IsTableExist(string tableName);
    }
}
