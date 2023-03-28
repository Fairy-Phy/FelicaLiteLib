using FelicaLiteLib.Enums;
using FelicaLiteLib.Utils;
using System;

namespace FelicaLiteLib {

	/// <summary>
	/// Felicaカードのクラス。
	/// ここで様々なカード情報の処理が行なえます。
	/// </summary>
	public class FelicaCard {

		/// <summary>
		/// デバイスクラス
		/// 基本的にいじるものではありません。
		/// </summary>
		public FelicaDevice Device { get; private set; }

		/// <summary>
		/// カードのIDm
		/// </summary>
		public byte[] IDm => Device.Felica_C.GetIDm();

		/// <summary>
		/// カードのPMm
		/// </summary>
		public byte[] PMm => Device.Felica_C.GetPMm();

		/// <summary>
		/// 現在の読み込みシステムコード
		/// </summary>
		public ushort CurrentSystemCode => Device.Felica_C.GetSystemCode();

		/// <summary>
		/// RCブロック
		/// </summary>
		public byte[] RC { get; private set; }

		/// <summary>
		/// RCブロックが書き込まれたか
		/// </summary>
		public bool WroteRC { get; private set; } = false;

		internal FelicaCard(FelicaDevice Device) {
			this.Device = Device;
		}

		/// <summary>
		/// ポーリングをします。接続確認をする場合に使用します。
		/// </summary>
		/// <param name="System_Code"></param>
		/// <param name="TimeSlot"></param>
		/// <returns>指定された<paramref name="System_Code"/>で接続されたか</returns>
		public bool Polling(SystemCode System_Code, byte TimeSlot = 0)
			=> Polling((ushort) System_Code, TimeSlot);

		/// <summary>
		/// ポーリングをします。接続確認をする場合に使用します。
		/// </summary>
		/// <remarks>通常こちらは使用するべきではありません</remarks>
		/// <param name="System_Code"></param>
		/// <param name="TimeSlot"></param>
		/// <returns>指定された<paramref name="System_Code"/>で接続されたか</returns>
		public bool Polling(ushort System_Code, byte TimeSlot = 0)
			=> Device.Polling(System_Code, TimeSlot);

		/// <summary>
		/// 接続されたカードのシステムコード一覧を取得します
		/// </summary>
		/// <returns>システムコード一覧</returns>
		/// <exception cref="FelicaException"></exception>
		public ushort[] GetSystemCodeList() {
			ushort PrevSystemCode = CurrentSystemCode;

			if (!Device.EnumSystemCode())
				throw new FelicaException("接続維持されていないか、取得できませんでした");

			if (Polling(SystemCode.FelicaLite)) {
				if (Polling(SystemCode.NDEF)) {
					Polling(PrevSystemCode); // 元のシステムコードに戻します

					return new ushort[] {
						(ushort) SystemCode.AnyCard,
						(ushort) SystemCode.FelicaLite,
						(ushort) SystemCode.NDEF
					};
				}
				else {
					Polling(PrevSystemCode); // 元のシステムコードに戻します

					return new ushort[] {
						(ushort) SystemCode.AnyCard,
						(ushort) SystemCode.FelicaLite
					};
				}
			}

			byte ResLength = Device.Felica_C.GetSystemCodesLength();
			ushort[] ResSysCodes = Device.Felica_C.GetSystemCodes();

			ushort[] Res = new ushort[ResLength];

			for (int i = 0; i < ResLength; i++)
				Res[i] = (ushort) ((ResSysCodes[i] & 0xff00) >> 8 | (ResSysCodes[i] & 0xff) << 8);

			Polling(PrevSystemCode); // 元のシステムコードに戻します

			return Res;
		}

		/* Felica Lite専用ライブラリのため未実装, もし改変してStandard共存にする場合はどうぞ...

		public void GetServiceCodeList() {
			throw new NotImplementedException();
		}
		
		*/

		/// <summary>
		/// MACなしでブロックに読み込みます
		/// </summary>
		/// <param name="Service_Code"></param>
		/// <param name="Addr"></param>
		/// <returns>指定ブロックの読込結果</returns>
		public byte[] ReadWithoutEncryption(ServiceCode Service_Code, Block Addr)
			=> ReadWithoutEncryption((int) Service_Code, (byte) Addr);

		/// <summary>
		/// MACなしでブロックに読み込みます
		/// </summary>
		/// <remarks>通常こちらは使用するべきではありません</remarks>
		/// <param name="Service_Code"></param>
		/// <param name="Addr"></param>
		/// <returns>指定ブロックの読込結果</returns>
		/// <exception cref="FelicaException"></exception>
		public byte[] ReadWithoutEncryption(int Service_Code, byte Addr) {
			if (!Polling(Device.Felica_C.GetSystemCode()))
				throw new FelicaException("接続が維持されていません");

			if (!Device.ReadWithoutEncryption(Service_Code, Addr, out byte[] Res))
				throw new FelicaException("データを読み込めませんでした");

			return Res;
		}

