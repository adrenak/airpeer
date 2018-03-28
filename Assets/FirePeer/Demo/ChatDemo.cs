using Byn.Net;
using System.Text;
using UnityEngine;
using System.Collections.Generic;

public class ChatDemo : MonoBehaviour {
    AirPeer[] m_Peers = new AirPeer[4];
    int index = -1;

    private void Update() {
        if (Input.GetKeyDown(KeyCode.S)) {
            index++;
            Starts();
        }
        if (Input.GetKeyDown(KeyCode.A)) 
            m_Peers[2].DisconnectFromServer();
    }

    private void Starts() {
        m_Peers[index] = AirPeer.CreateInstance((index).ToString());
        m_Peers[index].OnNetworkEvent += HandlePeerOnNetworkEvent;

        if(index == 0) {
            if (m_Peers[index].StartNetwork())
                m_Peers[index].StartServer("test-chat", _ => {});
        }
        else {
            if (m_Peers[index].StartNetwork())
                m_Peers[index].JoinServer("test-chat", _ => { });
        }
    }

    private void HandlePeerOnNetworkEvent(AirPeer sender, NetworkEvent netEvent) {
        Debug.Log("Message from " + netEvent.ConnectionId + " to " + sender.name + " => " + netEvent.GetMessageAsString());
    }
}
