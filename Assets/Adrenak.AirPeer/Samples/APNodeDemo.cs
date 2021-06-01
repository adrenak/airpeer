using UnityEngine;

namespace Adrenak.AirPeer.Samples {
    /// <summary>
    /// A simple demo scene that allows multiple nodes to form a network
    /// and exchange messages using an immediate mode GUI.
    /// </summary>
    public class APNodeDemo : MonoBehaviour {
        public string signalingServerURL = "ws://localhost:12776/";
        APNode node;

        private void Start() {
            node = new APNode(signalingServerURL, new string[]{
                "stun:stun.l.google.com:19302"
            });

            node.OnServerStartSuccess += () =>
                msg = ("Server started.");

            node.OnServerStartFailure += ex =>
                msg = ("Server could not start " + ex);

            node.OnServerStop += () =>
                msg = ("Server stopped");


            node.OnConnected += () =>
                msg = ("Connected");

            node.OnDisconnected += () =>
                msg = ("Disconnected from server");

            node.OnConnectionFailed += ex =>
                msg = ("Could not connect to server " + ex);


            node.OnClientJoined += id =>
                msg = "Client #" + id + " connected";

            node.OnClientLeft += id =>
                msg = "Client #" + id + " left";
                

            node.OnReceiveID += id =>
                msg = ("Assigned ID " + id);

            node.OnPacketReceived += (arg1, arg2) =>
                msg = ("Message received from peer #" + arg1 + " : " + arg2.Tag);

            node.OnBytesReceived += (id, bytes) =>
                msg = ("Message received from peer #" + id + " : " + bytes.Length);
        }

        string msg;
        string address = "address";
        string textInput;

        private void OnGUI() {
            int height = 20;
            int count = 0;
            int getHeight() {
                var value = height * count;
                count++;
                return value;
            }

            GUI.Label(new Rect(0, getHeight(), 4000, height), msg);
            var label = node.CurrentMode == APNode.Mode.Idle ? "Not Connected. Mode" : (node.CurrentMode == APNode.Mode.Client ? "I am Client" : "I am Server") + " ID : " + node.ID;

            GUI.Label(new Rect(0, getHeight(), 400, height), label);
            address = GUI.TextField(new Rect(0, getHeight(), 400, height), address);

            if (GUI.Button(new Rect(0, getHeight(), 400, height), "Create"))
                node.StartServer(address);
            if (GUI.Button(new Rect(0, getHeight(), 400, height), "Join"))
                node.Connect(address);
            if (GUI.Button(new Rect(0, getHeight(), 400, height), "Leave")) {
                if (node.CurrentMode == APNode.Mode.Server)
                    node.StopServer();
                else
                    node.Disconnect();
            }

            textInput = GUI.TextField(new Rect(0, getHeight(), 400, height), textInput);

            if (GUI.Button(new Rect(0, getHeight(), 400, height), "Send Message")) {
                node.SendPacket(node.Peers, new Packet().WithTag(textInput), true);
                textInput = "";
            }

            if (GUI.Button(new Rect(0, getHeight(), 400, height), "Print Peers")) {
                var str = "PEERS : [";
                foreach (var p in node.Peers)
                    str += p + "  ";
                str += "]";
                msg = (str);
            }
        }
    }
}
