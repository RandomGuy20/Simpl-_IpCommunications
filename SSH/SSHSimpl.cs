using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace IPCommunicationSuite.SSH
{
    public class SSHSimpl
    {
        #region Fields

        SSHClient client;
        SimplSharpString incomingData = "";
        
        #endregion


        #region Properties

        #endregion

        #region Delegates and Events

        public delegate void IncomingDataEventHandler(SimplSharpString data);
        public delegate void ConnectionStateEventHandler(ushort state);

        public IncomingDataEventHandler onIncomingData { get; set; }
        public ConnectionStateEventHandler onConnectionState { get; set; }
                
        #endregion
        

        #region Constructor

        public void Initialize(string ipAddress, string userName, string Password)
        {
            client = new SSHClient(ipAddress, userName, Password);
            client.onConnectionState += new SSHClient.ConnectionStateEventHandler(client_onConnectionState);
            client.onDataReceived += new SSHClient.DataReceivedEventHandler(client_onDataReceived);

        }
        
        #endregion


        #region Internal Methods

        void client_onDataReceived(string data)
        {
            incomingData = data;
            onIncomingData(incomingData);
        }

        void client_onConnectionState(bool connected)
        {
            onConnectionState(Convert.ToUInt16(connected));
        }


        #endregion


        #region Public Methods

        public void SetDebug(ushort debug)
        {
            client.Debug = Convert.ToBoolean(debug);
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
            client.SendCommand(Convert.ToString(data));
        }

        #endregion

    }
}