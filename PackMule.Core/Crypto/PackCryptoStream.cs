using System;
using System.IO;

namespace PackMule.Core.Crypto
{
	/// <summary>
	/// A stream that applies Mabinogi PACK "encryption" to its data.
	/// </summary>
	class PackCryptoStream : Stream
	{
		/// <summary>
		/// The underlying stream
		/// </summary>
		private readonly Stream _orig;
		/// <summary>
		/// The Mersenne Twister
		/// </summary>
		private readonly MersenneTwister _mt = new MersenneTwister();

		/// <summary>
		/// Initializes a new instance of the <see cref="PackCryptoStream"/> class.
		/// </summary>
		/// <param name="orig">The original.</param>
		/// <param name="seed">The seed.</param>
		public PackCryptoStream(Stream orig, int seed)
		{
			_orig = orig;

			var rSeed = (uint)((seed << 7) ^ 0xA9C36DE1);
			_mt.Init(rSeed);
		}

		/// <summary>
		/// When overridden in a derived class, gets a value indicating whether the current stream supports reading.
		/// </summary>
		/// <value><c>true</c> if this instance can read; otherwise, <c>false</c>.</value>
		public override bool CanRead
		{
			get { return _orig.CanRead; }
		}

		/// <summary>
		/// When overridden in a derived class, gets a value indicating whether the current stream supports seeking.
		/// </summary>
		/// <value><c>true</c> if this instance can seek; otherwise, <c>false</c>.</value>
		public override bool CanSeek
		{
			get { return false; }
		}

		/// <summary>
		/// When overridden in a derived class, gets a value indicating whether the current stream supports writing.
		/// </summary>
		/// <value><c>true</c> if this instance can write; otherwise, <c>false</c>.</value>
		public override bool CanWrite
		{
			get { return _orig.CanWrite; }
		}

		/// <summary>
		/// When overridden in a derived class, clears all buffers for this stream and causes any buffered data to be written to the underlying device.
		/// </summary>
		public override void Flush()
		{
			_orig.Flush();
		}

		/// <summary>
		/// When overridden in a derived class, gets the length in bytes of the stream.
		/// </summary>
		/// <value>The length.</value>
		public override long Length
		{
			get { return _orig.Length; }
		}

		/// <summary>
		/// When overridden in a derived class, gets or sets the position within the current stream.
		/// </summary>
		/// <value>The position.</value>
		/// <exception cref="System.NotSupportedException"></exception>
		public override long Position
		{
			get
			{
				return _orig.Position;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// When overridden in a derived class, reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
		/// </summary>
		/// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between <paramref name="offset" /> and (<paramref name="offset" /> + <paramref name="count" /> - 1) replaced by the bytes read from the current source.</param>
		/// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> at which to begin storing the data read from the current stream.</param>
		/// <param name="count">The maximum number of bytes to be read from the current stream.</param>
		/// <returns>The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.</returns>
		/// <exception cref="System.NotImplementedException"></exception>
		public override int Read(byte[] buffer, int offset, int count)
		{
			var read = _orig.Read(buffer, offset, count);

			for (var i = 0; i < read; i++)
			{
				buffer[offset + i] = (byte)(buffer[offset + i] ^ _mt.GenRand());
			}

			return read;
		}

		/// <summary>
		/// When overridden in a derived class, sets the position within the current stream.
		/// </summary>
		/// <param name="offset">A byte offset relative to the <paramref name="origin" /> parameter.</param>
		/// <param name="origin">A value of type <see cref="T:System.IO.SeekOrigin" /> indicating the reference point used to obtain the new position.</param>
		/// <returns>The new position within the current stream.</returns>
		/// <exception cref="System.NotImplementedException"></exception>
		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// When overridden in a derived class, sets the length of the current stream.
		/// </summary>
		/// <param name="value">The desired length of the current stream in bytes.</param>
		/// <exception cref="System.NotImplementedException"></exception>
		public override void SetLength(long value)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// When overridden in a derived class, writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
		/// </summary>
		/// <param name="buffer">An array of bytes. This method copies <paramref name="count" /> bytes from <paramref name="buffer" /> to the current stream.</param>
		/// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> at which to begin copying bytes to the current stream.</param>
		/// <param name="count">The number of bytes to be written to the current stream.</param>
		/// <exception cref="System.NotImplementedException"></exception>
		public override void Write(byte[] buffer, int offset, int count)
		{
			var toWrite = new byte[count];

			Buffer.BlockCopy(buffer, offset, toWrite, 0, count);

			for (var i = 0; i < count; i++)
			{
				toWrite[i] = (byte)(toWrite[i] ^ _mt.GenRand());
			}

			_orig.Write(toWrite, 0, count);
		}
	}
}
