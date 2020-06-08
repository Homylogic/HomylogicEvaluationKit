/* SETTINGS DATA LIST
 * 
 * Obsahuje zoznam načítaných nastavení.
 * 
 * 
 */
using System;
using System.Collections.Generic;
using System.Text;
using X.Data;
using X.Data.Factory;

namespace X.App.Settings
{
    public class SettingsList : DataList
    {
        readonly DBClient _dbClient;

        #region --- DATA LIST ---

        public override DBClient DBClient => _dbClient;
        public override DataRecord GetInitializedDataRecord(DBReader dbReader = null)
        {
            return new SettingsRecord(_dbClient) { ParentDataList = this };
        }
        public static void CreateTable(DBClient dbClient)
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendFormat("CREATE TABLE {0} (", SettingsRecord.TABLE_NAME);
            if (dbClient.ClientType == DBClient.ClientTypes.Sqlite)
                sql.Append("id INTEGER PRIMARY KEY, ");
            else
                sql.Append("id INTEGER PRIMARY KEY AUTO_INCREMENT, ");
            sql.Append("section INTEGER, ");
            sql.Append("key DATETIME, ");
            sql.Append("flag INTEGER, ");
            sql.Append("value TEXT");
            sql.Append(")");
            dbClient.ExecuteNonQuery(sql.ToString());
        } 

        #endregion

        public SettingsList(DBClient dbClient) { _dbClient = dbClient; }

    }
}
