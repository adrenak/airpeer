using Byn.Net;
using System.Linq;
using UnityEngine;
using System;
using System.Text;
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

        public event Action<ConnectionId> OnConnectionSuccess;
        public event Action<ConnectionId> OnConnectionEnd;
        public event Action<ConnectionId> OnConnectionFail;
        public event Action OnServerDown;

        public event Action<NetworkEvent, Packet, bool> OnGetMessage;

        IBasicNetwork m_Network;
        public List<ConnectionId> ConnectionIds { get; private set; }
        public ConnectionId Id;
        public NodeMode Mode { get; private set; }

        // ================================================
        // LIFECYCLE
        // ================================================
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

        // ================================================
        // NETWORK API
        // ================================================
        public void StartServer(string roomName) {
            m_Network.StartServer(roomName);
        }

        public void StopServer() {
            Send(Reserved.ServerDown, string.Empty, true, null);
            m_Network.StopServer();
        }

        public void Connect(string roomName) {
            m_Network.Connect(roomName);
        }

        public void Disconnect() {
            m_Network.Disconnect(new ConnectionId(1));
        }

        public bool Send(string payload, bool reliable = false, short[] receivers = null) {
            return Send(new Packet(null, Encoding.UTF8.GetBytes(payload)), reliable, receivers);
        }

        public bool Send(byte[] payload, bool reliable = false, short[] receivers = null) {
            return Send(new Packet(null, payload), reliable, receivers);
        }

        public bool Send(string tag, string payload, bool reliable = false, short[] receivers = null) {
            return Send(new Packet(tag, Encoding.UTF8.GetBytes(payload)), reliable, receivers);
        }

        public bool Send(string tag, byte[] payload, bool reliable = false, short[] receivers = null) {
            return Send(new Packet(tag, payload), reliable, receivers);
        }

        public bool Send(Packet packet, bool reliable = false, short[] receivers = null) {
            if (m_Network == null || ConnectionIds == null || ConnectionIds.Count == 0) return false;
           
            foreach(var cid in ConnectionIds) {
                if(receivers == null || receivers.Contains(cid.id))
                    m_Network.SendData(cid, packet.Serialize(), 0, packet.Serialize().Length, reliable);
            }
            return true;
        }

        // ================================================
        // INTERNAL
        // ================================================
        void ReadNetworkEvents() {
            NetworkEvent netEvent;
            while (m_Network != null && m_Network.Dequeue(out netEvent))
                ProcessNetworkEvent(netEvent);
        }

        void ProcessNetworkEvent(NetworkEvent netEvent) {
            switch (netEvent.Type) {
                case NetEventType.ServerInitialized:
                    Mode = NodeMode.Server;
                    OnServerStart.TryInvoke();
                    break;

                case NetEventType.ServerInitFailed:
                    Mode = NodeMode.Inactive;
                    OnServerFail.TryInvoke();
                    break;

                case NetEventType.ServerClosed:
                    Mode = NodeMode.Inactive;
                    OnServerStop.TryInvoke();
                    break;

                case NetEventType.NewConnection:
                    if (Mode != NodeMode.Server) Mode = NodeMode.Client;

                    ConnectionIds.Add(netEvent.ConnectionId);
                    OnConnectionSuccess.TryInvoke(netEvent.ConnectionId);
                    break;

                case NetEventType.ConnectionFailed:
                    if (Mode != NodeMode.Server)
                        Mode = NodeMode.Inactive;

                    ConnectionIds.Add(netEvent.ConnectionId);
                    OnConnectionFail.TryInvoke(netEvent.ConnectionId);
                    break;

                case NetEventType.Disconnected:
                    if (Mode != NodeMode.Server)
                        Mode = NodeMode.Inactive;

                    ConnectionIds.Remove(netEvent.ConnectionId);
                    OnConnectionEnd.TryInvoke(netEvent.ConnectionId);
                    break;

                case NetEventType.ReliableMessageReceived:
                    HandleMessage(netEvent, true);
                    break;

                case NetEventType.UnreliableMessageReceived:
                    HandleMessage(netEvent, false);
                    break;
            }
        }

        void HandleMessage(NetworkEvent netEvent, bool reliable) {
            string reservedTag = GetReservedTag(netEvent);

            if (reservedTag == string.Empty) 
                OnGetMessage(netEvent, Packet.Deserialize(netEvent.GetDataAsByteArray()), reliable);

            // handle reserved messages
            switch (reservedTag) {
                case Reserved.ServerDown:
                    OnServerDown.TryInvoke();
                    break;
            }
        }

        string GetReservedTag(NetworkEvent netEvent) {
            var packet = Packet.Deserialize(netEvent.GetDataAsByteArray());
            if (packet != null && packet.Tag.StartsWith("reserved")) 
                return packet.Tag;
            return string.Empty;
        }
    }
}
