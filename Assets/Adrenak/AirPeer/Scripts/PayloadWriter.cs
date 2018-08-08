using System;
using System.Text;
using UnityEngine;
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

        // Default types
        public void WriteShort(Int16 value) {
            var bytes = BitConverter.GetBytes(value);
            Core.EndianCorrection(bytes);
            WriteBytes(bytes);
        }

        public void WriteShortArray(Int16[] array) {
            var lenB = BitConverter.GetBytes(array.Length);
            Core.EndianCorrection(lenB);
            WriteBytes(lenB);

            foreach(var e in array) {
                var rB = BitConverter.GetBytes(e);
                Core.EndianCorrection(rB);
                WriteBytes(rB);
            }
        }

        public void WriteInt(Int32 value) {
            var bytes = BitConverter.GetBytes(value);
            Core.EndianCorrection(bytes);
            WriteBytes(bytes);
        }

        public void WriteIntArray(Int32[] array) {
            var lenB = BitConverter.GetBytes(array.Length);
            Core.EndianCorrection(lenB);
            WriteBytes(lenB);

            foreach (var e in array) {
                var rB = BitConverter.GetBytes(e);
                Core.EndianCorrection(rB);
                WriteBytes(rB);
            }
        }

        public void WriteLong(Int64 value) {
            var bytes = BitConverter.GetBytes(value);
            Core.EndianCorrection(bytes);
            WriteBytes(bytes);
        }

        public void WriteLongArray(Int64[] array) {
            var lenB = BitConverter.GetBytes(array.Length);
            Core.EndianCorrection(lenB);
            WriteBytes(lenB);

            foreach (var e in array) {
                var rB = BitConverter.GetBytes(e);
                Core.EndianCorrection(rB);
                WriteBytes(rB);
            }
        }

        public void WriteFloat(Single value) {
            var bytes = BitConverter.GetBytes(value);
            Core.EndianCorrection(bytes);
            WriteBytes(bytes);
        }

        public void WriteFloatArray(Single[] array) {
            var lenB = BitConverter.GetBytes(array.Length);
            Core.EndianCorrection(lenB);
            WriteBytes(lenB);

            foreach (var e in array) {
                var rB = BitConverter.GetBytes(e);
                Core.EndianCorrection(rB);
                WriteBytes(rB);
            }
        }

        public void WriteDouble(double value) {
            var bytes = BitConverter.GetBytes(value);
            Core.EndianCorrection(bytes);
            WriteBytes(bytes);
        }

        public void WriteDoubleArray(Double[] array) {
            var lenB = BitConverter.GetBytes(array.Length);
            Core.EndianCorrection(lenB);
            WriteBytes(lenB);

            foreach (var e in array) {
                var rB = BitConverter.GetBytes(e);
                Core.EndianCorrection(rB);
                WriteBytes(rB);
            }
        }

        public void WriteChar(char val) {
            WriteBytes(BitConverter.GetBytes(val));
        }

        public void WriteString(string str) {
            var strB = Encoding.UTF8.GetBytes(str);
            WriteInt(strB.Length);
            WriteBytes(strB);
        }

        // Unity types
        public void WriteVector3(Vector3 value) {
            var xbytes = BitConverter.GetBytes(value.x);
            var ybytes = BitConverter.GetBytes(value.y);
            var zbytes = BitConverter.GetBytes(value.z);

            Core.EndianCorrection(xbytes);
            Core.EndianCorrection(ybytes);
            Core.EndianCorrection(zbytes);
            
            WriteBytes(xbytes);
            WriteBytes(ybytes);
            WriteBytes(zbytes);
        }

        public void WriteVector3Array(Vector3[] array) {
            var lenB = BitConverter.GetBytes(array.Length);
            Core.EndianCorrection(lenB);
            WriteBytes(lenB);

            foreach (var e in array)
                WriteVector3(e);
        }

        public void WriteVector2(Vector2 value) {
            var xbytes = BitConverter.GetBytes(value.x);
            var ybytes = BitConverter.GetBytes(value.y);

            Core.EndianCorrection(xbytes);
            Core.EndianCorrection(ybytes);

            WriteBytes(xbytes);
            WriteBytes(ybytes);
        }

        public void WriteVector2Array(Vector2[] array) {
            var lenB = BitConverter.GetBytes(array.Length);
            Core.EndianCorrection(lenB);
            WriteBytes(lenB);

            foreach (var e in array)
                WriteVector2(e);
        }

        public void WriteRect(Rect rect) {
            var xbytes = BitConverter.GetBytes(rect.x);
            var ybytes = BitConverter.GetBytes(rect.y);
            var wbytes = BitConverter.GetBytes(rect.width);
            var hbytes = BitConverter.GetBytes(rect.height);

            Core.EndianCorrection(xbytes);
            Core.EndianCorrection(ybytes);
            Core.EndianCorrection(wbytes);
            Core.EndianCorrection(hbytes);

            WriteBytes(xbytes);
            WriteBytes(ybytes);
            WriteBytes(wbytes);
            WriteBytes(hbytes);
        }

        public void WriteRectArray(Rect[] array) {
            var lenB = BitConverter.GetBytes(array.Length);
            Core.EndianCorrection(lenB);
            WriteBytes(lenB);

            foreach (var e in array)
                WriteRect(e);
        }

        public void WriteBytes(byte[] block) {
            foreach (var b in block)
                m_Bytes.Add(b);
        }
    }
}
