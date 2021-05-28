using System;
using System.Text;

namespace Adrenak.AirPeer {
    /// <summary>
    /// Data structure representing how messages are send in AirPeer.
    /// </summary>
    public class Message {
        /// <summary>
        /// ID of the sender of the message
        /// </summary>
        public short sender;

        /// <summary>
        /// IDs of the intended recipients of the message
        /// </summary>
        public short[] recipients;

        /// <summary>
        /// Byte array representing the data the message object contains
        /// </summary>
        public byte[] bytes;

        /// <summary>
        /// Constructs a <see cref="Message"/> from a byte array
        /// </summary>
        /// <param name="bytes">The byte array representing the <see cref="Message"/></param>
        /// <returns><see cref="Message"/> instance if deserialization was successful or null</returns>
        public static Message Deserialize(byte[] bytes) {
            BytesReader reader = new BytesReader(bytes);

            var message = new Message();
            var flag = reader.ReadString();
            if (flag.Equals("MESSAGE_DATA")) {
                try {
                    message.sender = reader.ReadShort();
                    message.recipients = reader.ReadShortArray();
                    message.bytes = reader.ReadByteArray();
                }
                catch (Exception e) {
                    UnityEngine.Debug.LogError("Message deserialization error: " + e.Message);
                    message = null;
                }
            }
            else
                message = null;

            return message;
        }

        /// <summary>
        /// Serializes <see cref="Message"/> instance into a byte array 
        /// </summary>
        /// <returns>Returns byte array if successful else throws exception</returns>
        public byte[] Serialize() {
            BytesWriter writer = new BytesWriter();
            try {
                writer.WriteString("MESSAGE_DATA");
                writer.WriteShort(sender);
                writer.WriteShortArray(recipients);
                writer.WriteByteArray(bytes);
                return writer.Bytes;
            }
            catch (Exception e) {
                UnityEngine.Debug.LogError("Message serialization error : " + e.Message);
                throw;
            }
        }

        /// <summary>
        /// Returns string representation of the instance
        /// </summary>
        /// <returns>The string representation</returns>
        public override string ToString() {
            StringBuilder sb = new StringBuilder("Packet:");
            sb.Append("\n");
            sb.Append("sender=").Append(sender).Append("\n");
            sb.Append("recipients={").Append(string.Join(", ", recipients)).Append("}").Append("\n");
            sb.Append("bytesLen=").Append(bytes.Length.ToString()).Append("\n");
            sb.Append("bytes=").Append(bytes);
            return sb.ToString();
        }
    }
}