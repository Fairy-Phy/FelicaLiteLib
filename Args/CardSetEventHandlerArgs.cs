using System;

namespace FelicaLiteLib.Args {

	/// <summary>
	/// イベントハンドラーに渡されるクラス
	/// </summary>
	public sealed class CardSetEventHandlerArgs : EventArgs {

		/// <summary>
		/// カード情報
		/// ポーリングした時のカード情報等はここから取得します。
		/// </summary>
		public FelicaCard Card { get; private set; }

		internal CardSetEventHandlerArgs(FelicaCard Card) {
			this.Card = Card;
		}
	}
}