		/// <summary>
		/// MACを使用してブロックに読み込みます
		/// </summary>
		/// <param name="Service_Code"></param>
		/// <param name="Addr"></param>
		/// <param name="MasterKey"></param>
		/// <returns>指定ブロックの読込結果</returns>
		/// <exception cref="FelicaException"></exception>
		public byte[] ReadWithoutEncryptionWithMAC_A(ServiceCode Service_Code, Block Addr, byte[] MasterKey) {
			byte[] ReadedBytes =
				ReadWithoutEncryptionManual(Service_Code, Block.ID, Addr, Block.MAC_A );

			GenerateRC();

			int i = 0;
			byte[] ID = new byte[16];
			for (int n = 0; n < ID.Length; i++, n++) ID[n] = ReadedBytes[i];
			byte[] Res = new byte[16];
			for (int n = 0; n < Res.Length; i++, n++) Res[n] = ReadedBytes[i];
			byte[] MAC_A_C = new byte[16];
			for (int n = 0; n < MAC_A_C.Length; i++, n++) MAC_A_C[n] = ReadedBytes[i];

			byte[] CK = CardKey.Generate(MasterKey, ID);

			byte[] MAC_A = MAC.GenerateReadMAC_A(RC, CK, (Block.ID, ID), (Addr, Res), (Block.MAC_A, null));

			int cp = 0;
			for (; cp < MAC_A_C.Length && MAC_A_C[cp] == MAC_A[cp]; cp++) ;
			if (cp != MAC_A_C.Length)
				throw new FelicaException("MAC_Aが一致しませんでした");

			return Res;
		}

		/// <summary>
		/// MACなしでブロックに書き込みます
		/// </summary>
		/// <param name="Service_Code"></param>
		/// <param name="Addr"></param>
		/// <param name="Data"></param>
		public void WriteWithoutEncryption(ServiceCode Service_Code, Block Addr, byte[] Data)
			=> WriteWithoutEncryption((int) Service_Code, (byte) Addr, Data);

		/// <summary>
		/// MACなしでブロックに書き込みます
		/// </summary>
		/// <remarks>通常こちらは使用するべきではありません</remarks>
		/// <param name="Service_Code"></param>
		/// <param name="Addr"></param>
		/// <param name="Data"></param>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		/// <exception cref="FelicaException"></exception>
		public void WriteWithoutEncryption(int Service_Code, byte Addr, byte[] Data) {
			if (Data.Length != 16)
				throw new ArgumentOutOfRangeException(nameof(Data), "データは16バイトである必要があります");
			if (!Polling(Device.Felica_C.GetSystemCode()))
				throw new FelicaException("接続が維持されていません");

			if (!Device.WriteWithoutEncryption(Service_Code, Addr, Data))
				throw new FelicaException("書き込みできませんでした");
		}

		/// <summary>
		/// MACを使用してブロックに書き込みます
		/// </summary>
		/// <param name="Service_Code"></param>
		/// <param name="Addr"></param>
		/// <param name="Data"></param>
		/// <param name="MasterKey"></param>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		/// <exception cref="FelicaException"></exception>
		public void WriteWithoutEncryptionWithMAC_A(ServiceCode Service_Code, Block Addr, byte[] Data, byte[] MasterKey) {
			if (Data.Length != 16)
				throw new ArgumentOutOfRangeException(nameof(Data), "データは16バイトである必要があります");
			if (!Polling(Device.Felica_C.GetSystemCode()))
				throw new FelicaException("接続が維持されていません");

			GenerateRC();

			byte[] ID_WCNT = ReadWithoutEncryptionManual(ServiceCode.ReadOnly, Block.ID, Block.WCNT);

			int i = 0;
			byte[] ID = new byte[16];
			for (int n = 0; n < ID.Length; i++, n++) ID[n] = ID_WCNT[i];
			byte[] WCNT = new byte[16];
			for (int n = 0; n < WCNT.Length; i++, n++) WCNT[n] = ID_WCNT[i];

			byte[] CK = CardKey.Generate(MasterKey, ID);

			byte[] MAC_A = MAC.GenerateWriteMAC_A(RC, CK, WCNT, Addr, Data);

			byte[] WriteMAC_A = new byte[16];
			i = 0;
			for (; i < MAC_A.Length; i++) WriteMAC_A[i] = MAC_A[i];
			for (int n = 0; n < 4; n++, i++) WriteMAC_A[i] = WCNT[n];
			for (; i < WriteMAC_A.Length; i++) WriteMAC_A[i] = 0x00;

			WriteWithoutEncryptionManual(Service_Code, (Addr, Data), (Block.MAC_A, WriteMAC_A));
		}

