using System;
using System.ComponentModel.DataAnnotations;
using X.Homylogic;
using X.Homylogic.Models.Objects;
using X.Homylogic.Models.Objects.Devices.Homyoko;

namespace HomylogicAsp.Models.Devices.Homyoko
{
    public class EditIVTControllerViewModel : EditTCPSocketViewModel
    {
        [Required]
        public IVTController.PacketTypes PacketType { get; set; }
        public bool WriteToBuffer { get; set; }
        public bool CanAutoDataUpdate { get; set; }

        public EditIVTControllerViewModel()
        {
            this.SetViewModel(new IVTController());
        }
        public EditIVTControllerViewModel(IVTController weatherStation)
        {
            this.SetViewModel(weatherStation);
        }
        public IVTController GetIVTController()
        {
            IVTController ivtController;
            if (this.ID > 0)
                ivtController = (IVTController)Body.Runtime.Devices.FindDataRecord(this.ID);
            else
                ivtController = (IVTController)Body.Runtime.Devices.GetInitializedDevice(DeviceX.DeviceTypes.HomyokoIVTController);
            ivtController.Name = this.Name;
            ivtController.Notice = this.Notice;
            ivtController.Disabled = this.Disabled;
            ivtController.ShowOnHome = this.ShowOnHome;
            ivtController.IPAddress = this.IPAddress;
            ivtController.PortNumber = this.PortNumber;
            ivtController.PacketType = this.PacketType;
            ivtController.WriteToBuffer = this.WriteToBuffer;
            ivtController.CanAutoDataUpdate = this.CanAutoDataUpdate;
            return ivtController;
        }
        private void SetViewModel(IVTController ivtController) 
        {
            this.DeviceType = DeviceX.DeviceTypes.HomyokoIVTController;
            this.ID = ivtController.ID;
            this.Name = ivtController.Name;
            this.Notice = ivtController.Notice;
            this.Disabled = ivtController.Disabled;
            this.ShowOnHome = ivtController.ShowOnHome;
            this.IPAddress = ivtController.IPAddress;
            this.PortNumber = ivtController.PortNumber;
            this.PacketType = ivtController.PacketType;
            this.WriteToBuffer = ivtController.WriteToBuffer;
            this.CanAutoDataUpdate = ivtController.CanAutoDataUpdate;
        }

    }
}
