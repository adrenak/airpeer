using System;
using System.Linq;
using System.Collections.Generic;

using Byn.Net;

namespace Adrenak.AirPeer {
    /// <summary>
    /// An AirPeer Node. Can serve as a server and a client.
    /// </summary>
    public class APNode : IDisposable {
        /// <summary>
        /// Describes the possible modes of <see cref="APNode"/>
        /// </summary>
        public enum Mode {
            /// <summary>
            /// State when the node is neither a server (ie. hosting a network)
            /// or a client (ie. connected to a server hosting a network).
            /// </summary>
            Idle,

            /// <summary>
            /// State when the node is hosting a network at an address. 
            /// </summary>
            Server,

            /// <summary>
            /// State when the node is connected to a server
            /// </summary>
            Client
        }

        /// <summary>
        /// The current mode of the node. The <see cref="Mode"/> changes 
        /// to <see cref="Mode.Idle"/> in error scenarios too, such as when the
        /// node fails to become a server, fails to connect to a server. 
        /// Or when the server is stopped or the client is disconnected.
        /// </summary>
        public Mode CurrentMode { get; private set; }

        /// <summary>
        /// The address of the network the node is 
        /// (i) hosting as a server 
        /// or (ii) connected to as a client
        /// </summary>
        public string Address { get; private set; }

        /// <summary>
        /// The ID of the node
        /// </summary>
        public short ID { get; private set; } = -1;

        /// <summary>
        /// The IDs of other nodes on this network (not including own ID)
        /// </summary>
        public List<short> Peers { get; private set; } = new List<short>();

        // Common
        /// <summary>
        /// Fired when a new client joins the network
        /// </summary>
        public event Action<short> OnClientJoined;

        /// <summary>
        /// Fired when a client leaves the network
        /// </summary>
        public event Action<short> OnClientLeft;

        /// <summary>
        /// Fired when this node is assigned an ID
        /// </summary>
        public event Action<short> OnReceiveID;

        /// <summary>
        /// Fired when a packet message is received
        /// </summary>
        public event Action<short, Packet> OnPacketReceived;

        /// <summary>
        /// Fired when a byte array message is received
        /// </summary>
        public event Action<short, byte[]> OnBytesReceived;

        // Server relevant
        /// <summary>
        /// Fired when the server successfully starts
        /// </summary>
        public event Action OnServerStartSuccess;

        /// <summary>
        /// Fired when the server fails to start along with the exception.
        /// </summary>
        public event Action<Exception> OnServerStartFailure;

        /// <summary>
        /// Fired when the server stops. Called on a node that was hosting
        /// a server.
        /// </summary>
        public event Action OnServerStop;

        // Client relevant
        /// <summary>
        /// Fired when this node is connected to a server. 
        /// Called on the node that attempts to connect to a server.
        /// </summary>
        public event Action OnConnected;

        /// <summary>
        /// Fired when this node is disconnected. Called on a client
        /// node after it tries to disconnect,
        /// </summary>
        public event Action OnDisconnected;

        /// <summary>
        /// Fired when the server a client node was connected to is stopped.
        /// Called on client node.
        /// </summary>
        public event Action OnRemoteServerClosed;

        /// <summary>
        /// Fired when this node fails to connect. Called on the node trying
        /// to connect to a server.
        /// </summary>
        public event Action<Exception> OnConnectionFailed;

        string tmpAddress;
        readonly APNetwork network;
        ConnectionId serverCID = ConnectionId.INVALID;

        /// <summary>
        /// Constructs a <see cref="APNode"/>
        /// </summary>
        /// <param name="signalingServer">Signalling server URL</param>
        /// <param name="apNetworkGameObjectName">
        /// Optional: Name of the APNetwork GameObject
        /// </param>
        public APNode(
            string signalingServer,
            string apNetworkGameObjectName = null
        ) : this(signalingServer, new[] {
            "stun.l.google.com:19302",
            "stun1.l.google.com:19302",
            "stun2.l.google.com:19302",
            "stun3.l.google.com:19302",
            "stun4.l.google.com:19302",
            "stun.vivox.com:3478",
            "stun.freecall.com:3478"
            },
            apNetworkGameObjectName
        ) { }

        /// <summary>
        /// Constructs a <see cref="APNode"/> instance
        /// </summary>
        /// <param name="signalingServer">The signalling server URL</param>
        /// <param name="iceServers">List of URLs of ICE servers</param>
        /// <param name="apNetworkGameObjectName">
        /// Optional: Name of the APNetwork GameObject
        /// </param>
        public APNode(
            string signalingServer,
            string[] iceServers,
            string apNetworkGameObjectName = null
        ) {
            CurrentMode = Mode.Idle;
            network = APNetwork.New(
                signalingServer: signalingServer,
                iceServers: iceServers,
                gameObjectName: apNetworkGameObjectName
            );

            network.OnServerStartSuccess += Network_OnServerStartSuccess;
            network.OnServerStartFailure += Network_OnServerStartFailure;
            network.OnServerStopped += Network_OnServerStopped;

            network.OnNewConnection += Network_OnNewConnection;
            network.OnConnectionFailed += Network_OnConnectionFailed;
            network.OnDisconnection += Network_OnDisconnection;

            network.OnMessageReceived += Network_OnMessageReceived;
        }

