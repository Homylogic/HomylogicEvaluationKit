using System;
using System.ComponentModel.DataAnnotations;
using X.Homylogic;
using X.Homylogic.Models.Objects;
using X.Homylogic.Models.Objects.Devices;

namespace HomylogicAsp.Models.Devices
{
    public class EditTCPSocketViewModel : EditDeviceViewModel
    {
        [Required]
        public TCPDeviceX.SocketTypes SocketType { get; set; }
        [Required]
        public string IPAddress { get; set; } 
        [Required]
        [Range(1, 100000)]
        public int PortNumber { get; set; }
        public string PacketEndChar { get; set; } 

        public EditTCPSocketViewModel()
        {
            this.SetViewModel(new TCPDeviceX());
        }
        public EditTCPSocketViewModel(TCPDeviceX tcpDevice)
        {
            this.SetViewModel(tcpDevice);
        }
        public TCPDeviceX GetTCPDevice()
        {
            TCPDeviceX tcpDevice;
            if (this.ID > 0)
                tcpDevice = (TCPDeviceX)Body.Runtime.Devices.FindDataRecord(this.ID);
            else
                tcpDevice = (TCPDeviceX)Body.Runtime.Devices.GetInitializedDevice(DeviceX.DeviceTypes.TCPSocket);
            tcpDevice.Name = this.Name;
            tcpDevice.Notice = this.Notice;
            tcpDevice.Disabled = this.Disabled;
            tcpDevice.SocketType = this.SocketType;
            tcpDevice.IPAddress = this.IPAddress;
            tcpDevice.PortNumber = this.PortNumber;
            tcpDevice.PacketEndChar = this.PacketEndChar;
            return tcpDevice;
        }
        private void SetViewModel(TCPDeviceX tcpDevice) 
        {
            this.DeviceType = DeviceX.DeviceTypes.TCPSocket;
            this.ID = tcpDevice.ID;
            this.Name = tcpDevice.Name;
            this.Notice = tcpDevice.Notice;
            this.Disabled = tcpDevice.Disabled;
            this.SocketType = tcpDevice.SocketType;
            this.IPAddress = tcpDevice.IPAddress;
            this.PortNumber = tcpDevice.PortNumber;
            this.PacketEndChar = tcpDevice.PacketEndChar;
        }

    }
}
