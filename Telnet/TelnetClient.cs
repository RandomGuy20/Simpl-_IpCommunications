using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronSockets;
using IPCommunicationSuite.TCP;

namespace IPCommunicationSuite.Telnet
{
    public class TelnetClient
    {
        #region Fields

        private TCPIPClient telnet;
        

        private const int BUFFERSIZE = 1024;
        private const int PORT = 23;

        private string ipAddress;
        private string userName;
        private string password;

        private bool isConnected;


        #endregion

        #region Properties

        public string IpAddress { get { return ipAddress; } }
        public string Username { get { return userName; }  }
        public string Password { get { return password; } }

        public bool Debug { get; set; }
        public bool IsConnected { get { return isConnected; } }

        public int Port { get { return PORT; } }
  
        #endregion

        #region Delegates

        public delegate void StatusChangeEventHandler(SocketStatus status);
        public delegate void IncomingDataEventHandler(string data);

        #endregion

        #region Events

        public event StatusChangeEventHandler onStatusChange;
        public event IncomingDataEventHandler onIncomingData;

        #endregion

        #region Constructors

        public TelnetClient(string Ip, string pass, string user)
        {
            ipAddress = Ip;
            password = pass;
            userName = user;

            telnet = new TCPIPClient(ipAddress, PORT, BUFFERSIZE);
            telnet.Debug(false);
            telnet.onIncomingData += new TCPIPClient.IncomingDataEventHandler(telnet_onIncomingData);
            telnet.onStatusChange += new TCPIPClient.StatusChangeEventHandler(telnet_onStatusChange);
        }

        #endregion

        #region Internal Methods

        private void SendDebug(string data)
        {
            if (Debug)
            {
                CrestronConsole.PrintLine("\nTelnet Client Debug Message is: " + data);
                ErrorLog.Error("\nTelnet Client Debug Message is: " + data);
            }

        }

        void telnet_onStatusChange(SocketStatus status)
        {
            try
            {
                onStatusChange(status);
                isConnected = status == SocketStatus.SOCKET_STATUS_CONNECTED ? true : false;
                SendDebug("isConnected - " + isConnected);
            }
            catch (Exception ex)
            {
                SendDebug("Error when setting status change is: " + ex.Message);
            }

        }

        void telnet_onIncomingData(string data)
        {
            try
            {
                if (data.Contains("login:"))
                {
                    telnet.SendData(userName);
                    SendDebug("Telnet requested Login and sent this back: " + userName);
                }
                else if (data.Contains("Password:"))
                {
                    telnet.SendData(password);
                    SendDebug("Telnet requested password and send this back: " + password);
                }
                else if (data.Length > 0)
                {
                    onIncomingData(data);
                    SendDebug("Incoming data is: " + data);
                }
            }
            catch (Exception ex)
            {
                SendDebug("Telnet error when parsing incoming data is: " + ex.Message);
            }

        }

        #endregion

        #region Public Methods

        public void Connect()
        {
            try
            {
                telnet.Connect();
            }
            catch (Exception ex)
            {
                SendDebug("Error in Connect Method is: " + ex.Message);
            }
        }

        public void Disconnect()
        {
            telnet.Disconnect();
        }

        public void SendData(string data)
        {
            telnet.SendData(data);
        }

        public void SetTCPDebug(bool state)
        {
            telnet.Debug(state);
        }

        #endregion
    }
}