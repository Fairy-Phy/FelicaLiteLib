using FelicaLiteLib.Wrapper;
using System;
using System.Runtime.InteropServices;

namespace FelicaLiteLib {

	/// <summary>
	/// Felicaスマートカードデバイスクラス
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public class FelicaDevice : IDisposable {

		#region DLL Func Wrapper

		/// <summary>
		/// felica.dllをロードします。
		/// </summary>
		/// <param name="d">不必要です</param>
		/// <returns>正常にロードできればPasoriハンドル(ポインター)が返されます</returns>
		[DllImport("felicalib.dll", CallingConvention = CallingConvention.Cdecl)]
		private extern static IntPtr pasori_open(string _ = null);
		/// <summary>
		/// Pasori及びfelica.dllをアンロードします。
		/// </summary>
		/// <param name="PasoriPtr"></param>
		[DllImport("felicalib.dll", CallingConvention = CallingConvention.Cdecl)]
		private extern static void pasori_close(IntPtr PasoriPtr);
		/// <summary>
		/// Pasoriを初期化します。
		/// </summary>
		/// <param name="PasoriPtr"></param>
		/// <returns>正常に完了した場合0が返されます</returns>
		[DllImport("felicalib.dll", CallingConvention = CallingConvention.Cdecl)]
		private extern static int pasori_init(IntPtr PasoriPtr);
		/// <summary>
		/// Pollingを行います。
		/// 継続してPolling待機したい場合はwhileが必要です。
		/// </summary>
		/// <param name="PasoriPtr"></param>
		/// <param name="SystemCode"></param>
		/// <param name="RFU">不必要です</param>
		/// <param name="TimeSlot"></param>
		/// <returns>Pollingが成立した場合felicaハンドルが返されます</returns>
		[DllImport("felicalib.dll", CallingConvention = CallingConvention.Cdecl)]
		private extern static IntPtr felica_polling(IntPtr PasoriPtr, ushort SystemCode, byte RFU, byte TimeSlot);
		/// <summary>
		/// Felicaハンドルを開放します。
		/// </summary>
		/// <param name="Felica_C"></param>
		[DllImport("felicalib.dll", CallingConvention = CallingConvention.Cdecl)]
		private extern static void felica_free(IntPtr Felica_C);
		/// <summary>
		/// IDmを取得します
		/// </summary>
		/// <param name="Felica_C"></param>
		/// <param name="Data"></param>
		[DllImport("felicalib.dll", CallingConvention = CallingConvention.Cdecl)]
		private extern static void felica_getidm(IntPtr Felica_C, byte[] Data);
		/// <summary>
		/// PMmを取得します
		/// </summary>
		/// <param name="Felica_C"></param>
		/// <param name="Data"></param>
		[DllImport("felicalib.dll", CallingConvention = CallingConvention.Cdecl)]
		private extern static void felica_getpmm(IntPtr Felica_C, byte[] Data);
		/// <summary>
		/// サービスコードとブロック番号を指定してブロックを読み込みます。
		/// システムコードは felica_polling で指定したものが使用されます。
		/// </summary>
		/// <param name="Felica_C"></param>
		/// <param name="ServiceCode"></param>
		/// <param name="Mode">不必要です</param>
		/// <param name="Block"></param>
		/// <param name="Data"></param>
		/// <returns>正常に完了した場合0が返されます</returns>
		[DllImport("felicalib.dll", CallingConvention = CallingConvention.Cdecl)]
		private extern static int felica_read_without_encryption02(IntPtr Felica_C, int ServiceCode, int Mode, byte Block, byte[] Data);
		/// <summary>
		/// サービスコードとブロック番号を指定してブロックを書き込みます。
		/// システムコードは felica_polling で指定したものが使用されます。
		/// </summary>
		/// <param name="Felica_C"></param>
		/// <param name="Servicecode"></param>
		/// <param name="Block"></param>
		/// <param name="Data"></param>
		/// <returns>正常に完了した場合0が返されます</returns>
		[DllImport("felicalib.dll", CallingConvention = CallingConvention.Cdecl)]
		private extern static int felica_write_without_encryption(IntPtr Felica_C, int Servicecode, byte Block, byte[] Data);
		/// <summary>
		/// システムコード一覧を取得します。
		/// </summary>
		/// <param name="PasoriPtr"></param>
		/// <returns>正常に終了した場合<see cref="FelicaC.num_system_code"/>に総システム数、<see cref="FelicaC.system_code"/>にシステムコードの配列が入ります</returns>
		/// <remarks>返り値のシステムコードのエンディアンは逆に格納されているので注意すること。</remarks>
		[DllImport("felicalib.dll", CallingConvention = CallingConvention.Cdecl)]
		private extern static IntPtr felica_enum_systemcode(IntPtr PasoriPtr);
		/// <summary>
		/// システムコードを指定してサービスコード/エリアコード一覧を取得します。
		/// </summary>
		/// <param name="PasoriPtr"></param>
		/// <param name="SystemCode"></param>
		/// <returns>
		/// 正常に終了した場合
		/// <see cref="FelicaC.num_area_code"/>/<see cref="FelicaC.area_code"/>/<see cref="FelicaC.end_service_code"/>及び、
		/// <see cref="FelicaC.num_service_code"/>/<see cref="FelicaC.service_code"/>
		/// が返されます。
		/// </returns>
		/// <remarks>Felica Liteの場合エリアの概念は存在しないためご注意ください。</remarks>
		[DllImport("felicalib.dll", CallingConvention = CallingConvention.Cdecl)]
		private extern static IntPtr felica_enum_service(IntPtr PasoriPtr, ushort SystemCode);

		//Test
		[DllImport("felicalib.dll", CallingConvention = CallingConvention.Cdecl)]
		private extern static int felica_read_without_encryption_manual(IntPtr Felica_C, int ServiceCode, byte block_len, byte[] block_list, byte[] data);

		//Test
		[DllImport("felicalib.dll", CallingConvention = CallingConvention.Cdecl)]
		private extern static int felica_write_without_encryption_manual(IntPtr Felica_C, int ServiceCode, byte block_len, byte[] block_list, byte[] data);


		#endregion

		private IntPtr PasoriPtr = IntPtr.Zero;
		private IntPtr FelicaPtr = IntPtr.Zero;
		private FelicaC _Felica_C = null;

		internal FelicaC Felica_C => _Felica_C;

		/// <summary>
		/// カードリーダーを開きます
		/// </summary>
		/// <exception cref="FelicaException"></exception>
		internal FelicaDevice() {
			PasoriPtr = pasori_open();
			if (PasoriPtr == IntPtr.Zero)
				throw new FelicaException("felicalib.dll か felica.dll をロードできませんでした");
			if (pasori_init(PasoriPtr) != 0)
				throw new FelicaException("カードリーダーの初期化ができませんでした");
		}

		internal bool Polling(ushort SystemCode, byte TimeSlot = 0) {
			IntPtr FelicaC_Res = felica_polling(PasoriPtr, SystemCode, 0, TimeSlot);
			if (FelicaC_Res == IntPtr.Zero) return false;

			DisposeFelicaC();

			FelicaPtr = FelicaC_Res;
			_Felica_C = Marshal.PtrToStructure<FelicaC>(FelicaC_Res);

			return true;
		}

		internal bool ReadWithoutEncryption(int ServiceCode, byte Addr, out byte[] Data) {
			Data = new byte[16];
			int res = felica_read_without_encryption02(FelicaPtr, ServiceCode, 0, Addr, Data);

			if (res == 0) return true;
			else return false;
		}

		internal bool ReadWithoutEncryptionManual(int ServiceCode, byte[] Addrs, out byte[] Data) {
			int BlockLen = Addrs.Length * 2;
			byte[] BlockList = new byte[BlockLen];
			for (int i = 0, n = 0; i < BlockLen; i++, n++) {
				BlockList[i++] = 0x80;
				BlockList[i] = Addrs[n];
			}

			Data = new byte[16 * Addrs.Length];
			int res = felica_read_without_encryption_manual(FelicaPtr, ServiceCode, (byte) Addrs.Length, BlockList, Data);

			if (res == 0) return true;
			else return false;
		}

		internal bool WriteWithoutEncryption(int ServiceCode, byte Addr, byte[] Data) {
			int res = felica_write_without_encryption(FelicaPtr, ServiceCode, Addr, Data);

			if (res == 0) return true;
			else return false;
		}

		internal bool WriteWithoutEncryptionManual(int ServiceCode, byte[] Addrs, byte[] Data) {
			int BlockLen = Addrs.Length * 2;
			byte[] BlockList = new byte[BlockLen];
			for (int i = 0, n = 0; i < BlockLen; i++, n++) {
				BlockList[i++] = 0x80;
				BlockList[i] = Addrs[n];
			}

			int res = felica_write_without_encryption_manual(FelicaPtr, ServiceCode, (byte) Addrs.Length, BlockList, Data);

			if (res == 0) return true;
			else return false;
		}

		public bool EnumService(ushort SystemCode) {
			IntPtr FelicaC_Res = felica_enum_service(PasoriPtr, SystemCode);
			if (FelicaC_Res == IntPtr.Zero) return false;

			DisposeFelicaC();

			FelicaPtr = FelicaC_Res;
			_Felica_C = Marshal.PtrToStructure<FelicaC>(FelicaC_Res);

			return true;
		}

		public bool EnumSystemCode() {
			IntPtr FelicaC_Res = felica_enum_systemcode(PasoriPtr);
			if (FelicaC_Res == IntPtr.Zero) return false;

			DisposeFelicaC();

			FelicaPtr = FelicaC_Res;
			_Felica_C = Marshal.PtrToStructure<FelicaC>(FelicaC_Res);

			return true;
		}

		private void DisposeFelicaC() {
			if (FelicaPtr != IntPtr.Zero) {
				felica_free(FelicaPtr);
				FelicaPtr = IntPtr.Zero;
				_Felica_C = null;
			}
		}

		public void Dispose() {
			DisposeFelicaC();

			if (PasoriPtr != IntPtr.Zero) {
				pasori_close(PasoriPtr);
				PasoriPtr = IntPtr.Zero;
			}
		}

		~FelicaDevice() {
			Dispose();
		}
	}
}
