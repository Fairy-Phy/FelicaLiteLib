namespace FelicaLiteLib.Enums {

	/// <summary>
	/// システムコード
	/// </summary>
	public enum SystemCode : ushort {

		/// <summary>
		/// 全てのカードを受け付けます
		/// </summary>
		AnyCard = 0xffff,

		/// <summary>
		/// Felica Lite (Lite-S)のカードのみ受け付けます
		/// </summary>
		FelicaLite = 0x88b4,

		/// <summary>
		/// NDEF対応の設定がされているカードのみ受け付けます
		/// </summary>
		NDEF = 0x12fc
	}
}
