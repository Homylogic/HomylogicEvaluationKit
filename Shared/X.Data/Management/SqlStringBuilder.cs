using System;
using System.Collections.Generic;
using System.Text;

namespace X.Data.Management
{
    public sealed class SqlStringBuilder
    {
        readonly DBClient.ClientTypes _clientType;
        StringBuilder _stringBuilder = new StringBuilder();

        public SqlStringBuilder(DBClient.ClientTypes clientType) { _clientType = clientType; }

        /// <summary>
        /// CREATE TABLE [name] ...
        /// </summary>
        public void CreateTable(string name) { _stringBuilder.AppendFormat("CREATE TABLE {0} ", name); }
        /// <summary>
        /// CREATE INDEX {0} ON {1} (userID) ...
        /// </summary>
        public void CreateIndex(string indexName, string fieldName, string tableName) { _stringBuilder.AppendFormat("CREATE INDEX {0} ON {1} ({2})", indexName, tableName, fieldName); }
        /// <summary>
        /// Adds unique id - INTEGER PRIMARY KEY AUTO_INCREMENT{, by argument appendComma} ...
        /// </summary>
        public void AddPrimaryKey(bool appendComma = true) 
        {
            _stringBuilder.Append((_clientType switch
            {
                DBClient.ClientTypes.Sqlite => "id INTEGER PRIMARY KEY",
                DBClient.ClientTypes.MySql => "id BIGINT PRIMARY KEY AUTO_INCREMENT",
                _ => string.Empty
            }));
            if (appendComma) _stringBuilder.Append(", ");
        }
        /// <summary>
        /// Adds integer numeric value - [name] INTEGER{, by argument appendComma} ...
        /// </summary>
        public void Int32(string name, bool appendComma = true)
        {
            _stringBuilder.Append((_clientType switch
            {
                DBClient.ClientTypes.Sqlite => $"{name} INTEGER",
                DBClient.ClientTypes.MySql => $"{name} INT",
                _ => string.Empty
            }));
            if (appendComma) _stringBuilder.Append(", ");
        }
        /// <summary>
        /// Adds long numeric value - [name] BIG-INTEGER{, by argument appendComma} ...
        /// </summary>
        public void Int64(string name, bool appendComma = true)
        {
            _stringBuilder.Append((_clientType switch
            {
                DBClient.ClientTypes.Sqlite => $"{name} INTEGER",
                DBClient.ClientTypes.MySql => $"{name} BIGINT",
                _ => string.Empty
            }));
            if (appendComma) _stringBuilder.Append(", ");
        }
        /// <summary>
        /// Adds integer numeric value for store boolean value - [name] INTEGER{, by argument appendComma} ...
        /// </summary>
        public void Int01(string name, bool appendComma = true)
        {
            _stringBuilder.Append((_clientType switch
            {
                DBClient.ClientTypes.Sqlite => $"{name} INTEGER",
                DBClient.ClientTypes.MySql => $"{name} TINYINT",
                _ => string.Empty
            }));
            if (appendComma) _stringBuilder.Append(", ");
        }
        /// <summary>
        /// Adds float numeric value - [name] FLOAT{, by argument appendComma} ...
        /// </summary>
        public void Float(string name, bool appendComma = true)
        {
            _stringBuilder.Append((_clientType switch
            {
                DBClient.ClientTypes.Sqlite => $"{name} INTEGER",
                DBClient.ClientTypes.MySql => $"{name} FLOAT",
                _ => string.Empty
            }));
            if (appendComma) _stringBuilder.Append(", ");
        }
        /// <summary>
        /// Adds date and time value - [name] DATETIME{, by argument appendComma} ...
        /// </summary>
        public void DateTime(string name, bool appendComma = true)
        {
            _stringBuilder.AppendFormat("{0} DATETIME", name);
            if (appendComma) _stringBuilder.Append(", ");
        }
        /// <summary>
        /// Adds short text - [name] VARCHAR{, by argument appendComma} ...
        /// Max varLength is 250 chars.
        /// </summary>
        public void Chars(string name, int varLength = 200, bool appendComma = true)
        {
            if (varLength > 250) throw new ArgumentOutOfRangeException("Maximum database field text length is 250 chars.");
            _stringBuilder.Append((_clientType switch
            {
                DBClient.ClientTypes.Sqlite => $"{name} TEXT",
                DBClient.ClientTypes.MySql => $"{name} VARCHAR({varLength})",
                _ => string.Empty
            }));
            if (appendComma) _stringBuilder.Append(", ");
        }
        /// <summary>
        /// Adds long text - [name] TEXT{, by argument appendComma} ...
        /// </summary>
        public void Text(string name, bool appendComma = true)
        {
            _stringBuilder.AppendFormat("{0} TEXT", name);
            if (appendComma) _stringBuilder.Append(", ");
        }
        /// <summary>
        /// Adds unique arguments on field - ... NOT NULL UNIQUE
        /// </summary>
        public void UniqueNotNull(bool appendComma = true)
        {
            _stringBuilder.Append(" NOT NULL UNIQUE");
            if (appendComma) _stringBuilder.Append(", ");
        }

        /// <summary>
        /// Appends any text etc. brackets ().
        /// </summary>
        public void Append(string value) { _stringBuilder.Append(value); }
        /// <summary>
        /// Adds MySQL settings - ... ENGINE = MYISAM.
        /// </summary>
        public void EngineMyISAM()
        {
            if (_clientType == DBClient.ClientTypes.MySql)
                _stringBuilder.Append(" ENGINE = MYISAM");
        }

        public void Clear() { _stringBuilder.Clear(); } 
        public override string ToString() { return _stringBuilder.ToString(); }
    }
}
