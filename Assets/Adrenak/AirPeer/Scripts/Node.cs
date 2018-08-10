using Byn.Net;
using System.Linq;
using UnityEngine;
using System;
using System.Text;
using System.Collections.Generic;

namespace Adrenak.AirPeer {
    // TODO: Split this class into server, client, event manager, message manager
    public class Node : MonoBehaviour {
        string k_SignallingServer = "wss://because-why-not.com:12777/chatapp";
        string k_ICEServer1 = "stun:because-why-not.com:12779";
        string k_ICEServer2 = "stun:stun.l.google.com:19302";

        public event Action OnServerStart;
        public event Action OnServerStop;
        public event Action OnServerFail;

        public event Action<ConnectionId> OnConnectionSuccess;
        public event Action<ConnectionId> OnConnectionEnd;
        public event Action<ConnectionId> OnConnectionFail;
        public event Action<ConnectionId> OnJoin;
        public event Action<ConnectionId> OnLeave;
        public event Action OnServerDown;

        public event Action<ConnectionId, Packet, bool> OnGetMessage;

        IBasicNetwork m_Network;
        public List<ConnectionId> ConnectionIds { get; private set; }
        public ConnectionId Id {
            get {
                if (ConnectionIds == null || ConnectionIds.Count == 0)
                    return ConnectionId.INVALID;
                return ConnectionIds[0];
            }
        }
        public NodeMode Mode { get; private set; }

        // ================================================
        // LIFECYCLE
        // ================================================
        public static Node New(string name = "Node (Instance)") {
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
            m_Network.Dispose();
            m_Network = null;
        }

        void Update() {
            if (m_Network != null) {
                m_Network.Update();
                ReadNetworkEvents();
            }
            if(m_Network != null)
                m_Network.Flush();
        }

        void OnDestroy() {
            Deinit();
        }

        // ================================================
        // NETWORK API
        // ================================================
        public void StartServer(string roomName) {
            Init();
            m_Network.StartServer(roomName);
        }

        public void StopServer() {
            Send(Packet.From(Id).WithTag(ReservedTags.ServerDown), true);
            m_Network.StopServer();
        }

        public void Connect(string roomName) {
            m_Network.Connect(roomName);
        }

        public void Disconnect() {
            m_Network.Disconnect(Id );
        }

        public bool Send(Packet packet, bool reliable = false) {
            if (m_Network == null || ConnectionIds == null || ConnectionIds.Count == 0) return false;

            var receivers = packet.Receivers;
            foreach(var cid in ConnectionIds) {
                if (receivers.Contains(cid.id) && cid.id != Id.id)
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
                    ConnectionIds.Add(new ConnectionId(0));
                    Mode = NodeMode.Server;
                    OnServerStart.TryInvoke();
                    break;

                case NetEventType.ServerInitFailed:
                    Deinit();
                    Mode = NodeMode.Ready;
                    OnServerFail.TryInvoke();
                    break;

                case NetEventType.ServerClosed:
                    Mode = NodeMode.Ready;
                    OnServerStop.TryInvoke();
                    break;

                case NetEventType.NewConnection:
                    ConnectionId newid = netEvent.ConnectionId;
                    ConnectionIds.Add(newid);

                    if(Mode == NodeMode.Ready)
                        Mode = NodeMode.Client;
                    else if(Mode == NodeMode.Server) {
                        foreach(var id in ConnectionIds) {
                            if (id.id == 0 || id.id == newid.id) continue;

                            byte[] payload;

                            // Announce the new connection to the old ones
                            payload = PayloadWriter.New().WriteShort(newid.id).Bytes;
                            Send(Packet.From(Id).To(id).With(ReservedTags.ConnectionRegister, payload), true);

                            payload = PayloadWriter.New().WriteShort(id.id).Bytes;
                            Send(Packet.From(Id).To(newid).With(ReservedTags.ConnectionRegister, payload), true);
                        }
                    }
                    
                    OnConnectionSuccess.TryInvoke(netEvent.ConnectionId);
                    break;

                case NetEventType.ConnectionFailed:
                    Deinit();
                    Mode = NodeMode.Ready;

                    OnConnectionFail.TryInvoke(netEvent.ConnectionId);
                    break;

                case NetEventType.Disconnected:
                    if(Mode == NodeMode.Client) {
                        Mode = NodeMode.Ready;
                        Deinit();
                    }
                    else if(Mode == NodeMode.Server) {
                        var leftid = netEvent.ConnectionId;
                        ConnectionIds.Remove(netEvent.ConnectionId);

                        var payload = PayloadWriter.New().WriteShort(leftid.id).Bytes;
                        Send(Packet.From(Id).With(ReservedTags.ConnectionDeregister, payload), true);
                    }

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
            var packet = Packet.Deserialize(netEvent.GetDataAsByteArray());
            string reservedTag = GetReservedTag(packet);

            // If is not a reserved message
            if (reservedTag == string.Empty) {
                OnGetMessage.TryInvoke(netEvent.ConnectionId, packet, reliable);

                if(Mode == NodeMode.Server) {
                    var receivers = packet.Receivers;
                    foreach(var r in receivers) {
                        if (r == Id.id || r == netEvent.ConnectionId.id) continue;
                        Send(Packet.From(Id).To(r).With(ReservedTags.PacketForwarding, packet.Serialize()), true);
                    }
                }
                return;
            }

            // handle reserved messages
            switch (reservedTag) {
                case ReservedTags.ServerDown:
                    OnServerDown.TryInvoke();
                    break;
                case ReservedTags.ConnectionRegister:
                    ConnectionIds.Add(netEvent.ConnectionId);
                    OnJoin.TryInvoke(netEvent.ConnectionId);
                    break;
                case ReservedTags.ConnectionDeregister:
                    ConnectionIds.Remove(netEvent.ConnectionId);
                    OnLeave.TryInvoke(netEvent.ConnectionId);
                    break;
                case ReservedTags.PacketForwarding:
                    OnGetMessage.TryInvoke(netEvent.ConnectionId, packet, reliable);
                    break;
            }
        }

        string GetReservedTag(Packet packet) {
            if (packet != null && packet.Tag.StartsWith("reserved")) 
                return packet.Tag;
            return string.Empty;
        }
    }
}
