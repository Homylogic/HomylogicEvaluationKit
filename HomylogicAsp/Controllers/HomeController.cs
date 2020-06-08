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
                if (settingsViewModel.Settings.Security.Password != settingsViewModel.Security_Password)
                {
                    // Password has been changed.
                    Security.ResetAllAccess();
                }
                settingsViewModel.Settings.Security.Password = settingsViewModel.Security_Password;
                settingsViewModel.Settings.Save();
            }
            catch (Exception ex)
            {
                settingsViewModel.SaveException = ex;
            }
            return View("Settings", settingsViewModel);
        }
        // POST: Home/TryAllowAccessSettings 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TryAllowAccessSettings(SettingsViewModel settingsViewModel)
        {
            try
            {
                if (Body.Environment.Settings.Security.Password == settingsViewModel.PasswordAccess)
                {
                    Security.AllowUserAccess(this.HttpContext.Response);
                    settingsViewModel.HasAccess = true;
                }
                else {
                    settingsViewModel.SaveException = new Exception(); // Zobrazí že bolo zadané nesprávne heslo.
                    Thread.Sleep(1000);
                }
            }
            catch (Exception)
            {
            }
            return View("Settings", settingsViewModel);
        }

    }
}
