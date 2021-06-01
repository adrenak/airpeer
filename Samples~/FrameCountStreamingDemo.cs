using UnityEngine;
using UnityEngine.UI;

namespace Adrenak.AirPeer.Samples {
    /// <summary>
    /// A demo script that shows a constant stream of data sent from
    /// the server to all the clients.
    /// </summary>
    public class FrameCountStreamingDemo : MonoBehaviour {
        public string signalingServer = "ws://localhost:12776/";
        public InputField inputField;
        public Text msg1;
        public Text msg2;
        APNode node;

        private void Start() {
            node = new APNode(signalingServer);

            node.OnServerStartSuccess += () => {
                msg1.text = "Server started";
            };

            node.OnServerStartFailure += e => {
                msg1.text = "Server start failed " + e;
            };

            node.OnServerStop += () => {
                msg1.text = "Stopped server";
                msg2.text = "";
            };

            node.OnConnected += () => {
                msg1.text = "Connected to server";
            };

            node.OnReceiveID += id => Debug.Log("Got ID : " + id);

            node.OnDisconnected += () =>
                msg1.text = "Disconnected from server";

            node.OnConnectionFailed += ex =>
                Debug.LogError(ex);

            node.OnPacketReceived += (id, packet) => {
                var reader = new BytesReader(packet.Payload);
                msg2.text = "Received " + reader.ReadInt();
            };


            node.OnClientJoined += id =>
                Debug.Log("Client joined : " + id);

            node.OnClientLeft += id =>
                Debug.Log("Client left : " + id);
        }

        void Update() {
            if (node != null) {
                if (node.CurrentMode == APNode.Mode.Server) {
                    node.SendPacket(node.Peers, new Packet()
                    .WithPayload(System.BitConverter.GetBytes(Time.frameCount)));
                    msg2.text = "Sending " + Time.frameCount.ToString();
                }
            }
        }

        public void StartServer() => node.StartServer(inputField.text);

        public void Connect() => node.Connect(inputField.text);

        public void Disconnect() {
            if (node.CurrentMode == APNode.Mode.Client)
                node.Disconnect();
            else if (node.CurrentMode == APNode.Mode.Server)
                node.StopServer();
        }
    }
}
