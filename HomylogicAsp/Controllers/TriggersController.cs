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
using X.Homylogic.Models.Objects.Triggers;
using HomylogicAsp.Models.Triggers;
using System.Text;

namespace HomylogicAsp.Controllers
{
    public class TriggersController : Controller
    {
        // GET: Triggers
        public ActionResult Index()
        {
            return View(new TriggersViewModel(Request));
        }
        // GET: Triggers/Create/? 
        public ActionResult Create(int objectType)
        {
            TriggerX.TriggerTypes triggerType = (TriggerX.TriggerTypes)objectType;
            if (triggerType == TriggerX.TriggerTypes.Device) return View("EditDevice", new EditDeviceTriggerViewModel());
            if (triggerType == TriggerX.TriggerTypes.OutputBuffer) return View("EditOutputBuffer", new EditOutputBufferTriggerViewModel());
            return RedirectToAction("Index");
        }
        // GET: Triggers/Edit/?
        public ActionResult Edit(int id)
        {
            TriggerX trigger = (TriggerX)Body.Runtime.Triggers.FindDataRecord(id);
            if (trigger != null) 
            { 
                if (trigger.TriggerType == TriggerX.TriggerTypes.Device) return View("EditDevice", new EditDeviceTriggerViewModel((DeviceTriggerX)trigger));
                if (trigger.TriggerType == TriggerX.TriggerTypes.OutputBuffer) return View("EditOutputBuffer", new EditOutputBufferTriggerViewModel((OutputBufferTriggerX)trigger));
            }
            return RedirectToAction("Index");
        }
        // GET: Triggers/Delete/?
        public ActionResult Delete(int id)
        {
            TriggerX trigger = (TriggerX)Body.Runtime.Triggers.FindDataRecord(id);
            if (trigger != null)
            {
                return View(new EditTriggerViewModel() { ID = trigger.ID, Name = trigger.Name });
            }
            return RedirectToAction("Index");
        }
        // GET: GetIsStarted
        public string GetIsStarted()
        {
            StringBuilder result = new StringBuilder();
            try
            {
                for (int i = 0; i < Body.Runtime.Triggers.List.Count; i++)
                {
                    TriggerX trigger = (TriggerX)Body.Runtime.Triggers.List[i];
                    result.AppendFormat("{0}:{1};", trigger.ID, Convert.ToInt32(trigger.IsStarted));
                }
            }
            catch (Exception ex)
            {
                Body.Environment.Logs.Error($"Problem checking for state of triggers.", ex, this.GetType().Name);
            }
            return result.ToString();
        }

        // POST: Triggers/Start 
        [HttpPost]
        public JsonResult Start(int id)
        {
            try
            {
                TriggerX trigger = (TriggerX)Body.Runtime.Triggers.FindDataRecord(id);
                trigger.Start();
            }
            catch (Exception ex)
            {
                return new JsonResult(ex.Message);
            }
            return new JsonResult("OK");
        }
        // POST: Triggers/Stop 
        [HttpPost]
        public JsonResult Stop(int id)
        {
            try
            {
                TriggerX trigger = (TriggerX)Body.Runtime.Triggers.FindDataRecord(id);
                trigger.Stop();
            }
            catch (Exception ex)
            {
                return new JsonResult(ex.Message);
            }
            return new JsonResult("OK");
        }
        // POST: Triggers/SaveDeviceTrigger 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveDevice(EditDeviceTriggerViewModel editDeviceTriggerViewModel)
        {
            TriggerX trigger = null;
            try
            {
                trigger = editDeviceTriggerViewModel.GetDeviceTrigger();
                trigger.Save();
                Response.Cookies.Append("SelectedTriggerID", trigger.ID.ToString());
            }
            catch (Exception ex)
            {
                try
                {
                    if (trigger != null && trigger.RecordState == X.Data.Factory.DataRecord.RecordStateTypes.Edit)
                        trigger.Reload();    
                }
                catch (Exception)
                {
                }
                editDeviceTriggerViewModel.SaveException = ex;
                return View("EditDevice", editDeviceTriggerViewModel);
            }
            return RedirectToAction("Index");
        }
        // POST: Triggers/SaveOutputBuffer 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveOutputBuffer(EditOutputBufferTriggerViewModel editOutputBufferTriggerViewModel)
        {
            TriggerX trigger = null;
            try
            {
                trigger = editOutputBufferTriggerViewModel.GetOutputBufferTrigger();
                trigger.Save();
                Response.Cookies.Append("SelectedTriggerID", trigger.ID.ToString());
            }
            catch (Exception ex)
            {
                try
                {
                    if (trigger != null && trigger.RecordState == X.Data.Factory.DataRecord.RecordStateTypes.Edit)
                        trigger.Reload();
                }
                catch (Exception)
                {
                }
                editOutputBufferTriggerViewModel.SaveException = ex;
                return View("EditOutputBuffer", editOutputBufferTriggerViewModel);
            }
            return RedirectToAction("Index");
        }

        // POST: Devices/Delete/?
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(EditTriggerViewModel editTriggerViewModel)
        {
            try
            {
                TriggerX trigger = (TriggerX)Body.Runtime.Triggers.FindDataRecord(editTriggerViewModel.ID);
                trigger.Delete();
            }
            catch
            {
            }
            return RedirectToAction("Index");
        }
    }
}