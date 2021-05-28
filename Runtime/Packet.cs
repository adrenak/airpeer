using Byn.Net;
using System;
using System.Text;
using System.Linq;

namespace Adrenak.AirPeer {
    public class Packet {
        public struct ReservedTags {
            public const string ServerClosed = "reserved.server.stopped";
            public const string ClientJoined = "reserved.client.joined";
            public const string ClientLeft = "reserved.client.left";
            public const string ClientSetID = "reserved.client.setID";
            public const string PacketForwarding = "reserved.packet.forwarded";
        }

        public string Tag { get; private set; }
        public byte[] Payload { get; private set; }

        public bool HasNoTag {
            get { return Tag == string.Empty; }
        }

        public bool HasNoPayload {
            get { return Payload.Length == 0; }
        }

        public Packet() {
            Tag = string.Empty;
            Payload = new byte[0];
        }

        // ================================================
        // OBJECT BUILDER
        // ================================================
        public Packet With(string tag, string payload) {
            return WithTag(tag).WithPayload(payload);
        }

        public Packet With(string tag, byte[] payload) {
            return WithTag(tag).WithPayload(payload);
        }

        public Packet WithTag(string tag) {
            SetTag(tag);
            return this;
        }

        public Packet WithPayload(string payload) {
            SetPayloadString(payload);
            return this;
        }

        public Packet WithPayload(byte[] payload) {
            SetPayloadBytes(payload);
            return this;
        }

        void SetTag(string tag) {
            if (string.IsNullOrEmpty(tag))
                tag = string.Empty;
            Tag = tag;
        }

        void SetPayloadString(string payload) {
            if (string.IsNullOrEmpty(payload))
                payload = string.Empty;
            Payload = Encoding.UTF8.GetBytes(payload);
        }

        void SetPayloadBytes(byte[] payload) {
            if (payload == null || payload.Length == 0)
                payload = new byte[0];
            Payload = payload;
        }

        public static Packet Deserialize(byte[] bytes) {
            BytesReader reader = new BytesReader(bytes);

            var flag = reader.ReadString();
            var packet = new Packet();
            if (flag.Equals("PACKET_DATA")) {
                try {
                    packet.Tag = reader.ReadString();
                    packet.Payload = reader.ReadBytes(bytes.Length - reader.Index);
                }
                catch (Exception e) {
                    UnityEngine.Debug.LogError("Packet deserialization error: " + e.Message);
                    packet = null;
                }
            }
            else
                packet = null;

            return packet;
        }

        public byte[] Serialize() {
            try {
                BytesWriter writer = new BytesWriter();
                writer.WriteString("PACKET_DATA");
                writer.WriteString(Tag);
                writer.WriteBytes(Payload);
                return writer.Bytes;                
            }
            catch (Exception e) {
                UnityEngine.Debug.LogError("Packet serialization error : " + e.Message);
                throw;
            }
        }

        public override string ToString() {
            StringBuilder sb = new StringBuilder("Packet:");
            sb.Append("\n");
            sb.Append("Tag=").Append(Tag).Append("\n")
                .Append("PayloadLength=").Append(Payload.Length);
            return sb.ToString();
        }
    }
}
