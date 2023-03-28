using System;
using System.Linq;

namespace FelicaLiteLib.Utils {

	public static class SessionKey {

		public static (byte[], byte[]) Generate(byte[] RC, byte[] ID, byte[] MasterKey)
			=> Generate(RC, CardKey.Generate(MasterKey, ID));

		public static (byte[], byte[]) Generate(byte[] RC, byte[] CK) {
			byte[] RC1, RC2;
			RC1 = new byte[8];
			RC2 = new byte[8];

			int i = 0;
			for (int n = 0; n < RC1.Length; n++, i++) RC1[n] = RC[i];
			for (int n = 0; n < RC2.Length; n++, i++) RC2[n] = RC[i];

			byte[] CK1, CK2;
			CK1 = new byte[8];
			CK2 = new byte[8];
			i = 0;
			for (int n = 0; n < CK1.Length; n++, i++) CK1[n] = CK[i];
			for (int n = 0; n < CK2.Length; n++, i++) CK2[n] = CK[i];

			RC1 = RC1.Reverse().ToArray();
			CK1 = CK1.Reverse().ToArray();
			CK2 = CK2.Reverse().ToArray();

			byte[] SK1;
			byte[] SK2;
			using (TripleDES DES = new TripleDES(CK1, CK2, CK1)) {
				byte[] SK1_ = DES.Encrypto(RC1);
				SK1 = SK1_.Reverse().ToArray();
				ulong RC2_Ulong = BitConverter.ToUInt64(RC2.Reverse().ToArray(), 0);
				ulong RC2_ = RC2_Ulong ^ BitConverter.ToUInt64(SK1_, 0);

				byte[] SK2_ = DES.Encrypto(BitConverter.GetBytes(RC2_));
				SK2 = SK2_.Reverse().ToArray();
			}

			return (SK1, SK2);
		}
	}
}
