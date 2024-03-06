using System;
using System.Text;

using Adrenak.BRW;

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
        /// <param name="bytes">The byte array representing the data</param>
        /// <returns>
        /// <see cref="Message"/> if deserialization was successful, else null
        /// </returns>
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
                    var msg = "Message deserialization error: " + e.Message;
                    UnityEngine.Debug.LogError(msg);
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
        /// <returns>Returns byte array if successful else exception</returns>
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
                var msg = "Message serialization error : " + e.Message;
                UnityEngine.Debug.LogError(msg);
                throw;
            }
        }

        /// <summary>
        /// Returns string representation of the instance
        /// </summary>
        /// <returns>The string representation</returns>
        public override string ToString() {
            StringBuilder sb = new StringBuilder("Message:\n");
            sb.Append("sender=").Append(sender).Append("\n");
            var recipientsJoined = string.Join(", ", recipients);
            sb.Append("recipients={").Append(recipientsJoined).Append("}\n");
            sb.Append("bytesLen=").Append(bytes.Length).Append("\n");
            sb.Append("bytes=").Append(BitConverter.ToString(bytes));
            return sb.ToString();
        }
    }
}