using Adrenak.AirPeer;
using UnityEngine;

public class PayloadTest : MonoBehaviour {
    void Start() {
        GenericPayloadTest();
        CustomPayloadTest();
    }

    private void CustomPayloadTest() {
        Debug.Log("--------------------Custom Payload--------------------");

        // Populate a custom payload
        var p = new ExampleCustomPayload();
        p.position = new Vector3(0, 0, 0);
        p.eulerAngles = new Vector3(90, 90, 90);
        p.velocity = Vector3.up;

        // Create a new one using the bytes
        var bytes = p.GetBytes();
        var p2 = new ExampleCustomPayload();
        p2.SetBytes(bytes);
        
        // Show the values
        Debug.Log(p2.position);
        Debug.Log(p2.eulerAngles);
        Debug.Log(p2.velocity);
    }

    void GenericPayloadTest() {
        Debug.Log("--------------------Generic Payload--------------------");
        // Create a payload writer and write different values into it
        PayloadWriter w = PayloadWriter.New();
        w.WriteString("test");
        w.WriteShort(2);
        w.WriteInt(2);
        w.WriteLong(34);
        w.WriteFloat(4.2f);
        w.WriteDouble(435);
        w.WriteChar('a');
        w.WriteVector2(new Vector2(1, 1));
        w.WriteVector3(new Vector3(3, 3, 3));
        w.WriteRect(new Rect(0, 0, 1, 1));

        // Create a payload reader and read the values in the same order
        PayloadReader r = new PayloadReader(w.Bytes);
        Debug.Log(r.ReadString());
        Debug.Log(r.ReadShort());
        Debug.Log(r.ReadInt());
        Debug.Log(r.ReadLong());
        Debug.Log(r.ReadFloat());
        Debug.Log(r.ReadDouble());
        Debug.Log(r.ReadChar());
        Debug.Log(r.ReadVector2());
        Debug.Log(r.ReadVector3());
        Debug.Log(r.ReadRect());
    }
}
