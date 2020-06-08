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
using X.Homylogic.Models.Objects.Devices.Homyoko;
using System.Globalization;
using Newtonsoft.Json;
using X.Data;
using HomylogicAsp.Models.Devices.Homyoko;
using X.Homylogic.Models.Schedule;

namespace HomylogicAsp.Controllers
{
    public class ScheduleController : Controller
    {
        // GET: IVTController/?
        public ActionResult IVTController(int id)
        {
            return View("~/Views/Devices/Homyoko/ScheduleIVTController.cshtml", new Models.ScheduleViewModel(Request) { OwnerID = id });
        }
        public enum SchedulerTypes 
        {
            IVTController
        }
        // GET: Schedule/Createdevice/?=OwnerID 
        public ActionResult CreateIVTController(Int64 id)
        {
            return View("~/Views/Devices/Homyoko/ScheduleEditIVTController.cshtml", new EditScheduleIVTControllerViewModel() { OwnerID = id });
        }
        // GET: Schedule/Edit/?
        public ActionResult EditIVTController(Int64 id, Int64 scheduleID)
        {
            IVTController ivtController = (IVTController)Body.Runtime.Devices.FindDataRecord(id);
            if (ivtController != null) 
            {
                ScheduleRecord scheduleRecord = (ScheduleRecord)ivtController.Scheduler.FindDataRecord(scheduleID);
                if (scheduleRecord != null) 
                    return View("~/Views/Devices/Homyoko/ScheduleEditIVTController.cshtml", new EditScheduleIVTControllerViewModel(scheduleRecord) { OwnerID = id,  });
            }
            return RedirectToAction("IVTController", "Schedule", new { @id = id });
        }
        // GET: Schedule/Delete/?
        public ActionResult DeleteIVTController(Int64 id, Int64 scheduleID)
        {
            IVTController ivtController = (IVTController)Body.Runtime.Devices.FindDataRecord(id);
            if (ivtController != null)
            {
                ScheduleRecord scheduleRecord = (ScheduleRecord)ivtController.Scheduler.FindDataRecord(scheduleID);
                if (scheduleRecord != null)
                    scheduleRecord.Delete();
            }
            return RedirectToAction("IVTController", "Schedule", new { @id = id });
        }
        // POST: Schedule/SaveIVTController
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveIVTController(Models.Devices.Homyoko.EditScheduleIVTControllerViewModel editScheduleIVTControllerViewModel)
        {
            X.Homylogic.Models.Schedule.ScheduleRecord scheduleRecord = null;
            try
            {
                // Nastaviť akciu pre zobrazenie používateľovy.
                string methodNameCaption = null;
                List<KeyValuePair<string, string>> Actions = editScheduleIVTControllerViewModel.GetDeviceActions();
                foreach (KeyValuePair<string, string> action in Actions)
                {
                    if (action.Key == editScheduleIVTControllerViewModel.MethodName)
                    {
                        methodNameCaption = action.Value;
                        break;
                    }
                }
                scheduleRecord = editScheduleIVTControllerViewModel.GetDeviceScheduleRecord();
                scheduleRecord.ActionSettings = methodNameCaption;
                scheduleRecord.Save();
            }
            catch (Exception ex)
            {
                try
                {
                    if (scheduleRecord != null && scheduleRecord.RecordState == X.Data.Factory.DataRecord.RecordStateTypes.Edit)
                        scheduleRecord.Reload();
                }
                catch (Exception)
                {
                }
                editScheduleIVTControllerViewModel.SaveException = ex;
                return View("~/Views/Devices/Homyoko/ScheduleEditIVTController.cshtml", new EditScheduleIVTControllerViewModel() { OwnerID = editScheduleIVTControllerViewModel.OwnerID, SaveException = ex });
            }
            return RedirectToAction("IVTController", "Schedule", new { @id = editScheduleIVTControllerViewModel.OwnerID });
        }
    }
}