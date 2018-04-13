using Byn.Net;
using UnityEngine;
using AirPeer;

public class StreamDemo : MonoBehaviour {
    Node m_Server;
    Node m_Client;

	void Start () {
        Application.runInBackground = true;
        m_Server = Node.CreateInstance("Server");
        m_Server.StartNetwork();
        m_Server.StartServer("room-id", _ => {
            m_Client = Node.CreateInstance("Peer");
            m_Client.StartNetwork();
            m_Client.JoinServer("room-id");

            m_Client.OnGetUnreliableMessage += delegate (NetworkEvent obj) {
                Debug.Log(obj.GetDataAsString());
            };
        });
	}

    private void Update() {
        if (m_Server != null && m_Server.GetConnectionCount() > 0)
            m_Server.SendString("Frame : " + Time.frameCount, false);
    }
}
