namespace FelicaLiteLib.Enums {

	/// <summary>
	/// サービスコード
	/// </summary>
	public enum ServiceCode : int {

		/// <summary>
		/// RO権限で読み込みます
		/// </summary>
		ReadOnly = 0x000b,

		/// <summary>
		/// RW権限で読み書きします
		/// </summary>
		ReadWrite = 0x0009
	}
}