		byte[] ReadWithoutEncryptionManual(ServiceCode Service_Code, params Block[] Addrs) {
			byte[] Blocks = new byte[Addrs.Length];
			for (int i = 0; i < Blocks.Length; i++) {
				Blocks[i] = (byte) Addrs[i];
			}

			return ReadWithoutEncryptionManual((int) Service_Code, Blocks);
		}

		byte[] ReadWithoutEncryptionManual(int Service_Code, byte[] Addrs) {
			if (!Polling(Device.Felica_C.GetSystemCode()))
				throw new FelicaException("接続が維持されていません");

			if (!Device.ReadWithoutEncryptionManual(Service_Code, Addrs, out byte[] Res))
				throw new FelicaException("データを読み込めませんでした");

			return Res;
		}

		void WriteWithoutEncryptionManual(ServiceCode Service_Code, params (Block, byte[])[] Addrs_Data) {
			byte[] Addrs = new byte[Addrs_Data.Length];
			for (int i = 0; i < Addrs_Data.Length; i++) {
				Addrs[i] = (byte) Addrs_Data[i].Item1;
			}

			byte[] Data = new byte[16 * Addrs_Data.Length];
			for (int ad = 0, d = 0; ad < Addrs_Data.Length; ad++) {
				if (Addrs_Data[ad].Item2.Length % 16 != 0)
					throw new FelicaException($"{ad}番目のデータ要素数が16ではありません");
				for (int add = 0; add < Addrs_Data[ad].Item2.Length; add++, d++)
					Data[d] = Addrs_Data[ad].Item2[add];
			}

			WriteWithoutEncryptionManual((int) Service_Code, Addrs, Data);
		}

		void WriteWithoutEncryptionManual(int Service_Code, byte[] Addrs, byte[] Data) {
			if (!Polling(Device.Felica_C.GetSystemCode()))
				throw new FelicaException("接続が維持されていません");

			if (!Device.WriteWithoutEncryptionManual(Service_Code, Addrs, Data))
				throw new FelicaException("データを書き込めませんでした");
		}

		void CheckFelicaLiteS() {
			if (!Polling(SystemCode.FelicaLite))
				throw new FelicaException("接続が維持されていないかFelica Liteではありません");
		}

		bool CheckExtAuth() {
			byte[] STATE = ReadWithoutEncryption(ServiceCode.ReadOnly, Block.STATE);

			if (STATE[0] == 0x01) return true;
			else return false;
		}

		void GenerateRC() {
			if (WroteRC) return;

			RC = new byte[16];
			using (var Rand = new System.Security.Cryptography.RNGCryptoServiceProvider()) {
				Rand.GetBytes(RC);
			}
			WriteWithoutEncryption(ServiceCode.ReadWrite, Block.RC, RC);

			WroteRC = true;
		}

		/// <summary>
		/// 内部認証をします
		/// </summary>
		/// <param name="MasterKey"></param>
		/// <returns>内部認証されたか</returns>
		public bool InternalAuthenticationMAC_A(byte[] MasterKey) {
			CheckFelicaLiteS();

			GenerateRC();

			byte[] ReadedBlocks = ReadWithoutEncryptionManual(ServiceCode.ReadOnly, Block.ID, Block.CKV, Block.MAC_A);

			int i = 0;

			byte[] ID = new byte[16];
			for (int n = 0; n < ID.Length; i++, n++) ID[n] = ReadedBlocks[i];
			byte[] CKV = new byte[16];
			for (int n = 0; n < CKV.Length; i++, n++) CKV[n] = ReadedBlocks[i];
			byte[] MAC_A_C = new byte[8];
			for (int n = 0; n < MAC_A_C.Length; i++, n++) MAC_A_C[n] = ReadedBlocks[i];

			byte[] CK = CardKey.Generate(MasterKey, ID);

			byte[] MAC_A = MAC.GenerateReadMAC_A(RC, CK, (Block.ID, ID), (Block.CKV, CKV), (Block.MAC_A, null));

			int cp = 0;
			for (; cp < MAC_A_C.Length && MAC_A_C[cp] == MAC_A[cp]; cp++) ;
			if (cp != MAC_A_C.Length) return false;
			else return true;
		}

