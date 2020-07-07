/* HOMYLOGIC USER
 * 
 * Manage users and active logged user with privileges.
 * 
 */
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Text;
using X.App.Users;
using X.Data;
using X.Homylogic.Models.Objects;
using X.Homylogic.Models.Objects.Devices.Homyoko;

namespace X.Homylogic.Models
{
    public sealed class Users
    {
        /// <summary>
        /// Contains logged clients access tokens for users key = UserID, value = AccessToken-Guid.
        /// </summary>
        readonly List<KeyValuePair<Int64, string>> UserAccessTokens = new List<KeyValuePair<Int64, string>>(); 
        /// <summary>
        /// List of all loaded users.
        /// </summary>
        public UserList UserList { get; private set; }
        /// <summary>
        /// Defines homylogic application privileges (default permission values are used for guest user).
        /// </summary>
        public sealed class HomylogicPrivilegeList : PrivilegeList
        {
            public sealed class SettingsCategory
            {
                readonly HomylogicPrivilegeList _privilegeList;
                /// <summary>
                /// Permission for whole settings.
                /// </summary>
                [Permission("settings-global", "Allow settings")]
                public bool AllSettings { get; set; } = false;
                /// <summary>
                /// Permission home settings.
                /// </summary>
                [Permission("settings-application", "Allow application tab")]
                public bool Application { get; set; } = false;

                public SettingsCategory(HomylogicPrivilegeList privilegeList) { _privilegeList = privilegeList; }
            }
            public sealed class DevicesCategory
            {
                readonly HomylogicPrivilegeList _privilegeList;
                /// <summary>
                /// Permission for device list.
                /// </summary>
                [Permission("device-list", "Allow device list")]
                public bool DeviceList { get; set; } = false;
                /// <summary>
                /// Permissions for controlling defined devices.
                /// </summary>
                [GenericPermission("Allow devices control")]
                public List<GenericPermissionValues> CanControl
                {
                    get 
                    {
                        List<GenericPermissionValues> permissions = new List<GenericPermissionValues>();
                        for (int i = 0; i < Body.Runtime.Devices.List.Count; i++)
                        {
                            DeviceX device = (DeviceX)Body.Runtime.Devices.List[i];
                            if (device is IVTController)
                            {
                                string key = $"device-control-{device.ID}";
                                bool value = true; // Default value is allowed control (etc. for new added device, need disable privillege)
                                if (this._privilegeList._genericPermissionValues.ContainsKey(key))
                                    value = (bool)this._privilegeList._genericPermissionValues[key];

                                permissions.Add(new GenericPermissionValues() { Key = key, Caption = device.Name, PermissionValue = value });
                            }
                        }
                        return permissions;
                    }
                }
                public bool CanControlID(Int64 deviceID) 
                {
                    string key = $"device-control-{deviceID}";
                    foreach (GenericPermissionValues genericPermission in this.CanControl)
                    {
                        if (genericPermission.Key == key)
                            return (bool)genericPermission.PermissionValue;
                    }
                    return true; // Default value is allowed control (etc. for new added device, need disable privillege)
                }

                public DevicesCategory(HomylogicPrivilegeList privilegeList) { _privilegeList = privilegeList; }
            }
            public sealed class TriggersCategory
            {
                readonly HomylogicPrivilegeList _privilegeList;
                /// <summary>
                /// Permission for trigger list.
                /// </summary>
                [Permission("trigger-list", "Allow trigger list")]
                public bool TriggerList { get; set; } = false;

                public TriggersCategory(HomylogicPrivilegeList privilegeList) { _privilegeList = privilegeList; }
            }
            /// <summary>
            /// Settings category.
            /// </summary>
            [Category("Settings")]
            public SettingsCategory Settings { get; private set; }
            /// <summary>
            /// Devices category.
            /// </summary>
            [Category("Devices")]
            public DevicesCategory Devices { get; private set; }
            /// <summary>
            /// Devices category.
            /// </summary>
            [Category("Triggers")]
            public TriggersCategory Triggers { get; private set; }

