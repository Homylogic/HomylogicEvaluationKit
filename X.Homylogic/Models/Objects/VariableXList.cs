/* HOMYLOGIC VARIABLE X LIST
 * 
 * Contains list of loaded variables.
 * 
 * 
 */
using X.Data;
using X.Data.Factory;
using X.Data.Management;
using X.Homylogic.Models.Objects.Variables;

namespace X.Homylogic.Models.Objects
{
    public sealed class VariableXList : Factory.ObjectXList
    {
        #region --- DATA LIST ---

        public static void CreateTable(DBClient dbClient)
        {
            SqlStringBuilder sql = new SqlStringBuilder(dbClient.ClientType);
            sql.CreateTable(VariableX.TABLE_NAME);
            sql.Append("(");
            sql.AddPrimaryKey();
            sql.Chars("name", appendComma:false); sql.UniqueNotNull();
            sql.Text("notice");
            sql.Int01("showOnHome"); // For future use (not used now).
            sql.Int32("variableType");
            sql.Text("defaultValue");
            sql.Int01("disabled", appendComma:false);
            sql.Append(")");
            dbClient.ExecuteNonQuery(sql.ToString());
        }
        public override DataRecord GetInitializedDataRecord(DBReader dbReader = null) 
        {
            if (dbReader != null)
            {
                return GetInitializedVariable((VariableX.VariableTypes)dbReader.GetInt32("variableType"));
            }
            return new VariableX() { ParentDataList = this }; 
        }
        public VariableX GetInitializedVariable(VariableX.VariableTypes variableType) 
        {
            if (variableType == VariableX.VariableTypes.Text) return new TextVariableX() { ParentDataList = this };
            if (variableType == VariableX.VariableTypes.Integer) return new IntegerVariableX() { ParentDataList = this };
            if (variableType == VariableX.VariableTypes.Decimal) return new DecimalVariableX() { ParentDataList = this };
            if (variableType == VariableX.VariableTypes.YesNo) return new YesNoVariableX() { ParentDataList = this };
            return new VariableX() { ParentDataList = this };
        }
        public void LoadData()
        {
            base.LoadData("name, variableType");
        }

        #endregion

    }
}
