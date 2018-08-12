using System;
using System.Text;
using UnityEngine;
using System.Collections.Generic;

namespace Adrenak.AirPeer {
    public class PayloadWriter {
        List<byte> m_Bytes;

        PayloadWriter() {
            m_Bytes = new List<byte>();
        }

        public static PayloadWriter New() {
            return new PayloadWriter();
        }
        
        public byte[] Bytes {
            get { return m_Bytes.ToArray(); }
        }

        // Default types
        public PayloadWriter WriteShort(Int16 value) {
            var bytes = BitConverter.GetBytes(value);
            Core.EndianCorrection(bytes);
            WriteBytes(bytes);
            return this;
        }

        public PayloadWriter WriteInt(Int32 value) {
            var bytes = BitConverter.GetBytes(value);
            Core.EndianCorrection(bytes);
            WriteBytes(bytes);
            return this;
        }

        public PayloadWriter WriteLong(Int64 value) {
            var bytes = BitConverter.GetBytes(value);
            Core.EndianCorrection(bytes);
            WriteBytes(bytes);
            return this;
        }

        public PayloadWriter WriteFloat(Single value) {
            var bytes = BitConverter.GetBytes(value);
            Core.EndianCorrection(bytes);
            WriteBytes(bytes);
            return this;
        }

        public PayloadWriter WriteDouble(double value) {
            var bytes = BitConverter.GetBytes(value);
            Core.EndianCorrection(bytes);
            WriteBytes(bytes);
            return this;
        }

        public PayloadWriter WriteChar(char val) {
            WriteBytes(BitConverter.GetBytes(val));
            return this;
        }

        public PayloadWriter WriteShortArray(Int16[] array) {
            WriteInt(array.Length);

            foreach (var e in array)
                WriteShort(e);
            return this;
        }

        public PayloadWriter WriteIntArray(Int32[] array) {
            WriteInt(array.Length);

            foreach (var e in array)
                WriteInt(e);
            return this;
        }

        public PayloadWriter WriteLongArray(Int64[] array) {
            WriteInt(array.Length);

            foreach (var e in array)
                WriteLong(e);
            return this;
        }

        public PayloadWriter WriteFloatArray(Single[] array) {
            WriteInt(array.Length);

            foreach (var e in array)
                WriteFloat(e);
            return this;
        }


        public PayloadWriter WriteDoubleArray(Double[] array) {
            WriteInt(array.Length);

            foreach (var e in array)
                WriteDouble(e);
            return this;
        }

        public PayloadWriter WriteString(string str) {
            var strB = Encoding.UTF8.GetBytes(str);
            WriteInt(strB.Length);
            WriteBytes(strB);
            return this;
        }

        // Unity types
        public PayloadWriter WriteVector3(Vector3 value) {
            var xbytes = BitConverter.GetBytes(value.x);
            var ybytes = BitConverter.GetBytes(value.y);
            var zbytes = BitConverter.GetBytes(value.z);

            Core.EndianCorrection(xbytes);
            Core.EndianCorrection(ybytes);
            Core.EndianCorrection(zbytes);
            
            WriteBytes(xbytes);
            WriteBytes(ybytes);
            WriteBytes(zbytes);

            return this;
        }

        public PayloadWriter WriteVector3Array(Vector3[] array) {
            var lenB = BitConverter.GetBytes(array.Length);
            Core.EndianCorrection(lenB);
            WriteBytes(lenB);

            foreach (var e in array)
                WriteVector3(e);

            return this;
        }

        public PayloadWriter WriteVector2(Vector2 value) {
            var xbytes = BitConverter.GetBytes(value.x);
            var ybytes = BitConverter.GetBytes(value.y);

            Core.EndianCorrection(xbytes);
            Core.EndianCorrection(ybytes);

            WriteBytes(xbytes);
            WriteBytes(ybytes);

            return this;
        }

        public PayloadWriter WriteVector2Array(Vector2[] array) {
            var lenB = BitConverter.GetBytes(array.Length);
            Core.EndianCorrection(lenB);
            WriteBytes(lenB);

            foreach (var e in array)
                WriteVector2(e);

            return this;
        }

        public PayloadWriter WriteRect(Rect rect) {
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

            return this;
        }

        public PayloadWriter WriteRectArray(Rect[] array) {
            var lenB = BitConverter.GetBytes(array.Length);
            Core.EndianCorrection(lenB);
            WriteBytes(lenB);

            foreach (var e in array)
                WriteRect(e);

            return this;
        }

        public PayloadWriter WriteColor32(Color32 color) {
            WriteByte(color.r);
            WriteByte(color.g);
            WriteByte(color.b);
            WriteByte(color.a);
            return this;
        }

        public PayloadWriter WriteColor32Array(Color32[] array) {
            WriteInt(array.Length);

            for (int i = 0; i < array.Length; i++)
                WriteColor32(array[i]);
            return this;
        }

        public PayloadWriter WriteColor(Color color) {
            WriteFloat(color.r);
            WriteFloat(color.g);
            WriteFloat(color.b);
            WriteFloat(color.a);
            return this;
        }

        public PayloadWriter WriteColorArray(Color[] array) {
            WriteInt(array.Length);

            for (int i = 0; i < array.Length; i++)
                WriteColor(array[i]);
            return this;
        }

        public void WriteBytes(byte[] block) {
            foreach (var b in block)
                m_Bytes.Add(b);
        }

        public void WriteByte(byte b) {
            m_Bytes.Add(b);
        }
    }
}
