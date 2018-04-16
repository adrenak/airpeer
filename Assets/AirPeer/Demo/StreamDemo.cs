using Byn.Net;
using UnityEngine;
using AirPeer;

public class StreamDemo : MonoBehaviour {
    Peer m_Host;
    Peer m_Client;

	void Start () {
        Application.runInBackground = true;

        // Create a host peer
        m_Host = Peer.CreateInstance("Server");
        m_Host.StartNetwork();
        m_Host.StartServer("server-name", _ => {
            // WHen the host peer starts the server, start the client peer and listen to the messages
            m_Client = Peer.CreateInstance("Client");
            m_Client.StartNetwork();
            m_Client.ConnectToServer("server-name");

            m_Client.OnGetUnreliableMessage += delegate (NetworkEvent obj) {
                Debug.Log(obj.GetDataAsString());
            };
        });
	}

    private void Update() {
        if (m_Host != null && m_Host.GetConnectionCount() > 0)
            m_Host.SendString("Frame : " + Time.frameCount, false);
    }
}
