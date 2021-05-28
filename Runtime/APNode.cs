using System;
using System.Linq;
using System.Collections.Generic;

using Byn.Net;

namespace Adrenak.AirPeer {
    /// <summary>
    /// An AirPeer Node. Can serve as a server and a client.
    /// </summary>
    public class APNode {
        public enum Mode {
            Idle,
            Server,
            Client
        }

        /// <summary>
        /// the current mode of the node
        /// </summary>
        public Mode CurrentMode { get; private set; }

        /// <summary>
        /// The name of the server the node is hosting/connected to
        /// </summary>
        public string Address { get; private set; }

        /// <summary>
        /// The ID of the node
        /// </summary>
        public short ID { get; private set; } = -1;

        /// <summary>
        /// The IDs of the peers of this node
        /// </summary>
        public List<short> Peers = new List<short>();

        // Common
        /// <summary>
        /// Fired when another client joins the network
        /// </summary>
        public event Action<short> OnClientJoined;

        /// <summary>
        /// Fired when another client leaves the network
        /// </summary>
        public event Action<short> OnClientLeft;

        /// <summary>
        /// Fired when the node is assigned an ID
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
        /// Fired when the server fails to start along with the exception
        /// </summary>
        public event Action<Exception> OnServerStartFailure;

        /// <summary>
        /// Fired when the server stops
        /// </summary>
        public event Action OnServerStop;

        // Client relevant
        /// <summary>
        /// Fired when this node is connected to a server
        /// </summary>
        public event Action OnConnected;

        /// <summary>
        /// Fired when this node is disconnected
        /// </summary>
        public event Action OnDisconnected;

        /// <summary>
        /// Fired when this node fails to connect
        /// </summary>
        public event Action<Exception> OnConnectionFailed;

        string tmpAddress;
        readonly APNetwork network;
        ConnectionId serverCID = ConnectionId.INVALID;

        /// <summary>
        /// Constructs a peer with the given the signalling server and with a default list of ICE servers
        /// </summary>
        /// <param name="signalingServer">The URL of the signalling server</param>
        /// <param name="iceServer">List of URLs of ICE servers</param>
        public APNode(string signalingServer) : this(signalingServer, new[] {
            "stun.l.google.com:19302",
            "stun1.l.google.com:19302",
            "stun2.l.google.com:19302",
            "stun3.l.google.com:19302",
            "stun4.l.google.com:19302",
            "stun.vivox.com:3478",
            "stun.freecall.com:3478"
        }) { }

        /// <summary>
        /// Constructs a peer with the given the signalling server and a list of ICE servers
        /// </summary>
        /// <param name="signalingServer">The URL of the signalling server</param>
        /// <param name="iceServer">List of URLs of ICE servers</param>
        public APNode(string signalingServer, string[] iceServer) {
            CurrentMode = Mode.Idle;
            network = APNetwork.New(signalingServer, iceServer);

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
        /// Starts the server with a name
        /// </summary>
        public void StartServer(string address) {
            if (CurrentMode == Mode.Idle) {
                tmpAddress = address;
                network.StartServer(address);
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
        }

        /// <summary>
        /// Disconnects from a server (if it's connected)
        /// </summary>
        public void Disconnect() {
            if (CurrentMode == Mode.Client)
                network.Disconnect(serverCID);
        }

        /// <summary>
        /// Sends a packet message to a single recipient
        /// </summary>
        /// <param name="recipient">Recipient ID</param>
        /// <param name="packet">The packet containing the message</param>
        /// <param name="reliable">Whether the message is reliable or not</param>
        public void SendPacket(short recipient, Packet packet, bool reliable = false) =>
            SendPacket(new List<short> { recipient }, packet, reliable);

        /// <summary>
        /// Sends a packet message to several recipients
        /// </summary>
        /// <param name="recipients">Recipient IDs</param>
        /// <param name="packet">The packet containing the message</param>
        /// <param name="reliable">Whether the message is reliable or not</param>
        public void SendPacket(List<short> recipients, Packet packet, bool reliable = false) =>
            SendRaw(recipients, packet.Serialize(), reliable);

        /// <summary>
        /// Sends a byte array to a single recipient
        /// </summary>
        /// <param name="recipient">The recipient ID</param>
        /// <param name="bytes">The byte array to send</param>
        /// <param name="reliable">Whether the message is reliable or not</param>
        public void SendRaw(short recipient, byte[] bytes, bool reliable = false) =>
            SendRaw(new List<short> { recipient }, bytes, reliable);

        /// <summary>
        /// Sends a byte array to several recipients
        /// </summary>
        /// <param name="recipients">A list of the recipient IDs</param>
        /// <param name="bytes">The byte array to send</param>
        /// <param name="reliable">Whether the message is reliable or not</param>
        public void SendRaw(List<short> recipients, byte[] bytes, bool reliable = false) {
            if (recipients == null || recipients.Count == 0)
                recipients = new List<short> { 1 };

            var message = new Message {
                sender = ID,
                recipients = recipients.ToArray(),
                bytes = bytes
            };
            var bytesToSend = message.Serialize();

            // If we're a client, we only send to the server. The server will pass on the message
            // to any other clients in the recipient list
            if (CurrentMode == Mode.Client)
                network.SendData(serverCID, bytesToSend, 0, bytesToSend.Length, reliable);
            // If we're the server, we send to all the intended recipients separately
            else if (CurrentMode == Mode.Server) {
                foreach (var recipient in recipients)
                    if (recipient != ID)
                        network.SendData(new ConnectionId(recipient), bytesToSend, 0, bytesToSend.Length, reliable);
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
            OnServerStartFailure?.Invoke(new Exception(e.GetDataAsString() + e.Info));
        }

        // This is called only on the server, not on the client side
        void Network_OnServerStopped(NetworkEvent e) {
            // We let all the clients know that we have closed with a reserved tag packet.
            if (CurrentMode == Mode.Server) {   // Just making sure we're the server
                var packet = new Packet().WithTag(Packet.ReservedTags.ServerClosed);
                SendPacket(Peers, packet, true);
                Reset();
                OnServerStop?.Invoke();
            }
        }

        void Network_OnNewConnection(NetworkEvent e) {
            if (CurrentMode == Mode.Server) {
                var theNewID = e.ConnectionId.id;

                // Let the new client know what its ID is using a reserved tag packet
                var packet = new Packet().With(Packet.ReservedTags.ClientSetID, theNewID.GetBytes());
                SendPacket(theNewID, packet, true);

                // Using reserved tag packets, let the new client know about all the old clients
                // and the old clients know about the new client
                foreach (var anOldID in Peers) {
                    packet = new Packet().With(Packet.ReservedTags.ClientJoined, anOldID.GetBytes());
                    SendPacket(theNewID, packet, true);

                    packet = new Packet().With(Packet.ReservedTags.ClientJoined, theNewID.GetBytes());
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
            OnConnectionFailed?.Invoke(new Exception(e.GetDataAsString() + e.Info));
        }

        void Network_OnDisconnection(NetworkEvent e) {
            if (CurrentMode == Mode.Server) {
                Peers.Remove(e.ConnectionId.id);

                // Tell all the other clients that a client has left using reserved tag packets
                var packet = new Packet().With(Packet.ReservedTags.ClientLeft, e.ConnectionId.id.GetBytes());
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
                        OnServerStop?.Invoke();
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
                        network.SendData(new ConnectionId(recipient), m.Serialize(), 0, m.Serialize().Length, reliable);
                    }
                }
            }
        }

        void Reset() {
            ID = -1;
            CurrentMode = Mode.Idle;
            Peers.Clear();
            Address = null;
        }
    }
}
