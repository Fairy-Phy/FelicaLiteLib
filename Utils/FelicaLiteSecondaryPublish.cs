using FelicaLiteLib.Enums;

namespace FelicaLiteLib.Utils {

	public static class FelicaLiteSecondaryPublish {

		// ブロックをRO/RW設定する。ROにすると以後書き込めない
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

		// ブロックの読み込みに外部認証を挟むかの設定
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

		// ブロックの書き込みに外部認証を挟むかの設定
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

		// ブロックの書き込みにMAC_A署名を挟むかの設定
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
