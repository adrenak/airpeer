using Byn.Net;

using System;
using System.Linq;

using UnityEngine;

namespace Adrenak.AirPeer {
    /// <summary>
    /// Provides access to the WebRTC Network Plugin
    /// </summary>
    public class APNetwork : MonoBehaviour {
        /// <summary>
        /// Fired when the server successfully starts
        /// </summary>
        public event Action<NetworkEvent> OnServerStartSuccess;

        /// <summary>
        /// Fired when the server fails to start along with the exception
        /// </summary>
        public event Action<NetworkEvent> OnServerStartFailure;

        /// <summary>
        /// Fired when the server stops
        /// </summary>
        public event Action<NetworkEvent> OnServerStopped;

        /// <summary>
        /// Fired when a new connection is formed. Called on both the server and client
        /// </summary>
        public event Action<NetworkEvent> OnNewConnection;

        /// <summary>
        /// Fired when a connection attempt failed. 
        /// </summary>
        public event Action<NetworkEvent> OnConnectionFailed;

        /// <summary>
        /// Fired when a connection is closed. Called on both the server and client
        /// </summary>
        public event Action<NetworkEvent> OnDisconnection;

        /// <summary>
        /// Fired when a message is receipted
        /// </summary>
        public event Action<NetworkEvent, bool> OnMessageReceived;

        IBasicNetwork network;

        APNetwork() { }

        /// <summary>
        /// Creates a new APNetwork instance
        /// </summary>
        /// <param name="signalingServer">The URL of the signaling server</param>
        /// <param name="iceServers">List of ICE server URLs</param>
        public static APNetwork New(string signalingServer, string[] iceServers) {
            var go = new GameObject("Peer Network [" + signalingServer + "]");
            DontDestroyOnLoad(go);
            var instance = go.AddComponent<APNetwork>();

            instance.network = WebRtcNetworkFactory.Instance.CreateDefault(
                signalingServer,
                iceServers.Select(x => new IceServer(x)).ToList().ToArray()
            );

            if (instance.network == null) {
                Destroy(go);
                return null;
            }

            return instance;
        }

        /// <summary>
        /// Starts a server for the given address
        /// </summary>
        /// <param name="address">Address at which the server will start</param>
        public void StartServer(string address) => network.StartServer(address);

        /// <summary>
        /// Stops the server (if it's running)
        /// </summary>
        public void StopServer() => network.StopServer();

        /// <summary>
        /// Connects to a server using the address
        /// </summary>
        /// <param name="address">The address at which the server is running</param>
        public void Connect(string address) => network.Connect(address);

        /// <summary>
        /// Disconnects using a ConnectionId
        /// </summary>
        /// <param name="id">The Id from which to disconnect</param>
        public void Disconnect(ConnectionId id) => network.Disconnect(id);

        /// <summary>
        /// Sends data over a connection
        /// </summary>
        /// <param name="id">The Id of the connection over which data is send</param>
        /// <param name="data">The data to be sent</param>
        /// <param name="offset">Offset from the byte array data</param>
        /// <param name="length">Length of the data starting from offset in data</param>
        /// <param name="reliable">Whether data is sent UDP or TCP style</param>
        public void SendData(ConnectionId id, byte[] data, int offset, int length, bool reliable) =>
            network.SendData(id, data, offset, length, reliable);

        void Update() {
            if (network != null) {
                network.Update();
                network.Flush();

                // Dequeue while we have something
                do {
                    network.Dequeue(out NetworkEvent e);
                    if(e.Type != NetEventType.Invalid)
                        ProcessNetworkEvent(e);
                } while (network.Peek(out NetworkEvent e2));
            }
        }

        void ProcessNetworkEvent(NetworkEvent e) {
            switch (e.Type) {
                case NetEventType.ServerInitialized:
                    OnServerStartSuccess?.Invoke(e);
                    break;

                case NetEventType.ServerInitFailed:
                    OnServerStartFailure?.Invoke(e);
                    break;

                // Received after network.StopServer
                case NetEventType.ServerClosed:
                    OnServerStopped?.Invoke(e);
                    break;


                case NetEventType.NewConnection:
                    OnNewConnection?.Invoke(e);
                    break;

                case NetEventType.ConnectionFailed:
                    OnConnectionFailed?.Invoke(e);
                    break;

                case NetEventType.Disconnected:
                    OnDisconnection?.Invoke(e);
                    break;


                case NetEventType.ReliableMessageReceived:
                    OnMessageReceived?.Invoke(e, true);
                    break;

                case NetEventType.UnreliableMessageReceived:
                    OnMessageReceived?.Invoke(e, false);
                    break;
            }
        }
    }
}
