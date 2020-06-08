/* HOMYLOGIC BUFFER X LIST
 * 
 * Obsahuje zoznam načítaných údajov (input buffer) zo zariadenia alebo zoznam údajov pre odoslanie do zariadenia (output buffer).
 * 
 * 
 */
using System;
using System.Text;
using X.Data;

namespace X.Homylogic.Models.Objects
{
    public abstract class BufferXList : Factory.ObjectXList
    {
        /// <summary>
        /// Typ zásobníka, či sa používa pre čítanie alebo zapisovanie do/zo zariadenia.
        /// </summary>
        public BufferX.BufferTypes BufferType { get; protected set; }

        #region --- DATA LIST --- 

        public static void CreateTables(DBClient dbClient)
        {
            // Input buffer.
            StringBuilder sql = new StringBuilder();
            sql.AppendFormat("CREATE TABLE {0} (", Buffers.InputBufferX.TABLE_NAME);
            if (dbClient.ClientType == DBClient.ClientTypes.Sqlite)
                sql.Append("id INTEGER PRIMARY KEY, ");
            else
                sql.Append("id INTEGER PRIMARY KEY AUTO_INCREMENT, ");
            sql.Append("name TEXT, ");
            sql.Append("notice TEXT, ");
            sql.Append("disabled INTEGER, ");
            sql.Append("showOnHome INTEGER, ");
            sql.Append("deviceID INTEGER, ");
            sql.Append("processTime DATETIME, ");
            sql.Append("tag INTEGER, ");
            sql.Append("data TEXT");
            sql.Append(")");
            dbClient.ExecuteNonQuery(sql.ToString());

            // Output buffer.
            sql.Clear();
            sql.AppendFormat("CREATE TABLE {0} (", Buffers.OutputBufferX.TABLE_NAME);
            if (dbClient.ClientType == DBClient.ClientTypes.Sqlite)
                sql.Append("id INTEGER PRIMARY KEY, ");
            else
                sql.Append("id INTEGER PRIMARY KEY AUTO_INCREMENT, ");
            sql.Append("name TEXT, ");
            sql.Append("notice TEXT, ");
            sql.Append("disabled INTEGER, ");
            sql.Append("showOnHome INTEGER, ");
            sql.Append("deviceID INTEGER, ");
            sql.Append("processTime DATETIME, ");
            sql.Append("isProcessed INTEGER, ");
            sql.Append("data TEXT");
            sql.Append(")");
            dbClient.ExecuteNonQuery(sql.ToString());
        }
        public void LoadData()
        {
            base.LoadData("processTime DESC");
        }

        #endregion

    }
}
