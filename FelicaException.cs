using System;
using System.Runtime.Serialization;

namespace FelicaLiteLib {

	/// <summary>
	/// Felica例外クラス
	/// </summary>
	[Serializable]
	public class FelicaException : Exception {

		public FelicaException() { }

		public FelicaException(string message) : base(message) { }

		public FelicaException(string message, Exception inner) : base(message, inner) { }

		protected FelicaException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}
