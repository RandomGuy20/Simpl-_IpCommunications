using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace IPCommunicationSuite.Telnet
{
    public class TelnetClientSimpl
    {
        #region Fields

        private TelnetClient client;

        #endregion
        
        #region Delegates

        public delegate void IncomingDataEventhandler(SimplSharpString data);
        public delegate void ConnectionStateEventHandler(ushort state, ushort connected);

        #endregion

        #region Events

        public IncomingDataEventhandler onIncomingData { get; set; }
        public ConnectionStateEventHandler onConnectionState { get; set; }

        #endregion

        #region Constructors

        public void Initialize(string IpAddress, string Password, string Username )
        {
            client = new TelnetClient(IpAddress, Password, Username);
            client.onIncomingData += new TelnetClient.IncomingDataEventHandler(client_onIncomingData);
            client.onStatusChange += new TelnetClient.StatusChangeEventHandler(client_onStatusChange);
        }



        #endregion

        #region Internal Methods

        void client_onStatusChange(Crestron.SimplSharp.CrestronSockets.SocketStatus status)
        {
            if (status == Crestron.SimplSharp.CrestronSockets.SocketStatus.SOCKET_STATUS_CONNECTED)
                onConnectionState((ushort)status, 1);
            else
                onConnectionState((ushort)status, 0);
        }

        void client_onIncomingData(string data)
        {
            SimplSharpString info;
            info = data;
            onIncomingData(info);
        }

        #endregion

        #region Public Methods

        public void SetDebug(ushort state)
        {
            client.Debug = Convert.ToBoolean(state);
            client.SetTCPDebug(Convert.ToBoolean(state));
        }

        public void Connect()
        {
            client.Connect();
        }

        public void Disconnect()
        {
            client.Disconnect();
        }

        public void SendData(SimplSharpString data)
        {
            string info = data.ToString();
            client.SendData(info);
        }

        #endregion
    }
}