/* COMMON DATA READER
 * 
 * Objekt DataReader spoločný pre všetkých providerov.
 * 
 * 
 */
using System;
using System.Data.Common;
using X.Data.Management;

namespace X.Data
{
    public sealed class DBReader : 
        IDisposable
    {
        DbDataReader _providerReader;
        DBClient.ClientTypes _clientType;

        public DBReader(DbDataReader providerReader, DBClient.ClientTypes clientType) { _providerReader = providerReader; _clientType = clientType; }

        public object this[int i] => _providerReader[i]; 
        public object this[string name] => _providerReader[name];

        public bool IsClosed => _providerReader.IsClosed; 
        public int RecordsAffected => _providerReader.RecordsAffected; 
        public int FieldCount => _providerReader.FieldCount;

        public void Close() { _providerReader.Close(); }
        public void Dispose() { _providerReader.DisposeAsync(); }
        public bool NextResult() { return _providerReader.NextResult(); }
        public bool Read() { return _providerReader.Read(); }

        public string GetString(string name)
        {
            var ordinal = _providerReader.GetOrdinal(name);
            if (_providerReader.IsDBNull(ordinal)) return string.Empty;
            return _providerReader.GetString(ordinal);
        }
        public Int32 GetInt32(string name) 
        {
            var ordinal = _providerReader.GetOrdinal(name);
            if (_providerReader.IsDBNull(ordinal)) return 0;
            return _providerReader.GetInt32(ordinal);
        }
        public Int64 GetInt64(string name)
        {
            var ordinal = _providerReader.GetOrdinal(name);
            if (_providerReader.IsDBNull(ordinal)) return 0;
            return _providerReader.GetInt64(ordinal);
        }
        public bool GetBool(string name) 
        {
            var ordinal = _providerReader.GetOrdinal(name);
            if (_providerReader.IsDBNull(ordinal)) return false;
            Int32 boolInt32 = _providerReader.GetInt32(ordinal);
            Management.SqlConvert q = new Management.SqlConvert(_clientType);
            return q.Bol(boolInt32);
        }
        public DateTime GetDateTime(string name) 
        {
            var ordinal = _providerReader.GetOrdinal(name);
            if (_providerReader.IsDBNull(ordinal)) return DateTime.MinValue;
            return _providerReader.GetDateTime(ordinal);
        }
        public float GetFloat(string name)
        {
            var ordinal = _providerReader.GetOrdinal(name);
            if (_providerReader.IsDBNull(ordinal)) return 0F;
            return _providerReader.GetFloat(ordinal);
        }

    }
}
