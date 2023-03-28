using System;

namespace FelicaLiteLib.Args {

	public sealed class CardSetEventHandlerArgs : EventArgs {

		public FelicaCard Card { get; private set; }

		internal CardSetEventHandlerArgs(FelicaCard Card) {
			this.Card = Card;
		}
	}
}
