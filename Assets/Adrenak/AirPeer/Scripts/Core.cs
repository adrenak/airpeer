using System;

namespace Adrenak.AirPeer {
    public class Core {
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