using System;

namespace Adrenak.AirPeer {
    public class Core {
        // ENDIANNESS
        static bool preferLittleEndian = true;

        public static bool ReqEndianCorrection {
            get { return preferLittleEndian ^ BitConverter.IsLittleEndian; }
        }

        public static void EndianCorrection(byte[] bytes) {
            if (ReqEndianCorrection)
                Array.Reverse(bytes);
        }
    }
}