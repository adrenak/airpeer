using Byn.Net;
using System.Text;
using UnityEngine;
using System;
using System.Collections.Generic;

public class AirPeer : MonoBehaviour{
    public string k_SignallingServer = "wss://because-why-not.com:12777/chatapp";
    public string k_ICEServer1 = "stun:because-why-not.com:12779";
    public string k_ICEServer2 = "stun:stun.l.google.com:19302";

    public delegate void NetworkEventHandler(AirPeer sender, NetworkEvent netEvent);
    public event NetworkEventHandler OnNetworkEvent;

    IBasicNetwork m_Network;
    List<ConnectionId> m_ConnectionIds = new List<ConnectionId>();
    bool m_IsServer = false;

    Action<bool> m_ServerStartCallback;
    Action m_ServerStopCallback;

    Action<bool> m_ServerConnectCallback;
    Action m_ServerDisconnectCallback;

    public int connectionCount {
        get { return m_ConnectionIds.Count; }
    }
    
    public static AirPeer CreateInstance(string name = "AirPeerInstance") {
        var go = new GameObject(name);
        return go.AddComponent<AirPeer>();
    }

    private void OnDestroy() {
        if (m_Network != null) {
            Cleanup();
        }
    }

    private void Cleanup() {
        m_Network.Dispose();
        m_Network = null;
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
        m_ServerConnectCallback = callback;
        m_Network.Connect(roomName);
    }

    public void DisconnectFromServer(Action callback = null) {
        m_ServerDisconnectCallback = callback;
        m_Network.Disconnect(new ConnectionId(1));
    }

    public void Reset() {
        m_IsServer = false;
        m_ServerStartCallback = null;
        m_ServerStopCallback = null;
        m_ServerConnectCallback = null;
        m_ServerDisconnectCallback = null;

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
        switch (netEvent.Type) {
            case NetEventType.ServerInitialized:
                m_IsServer = true;
                if (m_ServerStartCallback != null) m_ServerStartCallback(true);
                break;
            case NetEventType.ServerInitFailed:
                Reset();
                if (m_ServerStartCallback != null) m_ServerStartCallback(false);
                break;
            case NetEventType.ServerClosed:
                Reset();
                if (m_ServerStopCallback != null) m_ServerStopCallback();
                break;
            case NetEventType.NewConnection:
                m_ConnectionIds.Add(netEvent.ConnectionId);

                if (m_IsServer)
                    SendString(netEvent.ConnectionId + " has joined");

                if (m_ServerConnectCallback != null) m_ServerConnectCallback(true);
                break;
            case NetEventType.ConnectionFailed:
                Reset();
                if (m_ServerConnectCallback != null) m_ServerConnectCallback(false);
                break;
            case NetEventType.Disconnected:
                m_ConnectionIds.Remove(netEvent.ConnectionId);
                if (!m_IsServer)
                    Reset();
                if (m_ServerDisconnectCallback != null) m_ServerDisconnectCallback();
                break;
            case NetEventType.ReliableMessageReceived:
            case NetEventType.UnreliableMessageReceived:
                break;
        }
        if (OnNetworkEvent != null) OnNetworkEvent(this, netEvent);
    }
}