        // ================================================
        // API
        // ================================================
        /// <summary>
        /// Starts the server using the given address.
        /// </summary>
        public void StartServer(string address) {
            if (CurrentMode == Mode.Idle) {
                tmpAddress = address;
                network.StartServer(address);
            }
            else if (CurrentMode == Mode.Server) {
                var e = new Exception($"Already hosting address {Address}");
                OnServerStartFailure?.Invoke(e);
            }
            else {
                var e = new Exception($"Already connected to addr {Address}");
                OnServerStartFailure?.Invoke(e);
            }
        }

        /// <summary>
        /// Stops the server (if it's running)
        /// </summary>
        public void StopServer() {
            if (CurrentMode == Mode.Server)
                network.StopServer();
        }

        /// <summary>
        /// Connects to a server using an address
        /// </summary>
        public void Connect(string address) {
            if (CurrentMode == Mode.Idle) {
                tmpAddress = address;
                network.Connect(address);
            }
            else if (CurrentMode == Mode.Client) {
                var e = new Exception("Already connected to addr " + Address);
                OnConnectionFailed?.Invoke(e);
            }
            else {
                var e = new Exception("Already connected to addr " + Address);
                OnConnectionFailed?.Invoke(e);
            }
        }

        /// <summary>
        /// Disconnects from a server (if it's connected)
        /// </summary>
        public void Disconnect() {
            if (CurrentMode == Mode.Client)
                network.Disconnect(serverCID);
        }

        /// <summary>
        /// Sends a packet message to a recipient peer
        /// </summary>
        /// <param name="recipient">Recipient peer ID</param>
        /// <param name="packet">The packet containing the message</param>
        /// <param name="reliable">Whether data is sent UDP/TCP style</param>
        public void SendPacket
        (short recipient, Packet packet, bool reliable = false) =>
            SendPacket(new List<short> { recipient }, packet, reliable);

        /// <summary>
        /// Sends a packet message to several recipient peers
        /// </summary>
        /// <param name="recipients">Recipient peer IDs</param>
        /// <param name="packet">The packet containing the message</param>
        /// <param name="reliable">Whether data is sent UDP/TCP style</param>
        public void SendPacket
        (List<short> recipients, Packet packet, bool reliable = false) =>
            SendRaw(recipients, packet.Serialize(), reliable);

        /// <summary>
        /// Sends a byte array to a single recipient peer
        /// </summary>
        /// <param name="recipient">The recipient peer ID</param>
        /// <param name="bytes">The byte array to send</param>
        /// <param name="reliable">Whether data is sent UDP/TCP style</param>
        public void SendRaw
        (short recipient, byte[] bytes, bool reliable = false) =>
            SendRaw(new List<short> { recipient }, bytes, reliable);

        /// <summary>
        /// Sends a byte array to several recipient peers
        /// </summary>
        /// <param name="recipients">A list of the recipient peer IDs</param>
        /// <param name="bytes">The byte array to send</param>
        /// <param name="reliable">Whether data is sent UDP/TCP style</param>
        public void SendRaw
        (List<short> recipients, byte[] bytes, bool reliable = false) {
            if (recipients == null || recipients.Count == 0)
                recipients = new List<short> { 1 };

            var message = new Message {
                sender = ID,
                recipients = recipients.ToArray(),
                bytes = bytes
            };
            var bytesToSend = message.Serialize();

            // If we're a client, we only send to the server. 
            // The server will pass on the message to clients in recipient list
            if (CurrentMode == Mode.Client) {
                network.SendData(
                    id: serverCID,
                    data: bytesToSend,
                    offset: 0,
                    len: bytesToSend.Length,
                    reliable: reliable
                );
            }
            // If we're the server, we send to all the recipients
            else if (CurrentMode == Mode.Server) {
                foreach (var recipient in recipients)
                    if (recipient != ID) {
                        network.SendData(
                            id: new ConnectionId(recipient),
                            data: bytesToSend,
                            offset: 0,
                            len: bytesToSend.Length,
                            reliable: reliable
                        );
                    }
            }
        }

        // ================================================
        // NETWORK HANDLERS
        // ================================================
        void Network_OnServerStartSuccess(NetworkEvent e) {
            ID = 0;   // server ID is 0
            CurrentMode = Mode.Server;
            Address = tmpAddress;
            OnServerStartSuccess?.Invoke();
            OnReceiveID?.Invoke(ID);
        }

        void Network_OnServerStartFailure(NetworkEvent e) {
            Reset();
            OnServerStartFailure?.Invoke(
                new Exception(e.GetDataAsString() + e.Info)
            );
        }

        // This is called only on the server, not on the client side
        void Network_OnServerStopped(NetworkEvent e) {
            // Using a reserved packet tag, 
            // we let all the clients know that we have closed the server
            if (CurrentMode == Mode.Server) {
                SendPacket(
                    Peers,
                    new Packet().WithTag(Packet.ReservedTags.ServerClosed),
                    true
                );
                Reset();
                OnServerStop?.Invoke();
            }
        }

