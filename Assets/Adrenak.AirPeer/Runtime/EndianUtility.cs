using System;

namespace Adrenak.AirPeer {
	public class EndianUtility {
		const bool UseLittleEndian = true;

		public static bool RequiresEndianCorrection {
			get { return UseLittleEndian ^ BitConverter.IsLittleEndian; }
		}

		public static void EndianCorrection(byte[] bytes) {
			if (RequiresEndianCorrection)
				Array.Reverse(bytes);
		}
	}
}