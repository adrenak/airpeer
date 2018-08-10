using UnityEngine;
using Adrenak.AirPeer;

public class ExampleCustomPayload : IPayload {
    public Vector3 position;
    public Vector3 eulerAngles;
    public Vector3 velocity;

    public byte[] GetBytes() {
        var writer = PayloadWriter.New();
        writer.WriteVector3(position);
        writer.WriteVector3(eulerAngles);
        writer.WriteVector3(velocity);

        return writer.Bytes;
    }

    public void SetBytes(byte[] bytes) {
        var reader = new PayloadReader(bytes);
        position = reader.ReadVector3();
        eulerAngles = reader.ReadVector3();
        velocity = reader.ReadVector3();
    }
}