		/// <summary>
		/// 外部認証をします
		/// </summary>
		/// <param name="MasterKey"></param>
		/// <returns>外部認証されたか</returns>
		public bool ExternalAuthenticationMAC_A(byte[] MasterKey) {
			CheckFelicaLiteS();

			GenerateRC();

			if (CheckExtAuth()) return true;

			byte[] ReadedBlocks = ReadWithoutEncryptionManual(ServiceCode.ReadOnly, Block.ID, Block.CKV, Block.WCNT);

			int i = 0;

			byte[] ID = new byte[16];
			for (int n = 0; n < ID.Length; i++, n++) ID[n] = ReadedBlocks[i];
			byte[] CKV = new byte[16];
			for (int n = 0; n < CKV.Length; i++, n++) CKV[n] = ReadedBlocks[i];
			byte[] WCNT = new byte[8];
			for (int n = 0; n < WCNT.Length; i++, n++) WCNT[n] = ReadedBlocks[i];

			byte[] CK = CardKey.Generate(MasterKey, ID);

			byte[] STATEWriteData = new byte[16];
			i = 0;
			STATEWriteData[i++] = 0x01;
			for (; i < STATEWriteData.Length; i++) STATEWriteData[i] = 0x00;

			byte[] MAC_A = MAC.GenerateWriteMAC_A(RC, CK, WCNT, Block.STATE, STATEWriteData);

			byte[] WriteMAC_A = new byte[16];
			i = 0;
			for (; i < MAC_A.Length; i++) WriteMAC_A[i] = MAC_A[i];
			for (int n = 0; n < 4; n++, i++) WriteMAC_A[i] = WCNT[n];
			for (; i < WriteMAC_A.Length; i++) WriteMAC_A[i] = 0x00;

			try {
				WriteWithoutEncryptionManual(ServiceCode.ReadWrite, (Block.STATE, STATEWriteData), (Block.MAC_A, WriteMAC_A));
			}
			catch (FelicaException) {
				return false;
				//throw new FelicaException("外部認証に失敗しました", Error);
			}

			return true;
		}

		/// <summary>
		/// 相互認証をします
		/// </summary>
		/// <param name="MasterKey"></param>
		/// <returns>相互認証されたか</returns>
		public bool MutualAuthenticationMAC_A(byte[] MasterKey) {
			CheckFelicaLiteS();

			GenerateRC();

			if (CheckExtAuth()) return true;

			byte[] ReadedBlocks = ReadWithoutEncryptionManual(ServiceCode.ReadOnly, Block.ID, Block.CKV, Block.WCNT, Block.MAC_A);

			int i = 0;

			byte[] ID = new byte[16];
			for (int n = 0; n < ID.Length; i++, n++) ID[n] = ReadedBlocks[i];
			byte[] CKV = new byte[16];
			for (int n = 0; n < CKV.Length; i++, n++) CKV[n] = ReadedBlocks[i];
			byte[] WCNT = new byte[16];
			for (int n = 0; n < WCNT.Length; i++, n++) WCNT[n] = ReadedBlocks[i];
			byte[] MAC_A_C = new byte[8];
			for (int n = 0; n < MAC_A_C.Length; i++, n++) MAC_A_C[n] = ReadedBlocks[i];

			byte[] CK = CardKey.Generate(MasterKey, ID);

			// 内部認証

			byte[] R_MAC_A = MAC.GenerateReadMAC_A(RC, CK, (Block.ID, ID), (Block.CKV, CKV), (Block.WCNT, WCNT), (Block.MAC_A, null));

			int cp = 0;
			for (; cp < MAC_A_C.Length && MAC_A_C[cp] == R_MAC_A[cp]; cp++) ;
			if (cp != MAC_A_C.Length)
				return false;
				//throw new FelicaException("相互認証に失敗しました");

			// 外部認証

			byte[] STATEWriteData = new byte[16];
			i = 0;
			STATEWriteData[i++] = 0x01;
			for (; i < STATEWriteData.Length; i++) STATEWriteData[i] = 0x00;

			byte[] W_MAC_A = MAC.GenerateWriteMAC_A(RC, CK, WCNT, Block.STATE, STATEWriteData);

			byte[] WriteMAC_A = new byte[16];
			i = 0;
			for (; i < W_MAC_A.Length; i++) WriteMAC_A[i] = W_MAC_A[i];
			for (int n = 0; n < 4; n++, i++) WriteMAC_A[i] = WCNT[n];
			for (; i < WriteMAC_A.Length; i++) WriteMAC_A[i] = 0x00;

			try {
				WriteWithoutEncryptionManual(ServiceCode.ReadWrite, (Block.STATE, STATEWriteData), (Block.MAC_A, WriteMAC_A));
			}
			catch (FelicaException) {
				return false;
				//throw new FelicaException("相互認証に失敗しました", Error);
			}

			return true;
		}
	}
}
