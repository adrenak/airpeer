using System;
using Byn.Net;
using System.Text;

namespace Adrenak.AirPeer {
    public static class Extensions {
        public static string GetDataAsString(this NetworkEvent netEvent) {
            if (netEvent.MessageData == null) return netEvent.Type.ToString();
            MessageDataBuffer buffer = netEvent.MessageData;
            string msg = Encoding.UTF8.GetString(buffer.Buffer, 0, buffer.ContentLength);
            buffer.Dispose();
            return msg;
        }

        public static string GetUTF8String(this byte[] bytes) {
            return Encoding.UTF8.GetString(bytes);
        }

        // ================================================
        // TO BYTE ARRAY
        // ================================================
        public static byte[] ToByteArray(this short[] arr, bool reverse = false) {
            int size = 2;
            int len = arr.Length * size;
            byte[] result = new byte[len];

            for(int i = 0; i < result.Length; i += size) {
                byte[] data = BitConverter.GetBytes(arr[i / size]);
                if (reverse) Array.Reverse(data);
                Array.Copy(data, 0, result, i, size);
            }
            return result;
        }

        public static byte[] ToByteArray(this int[] arr, bool reverse = false) {
            int size = 4;
            int len = arr.Length * size;
            byte[] result = new byte[len];

            for (int i = 0; i < result.Length; i += size) {
                byte[] data = BitConverter.GetBytes(arr[i / size]);
                if (reverse) Array.Reverse(data);
                Array.Copy(data, 0, result, i, size);
            }
            return result;
        }

        public static byte[] ToByteArray(this long[] arr, bool reverse = false) {
            int size = 8;
            int len = arr.Length * size;
            byte[] result = new byte[len];

            for (int i = 0; i < result.Length; i += size) {
                byte[] data = BitConverter.GetBytes(arr[i / size]);
                if (reverse) Array.Reverse(data);
                Array.Copy(data, 0, result, i, size);
            }
            return result;
        }

        public static byte[] ToByteArray(this float[] arr, bool reverse = false) {
            int size = 4;
            int len = arr.Length * size;
            byte[] result = new byte[len];

            for (int i = 0; i < result.Length; i += size) {
                byte[] data = BitConverter.GetBytes(arr[i / size]);
                if (reverse) Array.Reverse(data);
                Array.Copy(data, 0, result, i, size);
            }
            return result;
        }

        // ================================================
        // FROM BYTE ARRAY
        // ================================================
        public static double[] ToDoubleArray(this byte[] arr, bool reverse = false) {
            int size = 8;
            if (reverse) Array.Reverse(arr);

            int len = arr.Length / size;
            var result = new double[len];
            for (int i = 0; i < arr.Length; i += size) {
                result[i / size] = BitConverter.ToDouble(arr, i);
            }
            return result;
        }

        public static short[] ToShortArray(this byte[] arr, bool reverse = false) {
            int size = 2;
            if (reverse) Array.Reverse(arr);

            int len = arr.Length / size;
            var result = new short[len];
            for (int i = 0; i < arr.Length; i += size) {
                result[i / size] = BitConverter.ToInt16(arr, i);
            }
            return result;
        }

        public static int[] ToIntArray(this byte[] arr, bool reverse = false) {
            int size = 4;
            if (reverse) Array.Reverse(arr);

            int len = arr.Length / size;
            var result = new int[len];
            for (int i = 0; i < arr.Length; i += size) {
                result[i / size] = BitConverter.ToInt32(arr, i);
            }
            return result;
        }

        public static long[] ToLongArray(this byte[] arr, bool reverse = false) {
            int size = 8;
            if (reverse) Array.Reverse(arr);

            int len = arr.Length / size;
            var result = new long[len];
            for (int i = 0; i < arr.Length; i += size) {
                result[i / size] = BitConverter.ToInt64(arr, i);
            }
            return result;
        }

        public static float[] ToFloatArray(this byte[] arr, bool reverse = false) {
            int size = 4;
            if (reverse) Array.Reverse(arr);

            int len = arr.Length / size;
            var result = new float[len];
            for (int i = 0; i < arr.Length; i += size) {
                result[i / size] = BitConverter.ToSingle(arr, i);
            }
            return result;
        }

        public static byte[] ToByteArray(this double[] arr, bool reverse = false) {
            int size = 8;
            int len = arr.Length * size;
            byte[] result = new byte[len];

            for (int i = 0; i < result.Length; i += size) {
                byte[] data = BitConverter.GetBytes(arr[i / size]);
                if (reverse) Array.Reverse(data);
                Array.Copy(data, 0, result, i, size);
            }
            return result;
        }

        public static void TryInvoke(this Action action) {
            if (action != null) action();
        }

        public static void TryInvoke<T>(this Action<T> action, T param) {
            if (action != null) action(param);
        }
    }
}
