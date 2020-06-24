/* USER DATA RECORD
 * 
 * Contains main user record values.
 * 
 */
using System;
using System.Collections.Generic;
using System.Text;
using X.Data;
using X.Data.Factory;

namespace X.App.Users
{
    public sealed class UserRecord : DataRecord
    {
        const string CRYPTO_KEY = "!?<>čťšôôôôô125,5222";
        readonly DBClient _dbClient;
        readonly PrivilegeList _privilegeList;

        #region --- DATA PROPERTIES ---

        /// <summary>
        /// User login name.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// User login password.
        /// </summary>
        public string Password { get; set; }
        /// <summary>
        /// True for superuser (privileges are ignored admin has always full access).
        /// </summary>
        public bool IsAdmin { get; set; } = false;
        /// <summary>
        /// True for temporary disabled user (disabled user can't login, superadmin can't disabled).
        /// </summary>
        public bool Disabled { get; set; } = false;
        /// <summary>
        /// False for user who can't be deleted, etc. admin.
        /// </summary>
        public bool CanDelete { get; set; } = true;
        /// <summary>
        /// User privileges.
        /// </summary>
        public PrivilegeList Privileges => _privilegeList;

        #endregion

        #region --- DATA RECORD ---

        public override DBClient DBClient => _dbClient;
        public const string TABLE_NAME = "users";
        public override string TableName => TABLE_NAME;

        public override void SetRecordValues(DBReader dbReader, List<string> tags)
        {
            base.SetRecordValues(dbReader, tags);
            this.Name = dbReader.GetString("name");
            string passCrypted = dbReader.GetString("password");
            this.Password = string.IsNullOrEmpty(passCrypted) ? string.Empty : X.Basic.Text.Crypto.Decrypt(passCrypted, CRYPTO_KEY);
            this.IsAdmin = dbReader.GetBool("isAdmin");
            this.Disabled = dbReader.GetBool("disabled");
            this.CanDelete = dbReader.GetBool("canDelete");
            _privilegeList.SetOwnerUserID(this.ID); // Sets filter codition for DataList.
        }
        public override string SqlInsert(Data.Management.SqlConvert q, List<string> tags)
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendFormat("INSERT INTO {0} (", this.TableName);
            sql.Append("name, password, isAdmin, disabled, canDelete) VALUES (");
            sql.AppendFormat("{0}, ", q.Str(this.Name));
            string passCrypted = string.IsNullOrEmpty(this.Password) ? string.Empty : X.Basic.Text.Crypto.Encrypt(this.Password, CRYPTO_KEY);
            sql.AppendFormat("{0}, ", q.Str(passCrypted));
            sql.AppendFormat("{0}, ", q.Innt32(this.IsAdmin));
            sql.AppendFormat("{0}, ", q.Innt32(this.Disabled));
            sql.AppendFormat("{0})", q.Innt32(this.CanDelete));
            return sql.ToString();
        }
        public override string SqlUpdate(Data.Management.SqlConvert q, List<string> tags)
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendFormat("UPDATE {0} SET ", this.TableName);
            sql.AppendFormat("name = {0}, ", q.Str(this.Name));
            string passCrypted = string.IsNullOrEmpty(this.Password) ? string.Empty : X.Basic.Text.Crypto.Encrypt(this.Password, CRYPTO_KEY);
            sql.AppendFormat("password = {0}, ", q.Str(passCrypted)); ;
            sql.AppendFormat("isAdmin = {0}, ", q.Innt32(this.IsAdmin));
            sql.AppendFormat("disabled = {0}, ", q.Innt32(this.Disabled));
            sql.AppendFormat("canDelete = {0}", q.Innt32(this.CanDelete));
            return sql.ToString();
        }
        public override void Save()
        {
            base.Save();
            if (_privilegeList != null)
                _privilegeList.SetOwnerUserID(this.ID);
        }
        public override void Delete(long id)
        {
            if (_privilegeList != null)
                _privilegeList.DeleteAll(); // Deletes all according to filter condition (owner user ID).
            base.Delete(id);
        }

        #endregion

        public UserRecord(DBClient dbClient, PrivilegeList privilegeList) { _dbClient = dbClient; _privilegeList = privilegeList; }




    }
}
