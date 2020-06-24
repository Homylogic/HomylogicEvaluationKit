/* USER DATA LIST
 * 
 * Contains list of users.
 * 
 * 
 */
using System;
using System.Collections.Generic;
using System.Text;
using X.Data;
using X.Data.Factory;
using X.Data.Management;

namespace X.App.Users
{
    public sealed class UserList : DataList
    {
        readonly DBClient _dbClient;
        public delegate PrivilegeList InitializePrivilegeList();
        readonly InitializePrivilegeList _initializePrivilegeListDelegate;

        #region --- DATA LIST ---

        public override DBClient DBClient => _dbClient;
        public override DataRecord GetInitializedDataRecord(DBReader dbReader = null)
        {
            PrivilegeList privilegeList = _initializePrivilegeListDelegate.Invoke();
            return new UserRecord(_dbClient, privilegeList) { ParentDataList = this };
        }
        public static void CreateTable(DBClient dbClient)
        {
            SqlStringBuilder sql = new SqlStringBuilder(dbClient.ClientType);
            sql.CreateTable(UserRecord.TABLE_NAME);
            sql.Append("(");
            sql.AddPrimaryKey();
            sql.Chars("name");
            sql.Text("password");
            sql.Int01("isAdmin");
            sql.Int01("disabled");
            sql.Int01("canDelete", appendComma: false);
            sql.Append(")");
            dbClient.ExecuteNonQuery(sql.ToString());
        }

        #endregion

        public UserList(DBClient dbClient, InitializePrivilegeList initializePrivilegeListDelegate) { _dbClient = dbClient; _initializePrivilegeListDelegate = initializePrivilegeListDelegate; }

        /// <summary>
        /// Loads users and their privilegeas.
        /// </summary>
        public void LoadData()
        {
            base.LoadData();

            // Load privileges and update class permission property values according to loaded data.
            foreach (UserRecord user in this.List)
            {
                user.Privileges.LoadData();
                user.Privileges.PermissionsUpdate();
            }
        }

    }
}
