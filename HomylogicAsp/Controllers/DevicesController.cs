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
        // POST: Home/TryAllowAccessSettings 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TryAllowAccessSettings(DevicesViewModel devicesViewModel)
        {
            try
            {
                if (Body.Environment.Settings.Security.Password == devicesViewModel.PasswordAccess)
                {
                    Security.AllowUserAccess(this.HttpContext.Response);
                    devicesViewModel.HasAccess = true;
                }
                else
                {
                    devicesViewModel.PasswordException = new Exception(); // Zobrazí že bolo zadané nesprávne heslo.
                    Thread.Sleep(1000);
                }
            }
            catch (Exception)
            {
            }
            return View("Index", devicesViewModel);
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
                CultureInfo ci = (System.Globalization.CultureInfo)CultureInfo.CurrentCulture.Clone();
                ci.NumberFormat.NumberDecimalSeparator = ".";
                ci.NumberFormat.NegativeSign = "-";

                WeatherStation weatherStation = (WeatherStation)device;
                var jsonData = new
                {
                    MTime = weatherStation.MeasureTime.ToString("HH:mm:ss"),
                    T1 = weatherStation.Temperature1.ToString(ci),
                    T2 = weatherStation.Temperature2.ToString(ci),
                    Wind = weatherStation.Windspeed.ToString(ci),
                    WindAvg = weatherStation.WindspeedAvg.ToString(ci),
                    Shine = weatherStation.SunshinePercent.ToString(ci),
                    EdgeT1 = string.Format("{0}/{1}", weatherStation.EdgeTemperature1.Minimum.ToString(ci), weatherStation.EdgeTemperature1.Maximum.ToString(ci)),
                    EdgeT2 = string.Format("{0}/{1}", weatherStation.EdgeTemperature2.Minimum.ToString(ci), weatherStation.EdgeTemperature2.Maximum.ToString(ci)),
                    EdgeWind = string.Format("{0}/{1}", weatherStation.EdgeWindspeed.Minimum.ToString(ci), weatherStation.EdgeWindspeed.Maximum.ToString(ci)),
                    EdgeWindAvg = string.Format("{0}/{1}", weatherStation.EdgeWindspeedAvg.Minimum.ToString(ci), weatherStation.EdgeWindspeedAvg.Maximum.ToString(ci)),
                    EdgeShine = string.Format("{0}/{1}", weatherStation.EdgeSunshinePercent.Minimum.ToString(ci), weatherStation.EdgeSunshinePercent.Maximum.ToString(ci)),
                };
                return JsonConvert.SerializeObject(jsonData);
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
                CultureInfo ci = (System.Globalization.CultureInfo)CultureInfo.CurrentCulture.Clone();
                ci.NumberFormat.NumberDecimalSeparator = ".";
                ci.NumberFormat.NegativeSign = "-";

                WeatherStation weatherStation = (WeatherStation)device;
                var jsonData = new
                {
                    T1Caption = weatherStation.CustomsTemperature1.Caption,
                    T1Minimum = weatherStation.CustomsTemperature1.Minimum.ToString(ci),
                    T1Maximum = weatherStation.CustomsTemperature1.Maximum.ToString(ci),
                    T2Minimum = weatherStation.CustomsTemperature2.Minimum.ToString(ci),
                    T2Maximum = weatherStation.CustomsTemperature2.Maximum.ToString(ci),
                    WDLightAir = weatherStation.CustomsWindspeed.LightAir.ToString(ci),
                    WDGentleBreeze = weatherStation.CustomsWindspeed.GentleBreeze.ToString(ci),
                    WDStrongBreeze = weatherStation.CustomsWindspeed.StrongBreeze.ToString(ci),
                    SunDay = weatherStation.CustomsSunshine.Day.ToString(ci),
                };
                return JsonConvert.SerializeObject(jsonData);
            }
            else
            {
                Body.Environment.Logs.Error("Can't get customs of weather device.", new Exception($"Device (ID: {id}) is not weather station."), this.GetType().Name);
                return null;
            }
        }
        // GET: HistoryWeatherStation/?
        public ActionResult HistoryWeatherStation(int id) 
        {
            return View("Homyoko/HistoryWeatherStation", new Models.Devices.Homyoko.EditWeatherStationViewModel() { ID = id });
        }
        // GET: GetHistoryHomyokoWeatherStation/?
        public string GetHistoryHomyokoWeatherStation(int id)
        {
            // Načítaj zoznam histórie log údajov a vrát ich v čo najkratšej textovej podobe.
            StringBuilder result = new StringBuilder();
            string tableName = $"deviceHistory_{id}";
            string sql = $"SELECT logTime, temperature1, temperature2, windspeed, windspeedAvg, sunshine FROM {tableName} ORDER BY logTime DESC";
            DBClient dbClient = Body.Database.DBClientLogs;
            CultureInfo ci = (System.Globalization.CultureInfo)CultureInfo.CurrentCulture.Clone();
            ci.NumberFormat.NumberDecimalSeparator = ".";
            ci.NumberFormat.NegativeSign = "-";
            ci.NumberFormat.NumberGroupSeparator = "";
            try
            {
                dbClient.Open();
                using DBReader dbReader = dbClient.ExecuteReader(sql);
                int count = 0;
                while (dbReader.Read())
                {
                    string time = dbReader.GetDateTime("logTime").ToString("yyyy-MM-dd HH:mm:ss");
                    string temp1 = dbReader.GetFloat("temperature1").ToString(ci);
                    string temp2 = dbReader.GetFloat("temperature2").ToString(ci);
                    string wind = dbReader.GetFloat("windspeed").ToString(ci);
                    string windA = dbReader.GetFloat("windspeedAvg").ToString(ci);
                    int shineVal = dbReader.GetInt32("sunshine");
                    string shine = Math.Round(shineVal * WeatherStation.SUN_SHINE_PERCENT_COEF, 2).ToString(ci);
                    result.AppendFormat("{0},{1},{2},{3},{4},{5};", time, temp1, temp2, wind, windA, shine);
                    count++;
                    if (count > 300) break;
                }
            }
            catch (Exception)
            {
            }
            return result.ToString();
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
                CultureInfo ci = (System.Globalization.CultureInfo)CultureInfo.CurrentCulture.Clone();
                ci.NumberFormat.NumberDecimalSeparator = ".";
                ci.NumberFormat.NegativeSign = "-";

                IVTController ivtController = (IVTController)device;
                string waterFlowUIText = X.Basic.CodeDom.Ennum.GetDescription(typeof(IVTController.WaterFlowTypes), ivtController.WaterFlow);
                var jsonData = new
                {
                    MTime = ivtController.MeasureTime.ToString("HH:mm:ss"),
                    T = ivtController.TemperatureFloor.ToString(ci),
                    Flow = X.Basic.CodeDom.Ennum.GetDescription(typeof(IVTController.WaterFlowTypes), ivtController.WaterFlow)
                };
                return JsonConvert.SerializeObject(jsonData);
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
                CultureInfo ci = (System.Globalization.CultureInfo)CultureInfo.CurrentCulture.Clone();
                ci.NumberFormat.NumberDecimalSeparator = ".";
                ci.NumberFormat.NegativeSign = "-";

                IVTController ivtController = (IVTController)device;
                var jsonData = new
                {
                    Caption = ivtController.CustomsTemperature.Caption,
                    Minimum = ivtController.CustomsTemperature.Minimum.ToString(ci),
                    Maximum = ivtController.CustomsTemperature.Maximum.ToString(ci)
                };
                return JsonConvert.SerializeObject(jsonData);
            }
            else
            {
                Body.Environment.Logs.Error("Can't get customs of IVT controller device.", new Exception($"Device (ID: {id}) is not IVT controller."), this.GetType().Name);
                return null;
            }
        }

        // GET: HistoryIVTController/?
        public ActionResult HistoryIVTController(int id)
        {
            return View("Homyoko/HistoryIVTController", new Models.Devices.Homyoko.EditIVTControllerViewModel() { ID = id });
        }
        // GET: GetHistoryHomyokoIVTController/?
        public string GetHistoryHomyokoIVTController(int id)
        {
            // Načítaj zoznam histórie log údajov a vrát ich v čo najkratšej textovej podobe.
            StringBuilder result = new StringBuilder();
            string tableName = $"deviceHistory_{id}";
            string sql = $"SELECT logTime, temperatureFloor FROM {tableName} ORDER BY logTime DESC";
            DBClient dbClient = Body.Database.DBClientLogs;
            CultureInfo ci = (System.Globalization.CultureInfo)CultureInfo.CurrentCulture.Clone();
            ci.NumberFormat.NumberDecimalSeparator = ".";
            ci.NumberFormat.NegativeSign = "-";
            ci.NumberFormat.NumberGroupSeparator = "";
            try
            {
                dbClient.Open();
                using DBReader dbReader = dbClient.ExecuteReader(sql);
                int count = 0;
                while (dbReader.Read())
                {
                    string time = dbReader.GetDateTime("logTime").ToString("yyyy-MM-dd HH:mm:ss");
                    string temp = dbReader.GetFloat("temperatureFloor").ToString(ci);
                    result.AppendFormat("{0},{1};", time, temp);
                    count++;
                    if (count > 300) break;
                }
            }
            catch (Exception)
            {
            }
            return result.ToString();
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