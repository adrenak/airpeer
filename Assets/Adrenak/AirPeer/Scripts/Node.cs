using Byn.Net;
using System.Text;
using UnityEngine;
using System;
using System.Collections.Generic;

namespace Adrenak.AirPeer {
    public class Node : MonoBehaviour {
        public enum NodeMode {
            Inactive,
            Server,
            Client
        }
        string k_SignallingServer = "wss://because-why-not.com:12777/chatapp";
        string k_ICEServer1 = "stun:because-why-not.com:12779";
        string k_ICEServer2 = "stun:stun.l.google.com:19302";

        public event Action OnServerStart;
        public event Action OnServerStop;
        public event Action OnServerFail;

        public event Action<ConnectionId> OnConnection;
        public event Action<ConnectionId> OnDisconnection;
        public event Action<ConnectionId> OnConnectionFail;

        public event Action<NetworkEvent> OnNetworkEvent;
        public event Action<NetworkEvent> OnGetReliableMessage;
        public event Action<NetworkEvent> OnGetUnreliableMessage;

        IBasicNetwork m_Network;
        public List<ConnectionId> ConnectionIds { get; private set; }
        public NodeMode Mode { get; private set; }

        public static Node Create(string name = "Node (Instance)") {
            var go = new GameObject(name);
            DontDestroyOnLoad(go);
            return go.AddComponent<Node>();
        }

        public bool Init() {
            if(ConnectionIds != null) 
                ConnectionIds.Clear();

            if (m_Network != null) 
                m_Network.Dispose();

            ConnectionIds = new List<ConnectionId>();
            m_Network = WebRtcNetworkFactory.Instance.CreateDefault(
                k_SignallingServer,
                new[] {
                    new IceServer(k_ICEServer1),
                    new IceServer(k_ICEServer2)
                }
            );
            return m_Network != null;
        }

        public void Deinit() {
            ConnectionIds.Clear();
            ConnectionIds = null;
            m_Network.Dispose();
            m_Network = null;
        }

        void Update() {
            if (m_Network != null) {
                m_Network.Update();
                ReadNetworkEvents();
            }
            if (m_Network != null)
                m_Network.Flush();
        }

        void OnDestroy() {
            Deinit();
        }

        public void StartServer(string roomName) {
            m_Network.StartServer(roomName);
        }

        public void StopServer() {
            m_Network.StopServer();
        }

        public void Connect(string roomName) {
            m_Network.Connect(roomName);
        }

        public void Disconnect() {
            m_Network.Disconnect(new ConnectionId(1));
        }

        public bool SendString(string msg, bool reliable = false, int offset = 0) {
            if (string.IsNullOrEmpty(msg)) return false;

            byte[] bytes = Encoding.UTF8.GetBytes(msg);
            return SendData(bytes, reliable, offset);
        }

        public bool SendData(byte[] data, bool reliable = false, int offset = 0) {
            if (m_Network == null || ConnectionIds == null || ConnectionIds.Count == 0) return false;

            foreach (var id in ConnectionIds)
                m_Network.SendData(id, data, offset, data.Length, reliable);
            return true;
        }

        void ReadNetworkEvents() {
            NetworkEvent netEvent;
            while (m_Network != null && m_Network.Dequeue(out netEvent))
                ProcessNetworkEvent(netEvent);
        }

        void ProcessNetworkEvent(NetworkEvent netEvent) {
            OnNetworkEvent.TryInvoke(netEvent);
            switch (netEvent.Type) {
                case NetEventType.ServerInitialized:
                    Mode = NodeMode.Server;
                    OnServerStart.TryInvoke();
                    break;

                case NetEventType.ServerInitFailed:
                    Mode = NodeMode.Inactive;
                    OnServerStop.TryInvoke();
                    break;

                case NetEventType.ServerClosed:
                    Mode = NodeMode.Inactive;
                    OnServerFail.TryInvoke();
                    break;

                case NetEventType.NewConnection:
                    if (Mode != NodeMode.Server) Mode = NodeMode.Client;

                    ConnectionIds.Add(netEvent.ConnectionId);
                    OnConnection.TryInvoke(netEvent.ConnectionId);
                    break;

                case NetEventType.ConnectionFailed:
                    if (Mode != NodeMode.Server)
                        Mode = NodeMode.Inactive;

                    ConnectionIds.Add(netEvent.ConnectionId);
                    OnConnection.TryInvoke(netEvent.ConnectionId);
                    break;

                case NetEventType.Disconnected:
                    if (Mode != NodeMode.Server)
                        Mode = NodeMode.Inactive;

                    ConnectionIds.Remove(netEvent.ConnectionId);
                    OnDisconnection.TryInvoke(netEvent.ConnectionId);
                    break;

                case NetEventType.ReliableMessageReceived:
                    OnGetReliableMessage.TryInvoke(netEvent);
                    break;

                case NetEventType.UnreliableMessageReceived:
                    OnGetUnreliableMessage.TryInvoke(netEvent);
                    break;
            }
        }
    }
}
