using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Networking;
using System;

namespace UnetLLAPISample {
    public class LLAPINetworkEventArgs : EventArgs
    {
        public NetworkEventType eventType { set; get; }
        public byte[] data { set; get; }
        public LLAPINetworkEventArgs(NetworkEventType t, byte[] d)
        {
            eventType = t;
            data = d;
        }
    }

    public class LLAPINetworkManager : MonoBehaviour
    {
        public bool isServer = true;
        public string serverAddress = "127.0.0.1";
        public int serverPort = 8888;
        public int localPort = 8889;
        public int maxConnection = 10;
        public bool verpose = false;
        public readonly int MaxBufferSize = 65535;

        public delegate void NetworkEventHandler(object sender, LLAPINetworkEventArgs e);
        public event NetworkEventHandler OnConnected = delegate (object s, LLAPINetworkEventArgs e) { };
        public event NetworkEventHandler OnDisconnected = delegate (object s, LLAPINetworkEventArgs e) { };
        public event NetworkEventHandler OnDataReceived = delegate (object s, LLAPINetworkEventArgs e) { };
        public event NetworkEventHandler DataReceived = delegate (object s, LLAPINetworkEventArgs e) { };

        private bool connected = false;
        private int connectionId;

        private Dictionary<QosType, int> channelIdDictionary = new Dictionary<QosType, int>();
        private int hostId;

        private void Awake()
        {
            
        }

        void Start()
        {
            NetworkTransport.Init();
            ConnectionConfig config = new ConnectionConfig();
            foreach (QosType qosType in Enum.GetValues(typeof(QosType)))
            {
                var channeld = config.AddChannel(qosType);
                channelIdDictionary.Add(qosType, channeld);
            }

            HostTopology topology = new HostTopology(config, maxConnection);

            hostId = NetworkTransport.AddHost(topology, localPort);
        }

        void Update()
        {
            int recHostId;
            int connectionId;
            int channelId;
            byte[] recBuffer = new byte[MaxBufferSize];
            int bufferSize = MaxBufferSize;
            int dataSize;
            byte error;
            NetworkEventType recData = NetworkTransport.Receive(out recHostId, out connectionId, out channelId, recBuffer, bufferSize, out dataSize, out error);
            LLAPINetworkEventArgs e;
            switch (recData)
            {
                case NetworkEventType.Nothing:
                    if (!isServer && !connected)
                    {
                        Connect();
                        connected = true;
                    }
                    break;
                case NetworkEventType.ConnectEvent:
                    if (verpose)
                    {
                        Debug.Log("Receive Connect Event");
                    }
                    this.connectionId = connectionId;
                    connected = true;
                    e = new LLAPINetworkEventArgs(recData, null);
                    OnConnected(this, e);
                    break;
                case NetworkEventType.DataEvent:
                    if (verpose)
                    {
                        Debug.Log("Receive Data Event");
                    }
                    e = new LLAPINetworkEventArgs(recData, recBuffer);
                    OnDataReceived(this, e);
                    break;
                case NetworkEventType.DisconnectEvent:
                    if (verpose)
                    {
                        Debug.Log("Receive Disconnect Event");
                    }
                    connected = false;
                    e = new LLAPINetworkEventArgs(recData, null);
                    OnDisconnected(this, e);
                    break;
            }
        }

        void Connect()
        {
            byte error;
            connectionId = NetworkTransport.Connect(hostId, serverAddress, serverPort, 0, out error);
            if (verpose)
            {
                Debug.Log("Connected to server. ConnectionId: " + connectionId);
                LogNetworkError(error);
            }
        }

        public void SendPacketData(byte[] data, QosType qos = QosType.Reliable)
        {
            if (!channelIdDictionary.ContainsKey(qos))
            {
                if (verpose)
                {
                    Debug.Log("Network manager is not initialized");
                }
                return;
            }
            var channelId = channelIdDictionary[qos];
            byte error;
            NetworkTransport.Send(hostId, connectionId, channelId, data, data.Length, out error);
            Debug.Log("hoge");
            if (verpose)
            {
                LogNetworkError(error);
            }
        }

        void LogNetworkError(byte error)
        {
            if (error != (byte)NetworkError.Ok)
            {
                NetworkError nerror = (NetworkError)error;
                Debug.Log("Error: " + nerror.ToString());
            }
        }
    }
}