using System;
using System.ComponentModel.DataAnnotations;
using X.Homylogic;
using X.Homylogic.Models.Objects;
using X.Homylogic.Models.Objects.Devices.Homyoko;

namespace HomylogicAsp.Models.Devices.Homyoko
{
    public class EditWeatherStationViewModel : EditTCPSocketViewModel
    {
        [Required]
        public WeatherStation.PacketTypes PacketType { get; set; }
        public bool WriteToBuffer { get; set; }
        public bool CanAutoDataUpdate { get; set; }
        public WeatherStation.CustomsTemperatureValues CustomsTemperature1 { get; set; }
        public WeatherStation.CustomsTemperatureValues CustomsTemperature2 { get; set; }
        public WeatherStation.CustomsWindspeedValues CustomsWindspeed { get; set; }
        public WeatherStation.CustomsSunshineValues CustomsSunshine { get; set; }

        public EditWeatherStationViewModel()
        {
            this.SetViewModel(new WeatherStation());
        }
        public EditWeatherStationViewModel(WeatherStation weatherStation)
        {
            this.SetViewModel(weatherStation);
        }
        public WeatherStation GetWeatherStation()
        {
            WeatherStation weatherStation;
            if (this.ID > 0)
                weatherStation = (WeatherStation)Body.Runtime.Devices.FindDataRecord(this.ID);
            else
                weatherStation = (WeatherStation)Body.Runtime.Devices.GetInitializedDevice(DeviceX.DeviceTypes.HomyokoWeatherStation);
            weatherStation.Name = this.Name;
            weatherStation.Notice = this.Notice;
            weatherStation.Disabled = this.Disabled;
            weatherStation.ShowOnHome = this.ShowOnHome;
            weatherStation.IPAddress = this.IPAddress;
            weatherStation.PortNumber = this.PortNumber;
            weatherStation.PacketType = this.PacketType;
            weatherStation.WriteToBuffer = this.WriteToBuffer;
            weatherStation.CanAutoDataUpdate = this.CanAutoDataUpdate;
            weatherStation.CustomsTemperature1 = this.CustomsTemperature1;
            weatherStation.CustomsTemperature2 = this.CustomsTemperature2;
            weatherStation.CustomsWindspeed = this.CustomsWindspeed;
            weatherStation.CustomsSunshine = this.CustomsSunshine;
            return weatherStation;
        }
        private void SetViewModel(WeatherStation weatherStation) 
        {
            this.DeviceType = DeviceX.DeviceTypes.HomyokoWeatherStation;
            this.ID = weatherStation.ID;
            this.Name = weatherStation.Name;
            this.Notice = weatherStation.Notice;
            this.Disabled = weatherStation.Disabled;
            this.ShowOnHome = weatherStation.ShowOnHome;
            this.IPAddress = weatherStation.IPAddress;
            this.PortNumber = weatherStation.PortNumber;
            this.PacketType = weatherStation.PacketType;
            this.WriteToBuffer = weatherStation.WriteToBuffer;
            this.CanAutoDataUpdate = weatherStation.CanAutoDataUpdate;
            this.CustomsTemperature1 = weatherStation.CustomsTemperature1;
            this.CustomsTemperature2 = weatherStation.CustomsTemperature2;
            this.CustomsWindspeed = weatherStation.CustomsWindspeed;
            this.CustomsSunshine = weatherStation.CustomsSunshine;
        }

    }
}
