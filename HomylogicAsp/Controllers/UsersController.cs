using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using HomylogicAsp.Models.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using X.Homylogic;
using X.Homylogic.Models;

namespace HomylogicAsp.Controllers
{
    public class UsersController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public UsersController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }
        public IActionResult Index()
        {
            return View(new UsersViewModel(Request));
        }
        // GET: Users/Create/
        public ActionResult Create()
        {
            return View("EditUser", new EditUserViewModel());
        }
        // GET: Users/Edit/
        public ActionResult Edit(int id)
        {
            X.App.Users.UserRecord user = (X.App.Users.UserRecord)Body.Environment.Users.UserList.FindDataRecord(id);
            if (user != null)
            {
                return View("EditUser", new EditUserViewModel(user));
            }
            return RedirectToAction("Index");
        }
        // GET: Users/Delete/?
        public ActionResult Delete(int id)
        {
            X.App.Users.UserRecord user = (X.App.Users.UserRecord)Body.Environment.Users.UserList.FindDataRecord(id);
            if (user != null)
            {
                return View(new EditUserViewModel() { ID = user.ID, Name = user.Name });
            }
            return RedirectToAction("Index");
        }
        // POST: Users/Save
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Save(EditUserViewModel editUserViewModel)
        {
            X.App.Users.UserRecord user = null;
            try
            {
                user = editUserViewModel.GetUser();
                user.Save();
                user.Privileges.EditPermissionsSave(editUserViewModel.PermissionValues);
                user.Privileges.PermissionsUpdate();
                Response.Cookies.Append("SelectedUserID", user.ID.ToString());
            }
            catch (Exception ex)
            {
                try
                {
                    if (user != null && user.RecordState == X.Data.Factory.DataRecord.RecordStateTypes.Edit)
                        user.Reload();
                }
                catch (Exception)
                {
                }
                editUserViewModel.SaveException = ex;
                return View("EditUser", editUserViewModel);
            }
            return RedirectToAction("Index");
        }
        // POST: Users/Delete/?
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(EditUserViewModel editUserViewModel)
        {
            try
            {
                X.App.Users.UserRecord user = (X.App.Users.UserRecord)Body.Environment.Users.UserList.FindDataRecord(editUserViewModel.ID);
                if (!user.CanDelete) throw new InvalidOperationException($"User {user.Name} can't be deleted.");
                user.Delete();
            }
            catch
            {
            }
            return RedirectToAction("Index");
        }
    }
}
