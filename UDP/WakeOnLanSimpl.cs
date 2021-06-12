using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace IPCommunicationSuite.UDP
{
    public class WakeOnLanSimpl
    {
        #region Fields

        WakeOnLan wol;

        #endregion

        #region Properties

        #endregion

        #region Delegates

        public delegate void PacketSendEventhandler(ushort sent);

        #endregion

        #region Events

        public PacketSendEventhandler onPacketSent { get; set; }

        #endregion

        #region Constructors

        public void Initialize(string mac, string ipAddress, ushort port, ushort ethernetAdapter )
        {
            wol = new WakeOnLan(mac, ipAddress,(int)port, (EthernetAdapterType)ethernetAdapter);
            wol.onPacketSent += new WakeOnLan.PacketSentEventhandler(wol_onPacketSent);
        }

        void wol_onPacketSent(bool sent)
        {
            onPacketSent(Convert.ToUInt16(sent));
        }

        #endregion

        #region Internal Methods

        #endregion

        #region Public Methods

        public void SetDebug(ushort state)
        {
            wol.Debug = Convert.ToBoolean(state);
        }

        public void Wake()
        {
            if (wol != null)
            {
                wol.SendPacket();
            }
        }

        #endregion
    }
}