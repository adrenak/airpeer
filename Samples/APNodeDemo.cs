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
                Debug.Log("Server started.");

            node.OnServerStartFailure += ex =>
                Debug.LogError("Server could not start " + ex);

            node.OnServerStop += () =>
                Debug.Log("Server stopped");


            node.OnConnected += () =>
                Debug.Log("Connected");

            node.OnDisconnected += () =>
                Debug.Log("Disconnected from server");

            node.OnConnectionFailed += ex =>
                Debug.LogError("Could not connect to server " + ex);


            node.OnReceiveID += id =>
                Debug.Log("Assigned ID " + id);

            node.OnPacketReceived += (arg1, arg2) =>
                Debug.Log("Message received " + arg1 + " : " + arg2.Tag);

            node.OnBytesReceived += (id, bytes) =>
                Debug.Log("Message received " + id + " : " + bytes.Length);
        }

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
                var str = "PEERS : ";
                foreach (var p in node.Peers)
                    str += p + "  ";
                Debug.Log(str);
            }
        }
    }
}
