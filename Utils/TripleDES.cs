using System;
using System.Security.Cryptography;

namespace FelicaLiteLib.Utils {

	internal class TripleDES : IDisposable {

		readonly DESCryptoServiceProvider DES_A = new DESCryptoServiceProvider();

		readonly DESCryptoServiceProvider DES_B = new DESCryptoServiceProvider();

		readonly DESCryptoServiceProvider DES_C = new DESCryptoServiceProvider();

		readonly ICryptoTransform DES_A_E;

		readonly ICryptoTransform DES_B_E;

		readonly ICryptoTransform DES_C_E;

		internal TripleDES(byte[] A, byte[] B, byte[] C, bool CBCMode = false) {
			DES_A.Padding = DES_B.Padding = DES_C.Padding = PaddingMode.None;
			DES_A.Mode = DES_B.Mode = DES_C.Mode = CBCMode ? CipherMode.CBC : CipherMode.ECB;
			DES_A.Key = DES_A.IV = A;
			DES_B.Key = DES_B.IV = B;
			DES_C.Key = DES_C.IV = C;

			DES_A_E = DES_A.CreateEncryptor();
			DES_B_E = DES_B.CreateDecryptor();
			DES_C_E = DES_C.CreateEncryptor();
		}

		internal byte[] Encrypto(byte[] Data) {
			byte[] Encrypted_A = DES_A_E.TransformFinalBlock(Data, 0, Data.Length);
			byte[] Decrypted_B = DES_B_E.TransformFinalBlock(Encrypted_A, 0, Encrypted_A.Length);
			return DES_C_E.TransformFinalBlock(Decrypted_B, 0, Decrypted_B.Length);
		}

		public void Dispose() {
			DES_C_E.Dispose();
			DES_B_E.Dispose();
			DES_A_E.Dispose();
			DES_C.Dispose();
			DES_B.Dispose();
			DES_A.Dispose();
		}

		~TripleDES() {
			Dispose();
		}
	}
}
