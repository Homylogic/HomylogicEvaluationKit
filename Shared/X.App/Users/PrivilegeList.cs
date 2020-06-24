/* USER PRIVILEGES DATA LIST
 * 
 * Contais privileges per user.
 * 
 */
using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using X.Data;
using X.Data.Factory;
using X.Data.Management;

namespace X.App.Users
{
    public abstract class PrivilegeList : DataList
    {
        readonly DBClient _dbClient;
        protected Hashtable _genericPermissionValues = new Hashtable(); // Contains generic permission property values readed and converted from database, this hashtable is updated in PermissionsUpdate method.

        #region --- DATA LIST ---

        public override DBClient DBClient => _dbClient;
        public override DataRecord GetInitializedDataRecord(DBReader dbReader = null)
        {
            return new PrivilegeRecord(_dbClient) { ParentDataList = this };
        }
        public static void CreateTable(DBClient dbClient)
        {
            SqlStringBuilder sql = new SqlStringBuilder(dbClient.ClientType);
            sql.CreateTable(PrivilegeRecord.TABLE_NAME);
            sql.Append("(");
            sql.AddPrimaryKey();
            sql.Int64("userID");
            sql.Chars("keey"); // Keey bacause Key is used by DB.
            sql.Int32("permission", appendComma: false);
            sql.Append(")");
            dbClient.ExecuteNonQuery(sql.ToString());
            sql.Clear();
            sql.CreateIndex("userID_privilege", "userID", PrivilegeRecord.TABLE_NAME);
            dbClient.ExecuteNonQuery(sql.ToString());
        }

        #endregion

        public PrivilegeList(DBClient dbClient) { _dbClient = dbClient; }

