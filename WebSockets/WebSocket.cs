using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronWebSocketClient;
using Crestron.SimplSharp.Cryptography.X509Certificates;

namespace IPCommunicationSuite.WebSockets
{

    public class WebSocket
    {
        #region Fields

        private WebSocketClient client;
        private WebSocketClient.WEBSOCKET_RESULT_CODES result;

        #endregion

        #region Properties

        public bool IsConnected { get { return client.Connected; } }
        public bool Debug { get; set; }
        public bool KeepAlive { get{ return client.KeepAlive;}set {client.KeepAlive = value;} }

        public EthernetAdapterType EthernetAdapter 
        {
            get { return client.EthernetAdapter; }     
        }

        #endregion

        #region Delegates

        public delegate void WebSocketConnectionEventHandler(bool state);
        public delegate void WebSocketRxDataEventhandler(string data);

        #endregion

        #region Events

        public event WebSocketConnectionEventHandler onConnection;
        public event WebSocketRxDataEventhandler onDataRx;

        #endregion
  
        #region Constructors

        /// <summary>
        /// Unless a Specific Port is need send 0
        /// </summary>
        /// <param name="keepAlive"></param>
        /// <param name="port"></param>
        /// <param name="pr"></param>
        public WebSocket(bool keepAlive,uint port,string url)
        {

            try
            {

                client = new WebSocketClient();

                client.KeepAlive = keepAlive;
                client.ConnectionCallBack = WebSocketConnectedCallback;
                client.DisconnectCallBack = WebSocketDisconnectCallback;
                client.SendCallBack = WebSocketSendCallback;
                client.ReceiveCallBack = WebSocketReceiveCallback;
                client.URL = url;


                client.Port = port > 0 ? port : (uint?)null;
            }
            catch (Exception ex)
            {
                SendDebug("Error in Web Socket Contructor is: " + ex.Message);
            }


        }

        #endregion

        #region Internal Methods

        private void SendDebug(string message)
        {
            if (Debug)
            {
                CrestronConsole.PrintLine("WebSocket Debug message is: " + message);
                ErrorLog.Error("WebSocket Debug message is: " + message);
            }
        }

        private int WebSocketConnectedCallback(WebSocketClient.WEBSOCKET_RESULT_CODES error)
        {
            try
            {
                SendDebug("Connection on Connected Callback is: " + error.ToString());
                onConnection(client.Connected);
                return 0;
            }
            catch (Exception ex)
            {
                SendDebug("Error in Websocket Connected Callback is: " + ex.Message);
                return 0;
            }
            
        }

        private int WebSocketDisconnectCallback(WebSocketClient.WEBSOCKET_RESULT_CODES error, object obj)
        {
            try
            {
                SendDebug("Connection on Disconnect Callback is: " + error.ToString());
                onConnection(client.Connected);
                return 0;
            }
            catch (Exception ex)
            {
                SendDebug("Error in Websocket Disconnect Callback is: " + ex.Message);
                return 0;
            }
        }

        private int WebSocketReceiveCallback(byte[] bytes, uint length, WebSocketClient.WEBSOCKET_PACKET_TYPES opcode, WebSocketClient.WEBSOCKET_RESULT_CODES error)
        {
            try
            {
                SendDebug("Connection on Receive Callback is: " + error.ToString());

                var data = Encoding.Default.GetString(bytes, 0, bytes.Length);

                SendDebug("The Received String is: " + data);
                onDataRx(data);
                client.ReceiveAsync();
                return 0;
            }
            catch (Exception ex)
            {
                SendDebug("Error in Websocket Receive Callback is: " + ex.Message);
                client.ReceiveAsync();
                return 0;
            }
        }

        private int WebSocketSendCallback(WebSocketClient.WEBSOCKET_RESULT_CODES error)
        {
            try
            {
                SendDebug("Connection on Send Callback is: " + error.ToString());
                client.ReceiveAsync();
                return 0;
            }
            catch (Exception ex)
            {
                SendDebug("Error is WebSocket Send Callback is: " + ex.Message);
                client.ReceiveAsync();
                return 0;
            }

        }

        #endregion

        #region Public Methods

        public void Connect()
        {
            try
            {
                SendDebug("Attempting to connect to: " + client.URL);
                result = client.ConnectAsync();
                SendDebug("Client Attempting to Connect and error code is: " + result.ToString());
                if (result == WebSocketClient.WEBSOCKET_RESULT_CODES.WEBSOCKET_CLIENT_SUCCESS)
                    client.ReceiveAsync();
            }
            catch (Exception ex)
            {
                SendDebug("Error in connect method is: " + ex.Message);
            }

         }

        public void Disconnect()
        {
            try
            {
                result = client.DisconnectAsync(null);
                SendDebug("Error code when Disconnecting is: " + result.ToString());

            }
            catch (Exception ex)
            {
                SendDebug("Error in Disconnect is: " + ex.Message);
            }
        }

        public void SendData(string data)
        {
            try
            {
                byte[] bytes = Encoding.ASCII.GetBytes(data);
                result = client.SendAsync(bytes, (uint)bytes.Length, WebSocketClient.WEBSOCKET_PACKET_TYPES.LWS_WS_OPCODE_07__TEXT_FRAME, WebSocketClient.WEBSOCKET_PACKET_SEGMENT_CONTROL.WEBSOCKET_CLIENT_PACKET_END);
                SendDebug("Result when sending Async is: " + result.ToString());
            }
            catch (Exception ex)
            {
                SendDebug("Error when trying to send data is: " + ex.Message);
            }

        }

        #endregion
    }
}