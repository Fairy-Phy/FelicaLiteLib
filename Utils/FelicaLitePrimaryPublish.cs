using FelicaLiteLib.Enums;
using System;

namespace FelicaLiteLib.Utils {

	/// <summary>
	/// Felica Lite用一次発行をする拡張クラスです
	/// </summary>
	/// <remarks>
	/// MC[0~1][6~11]は二次発行で行います。
	/// </remarks>
	public static class FelicaLitePrimaryPublish {

		/// <summary>
		/// IDを設定します。
		/// </summary>
		/// <param name="Card"></param>
		/// <param name="ID"></param>
		/// <param name="DFD"></param>
		/// <param name="ForceWrite"></param>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		/// <exception cref="FelicaException"></exception>
		public static void WriteID(this FelicaCard Card, byte[] ID, byte[] DFD = null, bool ForceWrite = false) {
			if (DFD is null) DFD = new byte[2] { 0x00, 0x00 };
			if (DFD.Length != 2)
				throw new ArgumentOutOfRangeException(nameof(DFD), "DFDの要素数は2である必要があります");
			if (ID.Length != 6)
				throw new ArgumentOutOfRangeException(nameof(ID), "IDの要素数は6である必要があります");

			byte[] WriteBytes = new byte[16];

			int i = 0;
			for (; i < 8; i++) WriteBytes[i] = 0x00;
			for (int n = 0; i < 10; i++, n++) WriteBytes[i] = DFD[n];
			for (int n = 0; i < 16; i++, n++) WriteBytes[i] = ID[n];

			if (Card.CurrentSystemCode != (ushort) SystemCode.FelicaLite) {
				if (!Card.Polling(SystemCode.FelicaLite))
					throw new FelicaException("これはFelicaLite専用です");
			}

			if (!ForceWrite) {
				byte[] CheckIDBytes = Card.ReadWithoutEncryption(ServiceCode.ReadOnly, Block.ID);
				for (i = 8; i < 16; i++)
					if (CheckIDBytes[i] != 0x00)
						throw new FelicaException("既にIDが書き込まれています。");
			}

			Card.WriteWithoutEncryption(ServiceCode.ReadWrite, Block.ID, WriteBytes);
		}

		/// <summary>
		/// CK(カード鍵)を設定します。
		/// </summary>
		/// <remarks>
		/// CKは<paramref name="MasterKey"/>から変換されます
		/// </remarks>
		/// <param name="Card"></param>
		/// <param name="MasterKey"></param>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		/// <exception cref="FelicaException"></exception>
		public static void WriteCK(this FelicaCard Card, byte[] MasterKey) {
			if (MasterKey.Length != 24)
				throw new ArgumentOutOfRangeException(nameof(MasterKey), "Kの要素数は24である必要があります");

			byte[] ID = Card.ReadWithoutEncryption(ServiceCode.ReadOnly, Block.ID);

			int i;
			for (i = 8; i < 16 && ID[i] == 0x00; i++) ;
			if (i == 16)
				throw new FelicaException("IDが設定されていません");

			byte[] C = CardKey.Generate(MasterKey, ID);

			if (Card.CurrentSystemCode != (ushort) SystemCode.FelicaLite) {
				if (!Card.Polling(SystemCode.FelicaLite))
					throw new FelicaException("これはFelicaLite専用です");
			}

			try {
				Card.WriteWithoutEncryption(ServiceCode.ReadWrite, Block.CK, C);
			}
			catch (FelicaException Error) {
				// CKはReadできないのであくまで可能性
				throw new FelicaException("既に書き込まれている可能性があります", Error);
			}
		}

		/// <summary>
		/// CKV(カード鍵バージョン)を設定します。
		/// </summary>
		/// <param name="Card"></param>
		/// <param name="Key_Version"></param>
		/// <param name="ForceWrite"></param>
		public static void WriteCKV(this FelicaCard Card, ushort Key_Version, bool ForceWrite = false)
			=> Card.WriteCKV(BitConverter.GetBytes(Key_Version), ForceWrite);

