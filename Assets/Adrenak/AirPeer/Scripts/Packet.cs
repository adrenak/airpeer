using System;
using System.Text;

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

            // Receivers
            var receiverCount = reader.ReadShort();
            var receiversB = reader.ReadBlock(receiverCount * 2);
            var receivers = receiversB.ToShortArray();
            
            // Tag
            var tagLen = reader.ReadShort();
            var tagB = reader.ReadBlock(tagLen);
            var tag = Encoding.UTF8.GetString(tagB);
            
            // Payload
            var payload = reader.ReadBlock(bytes.Length - reader.index);

            packet.Receivers = receivers;
            packet.Tag = tag;
            packet.Payload = payload;

            return packet;
        }

        public byte[] Serialize() {
            PayloadWriter writer = new PayloadWriter();

            // Receivers
            writer.WriteShort((short)Receivers.Length);
            writer.WriteBlock(Receivers.ToByteArray());

            // tag
            writer.WriteShort((short)Tag.Length);
            writer.WriteBlock(Encoding.UTF8.GetBytes(Tag));

            // Payload
            writer.WriteBlock(Payload);

            return writer.Bytes;
        }
    }
}
