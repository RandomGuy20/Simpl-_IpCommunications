using System;
using System.Text;
using System.Text.RegularExpressions;
using Crestron.SimplSharp;
using Crestron.SimplSharp.Ssh;
using Crestron.SimplSharp.Ssh.Common;
using System.Collections.Generic;

namespace IPCommunicationSuite.SSH
{
    public class SSHClient
    {
        
        #region Fields

        private SshClient client;
        private ShellStream stream;
        private KeyboardInteractiveAuthenticationMethod authMethod;
        private ConnectionInfo connectInfo;

        private CTimer keepAlive;
        private CTimer connState;

        private bool isInitialized;

        private string username;
        private string ipAddress;
        private string password;

        #endregion

        #region Properties

        /// <summary>
        /// Returns if Default Constructor has been called
        /// </summary>
        public bool IsInitialized { get {return isInitialized;} }
        /// <summary>
        /// Returns Connection State of SSH
        /// </summary>
        public bool IsConnected { get { return client.IsConnected; } }
        /// <summary>
        /// Get and Set Debug State 
        /// </summary>
        public bool Debug { get; set; }

        /// <summary>
        /// Returns username
        /// </summary>
        public string UserName { get { return username; } }
        /// <summary>
        /// Returns IP Address
        /// </summary>
        public string IPAddress { get { return ipAddress; } }
        /// <summary>
        /// Returns Password
        /// </summary>
        public string Password { get { return password; } }

        
        #endregion


        #region Delegates

        public delegate void ConnectionStateEventHandler(bool connected);
        public delegate void DataReceivedEventHandler(string data);

        #endregion


        #region Events

        public event ConnectionStateEventHandler onConnectionState;
        public event DataReceivedEventHandler onDataReceived;
        
        #endregion

        #region Constructors

        public SSHClient(string IpAddress, string user, string pass)
        {
            ipAddress = IpAddress;
            username = user;
            password = pass;

            SendDebug("IP Address:" + ipAddress + "  Username:" + username + " Pass:" + password);
            
            authMethod = new KeyboardInteractiveAuthenticationMethod(username);
            authMethod.AuthenticationPrompt += new EventHandler<AuthenticationPromptEventArgs>(auth_AuthenticationPrompt);

            connectInfo = new ConnectionInfo(ipAddress, username, authMethod);


            client = new SshClient(connectInfo);
            client.ErrorOccurred += new EventHandler<ExceptionEventArgs>(client_ErrorOccurred);
            client.HostKeyReceived += new EventHandler<HostKeyEventArgs>(client_HostKeyReceived);





            isInitialized = true;
        }
        
        #endregion


        #region Internal Methods

        private void SendDebug(string message)
        {
            if (Debug)
            {
                CrestronConsole.PrintLine("SSH Debug message is: " + message);
                ErrorLog.Error("SSH Debug message is: " + message);
            }
        }

        private void KeepSSHAlive(object obj)
        {
            client.SendKeepAlive();
        }

        private void ConnectionState(object obj)
        {
            onConnectionState(client.IsConnected);
            SendDebug("Client Connection State is: " + client.IsConnected);
            if (!client.IsConnected)
            {
                Connect(); 
            }
        }

        void auth_AuthenticationPrompt(object sender, AuthenticationPromptEventArgs e)
        {
            SendDebug("Sending Password");

            foreach (AuthenticationPrompt item in e.Prompts)
            {
                item.Response = password;
            }
        }

        void stream_ErrorOccurred(object sender, ExceptionEventArgs e)
        {
            SendDebug("SSH Shellstream error is: " + e.ToString());
            Disconnect();
        }

        void stream_DataReceived(object sender, ShellDataEventArgs e)
        {
            var stream = (ShellStream)sender;
            string data = "";

            while (stream.DataAvailable)
                data = stream.Read();

            if (data != "" && data != null)
                onDataReceived(data);
        }

        void client_HostKeyReceived(object sender, HostKeyEventArgs e)
        {
            SendDebug("Host Key Received");
            e.CanTrust = true;
        }

        void client_ErrorOccurred(object sender, ExceptionEventArgs e)
        {
            SendDebug("SSh Client error is: " + e.Exception);
            Disconnect();
        }

        #endregion



        #region Public Methods

        public void Connect()
        {
            if (!client.IsConnected)
            {
                try
                {
                    try
                    {
                        SendDebug("Attempting to Connect IP Address:" + ipAddress + "  Username:" + username + " Pass:" + password);
                        client.Connect();
                    }
                    catch (SshConnectionException e)
                    {
                        SendDebug("SSH Connection error is:" + e.Message + " and Reason:" + e.DisconnectReason);
                        Disconnect();
                        return;
                    }

                    stream = client.CreateShellStream("terminal", 80, 24, 800, 600, 65534);
                    stream.DataReceived += new EventHandler<ShellDataEventArgs>(stream_DataReceived);
                    stream.ErrorOccurred += new EventHandler<ExceptionEventArgs>(stream_ErrorOccurred);

                    if (keepAlive != null)
                    {
                        keepAlive.Stop();
                        keepAlive.Dispose();
                    }
                    if (connState != null)
                    {
                        connState.Stop();
                        connState.Dispose();
                    }

                    connState = new CTimer(ConnectionState, null, 1000, 1000);

                    keepAlive = new CTimer(KeepSSHAlive, null, 1000, 1000);
                }
                catch (Exception e)
                {
                    SendDebug("SSH Error Connecting is " + e.Message);
                }
            }
        }
        
        public void Disconnect()
        {
            try
            {
                if (stream != null)
                    stream.Dispose();
            }
            catch (Exception e)
            {
                SendDebug("Error when Disconnect Stream is: " + e.Message);
            }

            try
            {
                if (client != null && client.IsConnected)
                {
                    client.Disconnect();
                    client.Dispose();
                    SendDebug("SSH is Disconnected");
                }
            }
            catch (Exception e)
            {
                SendDebug("Error when Disconnect SSH Client is: " + e.Message);
            }

            try
            {
                if (keepAlive != null)
                {
                    keepAlive.Stop();
                    keepAlive.Dispose();
                }
                if (connState != null)
                {
                    connState.Stop();
                    connState.Dispose();
                }
            }
            catch (Exception)
            {
                
                throw;
            }
        }

        public void SendCommand(string data)
        {
            if (stream.CanWrite)
                stream.WriteLine(data);
            SendDebug("Command being send is: " + data);
        }
        
        #endregion
    }
}