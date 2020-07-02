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
using X.Data;
using System.Threading;
using System.Runtime.CompilerServices;

namespace HomylogicAsp.Controllers
{
    public class DevicesController : Controller
    {
        // GET: Devices
        public ActionResult Index()
        {
            return View(new DevicesViewModel(Request));
        }
        // GET: Devices/Create/? 
        public ActionResult Create(int objectType)
        {
            DeviceX.DeviceTypes deviceType = (DeviceX.DeviceTypes)objectType;
            if (deviceType == DeviceX.DeviceTypes.Serial) return View("EditSerialPort", new EditSerialPortViewModel());
            if (deviceType == DeviceX.DeviceTypes.TCPSocket) return View("EditTCPSocket", new EditTCPSocketViewModel());
            if (deviceType == DeviceX.DeviceTypes.HomyokoWeatherStation) return View("Homyoko/EditWeatherStation", new Models.Devices.Homyoko.EditWeatherStationViewModel());
            if (deviceType == DeviceX.DeviceTypes.HomyokoIVTController) return View("Homyoko/EditIVTController", new Models.Devices.Homyoko.EditIVTControllerViewModel());
            return RedirectToAction("Index");
        }
        // GET: Devices/Edit/?
        public ActionResult Edit(int id)
        {
            DeviceX device = (DeviceX)Body.Runtime.Devices.FindDataRecord(id);
            if (device != null) 
            { 
                if (device.DeviceType == DeviceX.DeviceTypes.Serial) return View("EditSerialPort", new EditSerialPortViewModel((SerialDeviceX)device));
                if (device.DeviceType == DeviceX.DeviceTypes.TCPSocket) return View("EditTCPSocket", new EditTCPSocketViewModel((TCPDeviceX)device));
                if (device.DeviceType == DeviceX.DeviceTypes.HomyokoWeatherStation) return View("Homyoko/EditWeatherStation", new Models.Devices.Homyoko.EditWeatherStationViewModel((WeatherStation)device));
                if (device.DeviceType == DeviceX.DeviceTypes.HomyokoIVTController) return View("Homyoko/EditIVTController", new Models.Devices.Homyoko.EditIVTControllerViewModel((IVTController)device));
            }
            return RedirectToAction("Index");
        }
        // GET: Devices/Delete/?
        public ActionResult Delete(int id)
        {
            DeviceX device = (DeviceX)Body.Runtime.Devices.FindDataRecord(id);
            if (device != null)
            {
                return View(new EditDeviceViewModel() { ID = device.ID, Name = device.Name });
            }
            return RedirectToAction("Index");
        }
        // GET: GetIsOpen
        public string GetIsOpen()
        {
            StringBuilder result = new StringBuilder();
            try
            {
                for (int i = 0; i < Body.Runtime.Devices.List.Count; i++)
                {
                    DeviceX device = (DeviceX)Body.Runtime.Devices.List[i];
                    result.AppendFormat("{0}:{1};", device.ID, Convert.ToInt32(device.IsOpen));
                }
            }
            catch (Exception ex)
            {
                Body.Environment.Logs.Error($"Problem checking for state of devices.", ex, this.GetType().Name);
            }
            return result.ToString();
        }
        // POST: Devices/Open 
        [HttpPost]
        public JsonResult Open(int id)
        {
            try
            {
                DeviceX device = (DeviceX)Body.Runtime.Devices.FindDataRecord(id);
                device.Open();
            }
            catch (Exception ex)
            {
                return new JsonResult(ex.Message);
            }
            return new JsonResult("OK");
        }
        // POST: Devices/Close 
        [HttpPost]
        public JsonResult Close(int id)
        {
            try
            {
                DeviceX device = (DeviceX)Body.Runtime.Devices.FindDataRecord(id);
                device.Close();
            }
            catch (Exception ex)
            {
                return new JsonResult(ex.Message);
            }
            return new JsonResult("OK");
        }
        // GET: Devices/DeviceControls/?
        public ActionResult DeviceControls(int id)
        {
            DeviceX device = (DeviceX)Body.Runtime.Devices.FindDataRecord(id);
            if (device != null)
            {
                return View(new EditDeviceViewModel() { ID = device.ID, Name = device.Name, DeviceType = device.DeviceType });
            }
            return RedirectToAction("Index");
        }
        // POST: Devices/SaveSerialPort 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveSerialPort(EditSerialPortViewModel editSerialDeviceViewModel)
        {
            DeviceX device = null;
            try
            {
                device = editSerialDeviceViewModel.GetSerialDevice();
                device.Save();
                Response.Cookies.Append("SelectedDeviceID", device.ID.ToString());
            }
            catch (Exception ex)
            {
                try
                {
                    if (device != null && device.RecordState == X.Data.Factory.DataRecord.RecordStateTypes.Edit)
                        device.Reload();    
                }
                catch (Exception)
                {
                }
                editSerialDeviceViewModel.SaveException = ex;
                return View("EditSerialPort", editSerialDeviceViewModel);
            }
            return RedirectToAction("Index");
        }
        // POST: Devices/SaveTCPSocket 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveTCPSocket(EditTCPSocketViewModel editTCPSocketViewModel)
        {
            DeviceX device = null;
            try
            {
                device = editTCPSocketViewModel.GetTCPDevice();
                device.Save();
                Response.Cookies.Append("SelectedDeviceID", device.ID.ToString());
            }
            catch (Exception ex)
            {
                try
                {
                    if (device != null && device.RecordState == X.Data.Factory.DataRecord.RecordStateTypes.Edit)
                        device.Reload();
                }
                catch (Exception)
                {
                }
                editTCPSocketViewModel.SaveException = ex;
                return View("EditTCPSocket", editTCPSocketViewModel);
            }
            return RedirectToAction("Index");
        }
        // POST: Devices/SaveHomyokoWeatherStation 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveHomyokoWeatherStation(Models.Devices.Homyoko.EditWeatherStationViewModel editWeatherStationViewModel)
        {
            X.Homylogic.Models.Objects.Devices.Homyoko.WeatherStation weatherStation = null;
            try
            {
                weatherStation = editWeatherStationViewModel.GetWeatherStation();
                weatherStation.Save();
                Response.Cookies.Append("SelectedDeviceID", weatherStation.ID.ToString());
            }
            catch (Exception ex)
            {
                try
                {
                    if (weatherStation != null && weatherStation.RecordState == X.Data.Factory.DataRecord.RecordStateTypes.Edit)
                        weatherStation.Reload();
                }
                catch (Exception)
                {
                }
                editWeatherStationViewModel.SaveException = ex;
                return View("Homyoko/EditWeatherStation", editWeatherStationViewModel);
            }
            return RedirectToAction("Index");
        }
        // POST: Devices/SaveHomyokoIVTController
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveHomyokoIVTController(Models.Devices.Homyoko.EditIVTControllerViewModel editIVTControllerViewModel)
        {
            X.Homylogic.Models.Objects.Devices.Homyoko.IVTController ivtController = null;
            try
            {
                ivtController = editIVTControllerViewModel.GetIVTController();
                ivtController.Save();
                Response.Cookies.Append("SelectedDeviceID", ivtController.ID.ToString());
            }
            catch (Exception ex)
            {
                try
                {
                    if (ivtController != null && ivtController.RecordState == X.Data.Factory.DataRecord.RecordStateTypes.Edit)
                        ivtController.Reload();
                }
                catch (Exception)
                {
                }
                editIVTControllerViewModel.SaveException = ex;
                return View("Homyoko/EditIVTController", editIVTControllerViewModel);
            }
            return RedirectToAction("Index");
        }
        // POST: Devices/Delete/?
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(EditDeviceViewModel editDeviceViewModel)
        {
            try
            {
                DeviceX device = (DeviceX)Body.Runtime.Devices.FindDataRecord(editDeviceViewModel.ID);
                device.Delete();
            }
            catch
            {
            }
            return RedirectToAction("Index");
        }
        // GET: GetDataHomyokoWeatherStation/?
        public string GetDataHomyokoWeatherStation(int id)
        {
            DeviceX device = (DeviceX)Body.Runtime.Devices.FindDataRecord(id);
            if (device == null) 
            {
                Body.Environment.Logs.Error("Can't get data of weather device.", new Exception($"Device (Id: {id}) not found."), this.GetType().Name);
                return null;
            }
            if (device is WeatherStation)
            {
                WeatherStation weatherStation = (WeatherStation)device;
                // ! Using 'Newtonsoft.Json + JsonConvert.SerializeObject' is problem with memory leak, don't use JSON serializer.
                // Return simple CSV string.
                return string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}|{9}|{10}",
                    weatherStation.MeasureTime.ToString("HH:mm:ss"),
                    weatherStation.Temperature1.ToString(XCommon.CSVNumberCulture),
                    weatherStation.Temperature2.ToString(XCommon.CSVNumberCulture),
                    weatherStation.Windspeed.ToString(XCommon.CSVNumberCulture),
                    weatherStation.WindspeedAvg.ToString(XCommon.CSVNumberCulture),
                    weatherStation.SunshinePercent.ToString(XCommon.CSVNumberCulture),
                    string.Format("{0}/{1}", weatherStation.EdgeTemperature1.Minimum.ToString(XCommon.CSVNumberCulture), weatherStation.EdgeTemperature1.Maximum.ToString(XCommon.CSVNumberCulture)),
                    string.Format("{0}/{1}", weatherStation.EdgeTemperature2.Minimum.ToString(XCommon.CSVNumberCulture), weatherStation.EdgeTemperature2.Maximum.ToString(XCommon.CSVNumberCulture)),
                    string.Format("{0}/{1}", weatherStation.EdgeWindspeed.Minimum.ToString(XCommon.CSVNumberCulture), weatherStation.EdgeWindspeed.Maximum.ToString(XCommon.CSVNumberCulture)),
                    string.Format("{0}/{1}", weatherStation.EdgeWindspeedAvg.Minimum.ToString(XCommon.CSVNumberCulture), weatherStation.EdgeWindspeedAvg.Maximum.ToString(XCommon.CSVNumberCulture)),
                    string.Format("{0}/{1}", weatherStation.EdgeSunshinePercent.Minimum.ToString(XCommon.CSVNumberCulture), weatherStation.EdgeSunshinePercent.Maximum.ToString(XCommon.CSVNumberCulture))
                );
            }
            else {
                Body.Environment.Logs.Error("Can't get data of weather device.", new Exception($"Device (ID: {id}) is not weather station."), this.GetType().Name);
                return null;
            }
        }
        // GET: GetCustomsHomyokoWeatherStation/?
        public string GetCustomsHomyokoWeatherStation(int id)
        {
            DeviceX device = (DeviceX)Body.Runtime.Devices.FindDataRecord(id);
            if (device == null)
            {
                Body.Environment.Logs.Error("Can't get customs of weather device.", new Exception($"Device (Id: {id}) not found."), this.GetType().Name);
                return null;
            }
            if (device is WeatherStation)
            {
                WeatherStation weatherStation = (WeatherStation)device;
                // ! Using 'Newtonsoft.Json + JsonConvert.SerializeObject' is problem with memory leak, don't use JSON serializer.
                // Return simple CSV string.
                string CustomsTemperature1_Caption = weatherStation.CustomsTemperature1.Caption;
                if (!string.IsNullOrEmpty(CustomsTemperature1_Caption)) 
                    CustomsTemperature1_Caption = CustomsTemperature1_Caption.Replace("|", ",");
                return string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}",
                    CustomsTemperature1_Caption,
                    weatherStation.CustomsTemperature1.Minimum.ToString(XCommon.CSVNumberCulture),
                    weatherStation.CustomsTemperature1.Maximum.ToString(XCommon.CSVNumberCulture),
                    weatherStation.CustomsTemperature2.Minimum.ToString(XCommon.CSVNumberCulture),
                    weatherStation.CustomsTemperature2.Maximum.ToString(XCommon.CSVNumberCulture),
                    weatherStation.CustomsWindspeed.LightAir.ToString(XCommon.CSVNumberCulture),
                    weatherStation.CustomsWindspeed.GentleBreeze.ToString(XCommon.CSVNumberCulture),
                    weatherStation.CustomsWindspeed.StrongBreeze.ToString(XCommon.CSVNumberCulture),
                    weatherStation.CustomsSunshine.Day.ToString(XCommon.CSVNumberCulture)
                );
            }
            else
            {
                Body.Environment.Logs.Error("Can't get customs of weather device.", new Exception($"Device (ID: {id}) is not weather station."), this.GetType().Name);
                return null;
            }
        }
        // GET: GetDataHomyokoWeatherStation/?
        public string GetDataHomyokoIVTController(int id)
        {
            DeviceX device = (DeviceX)Body.Runtime.Devices.FindDataRecord(id);
            if (device == null)
            {
                Body.Environment.Logs.Error("Can't get data of IVT controller device.", new Exception($"Device (Id: {id}) not found."), this.GetType().Name);
                return null;
            }
            if (device is IVTController)
            {
                IVTController ivtController = (IVTController)device;
                // ! Using 'Newtonsoft.Json + JsonConvert.SerializeObject' is problem with memory leak, don't use JSON serializer.
                // Return simple CSV string.
                string waterFlowUIText = X.Basic.CodeDom.Ennum.GetDescription(typeof(IVTController.WaterFlowTypes), ivtController.WaterFlow);
                if (!string.IsNullOrEmpty(waterFlowUIText)) 
                    waterFlowUIText = waterFlowUIText.Replace("|", ",");
                return string.Format("{0}|{1}|{2}",
                    ivtController.MeasureTime.ToString("HH:mm:ss"),
                    ivtController.TemperatureFloor.ToString(XCommon.CSVNumberCulture),
                    waterFlowUIText
                );
            }
            else
            {
                Body.Environment.Logs.Error("Can't get data of IVT controller device.", new Exception($"Device (ID: {id}) is not IVT controller."), this.GetType().Name);
                return null;
            }
        }
        // GET: GetCustomsHomyokoIVTController/?
        public string GetCustomsHomyokoIVTController(int id)
        {
            DeviceX device = (DeviceX)Body.Runtime.Devices.FindDataRecord(id);
            if (device == null)
            {
                Body.Environment.Logs.Error("Can't get customs of IVT controller device.", new Exception($"Device (Id: {id}) not found."), this.GetType().Name);
                return null;
            }
            if (device is IVTController)
            {
                IVTController ivtController = (IVTController)device;
                // ! Using 'Newtonsoft.Json + JsonConvert.SerializeObject' is problem with memory leak, don't use JSON serializer.
                // Return simple CSV string.
                string CustomsTemperature_Caption = ivtController.CustomsTemperature.Caption;
                if (!string.IsNullOrEmpty(CustomsTemperature_Caption))
                    CustomsTemperature_Caption = CustomsTemperature_Caption.Replace("|", ",");
                return string.Format("{0}|{1}|{2}",
                    CustomsTemperature_Caption,
                    ivtController.CustomsTemperature.Minimum.ToString(XCommon.CSVNumberCulture),
                    ivtController.CustomsTemperature.Maximum.ToString(XCommon.CSVNumberCulture)
                );
            }
            else
            {
                Body.Environment.Logs.Error("Can't get customs of IVT controller device.", new Exception($"Device (ID: {id}) is not IVT controller."), this.GetType().Name);
                return null;
            }
        }
        // GET: GetControlStatusIVTController/?
        public string GetControlStatusIVTController(int id)
        {
            StringBuilder result = new StringBuilder();
            DeviceX device = (DeviceX)Body.Runtime.Devices.FindDataRecord(id);
            if (device != null && device is IVTController)
            {
                IVTController ivtController = (IVTController)device;
                try
                {
                    string ivtStatusText = (ivtController.IVTStatus switch
                    {
                        IVTController.StatusTypes.TurnedOn => "IVT was turned on",
                        IVTController.StatusTypes.TurnedOff => "IVT was turned off",
                        _ => string.Empty
                    });
                    string pumpStatusText = (ivtController.PumpStatus switch
                    {
                        IVTController.StatusTypes.TurnedOn => "Pumps was turned on",
                        IVTController.StatusTypes.TurnedOff => "Pumps was turned off",
                        _ => string.Empty
                    });

                    result.AppendFormat("{0}:{1}:{2}:{3}", ivtStatusText, pumpStatusText,Convert.ToInt32(ivtController.IsProblemNoWaterFlow), Convert.ToInt32(ivtController.IsProblemHighTemperature));
                }
                catch (Exception)
                {
                }
            }
            return result.ToString();
        }
        // GET: ControlIVTControllerHeatOn/?
        public string ControlIVTControllerHeatOn(int id)
        {
            DeviceX device = (DeviceX)Body.Runtime.Devices.FindDataRecord(id);
            if (device == null)
            {
                return $"Device (Id: {id}) not found.";
            }
            if (device is IVTController)
            {
                Body.Environment.Logs.Info($"User turned on heating on device {device.Name}.", string.Empty, "IVT control");
                IVTController ivtController = (IVTController)device;
                try
                {
                    ivtController.HeatOn();
                    return "OK";
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            }
            else
            {
                return $"Device (ID: {id}) is not IVT controller.";
            }
        }
        // GET: ControlIVTControllerHetOff/?
        public string ControlIVTControllerHeatOff(int id)
        {
            DeviceX device = (DeviceX)Body.Runtime.Devices.FindDataRecord(id);
            if (device == null)
            {
                return $"Device (Id: {id}) not found.";
            }
            if (device is IVTController)
            {
                Body.Environment.Logs.Info($"User turned off heating on device {device.Name}.", string.Empty, "IVT control");
                IVTController ivtController = (IVTController)device;
                try
                {
                    ivtController.HeatOff();
                    return "OK";
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            }
            else
            {
                return $"Device (ID: {id}) is not IVT controller.";
            }
        }
        // GET: ControlIVTControllerPumpOn/?
        public string ControlIVTControllerPumpOn(int id)
        {
            DeviceX device = (DeviceX)Body.Runtime.Devices.FindDataRecord(id);
            if (device == null)
            {
                return $"Device (Id: {id}) not found.";
            }
            if (device is IVTController)
            {
                Body.Environment.Logs.Info($"User turned on pumps on device {device.Name}.", string.Empty, "IVT control");
                IVTController ivtController = (IVTController)device;
                try
                {
                    ivtController.PumpOn();
                    return "OK";
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            }
            else
            {
                return $"Device (ID: {id}) is not IVT controller.";
            }
        }
        // GET: ControlIVTControllerPumpOff/?
        public string ControlIVTControllerPumpOff(int id)
        {
            DeviceX device = (DeviceX)Body.Runtime.Devices.FindDataRecord(id);
            if (device == null)
            {
                return $"Device (Id: {id}) not found.";
            }
            if (device is IVTController)
            {
                Body.Environment.Logs.Info($"User turned off pumps on device {device.Name}.", string.Empty, "IVT control");
                IVTController ivtController = (IVTController)device;
                try
                {
                    ivtController.PumpOff();
                    return "OK";
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            }
            else
            {
                return $"Device (ID: {id}) is not IVT controller.";
            }
        }
        // GET: GetInputBufferTabler/?=DeviceID
        public string GetInputBufferTable(int id)
        {
            StringBuilder result = new StringBuilder();
            result.Append("<table id='input-buffer-list' class='table table-bordered table-hover'>");
            result.Append("<tbody>");
            int readedBufferItemsCount = 0;
            try
            {
                for (int i = 0; i < X.Homylogic.Body.Runtime.InputBuffers.List.Count; i++)
                {
                    X.Homylogic.Models.Objects.Buffers.InputBufferX buffer = (X.Homylogic.Models.Objects.Buffers.InputBufferX)X.Homylogic.Body.Runtime.InputBuffers.List[i];
                    if (buffer.DeviceID != id) continue;
                    readedBufferItemsCount++;
                    if (readedBufferItemsCount > 50) break;
                    result.Append("<tr>");
                    result.AppendFormat("<td>{0}</td>", buffer.ProcessTime.ToString("dd.MM.yy HH:mm:ss"));
                    result.AppendFormat("<td>{0}</td>", buffer.Data);
                    result.Append("</tr>");
                }
            }
            catch (Exception ex)
            {
                Body.Environment.Logs.Error($"Problem reading input buffer items.", ex, this.GetType().Name);
            }
            result.Append("</tbody>");
            result.Append("</table>");
            return result.ToString();
        }
    }
}