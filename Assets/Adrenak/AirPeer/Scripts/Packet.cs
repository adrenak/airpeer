using System;

namespace Adrenak.AirPeer {
    public class Packet {
        public string Tag { get; private set; }
        public byte[] Payload { get; private set; }

        Packet() { }

        public Packet(string tag, byte[] payload) {
            // Ensure non null
            if (tag == null) tag = "";
            if (payload == null) payload = new byte[0];

            Tag = tag;
            Payload = payload;
        }

        public static Packet Deserialize(byte[] bytes) {
            var packet = new Packet();
            PayloadReader reader = new PayloadReader(bytes);
            packet.Tag = "";
            packet.Payload = new byte[0];

            try {
                packet.Tag = reader.ReadString();
                packet.Payload = reader.ReadBytes(bytes.Length - reader.index);
            }
            catch(Exception e) {
                UnityEngine.Debug.LogError("Packet deserializing error: " + e.Message);
                packet = null;
            }

            return packet;
        }

        public byte[] Serialize() {
            PayloadWriter writer = new PayloadWriter();

            writer.WriteString(Tag);
            writer.WriteBytes(Payload);

            return writer.Bytes;
        }
    }
}
