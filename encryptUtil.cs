//Monica Heim
//Anchorage,Alaska
//August 23,2020



using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

namespace CodingAssignmentAppleBluetooth
{
	class Program
	{
		static int THREADS = 8;
		static int BUFFER_SIZE = 10000000 / THREADS;

		// 22.91 s to encrypt originally
		// 0.70 s to copy
		// 12.32 s to encrypt with buffer and threads
		// 9.58 s after getting rid of MemoryStream
		// 9.36 s after less new allocation of memory (not too impressive)
		// 5.74 s with 100MB buffer and 8 threads
		static void Main(string[] args)
		{
			if (File.Exists("resultONEGB.txt")) File.Delete("resultONEGB.txt");
			Stopwatch sw = new Stopwatch();
			sw.Start();
			Console.WriteLine("Starting Encryption.");
			XOREncryptWithBufferAndThreads("resultONEGB.txt", "keyONEMB.txt", "plaintextONEGB.txt");
			Console.WriteLine($"{sw.Elapsed.TotalSeconds:F2} seconds to encrypt with buffer and threads");
			CheckFilesSame("resultONEGB.txt", "resultONEGBGood.txt");
			Console.WriteLine("Done.");
			Console.Read();
		}

		private static void XOREncrypt(byte[] output, byte[] key, byte[] plaintext, int offset, int length)
		{
			for (int i = 0; i < length; i++)
			{
				output[i] = (byte)(plaintext[i] ^ key[(i + offset) % key.Length]);
				if ((i + offset) % key.Length == key.Length - 1)
				{
					key = RotateKey(key);
				}
			}
		}

		private static void XOREncryptWithBufferAndThreads(Stream outputStream, byte[] key, Stream plaintext)
		{
			int numFullBlocks = (int)(plaintext.Length / BUFFER_SIZE);
			int numExtra = (int)(plaintext.Length % BUFFER_SIZE);
			int numThreadBlocks = (numFullBlocks + THREADS - 1) / THREADS;
			byte[][] inputBuffers = new byte[THREADS][];
			byte[][] outputBuffers = new byte[THREADS][];
			for (int i = 0; i < THREADS; i++)
			{
				inputBuffers[i] = new byte[BUFFER_SIZE];
				outputBuffers[i] = new byte[BUFFER_SIZE];
			}
			for (int i = 0; i < numThreadBlocks; i++)
			{
				int numThreads = i == numThreadBlocks - 1 ? numFullBlocks % THREADS : THREADS;
				for (int j = 0; j < numThreads; j++) plaintext.Read(inputBuffers[j], 0, BUFFER_SIZE);
				Parallel.For(0, numThreads, x =>
				{
					byte[] thisKey = RotateKey(key, (i * BUFFER_SIZE * THREADS + x) / key.Length);
					XOREncrypt(outputBuffers[x], thisKey, inputBuffers[x], i * BUFFER_SIZE * THREADS + x, BUFFER_SIZE);
				});
				for (int j = 0; j < numThreads; j++) outputStream.Write(outputBuffers[j], 0, BUFFER_SIZE);
			}
			if (numExtra > 0)
			{
				plaintext.Read(inputBuffers[0], 0, numExtra);
				byte[] thisKey = RotateKey(key, (numFullBlocks * BUFFER_SIZE) / key.Length);
				XOREncrypt(outputBuffers[0], thisKey, inputBuffers[0], numFullBlocks * BUFFER_SIZE, numExtra);
				outputStream.Write(outputBuffers[0], 0, numExtra);
			}
		}

		private static void XOREncryptWithBufferAndThreads(string outputPath, string keyPath, string plainttextPath)
		{
			using (var writer = File.OpenWrite(outputPath))
			{
				using (var reader = File.OpenRead(plainttextPath))
				{
					XOREncryptWithBufferAndThreads(writer, File.ReadAllBytes(keyPath), reader);
				}
			}
		}

		private static byte[] RotateKey(byte[] key, int x)
		{
			// rotate to the left by x bits
			byte[] newKey = new byte[key.Length];
			for (int i = 0; i < key.Length; i++)
			{
				byte prevByte = key[(i + x / 8) % key.Length];
				byte nextByte = key[(i + x / 8 + 1) % key.Length];
				newKey[i] = (byte)((prevByte << (x % 8)) + (nextByte >> (8 - x % 8)));
			}
			return newKey;
		}

		private static byte[] RotateKey(byte[] key)
		{
			// rotate to the left by one bit
			byte[] newKey = new byte[key.Length];
			for (int i = 0; i < key.Length; i++)
			{
				byte nextByte = key[(i + 1) % key.Length];
				newKey[i] = (byte)((key[i] << 1) + (nextByte >> 7));
			}
			return newKey;
		}

		private static void CheckFilesSame(string filePath1, string filePath2)
		{
			if (new FileInfo(filePath1).Length != new FileInfo(filePath2).Length) throw new NotImplementedException();
			using (var reader1 = File.OpenRead(filePath1))
			{
				using (var reader2 = File.OpenRead(filePath2))
				{
					int byte1 = reader1.ReadByte();
					int byte2 = reader2.ReadByte();
					if (byte1 != byte2) throw new NotImplementedException();
					if (byte1 < 0) return;
				}
			}
		}
	}
}
