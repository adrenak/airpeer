using Adrenak.BRW;

using UnityEngine;

namespace Adrenak.AirPeer.Samples {
    /// <summary>
    /// A demo showing writing and reading of data 
    /// using <see cref="BytesWriter"/> and <see cref="BytesReader"/>
    /// </summary>
    public class BytesReaderWriterDemo : MonoBehaviour {
        void Start() {
            var writer = new BytesWriter();
            writer.WriteInt(90);
            writer.WriteString("Vatsal");
            writer.WriteString("Adrenak");
            writer.WriteString("Ambastha");
            writer.WriteVector3(Vector3.one);
            writer.WriteFloatArray(new[] { 1.1f, 2.2f, 3.3f });

            var reader = new BytesReader(writer.Bytes);

            // READ IN THE SAME ORDER
            Debug.Log(reader.ReadInt());
            Debug.Log(reader.ReadString());
            Debug.Log(reader.ReadString());
            Debug.Log(reader.ReadString());
            Debug.Log(reader.ReadVector3());
            Debug.Log("Floats : " + string.Join(", ", reader.ReadFloatArray()));
        }
    }
}
