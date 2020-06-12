/* HOMYLOGIC BUFFER X LIST
 * 
 * Obsahuje zoznam načítaných údajov (input buffer) zo zariadenia alebo zoznam údajov pre odoslanie do zariadenia (output buffer).
 * 
 * 
 */
using System;
using System.Text;
using X.Data;
using X.Data.Management;

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
            SqlStringBuilder sql = new SqlStringBuilder(dbClient.ClientType);
            sql.CreateTable(Buffers.InputBufferX.TABLE_NAME);
            sql.Append("(");
            sql.AddPrimaryKey();
            sql.Chars("name");
            sql.Text("notice");
            sql.Int01("disabled");
            sql.Int01("showOnHome");
            sql.Int64("deviceID");
            sql.DateTime("processTime");
            sql.Int32("tag");
            sql.Text("data", appendComma: false);
            sql.Append(")");
            sql.EngineMyISAM();
            dbClient.ExecuteNonQuery(sql.ToString());
            sql.Clear();
            // ! Sqlite require unique index names per database !
            sql.CreateIndex("deviceID_inBuffer", "deviceID", Buffers.InputBufferX.TABLE_NAME);
            dbClient.ExecuteNonQuery(sql.ToString());

            // Output buffer.
            sql.Clear();
            sql.CreateTable(Buffers.OutputBufferX.TABLE_NAME);
            sql.Append("(");
            sql.AddPrimaryKey();
            sql.Chars("name");
            sql.Text("notice");
            sql.Int01("disabled");
            sql.Int01("showOnHome");
            sql.Int64("deviceID");
            sql.DateTime("processTime");
            sql.Int01("isProcessed");
            sql.Text("data", appendComma: false);
            sql.Append(")");
            sql.EngineMyISAM();
            dbClient.ExecuteNonQuery(sql.ToString());
            sql.Clear();
            sql.CreateIndex("deviceID_outBuffer", "deviceID", Buffers.OutputBufferX.TABLE_NAME);
            dbClient.ExecuteNonQuery(sql.ToString());
        }
        public void LoadData()
        {
            base.LoadData("processTime DESC");
        }

        #endregion

    }
}
