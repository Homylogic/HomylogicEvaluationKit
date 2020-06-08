using System;
using System.ComponentModel.DataAnnotations;
using X.Homylogic;
using X.Homylogic.Models.Objects;
using X.Homylogic.Models.Objects.Devices;

namespace HomylogicAsp.Models.Devices
{
    public class EditSerialPortViewModel : EditDeviceViewModel
    {
        [Required]
        [Range(1, 1000)]
        public int PortNumber { get; set; }
        [Required]
        [Range(1, 100000)]
        public int BaudRate { get; set; }
        public string PacketEndChar { get; set; }

        public EditSerialPortViewModel()
        {
            this.SetViewModel(new SerialDeviceX());
        }
        public EditSerialPortViewModel(SerialDeviceX serialDevice)
        {
            this.SetViewModel(serialDevice);
        }
        public SerialDeviceX GetSerialDevice()
        {
            SerialDeviceX serialDevice;
            if (this.ID > 0)
                serialDevice = (SerialDeviceX)Body.Runtime.Devices.FindDataRecord(this.ID);
            else
                serialDevice = (SerialDeviceX)Body.Runtime.Devices.GetInitializedDevice(DeviceX.DeviceTypes.Serial);
            serialDevice.Name = this.Name;
            serialDevice.Notice = this.Notice;
            serialDevice.Disabled = this.Disabled;
            serialDevice.PortNumber = this.PortNumber;
            serialDevice.PacketEndChar = this.PacketEndChar;
            return serialDevice;
        }
        private void SetViewModel(SerialDeviceX serialDevice) 
        {
            this.DeviceType = DeviceX.DeviceTypes.Serial;
            this.ID = serialDevice.ID;
            this.Name = serialDevice.Name;
            this.Notice = serialDevice.Notice;
            this.Disabled = serialDevice.Disabled;
            this.PortNumber = serialDevice.PortNumber;
            this.BaudRate = serialDevice.BaudRate;
            this.PacketEndChar = serialDevice.PacketEndChar;
        }

    }
}
