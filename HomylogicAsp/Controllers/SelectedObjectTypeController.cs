using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HomylogicAsp.Models;
using Microsoft.AspNetCore.Mvc;

namespace HomylogicAsp.Controllers
{
    public class SelectedObjectTypeController : Controller
    {
        public IActionResult Index(string objectTypeName)
        {
            return View(new SelectedObjectTypeViewModel(objectTypeName));
        }
    }
}