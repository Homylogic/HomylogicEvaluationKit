/* HOMYLOGIC DEVICE X
 * 
 * Obsahuje vlastnosti a metódy spoločné pre všetky zariadenia. 
 * 
 * 
 * 
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using X.Basic;
using X.Data;

namespace X.Homylogic.Models.Objects
{
    public class DeviceX : Factory.ObjectX
    {
        StringBuilder _partialReceivedData = new StringBuilder();

        bool _isOpen;
        /// <summary>
        /// Či bolo zariadenie otvorené pre komunikáciu.
        /// </summary>
        public bool IsOpen
        {
            get { return _isOpen; }
            protected set { _isOpen = value; }    
        }
        /// <summary>
        /// Či zariadenie umožňuje zapisovanie, pretože po otvorení nemusí byť zariadenie pripravené posielať údaje.
        /// </summary>
        public bool CanWrite { get; protected set; }

        #region --- DATA PROPERTIES ---

        public enum DeviceTypes : Int32
        {
            Unknown = 0,
            [Description("Serial port")]
            Serial = 1,
            [Description("TCP socket")]
            TCPSocket = 2,
            [Description("Homyoko meteo station")]
            HomyokoWeatherStation = 1001,
            [Description("Homyoko IVT controller")]
            HomyokoIVTController = 1002
        }
        /// <summary>
        /// Určuje typ zariadenia.
        /// </summary>
        public DeviceTypes DeviceType { get; protected set; } = DeviceTypes.Unknown;
        /// <summary>
        /// Či sa budú prijaté údaje zapisovať do vstupného zásobníka.
        /// </summary>
        public bool WriteToBuffer { get; set; } = true;
        /// <summary>
        /// Umožňuje zapnúť automatickú komunikáciu so vzdialeným zariadením a nastaviť tak vlastnosti runtime objektu automaticky.
        /// Používa sa pre zariadenia ktoré implementujú rozhranie 'IAutoDataUpdate'.
        /// </summary>
        public bool CanAutoDataUpdate { get; set; } = false;

        #endregion

        #region --- DATA RECORD ---

        public const string TABLE_NAME = "devices";
        public override string TableName => TABLE_NAME;

        public override void SetRecordValues(DBReader dbReader, List<string> tags)
        {
            base.SetRecordValues(dbReader, tags);
            this.DeviceType = (DeviceTypes)dbReader.GetInt32("deviceType");
            this.WriteToBuffer = dbReader.GetBool("writeToBuffer");
            this.CanAutoDataUpdate = dbReader.GetBool("autoDataUpdate");
        }
        public override string SqlInsert(Data.Management.SqlConvert q, List<string> tags)
        {
            string fields = "deviceType, autoDataUpdate, writeToBuffer, {0}";
            StringBuilder values = new StringBuilder();
            values.AppendFormat("{0}, ", (Int32)this.DeviceType);
            values.AppendFormat("{0}, ", q.Innt32(this.WriteToBuffer));
            values.AppendFormat("{0}, ", q.Innt32(this.CanAutoDataUpdate));
            values.Append(@"{1}");
            return string.Format(base.SqlInsert(q, tags), fields, values);
        }
        public override string SqlUpdate(Data.Management.SqlConvert q, List<string> tags)
        {
            StringBuilder values = new StringBuilder();
            values.AppendFormat("deviceType = {0}, ", (Int32)this.DeviceType);
            values.AppendFormat("writeToBuffer = {0}, ", q.Innt32(this.WriteToBuffer));
            values.AppendFormat("autoDataUpdate = {0}, ", q.Innt32(this.CanAutoDataUpdate));
            values.Append(@"{0}");
            return string.Format(base.SqlUpdate(q, tags), values);
        }
        public override XException Validate()
        {
            if (this.DeviceType == DeviceTypes.Unknown) return new XException("Invalid device type.", 1001);
            return base.Validate();
        }
        public override void Save()
        {
            // Zatvoriť otvorené zariadenie pred uložením nových zmien záznamu.
            this.Close();

            // Aktualivozať relačné tabuľky v ktorých sa nachádza názov tohoto zariadenia, ak bol názov zmenený.
            if (this.RecordState == RecordStateTypes.Edit && this.IsNameChanged) 
            {
                Body.Runtime.Triggers.RelationDeviceUpdateNames(this);
                Body.Runtime.InputBuffers.RelationDeviceUpdateNames(this);
                Body.Runtime.OutputBuffers.RelationDeviceUpdateNames(this);
            }

            base.Save();
        }
        public override void Delete(Int64 id)
        {
            // Zatvoriť otvorené zariadenie pred vymazaním záznamu.
            this.Close();

            // Vymazať všetky relačné záznamy, ktoré odkazujú na toto zariadenie.
            Body.Runtime.Triggers.RelationDeviceDelete(id, this.DBClient);
            Body.Runtime.InputBuffers.RelationDeviceDelete(id, this.DBClient);
            Body.Runtime.OutputBuffers.RelationDeviceDelete(id, this.DBClient);

            base.Delete(id);
        }

        #endregion

        public DeviceX() : base(true) {}

        // Metódy typu abstract, určené hlavne pre vyšší objekt v dedení. Avšak aby sa dala vytvoriť nová inštancia základného objektu zariadenia DeviceX tak sú metódy typu virtual.
        public virtual void Open() 
        {
            if (this.Disabled) throw new InvalidOperationException("Can't open disabled device.");
            this.IsOpen = true; 
        }
        public virtual void Close() { this.IsOpen = false; this.CanWrite = false; }
        public virtual void Write(string data) 
        {
            if (!this.IsOpen) throw new InvalidOperationException("Can't write data to closed device.");
            if (!this.CanWrite) throw new InvalidOperationException("Can't write data to device, device is not ready yet.");
        }

        #region --- EVENTS ---

        public class DataReceivedEventArgs : EventArgs
        {
            public DeviceX Device { get; private set; }
            public string Data { get; private set; }
            public DataReceivedEventArgs(DeviceX device, string data)
            {
                this.Device = device;
                this.Data = data;
            }
        }
        public delegate void DataReceivedEventHandler(DeviceX device, DataReceivedEventArgs e);
        /// <summary>
        /// Prijatie údajov počas komunikácie so zariadenia.
        /// </summary>
        public event DataReceivedEventHandler DataReceived;
        protected virtual void OnDataRecived(string data, string packetEndChar) 
        {
            // Spracovanie údajov a rozdelenie na pakety.
            if (string.IsNullOrEmpty(data)) return;
            string dataPacket;
            if (data.Contains(packetEndChar))
            {
                dataPacket = string.Format("{0}{1}",_partialReceivedData.ToString(), data);
                _partialReceivedData.Clear();
            }
            else {
                _partialReceivedData.Append(data);
                if (_partialReceivedData.Length < 8000)
                {
                    return;
                }
                else {
                    dataPacket = _partialReceivedData.ToString();
                    _partialReceivedData.Clear();
                }
            }

            // Vyvolanie udalosti.
            DataReceivedEventArgs eventArgs = new DataReceivedEventArgs(this, dataPacket);
            this.DataReceived?.Invoke(this, eventArgs);

            // Zapísanie údajov do InputBuffera.
            if (WriteToBuffer)
                Body.Runtime.InputBuffers.Enqueue(eventArgs);
        }

        #endregion

    }
}
