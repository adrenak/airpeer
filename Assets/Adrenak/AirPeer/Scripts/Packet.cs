using System;

namespace Adrenak.AirPeer {
    public class Packet {
        public Int16[] Receivers { get; private set; }
        public string Tag { get; private set; }
        public byte[] Payload { get; private set; }

        Packet() { }

        public Packet(Int16[] receivers, string tag, byte[] payload) {
            Receivers = receivers;
            Tag = tag;
            Payload = payload;
        }

        public static Packet Deserialize(byte[] bytes) {
            var packet = new Packet();
            PayloadReader reader = new PayloadReader(bytes);

            packet.Receivers = reader.ReadShortArray();
            packet.Tag = reader.ReadString();
            packet.Payload = reader.ReadBytes(bytes.Length - reader.index);

            return packet;
        }

        public byte[] Serialize() {
            PayloadWriter writer = new PayloadWriter();

            writer.WriteShortArray(Receivers);
            writer.WriteString(Tag);
            writer.WriteBytes(Payload);

            return writer.Bytes;
        }
    }
}
