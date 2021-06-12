using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;                          				// For Basic SIMPL# Classes
using Crestron.SimplSharp.CrestronSockets;

namespace IPCommunicationSuite.TCP
{
    public class TCPIPClient
    {
        #region Fields

        private TCPClient client;
        private CTimer statusCheck;
        private SocketErrorCodes errorCode;

        private string ipAddress;
        private string lastSent;

        private int port;
        private int bufferSize;

        private bool isConnected;
        private bool isDebug;

        #endregion

        #region Properties

        public string IpAddress { get { return client.AddressClientConnectedTo; } }
        public bool IsConnected { get { return isConnected; } }
 

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

        public TCPIPClient(string IpAddress, int Port, int BufferSize)
        {
            ipAddress = IpAddress;
            port = Port;
            bufferSize = BufferSize;

            client = new TCPClient(IpAddress, port, bufferSize);
            client.SocketStatusChange += new TCPClientSocketStatusChangeEventHandler(client_SocketStatusChange);
        }
        
        #endregion

        #region Internal Methods

        void client_SocketStatusChange(TCPClient myTCPClient, SocketStatus clientSocketStatus)
        {
            SendDebug("The Sockets Status is: " + clientSocketStatus.ToString());
            onStatusChange(clientSocketStatus);
            if (clientSocketStatus == SocketStatus.SOCKET_STATUS_CONNECTED)
            {
                isConnected = true;
                client.ReceiveDataAsync(onDataReceivedCallback);

                if (!statusCheck.Disposed)
                {
                    statusCheck.Stop();
                    statusCheck.Dispose();
                }

                statusCheck = new CTimer(StatusCheckMethod, null, 0, 5000);
            }
            else
            {
                isConnected = false;
            }
        }

        void onConnectionCallback(TCPClient tcpClient)
        {
            onStatusChange(tcpClient.ClientStatus);
            SendDebug("The Sockets Connection is: " + client.ClientStatus.ToString());
            if (tcpClient.ClientStatus == SocketStatus.SOCKET_STATUS_CONNECTED)
                client.ReceiveDataAsync(onDataReceivedCallback);
        }

        void OnSendDataCallback(TCPClient tcpClient, int bytes)
        {
            SendDebug("TCP Data Sent was: " + lastSent);
        }

        void onDataReceivedCallback(TCPClient tcpClient, int bytes)
        {
            try
            {
                if (bytes > 0)
                {
                    var data = Encoding.Default.GetString(client.IncomingDataBuffer, 0, bytes);
                    onIncomingData(data);
                    client.ReceiveDataAsync(onDataReceivedCallback);
                }
            }
            catch (Exception e)
            {
                SendDebug("TCP Client error reading incoming data is: " + e.Message);
            }
        }

        private void StatusCheckMethod(object obj)
        {
            if (client.ClientStatus != SocketStatus.SOCKET_STATUS_CONNECTED)
                Connect();
        }

        private void SendDebug(string data)
        {
            if (isDebug)
            {
                CrestronConsole.PrintLine("\nTCP Client Debug Message is: " + data);
                ErrorLog.Error("\nTCP Client Debug Message is: " + data);
            }

        }


        #endregion

        #region Public Methods

        public void Debug(bool state)
        {
            isDebug = state;
        }

        public void Connect()
        {
            if (client.ClientStatus != SocketStatus.SOCKET_STATUS_CONNECTED)
            {
                client.ConnectToServerAsync(onConnectionCallback);
            }
            
        }

        public void Disconnect()
        {
            client.DisconnectFromServer();
        }

        public void SendData(string data)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(data.ToString());
            lastSent = data;
            errorCode = client.SendDataAsync(bytes, bytes.Length, OnSendDataCallback);
        }

        #endregion
    }
}