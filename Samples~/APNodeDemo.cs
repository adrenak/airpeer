using UnityEngine;

namespace Adrenak.AirPeer.Samples {
    public class APNodeDemo : MonoBehaviour {
        APNode peer;

        private void Start() {
            peer = new APNode("ws://localhost:12776/chatapp", new string[]{
                "stun:stun.l.google.com:19302"
            });

            peer.OnServerStartSuccess += () =>
                Debug.Log("Server started.");

            peer.OnServerStartFailure += ex =>
                Debug.LogError("Server could not start " + ex);

            peer.OnServerStop += () =>
                Debug.Log("Server stopped");


            peer.OnConnected += () =>
                Debug.Log("Connected");

            peer.OnDisconnected += () =>
                Debug.Log("Disconnected from server");

            peer.OnConnectionFailed += ex =>
                Debug.LogError("Could not connect to server " + ex);


            peer.OnReceiveID += id =>
                Debug.Log("Assigned ID " + id);

            peer.OnPacketReceived += (arg1, arg2) =>
                Debug.Log("Message received " + arg1+ " : " + arg2.Tag);

            peer.OnBytesReceived += (id, bytes) =>
                Debug.Log("Message received " + id + " : " + bytes.Length);
        }

        string serverName = "room";
        string textInput;

        private void OnGUI() {
            int height = 20;
            int count = 0;
            int getHeight() {
                var value = height * count;
                count++;
                return value;
            }

            var label = peer.CurrentMode == APNode.Mode.Idle ? "Not Connected. Mode" : (peer.CurrentMode == APNode.Mode.Client ? "I am Client" : "I am Server") + " ID : " + peer.ID;

            GUI.Label(new Rect(0, getHeight(), 400, height), label);
            serverName = GUI.TextField(new Rect(0, getHeight(), 400, height), serverName);

            if (GUI.Button(new Rect(0, getHeight(), 400, height), "Create"))
                peer.StartServer(serverName);
            if (GUI.Button(new Rect(0, getHeight(), 400, height), "Join"))
                peer.Connect(serverName);
            if (GUI.Button(new Rect(0, getHeight(), 400, height), "Leave")) {
                if (peer.CurrentMode == APNode.Mode.Server)
                    peer.StopServer();
                else
                    peer.Disconnect();
            }

            textInput = GUI.TextField(new Rect(0, getHeight(), 400, height), textInput);

            if (GUI.Button(new Rect(0, getHeight(), 400, height), "Send Message")) {
                peer.SendPacket(peer.Peers, new Packet().WithTag(textInput), true);
                textInput = "";
            }

            if (GUI.Button(new Rect(0, getHeight(), 400, height), "Print Peers")) {
                var str = "PEERS : ";
                foreach (var p in peer.Peers)
                    str += p + "  ";
                Debug.Log(str);
            }
        }
    }
}
