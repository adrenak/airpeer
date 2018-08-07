using System;
using System.Collections.Generic;

namespace Adrenak.AirPeer {
    public class PayloadWriter {
        List<byte> m_Bytes;
        
        public PayloadWriter() {
            m_Bytes = new List<byte>();
        }

        public byte[] Bytes {
            get { return m_Bytes.ToArray(); }
        }

        public void WriteShort(Int16 value, bool reverse = false) {
            var bytes = BitConverter.GetBytes(value);
            if (reverse) Array.Reverse(bytes);
            WriteBlock(bytes);
        }

        public void WriteInt(Int32 value, bool reverse = false) {
            var bytes = BitConverter.GetBytes(value);
            if (reverse) Array.Reverse(bytes);
            WriteBlock(bytes);
        }

        public void WriteLong(Int64 value, bool reverse = false) {
            var bytes = BitConverter.GetBytes(value);
            if (reverse) Array.Reverse(bytes);
            WriteBlock(bytes);
        }

        public void WriteFloat(Single value, bool reverse = false) {
            var bytes = BitConverter.GetBytes(value);
            if (reverse) Array.Reverse(bytes);
            WriteBlock(bytes);
        }

        public void WriteDouble(double value, bool reverse = false) {
            var bytes = BitConverter.GetBytes(value);
            if (reverse) Array.Reverse(bytes);
            WriteBlock(bytes);
        }

        public void WriteBlock(byte[] block) {
            foreach(var b in block)
                m_Bytes.Add(b);
        }
    }
}
