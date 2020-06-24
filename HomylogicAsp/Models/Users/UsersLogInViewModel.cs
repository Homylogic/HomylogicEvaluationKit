using System;
using System.ComponentModel.DataAnnotations;
using X.Homylogic;
using X.Homylogic.Models;
using X.App.Users;
using Microsoft.AspNetCore.Http;
using X.Data.Factory;

namespace HomylogicAsp.Models.Users
{
    public class UserLogInViewModel 
    {
        public string AccessToken { get; set; }
        public string TargetURL { get; set; }
        public Exception LogInException { get; set; }
        public UserList UserList => Body.Environment.Users.UserList;
        [Required]
        public string Name { get; set; }
        public string Password { get; set; }
    }
}
