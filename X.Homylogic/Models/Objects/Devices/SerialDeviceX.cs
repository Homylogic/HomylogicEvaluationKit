/* HOMYLOGIC SERIAL DEVICE X
 * 
 * Umožňuje komunikáciu cez sériový port.
 * 
 * 
 */
using System;
using System.Text;
using System.IO.Ports;
using System.Threading;
using X.Basic;
using X.Data;
using System.Collections.Generic;

namespace X.Homylogic.Models.Objects.Devices
{
    public class SerialDeviceX : DeviceX
    {
        const string TITLE = "Serial port";

        Thread _threadSerialPort;
        SerialPort _serialPort;
        bool _isClosing;

        #region --- DATA PROPERTIES ---

        public int PortNumber { get; set; } = 0;
        public int BaudRate { get; set; } = 9600;
        public string PacketEndChar { get; set; } = "Asc(13)Asc(10)";
        /* Zatiaľ sa nepoužíva ...
        public byte DataBits { get; set; } = 8;
        public Parity Parity { get; set; } = Parity.None;
        public System.IO.Ports.StopBits StopBits { get; set; } = StopBits.One;
        System.IO.Ports.Handshake FlowControl { get; set; } = Handshake.None;
        public bool DTR { get; set; } = true;
        public bool RTS { get; set; } = true;
        */
        public override string Settings => $"COM{this.PortNumber}, {this.BaudRate}, {this.PacketEndChar}"; 

        #endregion

        #region --- DATA RECORD ---

        public override void SetRecordValues(DBReader dbReader, List<string> tags)
        {
            base.SetRecordValues(dbReader, tags);
            this.PortNumber = dbReader.GetInt32("setInt01");
            this.BaudRate = dbReader.GetInt32("setInt02");
            this.PacketEndChar = dbReader.GetString("setStr01");
        }
        public override string SqlInsert(Data.Management.SqlConvert q, List<string> tags)
        {
            string fields = "setInt01, setInt02, setStr01";
            StringBuilder values = new StringBuilder();
            values.AppendFormat("{0}, ", (Int32)this.PortNumber);
            values.AppendFormat("{0}, ", (Int32)(this.BaudRate));
            values.AppendFormat("{0}", q.Str(this.PacketEndChar));
            return string.Format(base.SqlInsert(q, tags), fields, values);
        }
        public override string SqlUpdate(Data.Management.SqlConvert q, List<string> tags)
        {
            StringBuilder values = new StringBuilder();
            values.AppendFormat("setInt01 = {0}, ", (Int32)this.PortNumber); ;
            values.AppendFormat("setInt02 = {0}, ", (Int32)(this.BaudRate));
            values.AppendFormat("setStr01 = {0}", q.Str(this.PacketEndChar));
            return string.Format(base.SqlUpdate(q, tags), values);
        }
        public override XException Validate()
        {
            if (this.PortNumber <= 0) return new XException("Invalid port number.", 10001);
            if (this.BaudRate <= 0) return new XException("Invalid baund rate.", 10002);
            return base.Validate();
        }

        #endregion

        public SerialDeviceX() { base.DeviceType = DeviceTypes.Serial; }

        /// <summary>
        /// Otvorí komunikáciu na sériovom porte.
        /// </summary>
        public override void Open()
        {
            if (this.IsOpen) return;
            base.Open(); // Skontrolovať stav zariadenia a nastaviť príznak IsOpen.
            _isClosing = false;
            if (_threadSerialPort == null || !_threadSerialPort.IsAlive) _threadSerialPort = new Thread(ThreadSerialPort) { Name = this.GetType().Name, IsBackground = true };
            _threadSerialPort.Start();
        }
        /// <summary>
        /// Zatvorí komunikáciu.
        /// </summary>
        public override void Close()
        {
            _isClosing = true;
        }
        /// <summary>
        /// Odošle zadané údaje na sériový port.
        /// </summary>
        public override void Write(string data)
        {
            // Skontroluje stav zariadenia, napr. či je komunikácia otvorené.
            base.Write(data);
            if (_isClosing) throw new InvalidOperationException($"Can't write data to closing serial device '{this.Name}'.");

            // Odoslanie údajov na port.
            try
            {
                byte[] bytes = Encoding.UTF8.GetBytes(data);
                _serialPort.Write(bytes, 0, bytes.Length);
            }
            catch (Exception ex)
            {
                Body.Environment.Logs.Error($"Can't write data to serial device COM{this.PortNumber}.", ex, $"{TITLE} : {this.Name}");
            }
        }
        /// <summary>
        /// Vlákno ktoré spustí komunikáciu.
        /// </summary>
        [MTAThread]
        void ThreadSerialPort() 
        {
g_start:
            try
            {
                // Zatvoriť objekt, pretože nastala chyba a zariadenie sa bude opätovne otvárať pre komunikáciu.
                if (_serialPort != null) {
                    _serialPort.DataReceived -= SerialPort_DataReceived;
                    _serialPort.Dispose();
                    _serialPort = null;
                }

                // Otvoriť novú komunikáciu.
                string portNumber = $"COM{this.PortNumber}";
                _serialPort = new SerialPort(portNumber, this.BaudRate);
                _serialPort.DataReceived += SerialPort_DataReceived;

                try
                {
                    _serialPort.Open();
                }
                catch (Exception ex)
                {
                    // Ukonči komunikáciu, pretože port nie je možné otvoriť.
                    Body.Environment.Logs.Error($"Can't open serial device on port COM{this.PortNumber}.", ex, $"{TITLE} : {this.Name}");
                    goto g_exit;
                }

                // Slučka tohoto vlákna, zatiaľ nevykonáva nič len drží zariadenie v otvoreno stave (pretože vlákno je aktívne).
                while (!_isClosing && !this.Disabled) 
                {
                    this.CanWrite = true;
                    Thread.Sleep(500);
                }
                this.CanWrite = false;
            }
            catch (Exception) {
                this.CanWrite = false;
                Thread.Sleep(5000);
                goto g_start; 
            }
g_exit:
            // Ukončenie komunikácie a zatvorenie zariadenia.
            try
            {
                if (_serialPort != null)
                {
                    _serialPort.Dispose();
                    _serialPort = null;
                }
            }
            catch (Exception)
            {
            }

            // Zaridenie bolo zatvorené a komunikácia bola ukončená.
            _isClosing = false;
            base.Close(); // Nastaviť príznak IsOpen a CanWrite.
        }
        /// <summary>
        /// Handler pre spracovanie prijatých údajov na sériovom porte.
        /// </summary>
        void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e) 
        {
            try
            {
                SerialPort serialPort = (SerialPort)sender;
                byte[] bytes = new byte[serialPort.BytesToRead - 1];
                int readBytes = serialPort.Read(bytes, 0, bytes.Length);
                if (readBytes > 0) 
                {
                    // Vykonaj prijatie údajov so zariadenia (zapíše údaje do bufferu a vyvolá udalosť na zariadení).
                    string data = Encoding.UTF8.GetString(bytes, 0, readBytes);
                    this.OnDataRecived(data, this.PacketEndChar);
                }
            }
            catch (Exception ex)
            {
                Body.Environment.Logs.Error($"Can't read data from serial device on port COM{this.PortNumber}.", ex, $"{TITLE} : {this.Name}");
            }
        }



    }


}
