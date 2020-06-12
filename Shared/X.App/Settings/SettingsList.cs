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
using X.Data.Management;

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
            SqlStringBuilder sql = new SqlStringBuilder(dbClient.ClientType);
            sql.CreateTable(SettingsRecord.TABLE_NAME);
            sql.Append("(");
            sql.AddPrimaryKey();
            sql.Int64("userID");
            sql.Chars("section");
            sql.Chars("settKey");
            sql.Chars("flag");
            sql.Chars("settValue", appendComma: false);
            sql.Append(")");
            dbClient.ExecuteNonQuery(sql.ToString());
            sql.Clear();
            sql.CreateIndex("userID", "userID", SettingsRecord.TABLE_NAME);
            dbClient.ExecuteNonQuery(sql.ToString());
        } 

        #endregion

        public SettingsList(DBClient dbClient) { _dbClient = dbClient; }

    }
}
