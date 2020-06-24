/* HOMYLOGIC TCP SOCKET DEVICE X
 * 
 * Umožňuje komunikáciu cez TCP socket.
 * 
 * 
 */
using System;
using System.Text;
using System.IO.Ports;
using System.Threading;
using System.Net.Sockets;
using X.Basic;
using X.Data;
using System.Collections.Generic;
using System.ComponentModel;

namespace X.Homylogic.Models.Objects.Devices
{
    public class TCPDeviceX : DeviceX
    {
        const string TITLE = "TCP socket";

        Thread _threadTCPSocket;
        TcpListener _tcpListener;
        TcpClient _tcpClient;
        NetworkStream _networkStream;
        bool _isClosing;
        /// <summary>
        /// Allows disable logs client/server disconnect.
        /// </summary>
        protected bool _ignoreLogDisconnect = false;

        #region --- DATA PROPERTIES ---

        public enum SocketTypes : Int32 
        {
            [Description("Server")]
            Server = 1,
            [Description("Client")]
            Client = 2
        }
        public SocketTypes SocketType { get; set; } = SocketTypes.Server;
        public string IPAddress { get; set; } = "127.0.0.1";
        public int PortNumber { get; set; } = 50000;
        public string PacketEndChar { get; set; } = @"\r\n";
        public override string Settings => $"TCP {this.SocketType}: {this.IPAddress}:{this.PortNumber}, {this.PacketEndChar}"; 

        #endregion

        #region --- DATA RECORD ---

        public override void SetRecordValues(DBReader dbReader, List<string> tags)
        {
            if (tags.Contains("ignore-tcp-device"))
                base.SetRecordValues(dbReader, tags);
            else { 
                base.SetRecordValues(dbReader, tags);
                this.SocketType = (SocketTypes)dbReader.GetInt32("setInt01");
                this.PortNumber = dbReader.GetInt32("setInt02");
                this.IPAddress = dbReader.GetString("setStr01");
                this.PacketEndChar = dbReader.GetString("setStr02");
            }
        }
        public override string SqlInsert(Data.Management.SqlConvert q, List<string> tags)
        {
            if (tags.Contains("ignore-tcp-device"))
                return base.SqlInsert(q, tags);
            else {
                string fields = "setInt01, setInt02, setStr01, setStr02";
                StringBuilder values = new StringBuilder();
                values.AppendFormat("{0}, ", (Int32)this.SocketType);
                values.AppendFormat("{0}, ", (Int32)(this.PortNumber));
                values.AppendFormat("{0}, ", q.Str(this.IPAddress));
                values.AppendFormat("{0}", q.Str(this.PacketEndChar));
                return string.Format(base.SqlInsert(q, tags), fields, values);
            }
        }
        public override string SqlUpdate(Data.Management.SqlConvert q, List<string> tags)
        {
            if (tags.Contains("ignore-tcp-device"))
                return base.SqlUpdate(q, tags);
            else { 
                StringBuilder values = new StringBuilder();
                values.AppendFormat("setInt01 = {0}, ", (Int32)this.SocketType); ;
                values.AppendFormat("setInt02 = {0}, ", (Int32)(this.PortNumber));
                values.AppendFormat("setStr01 = {0}, ", q.Str(this.IPAddress));
                values.AppendFormat("setStr02 = {0}", q.Str(this.PacketEndChar));
                return string.Format(base.SqlUpdate(q, tags), values);
            }
        }
        public override XException Validate()
        {
            if (string.IsNullOrEmpty(this.IPAddress)) return new XException("Invalid IP address.", 10001);
            if (this.PortNumber <= 0) return new XException("Invalid port number.", 10002);
            return base.Validate();
        }

        #endregion

        public TCPDeviceX() { base.DeviceType = DeviceTypes.TCPSocket; }

