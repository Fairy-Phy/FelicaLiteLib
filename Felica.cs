using FelicaLiteLib.Args;
using FelicaLiteLib.Enums;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FelicaLiteLib {

	/// <summary>
	/// Felicaリーダーのポーリング処理を制御するクラスです。
	/// ここでイベント処理を追加してポーリングをすると使用できます。
	/// </summary>
	public class Felica : IDisposable {

		/// <summary>
		/// ポーリングに成功した時に発火するイベントです
		/// </summary>
		public event EventHandler<CardSetEventHandlerArgs> CardSetEvent;

		private readonly FelicaDevice _Device;

		public FelicaDevice Device => _Device;

		private CancellationTokenSource cts;

		public Felica() {
			_Device = new FelicaDevice();
		}

		private Task PollingTask(CancellationToken token, ushort SystemCode, byte TimeSlot = 0) {
			while (!token.IsCancellationRequested) {
				if (_Device.Polling(SystemCode, TimeSlot)) {
					// Polling 成功
					CardSetEvent?.Invoke(this, new CardSetEventHandlerArgs(new FelicaCard(_Device)));

					StopPolling(); // 自主的に終了させる
				}
			}

			return Task.CompletedTask;
		}

		/// <summary>
		/// ポーリングを開始します。
		/// </summary>
		/// <param name="System_Code">ポーリング対象のシステムコード</param>
		public void StartPolling(SystemCode System_Code) {
			if (cts is null) {
				cts = new CancellationTokenSource();
				Task.Run(async () => {
					await PollingTask(cts.Token, (ushort) System_Code);
				}).ConfigureAwait(false);
			}
		}

		/// <summary>
		/// ポーリングを終了します。
		/// </summary>
		/// <remarks>
		/// ポーリング成功時は自動的に呼び出されます。
		/// </remarks>
		public void StopPolling() {
			if (cts is null) return;
			if (cts.IsCancellationRequested) return;

			cts.Cancel();

			cts.Dispose();
			cts = null;
		}

		public void Dispose() {
			_Device.Dispose();
		}

		~Felica() {
			Dispose();
		}
	}
}
