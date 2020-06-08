using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HomylogicAsp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using X.Homylogic;
using X.Homylogic.Models.Objects;
using X.Basic;
using X.Homylogic.Models.Objects.Devices;
using HomylogicAsp.Models.Devices;
using System.Text;

namespace HomylogicAsp.Controllers
{
    public class BuffersController : Controller
    {
        // GET: InputBufers
        public ActionResult Input()
        {
            return View(new BuffersViewModel());
        }

        public ActionResult Output()
        {
            return View(new BuffersViewModel());
        }


    }
}