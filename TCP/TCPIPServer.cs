using System;
using System.Text;
using Crestron.SimplSharp; // For Basic SIMPL# Classes
using Crestron.SimplSharp.CrestronSockets;

namespace IPCommunicationSuite.TCP
{
    public class TCPIPServer
    {
        #region Fields

        TCPServer server;
        CTimer stateTimer;
        ServerState currState;

        private int maxConnections;
        private int serverPort;

        private string lastSent;


        #endregion

        #region Properties

        public bool Debug { get; set; }

        public int CurrentConnections { get { return server.NumberOfClientsConnected; } }
        public int CurrentPort { get { return server.PortNumber; } }
        public int MaxConnectionsAllowed { get { return server.MaxNumberOfClientSupported; } }

        #endregion

        #region Delegates

        public delegate void ServerSocketStatusEventHandler(SocketStatus status);
        public delegate void ServerReceivedDataEventHandler(string data, uint client);
        public delegate void ServerConnectionAmountEventHandler(int clients);
        public delegate void ServerStateEventHandler(ServerState state);


        #endregion

        #region Events

        public event ServerSocketStatusEventHandler onServerStatus;
        public event ServerReceivedDataEventHandler onServerRXData;
        public event ServerConnectionAmountEventHandler onServerConnection;
        public event ServerStateEventHandler onServerStateChange;

        #endregion

        #region Constructors

        public TCPIPServer(int Port, int MaxConnections)
        {
            serverPort = Port;
            maxConnections = MaxConnections;
            lastSent = "";
            stateTimer = new CTimer(onStateCheckCallback,0,0,2000);

            server = new TCPServer(serverPort, maxConnections);
            server.SocketStatusChange += new TCPServerSocketStatusChangeEventHandler(server_SocketStatusChange);
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

        private void onTCPServerCallback(TCPServer myServer, uint clientIndex)
        {
            onServerConnection(myServer.NumberOfClientsConnected);
            if (clientIndex != 0)
            {
                SendDebug("Server listening on port: " + server.PortNumber + " connected with client# " + clientIndex);
                server.ReceiveDataAsync(clientIndex, onServerReceiveDataCallback);
                if (server.MaxNumberOfClientSupported == server.NumberOfClientsConnected)
                {
                    SendDebug("Server Client Limit Reached");
                    server.Stop();
                }

                //SocketErrorCodes result = server.WaitForConnectionAsync("0.0.0.0", onTCPServerCallback);
                SocketErrorCodes result = server.WaitForConnection("0.0.0.0", onTCPServerCallback);
                SendDebug("Wait for Connection returned result: " + result.ToString());
            }
            else
            {
                SendDebug("Error in Server Connection Callback");
                if (server.State == ServerState.SERVER_NOT_LISTENING)
                {
                    SendDebug("Server Not Listening, calling Enable Method...");
                    EnableServer();
                }
                else
                {
                    //server.WaitForConnectionAsync("0.0.0.0", onTCPServerCallback);
                    server.WaitForConnection("0.0.0.0", onTCPServerCallback);
                }
            }


        }

        private void onServerReceiveDataCallback(TCPServer tcpserver, uint clientIndex, int bytesReceived)
        {
            try
            {
                if (bytesReceived <= 0)
                {
                    SendDebug("ClientIndex: " + clientIndex + "has issues disconnecting.....");
                    server.Disconnect(clientIndex);

                    server.ReceiveDataAsync(clientIndex, onServerReceiveDataCallback);
                }
                else
                {
                    SendDebug("Server Received data from: " + clientIndex);
                    byte[] bytes = tcpserver.GetIncomingDataBufferForSpecificClient(clientIndex);
                    string rx = Encoding.ASCII.GetString(bytes, 0, bytesReceived);
                    onServerRXData(rx,clientIndex);

                    server.ReceiveDataAsync(clientIndex, onServerReceiveDataCallback);
                }
            }
            catch (Exception err)
            {
                SendDebug("Server Error Receiving data is: " + err.Message);
            }
        }

        private void onServerSendDataCallback(TCPServer myTCPServer, uint clientIndex, int numberOfBytesSent)
        {
            SendDebug("Sent: " + lastSent + " - To " + clientIndex);
        }

        void server_SocketStatusChange(TCPServer myTCPServer, uint clientIndex, SocketStatus status)
        {
            if (status == SocketStatus.SOCKET_STATUS_CONNECTED)
                SendDebug("Server: Client " + clientIndex + " connected");
            else
                SendDebug("Server: Client " + clientIndex + " disconnected");

            onServerStatus(status);
        }

        private void onStateCheckCallback(object obj)
        {
            if (server.State == ServerState.SERVER_LISTENING || server.State == ServerState.SERVER_NOT_LISTENING)
            {
                onServerStateChange(server.State);
            }
            
        }

        #endregion

        #region Public Methods

        public void EnableServer()
        {
            try
            {
                if (server.State != ServerState.SERVER_CONNECTED)
                {
                    //SocketErrorCodes result = server.WaitForConnectionAsync("0.0.0.0", onTCPServerCallback);
                    SocketErrorCodes result = server.WaitForConnection("0.0.0.0", onTCPServerCallback);
                    SendDebug("Wait for Connection returned result: " + result.ToString());
                }
                
            }
            catch (Exception err)
            {
                SendDebug("Error Enabling Server is: " + err.Message);
            }
        }

        public void DisableServer()
        {
            try
            {
                server.DisconnectAll();
                server.Stop();
             }
            catch (Exception err)
            {
                SendDebug("Error Disabling Server is: " + err.Message);
            }
        }

        public void DisconnectClient(int index)
        {
            try
            {
                if (server.State == ServerState.SERVER_CONNECTED)
                    server.Disconnect((uint)index);

            }
            catch (Exception err)
            {
                SendDebug("Error Disabling Server is: " + err.Message);
            }
        }

        public void SendData(uint clientIndex, string data)
        {
            if (server.ClientConnected(clientIndex))
            {
                byte[] bytes = Encoding.ASCII.GetBytes(data.ToString());
                server.SendDataAsync(clientIndex, bytes, bytes.Length, onServerSendDataCallback);
            }


        }

        #endregion
    }
}