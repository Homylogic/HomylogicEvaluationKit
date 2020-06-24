using System;
using System.ComponentModel.DataAnnotations;
using X.Homylogic;
using X.Homylogic.Models;
using X.App.Users;
using Microsoft.AspNetCore.Http;
using X.Data.Factory;

namespace HomylogicAsp.Models.Users
{
    public class UsersViewModel 
    {
        public UserList UserList => Body.Environment.Users.UserList;
        public Int64 SelectedID { get; set; }

        public UsersViewModel(HttpRequest httpRequest)
        {
            try
            {
                if (httpRequest != null)
                {
                    string cookValue = httpRequest.Cookies["SelectedUserID"];
                    if (string.IsNullOrEmpty(cookValue))
                    {
                        if (UserList.List.Count > 0)
                            SelectedID = UserList.List[0].ID;
                    }
                    else
                    {
                        SelectedID = Int64.Parse(cookValue);
                        DataRecord dataRecord = UserList.FindDataRecord(SelectedID);
                        if (dataRecord == null)
                        {
                            if (UserList.List.Count > 0)
                                SelectedID = UserList.List[0].ID;
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        public UsersViewModel() { }

    }
}
