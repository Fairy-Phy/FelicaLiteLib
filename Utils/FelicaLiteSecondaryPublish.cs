using FelicaLiteLib.Enums;

namespace FelicaLiteLib.Utils {

	/// <summary>
	/// Felica Lite用二次発行拡張クラスです
	/// </summary>
	public static class FelicaLiteSecondaryPublish {

		/// <summary>
		/// MC_SP_REG_ALL_RWを設定します。
		/// </summary>
		/// <remarks>
		/// MC[1] 7bitをROにすると以後このブロックには書き込めないようになるため注意
		/// </remarks>
		/// <param name="Card"></param>
		/// <param name="MC0">MC[0]</param>
		/// <param name="MC1">MC[1]</param>
		/// <exception cref="FelicaException">MC[1] 7bitがROに設定されている場合になります</exception>
		public static void WriteMC_RW(this FelicaCard Card, byte MC0, byte MC1) {
			byte[] WriteBytes = Card.ReadWithoutEncryption(ServiceCode.ReadOnly, Block.MC);
			if (((WriteBytes[1] & 0b1000_0000) >> 7) != 1)
				throw new FelicaException("既に二次発行が確定しています。");

			// RFは07x固定、いらないかもしれない
			WriteBytes[4] = 0x07;
			WriteBytes[0] = MC0;
			WriteBytes[1] = MC1;

			Card.WriteWithoutEncryption(ServiceCode.ReadWrite, Block.MC, WriteBytes);
		}

		/// <summary>
		/// MC_SP_REG_R_RESTRを設定します。
		/// </summary>
		/// <param name="Card"></param>
		/// <param name="MC6">MC[6]</param>
		/// <param name="MC7">MC[7]</param>
		/// <exception cref="FelicaException"></exception>
		public static void WriteMC_AuthRead(this FelicaCard Card, byte MC6, byte MC7) {
			byte[] WriteBytes = Card.ReadWithoutEncryption(ServiceCode.ReadOnly, Block.MC);
			if (((WriteBytes[1] & 0b1000_0000) >> 7) != 1)
				throw new FelicaException("既に二次発行が確定しています。");

			// RFは07x固定、いらないかもしれない
			WriteBytes[4] = 0x07;
			WriteBytes[6] = MC6;
			WriteBytes[7] = MC7;

			Card.WriteWithoutEncryption(ServiceCode.ReadWrite, Block.MC, WriteBytes);
		}

		/// <summary>
		/// MC_SP_REG_W_RESTRを設定します。
		/// </summary>
		/// <param name="Card"></param>
		/// <param name="MC8">MC[8]</param>
		/// <param name="MC9">MC[9]</param>
		/// <exception cref="FelicaException"></exception>
		public static void WriteMC_AuthWrite(this FelicaCard Card, byte MC8, byte MC9) {
			byte[] WriteBytes = Card.ReadWithoutEncryption(ServiceCode.ReadOnly, Block.MC);
			if (((WriteBytes[1] & 0b1000_0000) >> 7) != 1)
				throw new FelicaException("既に二次発行が確定しています。");

			// RFは07x固定、いらないかもしれない
			WriteBytes[4] = 0x07;
			WriteBytes[8] = MC8;
			WriteBytes[9] = MC9;

			Card.WriteWithoutEncryption(ServiceCode.ReadWrite, Block.MC, WriteBytes);
		}

		/// <summary>
		/// MC_SP_REG_W_MAC_Aを設定します。
		/// </summary>
		/// <param name="Card"></param>
		/// <param name="MC10"></param>
		/// <param name="MC11"></param>
		/// <exception cref="FelicaException"></exception>
		public static void WriteMC_ReqWriteMAC_A(this FelicaCard Card, byte MC10, byte MC11) {
			byte[] WriteBytes = Card.ReadWithoutEncryption(ServiceCode.ReadOnly, Block.MC);
			if (((WriteBytes[1] & 0b1000_0000) >> 7) != 1)
				throw new FelicaException("既に二次発行が確定しています。");

			// RFは07x固定、いらないかもしれない
			WriteBytes[4] = 0x07;
			WriteBytes[10] = MC10;
			WriteBytes[11] = MC11;

			Card.WriteWithoutEncryption(ServiceCode.ReadWrite, Block.MC, WriteBytes);
		}
	}
}
