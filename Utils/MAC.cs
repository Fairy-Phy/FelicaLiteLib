using FelicaLiteLib.Enums;
using System;

namespace FelicaLiteLib.Utils {

	public static class MAC {

		/// <summary>
		/// 
		/// </summary>
		/// <param name="RC"></param>
		/// <param name="CK"></param>
		/// <param name="BlockListResponse">MAC_Aを読みだした際のブロックリスト。※必ず読みだした順番に要素を入れてください！。</param>
		/// <returns></returns>
		public static byte[] GenerateReadMAC_A(byte[] RC, byte[] CK, params (Block, byte[])[] BlockListResponse) {
			if (BlockListResponse.Length < 2 || BlockListResponse.Length > 4)
				throw new ArgumentOutOfRangeException(nameof(BlockListResponse), "ブロック数は2以上4以下である必要があります");

			int i;

			byte[] RC1 = new byte[8];
			i = 0;
			for (int n = RC1.Length - 1; n >= 0; n--, i++) RC1[n] = RC[i];
			//Reverse(ref RC1); BitConverterで何故かリトルエンディアン処理される

			(byte[] SK1, byte[] SK2) = SessionKey.Generate(RC, CK);
			Reverse(ref SK1);
			Reverse(ref SK2);

			// 初期ブロックデータは必ず最後は0x91(MAC_A)で終わるはずである
			byte[] BlockData = new byte[8];
			i = 0;
			for (int n = 0; n < BlockListResponse.Length; n++, i++) {
				BlockData[i++] = (byte) BlockListResponse[n].Item1;
				BlockData[i] = 0x00;
			}
			for (; i < BlockData.Length; i++) BlockData[i] = 0xff;
			Reverse(ref BlockData);

			/*byte[] FirstData = BitConverter.GetBytes(
				BitConverter.ToUInt64(BlockData, 0) ^ BitConverter.ToUInt64(RC1, 0)
			);*/
			//Reverse(ref FirstData);

			byte[] MAC_A;
			using (TripleDES DES = new TripleDES(SK1, SK2, SK1)) {

				MAC_A = DES.Encrypto(BlockData.XOR(RC1)/*FirstData*/);

				for (i = 0; i < BlockListResponse.Length; i++) {
					if (BlockListResponse[i].Item1 == Block.MAC_A && i == BlockListResponse.Length - 1)
						break;

					byte[] Data1 = new byte[8];
					byte[] Data2 = new byte[8];
					int n = 0;
					for (int d = Data1.Length - 1; d >= 0; d--, n++) Data1[d] = BlockListResponse[i].Item2[n];
					for (int d = Data2.Length - 1; d >= 0; d--, n++) Data2[d] = BlockListResponse[i].Item2[n];

					MAC_A = DES.Encrypto(
						Data1.XOR(MAC_A)
						//BitConverter.GetBytes(BitConverter.ToUInt64(Data1, 0) ^ BitConverter.ToUInt64(MAC_A, 0))
					);
					MAC_A = DES.Encrypto(
						Data2.XOR(MAC_A)
						//BitConverter.GetBytes(BitConverter.ToUInt64(Data2, 0) ^ BitConverter.ToUInt64(MAC_A, 0))
					);
				}
			}

			Reverse(ref MAC_A);

			return MAC_A;
		}

		public static byte[] GenerateWriteMAC_A(byte[] RC, byte[] CK, byte[] WCNT, Block FirstBlock, byte[] Data) {
			if (Data.Length != 16)
				throw new ArgumentOutOfRangeException(nameof(Data), "データは16バイトである必要があります");

			byte[] BlockNumData = {
				WCNT[0], WCNT[1], WCNT[2], WCNT[3],
				(byte) FirstBlock, 0x00,
				(byte) Block.MAC_A, 0x00
			};
			Reverse(ref BlockNumData);

			(byte[] SK1, byte[] SK2) = SessionKey.Generate(RC, CK);
			Reverse(ref SK1);
			Reverse(ref SK2);

			int i;

			byte[] Data1 = new byte[8];
			byte[] Data2 = new byte[8];
			i = 0;
			//for (int n = 0; n < Data1.Length; n++, i++) Data1[n] = Data[i];
			//for (int n = 0; n < Data2.Length; n++, i++) Data2[n] = Data[i];
			for (int n = Data1.Length - 1; n >= 0; n--, i++) Data1[n] = Data[i];
			for (int n = Data2.Length - 1; n >= 0; n--, i++) Data2[n] = Data[i];

			byte[] RC1 = new byte[8];
			i = 0;
			//for (int n = 0; n < RC1.Length; n++, i++) RC1[n] = RC[i];
			for (int n = RC1.Length - 1; n >= 0; n--, i++) RC1[n] = RC[i];

			byte[] FirstData = BlockNumData.XOR(RC1); // BitConverter.GetBytes(BitConverter.ToUInt64(BlockNumData, 0) ^ BitConverter.ToUInt64(RC1, 0));
			//Reverse(ref FirstData);

			byte[] MAC_A;
			using (TripleDES DES = new TripleDES(SK2, SK1, SK2)) {

				MAC_A = DES.Encrypto(FirstData);

				MAC_A = DES.Encrypto(
					Data1.XOR(MAC_A)
					//BitConverter.GetBytes(BitConverter.ToUInt64(Data1, 0) ^ BitConverter.ToUInt64(MAC_A, 0))
				);
				MAC_A = DES.Encrypto(
					Data2.XOR(MAC_A)
					//BitConverter.GetBytes(BitConverter.ToUInt64(Data2, 0) ^ BitConverter.ToUInt64(MAC_A, 0))
				);
			}

			Reverse(ref MAC_A);

			return MAC_A;
		}

		// BitConverterだと反転させたのに勝手にもとに戻されるので手動
		static byte[] XOR(this byte[] Left, byte[] Right) {
			int Length = Math.Max(Left.Length, Right.Length);

			byte[] Res = new byte[Length];

			for (
				int res_i = Length - 1, l_i = Left.Length - 1, r_i = Right.Length - 1;
				res_i >= 0;
				res_i--, l_i--, r_i--) {
				byte l = l_i < 0 ? (byte) 0x00 : Left[l_i];
				byte r = r_i < 0 ? (byte) 0x00 : Right[r_i];
				Res[res_i] = unchecked((byte) unchecked((uint) l ^ r));
			}

			return Res;
		}

		static void Reverse<T>(ref T[] Data) where T : struct {
			for (int f = 0, e = Data.Length - 1; e - f >= 0; f++, e--) {
				T Tmp = Data[f];
				Data[f] = Data[e];
				Data[e] = Tmp;
			}
		}
	}
}