            public HomylogicPrivilegeList() : base(Body.Database.DBClient.Clone()) 
            {
                this.Settings = new SettingsCategory(this);
                this.Devices = new DevicesCategory(this);
                this.Triggers = new TriggersCategory(this);
            }
        }
        /// <summary>
        /// Adds default users to database.
        /// </summary>
        public static void CreateUsers(DBClient dbClient)
        {
            // This users need always same ID (Guest = 1, Admin = 2).
            // Guest common user.
            // ! First create guest user, because first user is used when we can't found default defined user.
            UserRecord guestUser = new UserRecord(dbClient, null) { Name = "Guest", CanDelete = false};
            guestUser.Save();
            // Admin superuser.
            UserRecord adminUser = new UserRecord(dbClient, null) { Name = "Admin", IsAdmin = true, CanDelete = false };
            adminUser.Save();
        }
        /// <summary>
        /// Initialize and loads user list and users privileges.
        /// </summary>
        public void Load() 
        {
            UserList.InitializePrivilegeList initializePrivilegeList = new UserList.InitializePrivilegeList(InitializePrivilegeList);
            if (this.UserList == null) this.UserList = new UserList(Body.Database.DBClient.Clone(), initializePrivilegeList);
            this.UserList.LoadData();
        }
        /// <summary>
        /// Returns initialized privileges.
        /// </summary>
        HomylogicPrivilegeList InitializePrivilegeList() {return new HomylogicPrivilegeList();}
        /// <summary>
        /// Returns currently logged user by access token or returns default user (usaually guest user).
        /// </summary>
        public UserRecord GetCurrentUser(string accessToken) 
        {
            UserRecord currentUser = null;
            // Find user by access token.
            if (!string.IsNullOrEmpty(accessToken)) { 
                foreach (KeyValuePair<Int64, string> userAccessToken in this.UserAccessTokens)
                {
                    if (userAccessToken.Value == accessToken) { 
                        currentUser = (UserRecord)Body.Environment.Users.UserList.FindDataRecord(userAccessToken.Key);
                        if (currentUser.Disabled) currentUser = null;
                        break;
                    }
                }
            }
            // User not found by token, return default user.
            if (currentUser == null) {
                currentUser = (UserRecord)Body.Environment.Users.UserList.FindDataRecord(Body.Environment.Settings.Security.DefaultUserID);
            }
            // Default user not exist, returns first defined user (should be guest).
            // User not found by token, return default user.
            if (currentUser == null) {
                currentUser = (UserRecord)Body.Environment.Users.UserList.List[0];
            }
            return currentUser;
        }
        /// <summary>
        /// Try logs in user, after sucessfull login adds client access token cookie value to user access list.
        /// </summary>
        public bool LogIn(string name, string password) 
        {
            if (string.IsNullOrEmpty(name)) return false;
            if (password == null) password = string.Empty;

            for (int i = 0; i < this.UserList.List.Count; i++)
            {
                UserRecord user = (UserRecord)this.UserList.List[i];
                if (user.Disabled) continue;
                if (user.Name.ToLower() == name.ToLower()) {
                    string userPassword = user.Password;
                    if (userPassword == null) userPassword = string.Empty;
                    return (userPassword == password);                
                }
            }
            return false;
        }
        /// <summary>
        /// Adds client to allowed access tokens.
        /// </summary>
        public string AddUserAccess(string userName) 
        {
            Int64 userID = 0;
            for (int i = 0; i < this.UserList.List.Count; i++)
            {
                UserRecord user = (UserRecord)this.UserList.List[i];
                if (user.Name.ToLower() == userName.ToLower())
                {
                    userID = user.ID;
                    break;
                }
            }
            string accessToken = Guid.NewGuid().ToString();
            KeyValuePair<Int64, string> userAccessToken = new KeyValuePair<Int64, string>(userID, accessToken);
            if (!this.UserAccessTokens.Contains(userAccessToken))
                this.UserAccessTokens.Add(userAccessToken);
            return accessToken;
        }
    }
}
