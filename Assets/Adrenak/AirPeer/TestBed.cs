using Byn.Net;
using Adrenak.AirPeer;
using UnityEngine;

public class TestBed : MonoBehaviour {
    Node[] nodes;

    private void Start() {
        nodes = new Node[3];
        for(int i = 0; i < nodes.Length; i++) {
            var n = nodes[i];
            n = Node.New();
            n.Init();
        }
    }

    void Listen(Node node, int index) {
        node.OnServerFail += delegate () {
            Debug.Log(index + " fail");
        };

        node.OnServerStart += delegate () {
            Debug.Log(index + " start");
        };

        node.OnServerStop += delegate () {
            Debug.Log(index + " stop");
        };

        node.OnConnectionSuccess += delegate (ConnectionId id) {
            Debug.Log(index + " connected");
        };

        node.OnConnectionFail += delegate (ConnectionId id) {
            Debug.Log(index + " connection fail");
        };

        node.OnConnectionEnd += delegate (ConnectionId id) {
            Debug.Log(index + " disconnected ");
        };

        node.OnServerDown += delegate () {
            Debug.Log(index + " disconnect as server down");
        };

        node.OnGetMessage += delegate (ConnectionId arg1, Packet arg2, bool arg3) {
            Debug.Log(index + " : message from " + arg1.id + " : " + arg2.Payload.ToUTF8String());
        };
    }
}
