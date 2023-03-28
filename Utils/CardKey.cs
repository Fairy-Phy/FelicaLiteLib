using System;

namespace FelicaLiteLib.Utils {

	/// <summary>
	/// CKの生成クラス
	/// </summary>
	public static class CardKey {

		static readonly byte[] PlaneText = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

		/// <summary>
		/// CKを生成します
		/// </summary>
		/// <param name="K"></param>
		/// <param name="M"></param>
		/// <returns></returns>
		public static byte[] Generate(byte[] K, byte[] M) {
			byte[] KA, KB, KC;
			KA = new byte[8];
			KB = new byte[8];
			KC = new byte[8];
			int i = 0;
			for (int j = 0; j < 8; j++, i++)
				KA[j] = K[i];
			for (int j = 0; j < 8; j++, i++)
				KB[j] = K[i];
			for (int j = 0; j < 8; j++, i++)
				KC[j] = K[i];

			byte[] C = new byte[16];
			using (TripleDES DES = new TripleDES(KA, KB, KC)) {
				ulong L = BitConverter.ToUInt64(DES.Encrypto(PlaneText), 0);

				ulong K1;
				if (((L & (1ul << 63)) >> 63) == 1) { // 最上位bitが1
					K1 = L << 1;
					K1 ^= 0x1b;
				}
				else {
					K1 = L << 1;
				}

				ulong M1 = 0, M2 = 0;
				for (i = 0; i < 4; i++) {
					M1 <<= 8;
					M1 |= M[i];
				}
				for (; i < 8; i++) {
					M2 <<= 8;
					M2 |= M[i];
				}
				M2 ^= K1;

				ulong C1 = BitConverter.ToUInt64(DES.Encrypto(BitConverter.GetBytes(M1)), 0);

				byte[] T = DES.Encrypto(BitConverter.GetBytes(C1 ^ M2));

				ulong M_1 = M1 ^ 0x8000000000000000;

				ulong C_1 = BitConverter.ToUInt64(DES.Encrypto(BitConverter.GetBytes(M_1)), 0);

				byte[] T_ = DES.Encrypto(BitConverter.GetBytes(C_1 ^ M2));

				i = 0;
				for (int j = 0; j < T.Length; j++, i++)
					C[i] = T[j];
				for (int j = 0; j < T_.Length; j++, i++)
					C[i] = T_[j];
			}

			return C;
		}
	}
}
