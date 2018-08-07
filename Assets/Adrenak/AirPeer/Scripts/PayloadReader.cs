using System;

namespace Adrenak.AirPeer {
    public class PayloadReader {
        public int index;
        byte[] m_Payload;

        public PayloadReader(byte[] payload) {
            m_Payload = payload;
            index = 0;
        }

        public Int16 ReadShort(Int16 defaultValue = 0) {
            var result = BitConverter.ToInt16(m_Payload, index);
            index += 2;
            return result;
        }

        public Int32 ReadInt(Int32 defaultValue = 0) {
            var result = BitConverter.ToInt32(m_Payload, index);
            index += 4;
            return result;
        }

        public Int64 ReadLong(Int64 defaultValue = 0) {
            var result = BitConverter.ToInt64(m_Payload, index);
            index += 8;
            return result;
        }

        public float ReadFloat(float defaultValue = 0) {
            var result = BitConverter.ToSingle(m_Payload, index);
            index += 4;
            return result;
        }

        public double ReadDouble(double defaultValue = 0) {
            var result = BitConverter.ToDouble(m_Payload, index);
            index += 8;
            return result;
        }

        public byte[] ReadBlock(int length) {
            try {
                byte[] b = new byte[length];
                Buffer.BlockCopy(m_Payload, index, b, 0, length);
                index += length;
                return b;
            }
            catch {
                return null;
            }
        }
    }
}