        public Int64 OwnerUserID { get; private set; }
        /// <summary>
        /// Sets owner user ID a sets filter condition for this list.
        /// </summary>
        internal void SetOwnerUserID(Int64 userID) 
        {
            this.OwnerUserID = userID;
            this.FilterCondition = $"userID = {userID}";
        }
        public class EditPermissionValues
        {
            public bool IsCategory { get; set; }
            public bool IsSubCategory { get; set; }
            public string Key { get; set; }
            public string Caption { get; set; }
            public enum ValueTypes { Boolean }
            public ValueTypes ValueType { get; private set; }
            public bool ValueBool { get; set; }
            public EditPermissionValues() { } // Required for asp viewmodel.
            public EditPermissionValues(object value) 
            {
                if (value == null) return;
                if (value is bool) {
                    this.ValueType = ValueTypes.Boolean;
                    this.ValueBool = (bool)value;
                    return;
                }
                throw new NotSupportedException("Unknown user permission value type.");
            }
        }
        /// <summary>
        /// Reads loaded permission values from list and returns all permissions for view and edit.
        /// </summary>
        public List<EditPermissionValues> EditPermissionsRead()
        {
            List<EditPermissionValues> permissions = new List<EditPermissionValues>();
            // Reads all defined categories in this inherited list.
            foreach (PropertyInfo categoryProperty in this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                CategoryAttribute categoryAtrtribute = categoryProperty.GetCustomAttribute<CategoryAttribute>();
                if (categoryAtrtribute != null) {
                    permissions.Add(new EditPermissionValues
                    {
                        IsCategory = true,
                        Caption = categoryAtrtribute.Caption,
                    });

                    // Reads all defined permissions in category.
                    object categoryPropValue = categoryProperty.GetValue(this);
                    foreach (PropertyInfo permissionProperty in categoryPropValue.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                    {
                        PermissionAttribute permissionAttribute = permissionProperty.GetCustomAttribute<PermissionAttribute>();
                        if (permissionAttribute != null) {
                            permissions.Add(new EditPermissionValues(permissionProperty.GetValue(categoryPropValue))
                                {
                                    Key = permissionAttribute.Key,
                                    Caption = permissionAttribute.Caption
                                });
                            continue;
                        }
                        GenericPermissionAttribute genericPermissionAttribute = permissionProperty.GetCustomAttribute<GenericPermissionAttribute>();
                        if (genericPermissionAttribute != null)
                        {
                            permissions.Add(new EditPermissionValues
                            {
                                IsSubCategory = true,
                                Caption = genericPermissionAttribute.Caption,
                            });
                            object permissionPropValue = permissionProperty.GetValue(categoryPropValue);
                            if (permissionPropValue is List<GenericPermissionValues> genericPermissionList) {
                                foreach (GenericPermissionValues genericPermission in genericPermissionList)
                                {
                                    permissions.Add(new EditPermissionValues(genericPermission.PermissionValue)
                                    {
                                        Key = genericPermission.Key,
                                        Caption = genericPermission.Caption
                                    });
                                }
                            }
                            continue;
                        }
                    }
                }
            }
            return permissions;
        }
        /// <summary>
        /// Saves edited permission to database from permissions values and updates this class properties values.
        /// </summary>
        /// <param name="permissions">Editing permissions values.</param>
        public void EditPermissionsSave(List<EditPermissionValues> permissions) 
        {
            // Convert permissions edit values to DB values Int32, by privilege key.
            SqlConvert q = new SqlConvert(this.DBClient.ClientType);
            Hashtable permissionValues = new Hashtable();
            foreach (EditPermissionValues permission in permissions)
            {
                // if (permission.IsCategory || permission.IsSubCategory) continue;
                // We can use 'Key' rather, because of ASP ViewModel @Html.HiddenFor now require only Key.
                if (string.IsNullOrEmpty(permission.Key)) continue;
                Int32 value = 0;
                switch (permission.ValueType) 
                {
                    case EditPermissionValues.ValueTypes.Boolean:
                        value = q.Innt32(permission.ValueBool);
                        break;
                }
                permissionValues.Add(permission.Key, value);
            }

            // Reads loaded records to helper hashtable (records by permissions key).
            Hashtable privilegeRecords = new Hashtable();
            for (int i = 0; i < this.List.Count; i++)
            {
                PrivilegeRecord privilege = (PrivilegeRecord)this.List[i];
                privilegeRecords.Add(privilege.Key, privilege);
            }

            // Update data list, create new records or update values on existing records.
            foreach (DictionaryEntry permissionEntry in permissionValues)
            {
                string key = (string)permissionEntry.Key;
                PrivilegeRecord privilege = null;
                if (privilegeRecords.Count > 0 && privilegeRecords.ContainsKey(key)) {
                    privilege = (PrivilegeRecord)privilegeRecords[key];
                }
                else {
                    privilege = (PrivilegeRecord)this.GetInitializedDataRecord();
                    privilege.UserID = this.OwnerUserID;
                    privilege.Key = key;
                }
                privilege.Permission = (Int32)permissionEntry.Value;
                privilege.Save();
            }
        }
        /// <summary>
        /// Updates permissions property values, according to current loaded database records.
        /// </summary>
        public void PermissionsUpdate()
        {
            // Clears all generic permissions from database.
            _genericPermissionValues.Clear();

            // Reads all privilege permissions by key.
            SqlConvert q = new SqlConvert(this.DBClient.ClientType);
            Hashtable permissionValues = new Hashtable();
            for (int i = 0; i < this.List.Count; i++)
            {
                PrivilegeRecord privilege = (PrivilegeRecord)this.List[i];
                permissionValues.Add(privilege.Key, privilege.Permission);
            }

            // Updates all class properties values.
            foreach (PropertyInfo categoryProperty in this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                CategoryAttribute categoryAtrtribute = categoryProperty.GetCustomAttribute<CategoryAttribute>();
                if (categoryAtrtribute != null)
                {
                    // Reads all defined permissions in category.
                    object categoryPropValue = categoryProperty.GetValue(this);
                    foreach (PropertyInfo permissionProperty in categoryPropValue.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                    {
                        PermissionAttribute permissionAttribute = permissionProperty.GetCustomAttribute<PermissionAttribute>();
                        if (permissionAttribute != null)
                        {
                            if (permissionValues.ContainsKey(permissionAttribute.Key)) { 
                                System.Type valueType = permissionProperty.PropertyType;
                                if (valueType == typeof(bool))
                                    permissionProperty.SetValue(categoryPropValue, q.Bol((Int32)permissionValues[permissionAttribute.Key]));
                            }
                            continue;
                        }
                        GenericPermissionAttribute genericPermissionAttribute = permissionProperty.GetCustomAttribute<GenericPermissionAttribute>();
                        if (genericPermissionAttribute != null)
                        {
                            object permissionPropValue = permissionProperty.GetValue(categoryPropValue);
                            if (permissionPropValue is List<GenericPermissionValues> genericPermissionList)
                            {
                                for (int i = 0; i < genericPermissionList.Count; i++)
                                {
                                    GenericPermissionValues genericPermission = genericPermissionList[i];
                                    if (permissionValues.ContainsKey(genericPermission.Key))
                                    {
                                        System.Type valueType = genericPermission.PermissionValue.GetType();
                                        if (valueType == typeof(bool)) 
                                        {
                                            bool value = q.Bol((Int32)permissionValues[genericPermission.Key]);
                                            if (_genericPermissionValues.ContainsKey(genericPermission.Key)) {
                                                _genericPermissionValues[genericPermission.Key] = value;
                                            }
                                            else {
                                                _genericPermissionValues.Add(genericPermission.Key, value);
                                            }
                                        }
                                    }
                                }
                            }
                            continue;
                        }
                    }
                }
            }
        }

        #region --- ATTRIBUTES ---

        [AttributeUsage(AttributeTargets.Property)]
        public sealed class CategoryAttribute : Attribute 
        {
            readonly string _caption;
            public string Caption => _caption;
            public CategoryAttribute(string caption) { _caption = caption; }
        }
        [AttributeUsage(AttributeTargets.Property)]
        public sealed class PermissionAttribute : Attribute
        {
            readonly string _key, _caption;
            public string Key => _key;
            public string Caption => _caption;
            public PermissionAttribute(string key, string caption) { _key = key; _caption = caption; }
        }
        /// <summary>
        /// Structure for generic (generated) permissions etc. by defined objects (etc. devices).
        /// </summary>
        public struct GenericPermissionValues 
        {
            public string Key;
            public string Caption;
            public object PermissionValue;
        }
        [AttributeUsage(AttributeTargets.Property)]
        public sealed class GenericPermissionAttribute : Attribute
        {
            readonly string _caption;
            public string Caption => _caption;
            public GenericPermissionAttribute(string caption) { _caption = caption; }
        }

        #endregion


    }
}
