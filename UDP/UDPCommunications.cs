using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronSockets;

namespace IPCommunicationSuite.UDP
{

    public class UDPCommunications
    {
        #region Fields

        private UDPServer server;
        private IPEndPoint endpoint;
        private CTimer statusChecker;

        private bool isInitialized;

        private int bufferSize;

        #endregion

        #region Properties

        public int PortNumber { get { return endpoint.Port; } }
        public int BufferSize { get { return bufferSize; } }

        public bool IsInitialized { get { return isInitialized; } }
        public bool Debug { get; set; }
        public bool IsEnabled { get; private set; }

        public string IpAddress { get { return endpoint.Address.ToString(); } }

        #endregion

        #region Delegates

        public delegate void ServerReceivedData(string data);
        public delegate void ServerEnabledEventHandler(bool state);

        #endregion

        #region Events

        public event ServerReceivedData onServerRxData;
        public event ServerEnabledEventHandler onServerEnabled;

        #endregion

        #region Constructors

        public UDPCommunications(string clientIP,int Port, int BufferSize)
        {
            try
            {
                bufferSize = BufferSize;

                endpoint = new IPEndPoint(IPAddress.Parse(clientIP), Port);
                server = new UDPServer(endpoint, Port, bufferSize);
                isInitialized = true;
                IsEnabled = false;

                statusChecker = new CTimer(CheckEnabledTimer, null, 0, 100);
            }
            catch (Exception err)
            {
                SendDebug("Error in UDp Constructor is: " + err.Message);
            }

        }

        #endregion

        #region Internal Methods

        private void SendDebug(string data)
        {
            if (Debug)
            {
                CrestronConsole.PrintLine("\nTCP Server Debug Message is: " + data);
                ErrorLog.Error("\nTCP Server Debug Message is: " + data);
            }
        }

        private void DataReceivedCallback(UDPServer server, int numberOfBytesReceived)
        {
            if (server.DataAvailable)
	        {
                String data = Encoding.ASCII.GetString(server.IncomingDataBuffer,0,numberOfBytesReceived);
                SendDebug("UDP RX Data From: " + server.IPAddressLastMessageReceivedFrom + " data is " + data);
                onServerRxData(data);
                SocketErrorCodes rxResult = server.ReceiveDataAsync(DataReceivedCallback);
                SendDebug("UDP Error Code RX Data Async is: " + rxResult.ToString());
	        }
            
        }

        private void DataSentCallback(UDPServer myUDPServer, int numberOfBytesSent)
        {
            SendDebug("UDP was able to send Data");
        }

        private void CheckEnabledTimer(object obj)
        {
            onServerEnabled(IsEnabled);
        }

        #endregion

        #region Public Methods

        public void EnableUDP()
        {
            try
            {
                SocketErrorCodes result = server.EnableUDPServer(endpoint.Address, endpoint.Port);
                SendDebug("UDP Error Code on Enabling Server is: " + result.ToString());
                if (result == SocketErrorCodes.SOCKET_OK)
                {
                    IsEnabled = true;
                    SocketErrorCodes rxResult = server.ReceiveDataAsync(DataReceivedCallback);
                    SendDebug("UDP Error Code RX Data Async is: " + rxResult.ToString());
                }
                   
            }
            catch (Exception ex)
            {
                SendDebug("Error Enabling UDP Server is: " + ex.Message);
            }

        }

        public void DisableUDP()
        {
            try
            {
                SocketErrorCodes result = server.DisableUDPServer();
                SendDebug("UDP Disabling Error is: " + result.ToString());
                if (result == SocketErrorCodes.SOCKET_OK)
                    IsEnabled = false;
            }
            catch (Exception ex)
            {
                SendDebug("Error disabling UDP is: " + ex.Message);
            }
        }

        public void SendData(string data)
        {
            try
            {
                byte[] bytes = Encoding.ASCII.GetBytes(data);
                SocketErrorCodes result = server.SendDataAsync(bytes, bytes.Length, endpoint, DataSentCallback);
                if (result != SocketErrorCodes.SOCKET_OK)
                    SendDebug("UDp Was not able to send data and error is: " + result.ToString());
 
            }
            catch (Exception ex)
            {
                SendDebug("UDp Error sending string is: " + ex.Message);
            }

        }
        #endregion
    }
}