        /// <summary>
        /// Otvorí komunikáciu pre TCP Scoket.
        /// </summary>
        public override void Open()
        {
            if (this.IsOpen) return;
            base.Open(); // Skontrolovať stav zariadenia a nastaviť príznak IsOpen.
            _isClosing = false;

            if (_threadTCPSocket == null || !_threadTCPSocket.IsAlive) _threadTCPSocket = new Thread(ThreadTCPSocket) { Name = this.GetType().Name, IsBackground = true };
            _threadTCPSocket.Start();
        }
        /// <summary>
        /// Zatvorí komunikáciu.
        /// </summary>
        public override void Close()
        {
            _isClosing = true;
            this.CloseTCPObjects();
        }
        /// <summary>
        /// Zatvorí objekty TCP.
        /// </summary>
        void CloseTCPObjects() 
        {
            if (_tcpListener != null)
            {
                _tcpListener.Stop();
                if (_tcpListener.Server != null)
                    _tcpListener.Server.Dispose();
            }
            _tcpListener = null;

            if (_tcpClient != null)
                _tcpClient.Dispose();
            _tcpClient = null;

            if (_networkStream != null) 
                _networkStream.Dispose();
            _networkStream = null;
        }
        /// <summary>
        /// Odošle zadané údaje na pripojený TCP socket.
        /// </summary>
        public override void Write(string data) 
        {
            // Skontroluje stav zariadenia, napr. či je komunikácia otvorená.
            base.Write(data);
            if (_isClosing) throw new InvalidOperationException($"Can't write data to closing TCP socket device '{this.Name}'.");

            // Odoslanie údajov na port.
            try
            {
                byte[] bytes = Encoding.UTF8.GetBytes(data);
                _networkStream.Write(bytes, 0, bytes.Length);
                _networkStream.Flush();
            }
            catch (Exception ex)
            {
                Body.Environment.Logs.Error($"Can't write data to TCP device {this.IPAddress}:{this.PortNumber}.", ex, $"{TITLE} : {this.Name}");
            }
        }
        /// <summary>
        /// Vlákno ktoré spustí komunikáciu.
        /// </summary>
        [MTAThread]
        void ThreadTCPSocket() 
        {
g_start:    
            try
            {
                // Zatvoriť objekty, pretože nastala chyba a zariadenie sa bude opätovne otvárať pre komunikáciu.
                this.CloseTCPObjects();

                // Otvoriť novú komunikáciu, podľa typu nastavenia.
                try
                {
                    switch (this.SocketType)
                    {
                        case SocketTypes.Server:
                            // Spusti TCP server.
                            System.Net.IPAddress ipAddress = System.Net.IPAddress.Parse(this.IPAddress);
                            _tcpListener = new TcpListener(ipAddress, this.PortNumber);
                            _tcpListener.Start();

                            // Ćakaj na pripojenie klienta.
                            while (!_isClosing && !this.Disabled)
                            {
                                if (!_tcpListener.Pending())
                                {
                                    Thread.Sleep(100);
                                    continue;
                                }
                                else {
                                    // Nejaký klient sa chce pripojiť.
                                    _tcpClient = _tcpListener.AcceptTcpClient();
                                    break;
                                }
                            }
                            break;

                        case SocketTypes.Client:
                            // Skús pripojiť klienta.
                            _tcpClient = new TcpClient(this.IPAddress, this.PortNumber);
                            break;

                        default: throw new InvalidOperationException("Invalid device socket type.");
                    }
                }
                catch (SocketException ex) 
                {
                    // Ak nenastalo manuálne zatvorenie servera počas čakania na klientov.
                    if (ex.ErrorCode != 10004) 
                    {
                        this.OnCantOpenSocket(ex);
                    }
                    goto g_exit;
                }
                catch (Exception ex)
                {
                    // Ukonči komunikáciu, pretože TCP Socket nie je možné otvoriť.
                    this.OnCantOpenSocket(ex);
                    goto g_exit;
                }

                if (_isClosing || this.Disabled) goto g_exit;

                // Nastav objekt pre čítanie a zápis údajov.
                _networkStream = _tcpClient.GetStream();
                this.CanWrite = true;

                // Slučka tohoto vlákna, slúži pre čítanie údajov zo zariadenia.
                while (!_isClosing && !this.Disabled)
                {
                    try
                    {
                        // Čakaj na prijatie údajov.
                        while (_networkStream != null && !_networkStream.DataAvailable)
                        {
                            // Test či je klient stále pripojený.
                            if (_tcpClient.Client.Poll(0, SelectMode.SelectRead))
                            {
                                byte[] buff = new byte[1];
                                if (_tcpClient.Client.Receive(buff, SocketFlags.Peek) == 0)
                                {
                                    if (this.SocketType == SocketTypes.Server)
                                    {
                                        if (!_ignoreLogDisconnect)
                                            Body.Environment.Logs.Info($"Remote client has beend disconnected.", source:$"{TITLE} : {this.Name}");
                                        this.CanWrite = false;
                                        goto g_start;
                                    }
                                    else
                                    {
                                        if (!_ignoreLogDisconnect)
                                            Body.Environment.Logs.Info($"Remote server has beend disconnected.", source:$"{TITLE} : {this.Name}");
                                        goto g_exit;
                                    }
                                }
                            }
                            Thread.Sleep(100);
                        }
                        if (_networkStream == null) goto g_exit;

                        try
                        {
                            byte[] bytes = new byte[1024];
                            int readBytes = _networkStream.Read(bytes, 0, bytes.Length);
                            if (readBytes > 0)
                            {
                                // Vykonaj prijatie údajov so zariadenia (zapíše údaje do bufferu a vyvolá udalosť na zariadení).
                                string data = Encoding.UTF8.GetString(bytes, 0, readBytes);
                                this.OnDataRecived(data, this.PacketEndChar);
                            }
                        }
                        catch (Exception ex)
                        {
                            Body.Environment.Logs.Error($"Problem processing received TCP socket data.", ex, $"{TITLE} : {this.Name}");
                        }
                    }
                    catch (SocketException ex) 
                    { 
                        if (ex.ErrorCode == 10053) goto g_exit; // Zariadenie bolo zatvorené.
                    }
                    catch (Exception ex)
                    {
                        if (_networkStream == null) goto g_exit; // Zariadenie bolo zatvorené.
                        Body.Environment.Logs.Error($"Can't read data from TCP socket device.", ex, $"{TITLE} : {this.Name}");
                    }
                }
                this.CanWrite = false;
            }
            catch (ThreadAbortException) {}
            catch (Exception) {
                this.CanWrite = false;
                Thread.Sleep(5000);
                goto g_start; 
            }
g_exit:
            // Ukončenie komunikácie a zatvorenie objektov.
            this.CloseTCPObjects();

            // Zaridenie bolo zatvorené a komunikácia bola ukončená.
            _isClosing = false;
            base.Close(); // Nastaviť príznak IsOpen a CanWrite.
        }
        /// <summary>
        /// Can handle exception when socket can't be openned.
        /// </summary>
        protected virtual void OnCantOpenSocket(Exception ex) 
        {
            Body.Environment.Logs.Error($"Can't open TCP socket device {this.IPAddress}:{this.PortNumber}.", ex, $"{TITLE} : {this.Name}");
        }

    }


}
