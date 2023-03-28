using FelicaLiteLib.Args;
using FelicaLiteLib.Enums;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FelicaLiteLib {

	public class Felica : IDisposable {

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

		public void StartPolling(SystemCode System_Code) {
			if (cts is null) {
				cts = new CancellationTokenSource();
				Task.Run(async () => {
					await PollingTask(cts.Token, (ushort) System_Code);
				}).ConfigureAwait(false);
			}
		}

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
