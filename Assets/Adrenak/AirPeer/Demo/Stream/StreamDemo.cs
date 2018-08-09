using Byn.Net;
using UnityEngine;
using Adrenak.AirPeer;

public class StreamDemo : MonoBehaviour {
    Node m_Host;
    Node m_Client;

	void Start () {
        Application.runInBackground = true;

        // Create a host peer
        m_Host = Node.Create("Server");
        if (!m_Host.Init()) {
            Debug.Log("Could not start network");
            return;
        }
        m_Host.OnServerStart += delegate () {
            m_Client = Node.Create("Client");
            m_Client.Init();
            m_Client.Connect("server-name");
            
            m_Client.OnGetUnreliableMessage += delegate (NetworkEvent obj) {
                Debug.Log("Message received at client: " + obj.GetDataAsString());
            };
        };

        m_Host.OnServerFail += delegate() {
            Debug.LogError("Could not start server");
        };

        m_Host.OnServerStop += delegate () {
            Debug.Log("Server stopped");
        };
        m_Host.StartServer("server-name");
	}
    
    private void Update() {
        if (m_Host != null && m_Host.ConnectionIds.Count > 0) {
            var msg = "Message from host to client: " + Time.frameCount;
            Debug.Log(msg);
            
        }
    }
}
