using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace IPCommunicationSuite.WebSockets
{
    public class WebSocketSimpl
    {
        #region Fields

        private WebSocket socket;

        #endregion

        #region Properties

        #endregion

        #region Delegates

        public delegate void IncomingDataEventhandler(SimplSharpString data);
        public delegate void ConnectionStateEventHandler(ushort state);

        #endregion

        #region Events

        public IncomingDataEventhandler onIncomingData { get; set; }
        public ConnectionStateEventHandler onConnectionState { get; set; }

        #endregion

        #region Constructors

        public void Initialize(ushort keepAlive,ushort port, string ipAddress)
        {
            socket = new WebSocket(Convert.ToBoolean(keepAlive), port, (String)ipAddress);
            socket.onConnection += new WebSocket.WebSocketConnectionEventHandler(socket_onConnection);
            socket.onDataRx += new WebSocket.WebSocketRxDataEventhandler(socket_onDataRx);
        }



        #endregion

        #region Internal Methods

        void socket_onDataRx(string data)
        {
            SimplSharpString info;
            info = data;
            onIncomingData(info);
        }

        void socket_onConnection(bool state)
        {
            if (state)
                onConnectionState(1);
            else
                onConnectionState(0);
        }

        #endregion

        #region Public Methods

        public void SetDebug(ushort state)
        {
            socket.Debug = Convert.ToBoolean(state);
        }

        public void Connect()
        {
            socket.Connect();
        }

        public void Disconnect()
        {
            socket.Disconnect();
        }

        public void SendData(SimplSharpString data)
        {

            try
            {
                string info = data.ToString();
                CrestronConsole.PrintLine("Sending Data: " + info);
                socket.SendData(info);
            }
            catch (Exception ex)
            {
                CrestronConsole.PrintLine("WebSocket Error Sending String isL: " + ex);
            }

        }

        #endregion
    }
}