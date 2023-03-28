namespace FelicaLiteLib.Enums {

	/// <summary>
	/// ブロックとそれらに対応したアドレス(Addr)
	/// </summary>
	public enum Block : byte {

		S_PAD0 = 0x00,
		S_PAD1 = 0x01,
		S_PAD2 = 0x02,
		S_PAD3 = 0x03,
		S_PAD4 = 0x04,
		S_PAD5 = 0x05,
		S_PAD6 = 0x06,
		S_PAD7 = 0x07,
		S_PAD8 = 0x08,
		S_PAD9 = 0x09,
		S_PAD10 = 0x0a,
		S_PAD11 = 0x0b,
		S_PAD12 = 0x0c,
		S_PAD13 = 0x0d,
		REG = 0x0e,
		RC = 0x80,
		MAC = 0x81,
		ID = 0x82,
		D_ID = 0x83,
		SER_C = 0x84,
		SYS_C = 0x85,
		CKV = 0x86,
		CK = 0x87,
		MC = 0x88,
		WCNT = 0x90,
		MAC_A = 0x91,
		STATE = 0x92,
		CRC_CHECK = 0xa0
	}
}
