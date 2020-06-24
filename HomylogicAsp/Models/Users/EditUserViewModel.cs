using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using X.Homylogic;
using X.Homylogic.Models;
using X.Homylogic.Models.Objects;
using X.Homylogic.Models.Objects.Devices;
using X.App.Users;
using System.Collections.Generic;

namespace HomylogicAsp.Models.Users
{
    public class EditUserViewModel
    {
        public Exception SaveException;
        public Int64 ID { get; set; }
        [Required]
        [StringLength(50)]
        public String Name { get; set; }
        public string Password { get; set; }
        public bool IsAdmin { get; set; }
        public bool Disabled { get; set; }
        public List<X.App.Users.PrivilegeList.EditPermissionValues> PermissionValues { get; set; }

        public EditUserViewModel()
        {
            /* this.PermissionValues = user.Privileges.GetEditPermissions();
               This initialization is on .cshtml page when page is loading, because this constructor is also called on Post form when saving record (optimalization for unnecessary getting editing privileges). */
            X.App.Users.UserRecord user = new X.App.Users.UserRecord(Body.Database.DBClient, new X.Homylogic.Models.Users.HomylogicPrivilegeList());
            this.SetViewModel(user);
        }
        public EditUserViewModel(X.App.Users.UserRecord user)
        {
            this.PermissionValues = user.Privileges.EditPermissionsRead();
            this.SetViewModel(user);
        }
        public X.App.Users.UserRecord GetUser()
        {
            UserRecord user;
            if (this.ID > 0)
                user = (UserRecord)Body.Environment.Users.UserList.FindDataRecord(this.ID);
            else
                user = (UserRecord)Body.Environment.Users.UserList.GetInitializedDataRecord();
            user.Name = this.Name;
            user.Password = this.Password;
            user.IsAdmin = this.IsAdmin;
            user.Disabled = this.Disabled;
            return user;
        }
        private void SetViewModel(X.App.Users.UserRecord user) 
        {
            this.ID = user.ID;
            this.Name = user.Name;
            this.Password = user.Password;
            this.IsAdmin = user.IsAdmin;
            this.Disabled = user.Disabled;
        }
    }
}
