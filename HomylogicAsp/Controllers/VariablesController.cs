using System;
using Microsoft.AspNetCore.Mvc;
using HomylogicAsp.Models;
using X.Homylogic;
using X.Homylogic.Models.Objects;
using HomylogicAsp.Models.Variables;
using X.Homylogic.Models.Objects.Variables;

namespace HomylogicAsp.Controllers
{
    public class VariablesController : Controller
    {
        // GET: Variables
        public ActionResult Index()
        {
            return View(new VariablesViewModel(Request));
        }
        // GET: Variables/Create/? 
        public ActionResult Create(int objectType)
        {
            VariableX.VariableTypes variableType = (VariableX.VariableTypes)objectType;
            if (variableType == VariableX.VariableTypes.Text) return View("EditText", new EditTextViewModel());
            if (variableType == VariableX.VariableTypes.Integer) return View("EditInteger", new EditIntegerViewModel());
            if (variableType == VariableX.VariableTypes.Decimal) return View("EditDecimal", new EditDecimalViewModel());
            if (variableType == VariableX.VariableTypes.YesNo) return View("EditYesNO", new EditYesNoViewModel());
            return RedirectToAction("Index");
        }
        // GET: Variables/Edit/?
        public ActionResult Edit(int id)
        {
            VariableX variable = (VariableX)Body.Runtime.Variables.FindDataRecord(id);
            if (variable != null)
            {
                if (variable.VariableType == VariableX.VariableTypes.Text) return View("EditText", new EditTextViewModel((TextVariableX)variable));
                if (variable.VariableType == VariableX.VariableTypes.Integer) return View("EditInteger", new EditIntegerViewModel((IntegerVariableX)variable));
                if (variable.VariableType == VariableX.VariableTypes.Decimal) return View("EditDecimal", new EditDecimalViewModel((DecimalVariableX)variable));
                if (variable.VariableType == VariableX.VariableTypes.YesNo) return View("EditYesNo", new EditYesNoViewModel((YesNoVariableX)variable));
            }
            return RedirectToAction("Index");
        }
        // GET: Variables/Delete/?
        public ActionResult Delete(int id)
        {
            VariableX variable = (VariableX)Body.Runtime.Variables.FindDataRecord(id);
            if (variable != null)
            {
                return View(new EditVariableViewModel() { ID = variable.ID, Name = variable.Name });
            }
            return RedirectToAction("Index");
        }
        // POST: Variables/SaveTextVariable 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveTextVariable(EditTextViewModel editTextViewModel)
        {
            VariableX variable = null;
            try
            {
                variable = editTextViewModel.GetTextVariable();
                variable.Save();
                Response.Cookies.Append("SelectedVariableID", variable.ID.ToString());
            }
            catch (Exception ex)
            {
                try
                {
                    if (variable != null && variable.RecordState == X.Data.Factory.DataRecord.RecordStateTypes.Edit)
                        variable.Reload();
                }
                catch (Exception)
                {
                }
                editTextViewModel.SaveException = ex;
                return View("EditText", editTextViewModel);
            }
            return RedirectToAction("Index");
        }
        // POST: Variables/SaveIntegerVariable 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveIntegerVariable(EditIntegerViewModel editIntegerViewModel)
        {
            VariableX variable = null;
            try
            {
                variable = editIntegerViewModel.GetIntegerVariable();
                variable.Save();
                Response.Cookies.Append("SelectedVariableID", variable.ID.ToString());
            }
            catch (Exception ex)
            {
                try
                {
                    if (variable != null && variable.RecordState == X.Data.Factory.DataRecord.RecordStateTypes.Edit)
                        variable.Reload();
                }
                catch (Exception)
                {
                }
                editIntegerViewModel.SaveException = ex;
                return View("EditInteger", editIntegerViewModel);
            }
            return RedirectToAction("Index");
        }
        // POST: Variables/SaveDecimalVariable 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveDecimalVariable(EditDecimalViewModel editDecimalViewModel)
        {
            VariableX variable = null;
            try
            {
                variable = editDecimalViewModel.GetDecimalVariable();
                variable.Save();
                Response.Cookies.Append("SelectedVariableID", variable.ID.ToString());
            }
            catch (Exception ex)
            {
                try
                {
                    if (variable != null && variable.RecordState == X.Data.Factory.DataRecord.RecordStateTypes.Edit)
                        variable.Reload();
                }
                catch (Exception)
                {
                }
                editDecimalViewModel.SaveException = ex;
                return View("EditDecimal", editDecimalViewModel);
            }
            return RedirectToAction("Index");
        }
        // POST: Variables/SaveYesNoVariable
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveYesNoVariable(EditYesNoViewModel editYesNoViewModel)
        {
            VariableX variable = null;
            try
            {
                variable = editYesNoViewModel.GetDecimalVariable();
                variable.Save();
                Response.Cookies.Append("SelectedVariableID", variable.ID.ToString());
            }
            catch (Exception ex)
            {
                try
                {
                    if (variable != null && variable.RecordState == X.Data.Factory.DataRecord.RecordStateTypes.Edit)
                        variable.Reload();
                }
                catch (Exception)
                {
                }
                editYesNoViewModel.SaveException = ex;
                return View("EditYesNo", editYesNoViewModel);
            }
            return RedirectToAction("Index");
        }
        // POST: Variables/Delete/?
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(EditVariableViewModel editVariableViewModel)
        {
            try
            {
                VariableX variable = (VariableX)Body.Runtime.Variables.FindDataRecord(editVariableViewModel.ID);
                variable.Delete();
            }
            catch
            {
            }
            return RedirectToAction("Index");
        }
    }
}
