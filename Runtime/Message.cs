using System;
using System.Text;

namespace Adrenak.AirPeer {
    public class Message {
        public short sender;
        public short[] recipients;
        public byte[] bytes;

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

        public override string ToString() {
            StringBuilder sb = new StringBuilder("Packet:");
            sb.Append("\n");
            sb.Append("sender=").Append(sender).Append("\n");
            sb.Append("recipients={").Append(string.Join(", ", recipients)).Append("}").Append("\n");
            sb.Append("bytesLen=").Append(bytes.Length.ToString());
            return sb.ToString();
        }
    }
}