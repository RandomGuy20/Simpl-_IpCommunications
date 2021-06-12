using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;                          				// For Basic SIMPL# Classes
using Crestron.SimplSharp.CrestronSockets;

namespace IPCommunicationSuite.TCP
{
    public class TCPClientSIMPL
    {
        #region Fields

        TCPIPClient client;

        #endregion

        #region Properties

        #endregion

        #region Delegates

        public delegate void DataReceivedEventHandler(SimplSharpString data);
        public delegate void ConnectionStatusEventHandler(ushort state, ushort errorCode);

        #endregion

        #region Events

        public DataReceivedEventHandler onDataRX {get; set;}
        public ConnectionStatusEventHandler onConnection{ get;set;}


        #endregion

        #region Constructors

        public void Initialize(SimplSharpString ipAddress, ushort port)
        {
            client = new TCPIPClient(ipAddress.ToString(), (int)port, 5000);
            client.onIncomingData += new TCPIPClient.IncomingDataEventHandler(client_onIncomingData);
            client.onStatusChange += new TCPIPClient.StatusChangeEventHandler(client_onStatusChange);
        }



        #endregion

        #region Internal Methods

        void client_onStatusChange(SocketStatus status)
        {
            if (status == SocketStatus.SOCKET_STATUS_CONNECTED)
                onConnection(1, (ushort)status);
            else
                onConnection(0, (ushort)status);
        }

        void client_onIncomingData(string data)
        {
            SimplSharpString newData = data;
            onDataRX(newData);
        }

        #endregion

        #region Public Methods

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
            client.SendData(data.ToString());
        }

        public void Debug(ushort value)
        {
            client.Debug(Convert.ToBoolean(value));
        }


        #endregion
    }
}