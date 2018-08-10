using Byn.Net;
using UnityEngine;

namespace Adrenak.AirPeer {
    public class Test : MonoBehaviour {
        Node n1, n2;

        private void Start() {
            n1 = Node.New();
            n1.Init();
            n2 = Node.New();
            n2.Init();

            setup1();
            setup2();
        }

        [ContextMenu("id 1")]
        void id1() {
            Debug.Log(n1.Id.id);
        }

        [ContextMenu("setup 1")]
        private void setup1() {
            n1.OnServerFail += delegate () {
                Debug.Log("n1 fail");
            };

            n1.OnServerStart += delegate () {
                Debug.Log("n1 start");
            };

            n1.OnServerStop += delegate () {
                Debug.Log("n1 stop");
            };

            n1.OnConnectionSuccess += delegate (ConnectionId id) {
                Debug.Log("n1: on connection" + id.id);
            };

            n1.OnConnectionFail += delegate (ConnectionId id) {
                Debug.Log("n1: on connection fail" + id.id);
            };

            n1.OnConnectionEnd+= delegate (ConnectionId id) {
                Debug.Log("n1: on disconnect" + id.id);
            };

            n1.OnServerDown += delegate () {
                Debug.Log("n1: on down");
            };

            n1.OnGetMessage += delegate (ConnectionId arg1, Packet arg2, bool arg3) {
                Debug.Log("n1 : message from " + arg1.id + " : " + arg2.Payload.ToUTF8String());
            };
        }

        [ContextMenu("start server1")]
        void startserver1() {
            n1.StartServer("A");
        }

        [ContextMenu("stop server1")]
        void stopserver1() {
            n1.StopServer();
        }

        [ContextMenu("connect 1")]
        void connect1() {
            n1.Connect("A");
        }

        [ContextMenu("dissconn 1")]
        void disconn1() {
            n1.Disconnect();
        }

        [ContextMenu("send 1")]
        void send1() {
            n1.Send(Packet.From(n1.Id).WithPayload("payload1"));
        }

        [ContextMenu("id 2")]
        void id2() {
            Debug.Log(n2.Id.id);
        }



        [ContextMenu("setup 2")]
        private void setup2() {
            n2.OnServerFail += delegate () {
                Debug.Log("n2 fail");
            };

            n2.OnServerStart += delegate () {
                Debug.Log("n2 start");
            };

            n2.OnServerStop += delegate () {
                Debug.Log("n2 stop");
            };

            n2.OnConnectionSuccess += delegate (ConnectionId id) {
                Debug.Log("n2: on connection" + id.id);
            };

            n2.OnConnectionFail += delegate (ConnectionId id) {
                Debug.Log("n2: on connection fail" + id.id);
            };

            n2.OnConnectionEnd += delegate (ConnectionId id) {
                Debug.Log("n2: on disconnect" + id.id);
            };

            n2.OnServerDown += delegate () {
                Debug.Log("n2: on down");
            };

            n2.OnGetMessage += delegate (ConnectionId arg1, Packet arg2, bool arg3) {
                Debug.Log("n2 : message from " + arg1.id + " : " + arg2.Payload.ToUTF8String());
            };
        }

        [ContextMenu("start server2")]
        void startserver2() {
            n2.StartServer("A");
        }

        [ContextMenu("stop server2")]
        void stopserver2() {
            n2.StopServer();
        }

        [ContextMenu("connect 2")]
        void connect2() {
            n2.Connect("A");
        }

        [ContextMenu("dissconn 2")]
        void disconn2() {
            n2.Disconnect();
        }
        [ContextMenu("send 2")]
        void send2() {
            n2.Send(Packet.From(n2.Id).WithPayload("payload2"));
        }


        [ContextMenu("XXXX")]
        void xxxx() {
            Debug.Log("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx");
        }
    }
}
