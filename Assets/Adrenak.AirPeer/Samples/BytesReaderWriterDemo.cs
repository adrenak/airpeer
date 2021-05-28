using UnityEngine;

namespace Adrenak.AirPeer.Samples {
	public class BytesReaderWriterDemo : MonoBehaviour {
		void Start() {
			var writer = new BytesWriter();
			writer.WriteInt(90);
			writer.WriteString("Vatsal");
			writer.WriteString("Adrenak");
			writer.WriteString("Ambastha");
			writer.WriteVector3(Vector3.one);

			var reader = new BytesReader(writer.Bytes);

			// READ IN THE SAME ORDER
			Debug.Log(reader.ReadInt());
			Debug.Log(reader.ReadString());
			Debug.Log(reader.ReadString());
			Debug.Log(reader.ReadString());
			Debug.Log(reader.ReadVector3());
		}
	}
}
