using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace IPCommunicationSuite.UDP
{
    public class UDPCommSimpl
    {
        #region Fields

        private UDPCommunications udp;

        #endregion

        #region Delegates

        public delegate void UDPServerEnabledEventHandler(ushort state);
        public delegate void UDPServerRXData(SimplSharpString data);

        #endregion

        #region Events

        public UDPServerEnabledEventHandler onUdpServer { get; set; }
        public UDPServerRXData onUdpRxData { get; set; }

        #endregion

        #region Constructors

        public void Initialize(string ipAddress,ushort port)
        {
            udp = new UDPCommunications(ipAddress, (int)port, 1024);
            udp.onServerEnabled += new UDPCommunications.ServerEnabledEventHandler(udp_onServerEnabled);
            udp.onServerRxData += new UDPCommunications.ServerReceivedData(udp_onServerRxData);
        }

        #endregion

        #region Internal Methods

        void udp_onServerRxData(string data)
        {
            SimplSharpString info;
            info = data;
            onUdpRxData(info);
        }

        void udp_onServerEnabled(bool state)
        {
            onUdpServer(Convert.ToUInt16(state));
        }

        #endregion

        #region Public Methods

        public void Enable()
        {
            udp.EnableUDP();
        }

        public void Disable()
        {
            udp.DisableUDP();
        }

        public void SetDebug(ushort state)
        {
            udp.Debug = Convert.ToBoolean(state);
        }

        public void SendData(SimplSharpString data)
        {
            string info = data.ToString();
            udp.SendData(info);
        }

        #endregion
    }
}