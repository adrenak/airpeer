using System;

namespace Adrenak.AirPeer {
	/// <summary>
	/// The utility used by <see cref="BytesReader"/> and
	/// <see cref="BytesWriter"/> for reading and writing bytes.
	/// </summary>
	public static class EndianUtility {
		/// <summary>
		/// Whether the big or the little Endian is used.
		/// Default is Little Endian
		/// There is little reason to change this. 
		/// Don't do so unless you're sure what you're doing.
		/// </summary>
		static bool UseLittleEndian = true;

		public static bool RequiresEndianCorrection {
			get { return UseLittleEndian ^ BitConverter.IsLittleEndian; }
		}

		public static void EndianCorrection(byte[] bytes) {
			if (RequiresEndianCorrection)
				Array.Reverse(bytes);
		}
	}
}