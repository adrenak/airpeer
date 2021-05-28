using System;
using System.Text;

namespace Adrenak.AirPeer {
    /// <summary>
    /// Wraps a byte[] to provide a higher level object representing 
    /// network data.
    /// </summary>
    public class Packet {
        /// <summary>
        /// A collection of tags that AirPeer uses for internal 
        /// communication within different nodes. DO NOT change these.
        /// </summary>
        public struct ReservedTags {
            public const string ServerClosed = "reserved.server.stopped";
            public const string ClientJoined = "reserved.client.joined";
            public const string ClientLeft = "reserved.client.left";
            public const string ClientSetID = "reserved.client.setID";
        }

        /// <summary>
        /// A string tag that can be used to cetegorize or identify the packet.
        /// </summary>
        public string Tag { get; private set; }

        /// <summary>
        /// The actual byte[] data that the packet represents
        /// </summary>
        public byte[] Payload { get; private set; }

        /// <summary>
        /// Whether the packet has a tag or not
        /// </summary>
        public bool HasNoTag => Tag == string.Empty;

        /// <summary>
        /// Whether the packet contains any payload data or not
        /// </summary>
        public bool HasNoPayload => Payload.Length == 0;

        /// <summary>
        /// Constructs a new packet with no tag and no payload data.
        /// </summary>
        public Packet() {
            Tag = string.Empty;
            Payload = new byte[0];
        }

        // ================================================
        // OBJECT BUILDER
        // ================================================
        /// <summary>
        /// A builder method that allows setting tag and payload
        /// </summary>
        /// <param name="tag">The tag to be set</param>
        /// <param name="payload">The payload data to be set in the form of a string</param>
        /// <returns>Returns itself for builder method chaining</returns>
        public Packet With(string tag, string payload) =>
            WithTag(tag).WithPayload(payload);

        /// <summary>
        /// A builder method that allows setting tag and pyaload
        /// </summary>
        /// <param name="tag">The tag to be set</param>
        /// <param name="payload">THe payload data to be set in the form of a byte array</param>
        /// <returns>Returns itself for builder method chaining</returns>
        public Packet With(string tag, byte[] payload) =>
            WithTag(tag).WithPayload(payload);

        /// <summary>
        /// A builder method that allows setting the tag
        /// </summary>
        /// <param name="tag">The tag to be set</param>
        /// <returns>Returns itself for builder method chaining</returns>
        public Packet WithTag(string tag) {
            SetTag(tag);
            return this;
        }

        /// <summary>
        /// A builder method that allows setting the payload
        /// </summary>
        /// <param name="payload">The payload to be set in the form of a string</param>
        /// <returns>Returns itself for builder method chaining</returns>
        public Packet WithPayload(string payload) {
            SetPayloadString(payload);
            return this;
        }

        /// <summary>
        /// A builder method that allows setting the payload
        /// </summary>
        /// <param name="payload">The payload to be set in the form of a byte array</param>
        /// <returns>Returns itself for builder method chaining</returns>
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

        /// <summary>
        /// Constructs a <see cref="Packet"/> from a byte array
        /// </summary>
        /// <param name="bytes">The byte array representing the <see cref="Packet"/></param>
        /// <returns><see cref="Packet"/> instance if deserialization was successful or null</returns>
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

        /// <summary>
        /// Serializes <see cref="Packet"/> instance into a byte array 
        /// </summary>
        /// <returns>Returns byte array if successful else throws exception</returns>
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

        /// <summary>
        /// Returns string representation of the instance
        /// </summary>
        /// <returns>The string representation</returns>
        public override string ToString() {
            try {
                StringBuilder sb = new StringBuilder("Packet:");
                sb.Append("\n");
                sb.Append("Tag=").Append(Tag).Append("\n")
                    .Append("PayloadLength=").Append(Payload.Length).Append("\n")
                    .Append("Payload=").Append(Payload).Append("\n");
                return sb.ToString();
            }
            catch { return base.ToString(); }
        }
    }
}