        void Network_OnNewConnection(NetworkEvent e) {
            if (CurrentMode == Mode.Server) {
                var theNewID = e.ConnectionId.id;

                // Notify a new client of its ID is using a reserved tag packet
                var tag = Packet.ReservedTags.ClientSetID;
                var packet = new Packet().With(tag, theNewID.GetBytes());
                SendPacket(theNewID, packet, true);

                // Using reserved tag packets, let the new client know about 
                // all the old clients and the old clients about the new client
                foreach (var anOldID in Peers) {
                    tag = Packet.ReservedTags.ClientJoined;
                    packet = new Packet().With(tag, anOldID.GetBytes());
                    SendPacket(theNewID, packet, true);

                    tag = Packet.ReservedTags.ClientJoined;
                    packet = new Packet().With(tag, theNewID.GetBytes());
                    SendPacket(anOldID, packet, true);
                }

                Peers.Add(theNewID);
                OnClientJoined?.Invoke(theNewID);
            }
            else if (CurrentMode == Mode.Idle) {
                serverCID = e.ConnectionId;
                CurrentMode = Mode.Client;
                // Add the server as a peer. To the client, server ID is 0 
                Peers.Add(0);
                Address = tmpAddress;
                OnConnected?.Invoke();
            }
        }

        void Network_OnConnectionFailed(NetworkEvent e) {
            Reset();
            var ex = new Exception(e.GetDataAsString() + e.Info);
            OnConnectionFailed?.Invoke(ex);
        }

        void Network_OnDisconnection(NetworkEvent e) {
            if (CurrentMode == Mode.Server) {
                Peers.Remove(e.ConnectionId.id);

                // Using a reserved tag, we let all the other clients know 
                // that a client has left
                var tag = Packet.ReservedTags.ClientLeft;
                var packet = new Packet().WithTag(tag)
                    .WithPayload(e.ConnectionId.id.GetBytes());
                SendPacket(Peers, packet, true);

                OnClientLeft?.Invoke(e.ConnectionId.id);
            }
            else if (CurrentMode == Mode.Client) {
                Reset();
                OnDisconnected?.Invoke();
            }
        }

        void Network_OnMessageReceived(NetworkEvent e, bool reliable) {
            var data = e.GetDataAsByteArray();

            var message = Message.Deserialize(data);
            var packet = Packet.Deserialize(message.bytes);

            // Try to look for reserved keywords
            if (packet != null && packet.Tag.StartsWith("reserved")) {
                switch (packet.Tag) {
                    case Packet.ReservedTags.ClientJoined:
                        if (CurrentMode == Mode.Client) {
                            var cid = packet.Payload.ToShort();
                            Peers.Add(cid);
                            OnClientJoined?.Invoke(cid);
                        }
                        break;

                    case Packet.ReservedTags.ClientLeft:
                        if (CurrentMode == Mode.Client) {
                            var cid = packet.Payload.ToShort();
                            Peers.Remove(cid);
                            OnClientLeft?.Invoke(cid);
                        }
                        break;

                    case Packet.ReservedTags.ClientSetID:
                        if (CurrentMode == Mode.Client) {
                            ID = packet.Payload.ToShort();
                            OnReceiveID?.Invoke(ID);
                        }
                        break;

                    case Packet.ReservedTags.ServerClosed:
                        Reset();
                        OnRemoteServerClosed?.Invoke();
                        break;
                }
                return;
            }

            // If we were an intended recipient
            if (message.recipients.Contains(ID)) {
                if (packet != null)
                    OnPacketReceived?.Invoke(message.sender, packet);
                else
                    OnBytesReceived?.Invoke(message.sender, message.bytes);
            }

            // If we're a server, forward the message to the intended
            // recipients, except ourselves
            if (CurrentMode == Mode.Server) {
                foreach (var recipient in message.recipients) {
                    if (recipient != 0) {
                        if (packet == null) {
                            packet = new Packet();
                            packet.WithPayload(message.bytes);
                        }
                        var m = new Message {
                            sender = message.sender,
                            recipients = new short[] { recipient },
                            bytes = packet.Serialize()
                        };
                        network.SendData(
                            id: new ConnectionId(recipient),
                            data: m.Serialize(),
                            offset: 0,
                            len: m.Serialize().Length,
                            reliable: reliable
                        );
                    }
                }
            }
        }

        void Reset() {
            ID = -1;
            CurrentMode = Mode.Idle;
            foreach (var peer in Peers)
                OnClientLeft?.Invoke(peer);
            Peers.Clear();
            Address = null;
        }

        /// <summary>
        /// Disposes this <see cref="APNode"/> instance by disposing
        /// the internal <see cref="APNetwork"/> instance and resetting
        /// state
        /// </summary>
        public void Dispose() {
            OnDisconnected?.Invoke();
            network.Dispose();
            Reset();
        }
    }
}