		/// <summary>
		/// CKV(カード鍵バージョン)を設定します。
		/// </summary>
		/// <param name="Card"></param>
		/// <param name="Key_Version"></param>
		/// <param name="ForceWrite"></param>
		public static void WriteCKV(this FelicaCard Card, byte[] Key_Version, bool ForceWrite = false) {
			if (Key_Version.Length != 2)
				throw new ArgumentOutOfRangeException(nameof(Key_Version), "Key_Versionの要素数は2である必要があります");

			byte[] WriteBytes = new byte[16];
			int i = 0;
			for (; i < Key_Version.Length; i++) WriteBytes[i] = Key_Version[i];
			for (; i < 16; i++) WriteBytes[i] = 0x00;

			if (Card.CurrentSystemCode != (ushort) SystemCode.FelicaLite) {
				if (!Card.Polling(SystemCode.FelicaLite))
					throw new FelicaException("これはFelicaLite専用です");
			}

			if (!ForceWrite) {
				byte[] CheckCKVBytes = Card.ReadWithoutEncryption(ServiceCode.ReadOnly, Block.CKV);
				for (i = 0; i < 2; i++)
					if (CheckCKVBytes[i] != 0x00)
						throw new FelicaException("既にCKVが書き込まれています。");
			}

			Card.WriteWithoutEncryption(ServiceCode.ReadWrite, Block.CKV, WriteBytes);
		}

		/// <summary>
		/// 一次発行時に設定するMCを設定します。
		/// </summary>
		/// <remarks>
		/// <paramref name="WriteSTATEWithMAC"/>は既に<see cref="true"/>で設定されている場合に<see cref="false"/>で書き込もうとした場合例外を返します
		/// </remarks>
		/// <param name="Card"></param>
		/// <param name="SupportNDEF"></param>
		/// <param name="AllowWriteCKCKVWithMAC"></param>
		/// <param name="WriteSTATEWithMAC"></param>
		public static void WriteMV(this FelicaCard Card, bool SupportNDEF, bool AllowWriteCKCKVWithMAC, bool WriteSTATEWithMAC) {
			byte[] WriteBytes = Card.ReadWithoutEncryption(ServiceCode.ReadOnly, Block.MC);
			if (WriteBytes[2] != 0xff)
				throw new FelicaException("既に一次発行が確定しています。");
			if (WriteBytes[12] == 0x01 && !WriteSTATEWithMAC)
				throw new FelicaException("既にMC_STATE_W_MAC_Aが0x01のため設定を反映できません");

			bool EqFlag = true;

			// RFは07x固定、いらないかもしれない
			WriteBytes[4] = 0x07;
			if (SupportNDEF) {
				if (WriteBytes[3] != 0x01) EqFlag = false;
				else WriteBytes[3] = 0x01;
			}
			else {
				if (WriteBytes[3] != 0x00) EqFlag = false;
				else WriteBytes[3] = 0x00;
			}
			if (AllowWriteCKCKVWithMAC) {
				if (WriteBytes[5] != 0x01) EqFlag = false;
				else WriteBytes[5] = 0x01;
			}
			else {
				if (WriteBytes[5] != 0x00) EqFlag = false;
				else WriteBytes[5] = 0x00;
			}
			if (WriteSTATEWithMAC) {
				if (WriteBytes[12] != 0x01) EqFlag = false;
				else WriteBytes[12] = 0x01;
			}
			else {
				if (WriteBytes[12] != 0x00) EqFlag = false;
				else WriteBytes[12] = 0x00;
			}

			if (EqFlag) return;

			Card.WriteWithoutEncryption(ServiceCode.ReadWrite, Block.MC, WriteBytes);
		}

		/// <summary>
		/// 一次発行を確定させます。
		/// </summary>
		/// <remarks>
		///	実行後は
		///	<see cref="Block.ID"/>, <see cref="Block.SER_C"/>(設定によっては<see cref="Block.CK"/>, <see cref="Block.CKV"/>含む)と
		///	<see cref="Block.MC"/>[2]~[5]の書き込みは一切できなくなり、<see cref="Block.WCNT"/>がリセットされます。
		/// </remarks>
		public static void PRIMARY_PUBLISH(this FelicaCard Card) {
			byte[] WriteBytes = Card.ReadWithoutEncryption(ServiceCode.ReadOnly, Block.MC);
			if (WriteBytes[2] != 0xff)
				throw new FelicaException("既に一次発行が確定しています。");

			WriteBytes[4] = 0x07;
			WriteBytes[2] = 0x00;
			Card.WriteWithoutEncryption(ServiceCode.ReadWrite, Block.MC, WriteBytes);
		}
	}
}
