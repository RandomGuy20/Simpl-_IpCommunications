using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronSockets;
using System.Text.RegularExpressions;

namespace IPCommunicationSuite.UDP
{

    public class WakeOnLan
    {
        #region Fields

        private UDPServer server;
        private SocketErrorCodes errorCodes;
        EthernetAdapterType adapter;

        private byte[] wolPacket = new byte[126];
        private byte[] macAddress = new byte[6];

        private string ipAddress;

        private int portNumber;


        #endregion

        #region Properties

        public bool Debug { get; set; }

        /// <summary>
        /// If the MAc is not formatted with : or - it will be ignored
        /// </summary>
        public string MacAddress 
        {
            get
            {
                string mac = "";
                foreach (var item in macAddress)
                {
                    mac += (char)item;
                }
                return mac;
            }
            set
            {
                if(MacAddress.Contains(":") || MacAddress.Contains("-"))
                {
                    macAddress = BuildMac(MacAddress);
                }
                
            }
        }

        public string IpAddress { get { return ipAddress; } set { ipAddress = value; } }

        public int PortNumber { get { return portNumber; } set { portNumber = value; } }

        public EthernetAdapterType EthernetAdapter { get { return adapter; } set { adapter = value; } }

        #endregion

        #region Delegates

        public delegate void PacketSentEventhandler(bool sent);

        #endregion

        #region Events

        public event PacketSentEventhandler onPacketSent;

        #endregion

        #region Constructors

        public WakeOnLan(string MacAddress, string IpAddress, int PortNumber, EthernetAdapterType EthernetAdapter)
        {
            server = new UDPServer();
            portNumber = PortNumber;
            ipAddress = IpAddress;
            adapter = EthernetAdapter;

            macAddress = BuildMac(MacAddress);
        }

        #endregion

        #region Internal Methods

        private byte[] BuildMac(string MacAddress)
        {
            var tempMac = Regex.Replace(MacAddress, "[-|:]", "");
            byte[] bytes = new byte[6];

            for (int i = 0; i < 6; i++)
            {
                bytes[i] = Convert.ToByte(tempMac.Substring(i * 2, 2), 16);
            }

            return bytes;
        }

        private void SendDebug(string data)
        {
            if (Debug)
            {
                CrestronConsole.PrintLine("\nWake On LAN Debug Message is: " + data);
                ErrorLog.Error("\nWake On LAN Debug Message is: " + data);
            }
        }

        #endregion

        #region Public Methods

        public void SendPacket()
        {

            try
            {
                #region Build the Packet

                try
                {
                    for (int i = 0; i < 6; i++)
                    {
                        wolPacket[i] = 0xFF;
                    }
                    for (int i = 1; i <= 16; i++)
                    {
                        for (int j = 0; j < 6; j++)
                        {
                            wolPacket[i * 6 + j] = macAddress[j];
                        }
                    }
                    SendDebug("Packet is: " + wolPacket.ToString());
                }
                catch (Exception ex)
                {
                    SendDebug("Error building Magic Packet in the Send packet method is: " + ex.Message);
                    return;
                }

                #endregion

                #region Set Server and Biond adapter
                errorCodes = server.EnableUDPServer("0.0.0.0", 0, portNumber);
                SendDebug("Enabling UDP Server for WOL error code is: " + errorCodes.ToString());

                server.EthernetAdapterToBindTo = adapter;
                SendDebug("UDP Datagram is bound to: " + adapter.ToString());

                #endregion

                #region Send the command

                errorCodes = server.SendData(wolPacket, wolPacket.Length, ipAddress, portNumber, false);

                if (errorCodes == SocketErrorCodes.SOCKET_OK)
                {
                    SendDebug("Send packet to IP: " + ipAddress + " and Port: " + portNumber + " and length of packet is " + wolPacket.Length);
                    onPacketSent(true);
                }
                else
                {
                    SendDebug("Error sending Magic packet is: " + errorCodes.ToString());
                    onPacketSent(false);
                }
                    

                errorCodes = server.DisableUDPServer();
                SendDebug("Status on disabling server after sending packet is: " + errorCodes.ToString());

                #endregion
            }
            catch (Exception err)
            {
                SendDebug("Error in the Send packet Method is: " + err.Message);
            }



        }

        #endregion
    }
}