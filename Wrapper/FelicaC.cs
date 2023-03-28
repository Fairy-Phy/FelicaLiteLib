using System;
using System.Runtime.InteropServices;

namespace FelicaLiteLib.Wrapper {

	[StructLayout(LayoutKind.Sequential)]
	public class FelicaC {

		//[MarshalAs(UnmanagedType.LPStruct)]
		IntPtr p;

		[MarshalAs(UnmanagedType.U2)]
		ushort systemcode;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
		byte[] IDm;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
		byte[] PMm;

		[MarshalAs(UnmanagedType.U1)]
		byte num_system_code;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
		ushort[] system_code;

		[MarshalAs(UnmanagedType.U1)]
		byte num_area_code;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
		ushort[] area_code;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
		ushort[] end_service_code;

		[MarshalAs(UnmanagedType.U1)]
		byte num_service_code;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 512)]
		ushort[] service_code;

		internal byte[] GetIDm() => IDm;

		internal byte[] GetPMm() => PMm;

		internal ushort GetSystemCode() => (ushort) ((systemcode & 0xff00) >> 8 | (systemcode & 0xff) << 8);

		internal ushort[] GetAreaCodes() => area_code;

		internal ushort[] GetEndServiceCodes() => end_service_code;

		internal ushort[] GetServiceCodes() => service_code;

		internal ushort[] GetSystemCodes() => system_code;

		internal byte GetSystemCodesLength() => num_system_code;

		internal byte GetAreaCodesLength() => num_area_code;

		internal byte GetServiceCodesLength() => num_service_code;
	}
}
