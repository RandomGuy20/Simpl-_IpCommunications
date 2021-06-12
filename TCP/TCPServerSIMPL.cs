using System;
using System.Text;
using Crestron.SimplSharp; // For Basic SIMPL# Classes
using Crestron.SimplSharp.CrestronSockets;

namespace IPCommunicationSuite.TCP
{
    public class TCPServerSIMPL
    {
        #region Fields

        TCPIPServer server;


        #endregion

        #region Properties

        #endregion

        #region Delegates

        public delegate void ConnectionAmountEventhandler(ushort connections);
        public delegate void ConnectedEventHandler(ushort state);
        public delegate void ServerReceivedStringEventHandler(SimplSharpString data, ushort clientIndex);
        public delegate void ServerListeningStatusEventHandler(ushort listening);


        #endregion

        #region Events

        public ConnectionAmountEventhandler onConnectionAmount { get; set; }
        public ConnectedEventHandler onConnected { get; set; }
        public ServerReceivedStringEventHandler onRXData { get; set; }
        public ServerListeningStatusEventHandler onServerStateChange { get; set; }

        #endregion

        #region Constructors

        public void Initialize(ushort port, ushort maxConnections)
        {
            server = new TCPIPServer((int)port, (int)maxConnections);
            server.onServerRXData += new TCPIPServer.ServerReceivedDataEventHandler(server_onServerRXData);
            server.onServerStatus += new TCPIPServer.ServerSocketStatusEventHandler(server_onServerStatus);
            server.onServerConnection += new TCPIPServer.ServerConnectionAmountEventHandler(server_onServerConnection);
            server.onServerStateChange += new TCPIPServer.ServerStateEventHandler(server_onServerStateChange);
        }





        #endregion

        #region Internal Methods

        void server_onServerConnection(int clients)
        {

            if (clients <= 0)
                onConnectionAmount(0);
            else
                onConnectionAmount((ushort)clients);
        }

        void server_onServerStatus(SocketStatus status)
        {
            if (status == SocketStatus.SOCKET_STATUS_CONNECTED)
                onConnected(1);
            else
                onConnected(0);
        }

        void server_onServerRXData(string data, uint client)
        {
            SimplSharpString rxData = data;
            onRXData(rxData,(ushort)client);
        }

        void server_onServerStateChange(ServerState state)
        {
            switch (state)
            {
                case ServerState.SERVER_LISTENING:
                    onServerStateChange(1);
                    break;
                case ServerState.SERVER_NOT_LISTENING:
                    onServerStateChange(0);
                    break;
                default:
                    break;
            }
        }

        #endregion

        #region Public Methods

        public void Enable()
        {
            server.EnableServer();
        }

        public void Disable()
        {
            server.DisableServer();
        }

        public void DisconnectClient(ushort client)
        {
            server.DisconnectClient((int)client);
        }

        public void SendData(SimplSharpString data, ushort clientIndex)
        {
            string txData = data.ToString();
            server.SendData(clientIndex, txData);
        }

        public void Debug(ushort value)
        {
            server.Debug = Convert.ToBoolean(value);
        }

        #endregion
    }
}