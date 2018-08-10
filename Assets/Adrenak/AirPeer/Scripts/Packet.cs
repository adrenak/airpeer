using Byn.Net;
using System;
using System.Text;
using System.Linq;

namespace Adrenak.AirPeer {
    public class Packet {
        public short Sender { get; private set; }
        public short[] Receivers { get; private set; }
        public string Tag { get; private set; }
        public byte[] Payload { get; private set; }

        public bool IsToAll {
            get { return Receivers == null || Receivers.Length == 0; }
        }

        public bool HasNoTag {
            get { return Tag == string.Empty; }
        }

        public bool HasNoPayload {
            get { return Payload.Length == 0; }
        }

        Packet() {
            Sender = -1;
            Receivers = new short[0];
            Tag = string.Empty;
            Payload = new byte[0];
        }

        // ================================================
        // OBJECT BUILDER
        // ================================================
        [Obsolete("Avoid using this. Not yet determined if feature this is a good idea")]
        public static Packet From(Node node) {
            return From(node.Id);
        }

        public static Packet From(ConnectionId cid) {
            return From(cid.id);
        }

        public static Packet From(short sender) {
            Packet cted = new Packet();
            cted.Sender = sender;
            return cted;
        }

        public Packet To(ConnectionId receiver) {
            return To(new[] { receiver.id });
        }

        public Packet To(short receiver) {
            return To(new[] { receiver });
        }

        public Packet To(ConnectionId[] receivers) {
            if (receivers == null) receivers = new ConnectionId[0];
            Receivers = receivers.Select(x => x.id).ToList().ToArray();
            return this;
        }

        public Packet To(short[] receivers) {
            if (receivers == null) receivers = new short[0];
            Receivers = receivers;
            return this;
        }

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

        // ================================================
        // (DE)SERIALIZATION
        // ================================================
        public static Packet Deserialize(byte[] bytes) {
            PayloadReader reader = new PayloadReader(bytes);

            var packet = new Packet();
            try {
                packet.Sender = reader.ReadShort();
                packet.Receivers = reader.ReadShortArray();
                packet.Tag = reader.ReadString();
                packet.Payload = reader.ReadBytes(bytes.Length - reader.index);
            }
            catch(Exception e) {
                UnityEngine.Debug.LogError("Packet deserialization error: " + e.Message);
                packet = null;
            }

            return packet;
        }

        public byte[] Serialize() {
            PayloadWriter writer = PayloadWriter.New();
            try {
                writer.WriteShort(Sender);
                writer.WriteShortArray(Receivers);
                writer.WriteString(Tag);
                writer.WriteBytes(Payload);
            }
            catch (Exception e) {
                UnityEngine.Debug.LogError("Packet serialization error : " + e.Message);
            }

            return writer.Bytes;
        }
    }
}
