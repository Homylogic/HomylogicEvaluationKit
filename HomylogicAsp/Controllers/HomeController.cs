using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using HomylogicAsp.Models;
using X.Homylogic;
using System.Threading;
using HomylogicAsp.Models.Users;
using System.Security.Authentication;

namespace HomylogicAsp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }
        public IActionResult Index()
        {
            return View(new HomeViewModel());
        }
        public IActionResult Settings()
        {
            return View(new SettingsViewModel());
        }
        public IActionResult About()
        {
            return View(new AboutViewModel());
        }
        public IActionResult UserLogIn(string url)
        {
            return View(new UserLogInViewModel() { TargetURL = url } );
        }
        public IActionResult SSL()
        {
            return View(new AboutViewModel());
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        // GET: Home/Download 
        public ActionResult Download(string file)
        {
            string path = string.Format("wwwroot/{0}", file);
            System.IO.FileStream fs = System.IO.File.OpenRead(path);
            byte[] fileBytes = new byte[fs.Length];
            int br = fs.Read(fileBytes, 0, fileBytes.Length);
            if (br != fs.Length)
                throw new System.IO.IOException(file);
            string fileName = System.IO.Path.GetFileName(file);
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }
        // POST: Home/SaveSettings 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveSettings(SettingsViewModel settingsViewModel)
        {
            try
            {
                settingsViewModel.Settings.Home.BackgroundImage = settingsViewModel.Home_BackgroundImage;
                settingsViewModel.Settings.Security.DefaultUserID = settingsViewModel.Security_DefaultUserID;
                settingsViewModel.Settings.Save();
            }
            catch (Exception ex)
            {
                settingsViewModel.SaveException = ex;
            }
            return View("Settings", settingsViewModel);
        }
        // POST: Home/UserLogIn 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UserLogIn(UserLogInViewModel userLogInViewModel)
        {
            try
            {
                if (string.IsNullOrEmpty(userLogInViewModel.Name)) throw new ArgumentNullException("User name is empty, can't login.");
                if (Body.Environment.Users.LogIn(userLogInViewModel.Name, userLogInViewModel.Password)) {
                    
                    string accessToken = Body.Environment.Users.AddUserAccess(userLogInViewModel.Name);
                    HttpContext.Response.Cookies.Append("AccessToken", accessToken);
                    userLogInViewModel.AccessToken = accessToken;
                    userLogInViewModel.Name = null;
                    userLogInViewModel.Password = null;
                    // Redirect to target URL after sucessfull login.
                    if (!string.IsNullOrEmpty(userLogInViewModel.TargetURL))
                        return LocalRedirect(userLogInViewModel.TargetURL);
                }
                else {
                    userLogInViewModel.LogInException = new InvalidCredentialException("Invalid user password.");
                }
            }
            catch (Exception ex)
            {
                userLogInViewModel.LogInException = ex;
            }
            return View(userLogInViewModel);
        }
    }
}
