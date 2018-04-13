using Byn.Net;
using System.Text;
using UnityEngine;
using System;
using System.Collections.Generic;

namespace AirPeer {
    public class Node : MonoBehaviour {
        string k_SignallingServer = "wss://because-why-not.com:12777/chatapp";
        string k_ICEServer1 = "stun:because-why-not.com:12779";
        string k_ICEServer2 = "stun:stun.l.google.com:19302";

        public event Action<NetworkEvent> OnNetworkEvent;
        public event Action OnConnected;
        public event Action OnDisconnected;
        public event Action OnConnectingFailed;
        public event Action<NetworkEvent> OnGetReliableMessage;
        public event Action<NetworkEvent> OnGetUnreliableMessage;

        IBasicNetwork m_Network;
        List<ConnectionId> m_ConnectionIds = new List<ConnectionId>();
        bool m_IsServer = false;

        Action<bool> m_ServerStartCallback;
        Action m_ServerStopCallback;

        Action<bool> m_ConnectionCallback;
        Action m_DisconnectionCallback;

        public static Node CreateInstance(string name = "AirPeerInstance") {
            var go = new GameObject(name);
            return go.AddComponent<Node>();
        }

        private void OnDestroy() {
            if (m_Network != null) 
                Cleanup();
        }

        private void Cleanup() {
            m_Network.Dispose();
            m_Network = null;
        }

        public int GetConnectionCount() {
            return m_ConnectionIds.Count;
        }

        public bool IsServer() {
            return m_IsServer;
        }
    
        public bool StartNetwork() {
            m_Network = WebRtcNetworkFactory.Instance.CreateDefault(
                k_SignallingServer,
                new[] {
                    new IceServer(k_ICEServer1),
                    new IceServer(k_ICEServer2)
                }
            );
            return m_Network != null;
        }

        public void StartServer(string roomName, Action<bool> callback = null) {
            m_ServerStartCallback = callback;
            m_Network.StartServer(roomName);
        }

        public void StopServer(Action callback = null) {
            m_ServerStopCallback = callback;
            m_Network.StopServer();
        }

        public void JoinServer(string roomName, Action<bool> callback = null) {
            m_ConnectionCallback = callback;
            m_Network.Connect(roomName);
        }

        public void DisconnectFromServer(Action callback = null) {
            m_DisconnectionCallback = callback;
            m_Network.Disconnect(new ConnectionId(1));
        }

        public void Reset() {
            m_ServerStartCallback = null;
            m_ServerStopCallback = null;
            m_ConnectionCallback = null;
            m_DisconnectionCallback = null;

            m_ConnectionIds = new List<ConnectionId>();
            Cleanup();
        }

        public bool SendString(string msg, bool reliable = false) {
            if (m_Network == null && m_ConnectionIds.Count == 0 && m_ConnectionIds.Count > 0)
                return false;
            else {
                byte[] msgData = Encoding.UTF8.GetBytes(msg);
                foreach (ConnectionId id in m_ConnectionIds)
                    m_Network.SendData(id, msgData, 0, msgData.Length, reliable);
                return true;
            }
        }

        public void Update() {
            if (m_Network != null) {
                m_Network.Update();
                ReadNetwork();
            }
            if (m_Network != null)
                m_Network.Flush();
        }

        void ReadNetwork() {
            NetworkEvent netEvent;
            while (m_Network != null && m_Network.Dequeue(out netEvent)) 
                ProcessNetworkEvent(netEvent);
        }

        void ProcessNetworkEvent(NetworkEvent netEvent) {
            OnNetworkEvent.TryInvoke(netEvent);
            switch (netEvent.Type) {
                case NetEventType.ServerInitialized:
                    m_ServerStartCallback.TryInvoke(true);
                    break;
                case NetEventType.ServerInitFailed:
                    Reset();
                    m_ServerStartCallback.TryInvoke(false);
                    if (m_ServerStartCallback != null) m_ServerStartCallback(false);
                    break;
                case NetEventType.ServerClosed:
                    Reset();
                    m_ServerStopCallback.TryInvoke();
                    break;
                case NetEventType.NewConnection:
                    m_ConnectionIds.Add(netEvent.ConnectionId);
                    m_ConnectionCallback.TryInvoke(true);
                    OnConnected.TryInvoke();
                    break;
                case NetEventType.ConnectionFailed:
                    Reset();
                    m_ConnectionCallback.TryInvoke(false);
                    OnConnectingFailed.TryInvoke();
                    break;
                case NetEventType.Disconnected:
                    if (!m_IsServer) Reset();
                    m_ConnectionIds.Remove(netEvent.ConnectionId);
                    OnDisconnected.TryInvoke();
                    m_DisconnectionCallback.TryInvoke();
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
