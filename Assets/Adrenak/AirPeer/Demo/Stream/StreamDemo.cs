using Byn.Net;
using UnityEngine;
using Adrenak.AirPeer;

public class StreamDemo : MonoBehaviour {
    Node host;
    Node client;

	void Start () {
        Application.runInBackground = true;

        // Create a host peer
        host = Node.Create("Server");
        if (!host.Init()) {
            Debug.Log("Could not start network");
            return;
        }
        host.OnServerStart += delegate () {
            client = Node.Create("Client");
            client.Init();
            client.Connect("server-name");

            client.OnGetMessage += (e, p, r) => {
                Debug.Log("Message received at client: " + p.Payload.ToUTF8String());
            };
        };

        host.OnServerFail += delegate() {
            Debug.LogError("Could not start server");
        };

        host.OnServerStop += delegate () {
            Debug.Log("Server stopped");
        };
        host.StartServer("server-name");
	}
    
    private void Update() {
        if (host != null && host.ConnectionIds.Count > 0) {
            var msg = "Message from host to client: " + Time.frameCount;
            Debug.Log(msg);
            
        }
    }
}
