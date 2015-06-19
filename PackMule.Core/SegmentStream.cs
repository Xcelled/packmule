using System;
using System.IO;

namespace PackMule.Core
{
	/// <summary>
	/// A class that allows reading a chunk of an existing stream as if it were a discrete, finite one.
	/// </summary>
	class SegmentStream : Stream
	{
		private readonly Stream _other;
		private readonly long _start, _count;

		private readonly bool _disposeOther;

		/// <summary>
		/// Initializes a new instance of the <see cref="SegmentStream" /> class.
		/// </summary>
		/// <param name="other">The source stream.</param>
		/// <param name="start">The position at which the chunk starts.</param>
		/// <param name="count">The number of bytes in the chunk.</param>
		/// <param name="disposeOther">if set to <c>true</c>, this stream will call Dispose on <c ref="other" /> when it is disposed.</param>
		/// <exception cref="System.ArgumentException">Must be seekable.</exception>
		public SegmentStream(Stream other, long start, long count, bool disposeOther = false)
		{
			if (!other.CanSeek)
				throw new ArgumentException("Must be seekable.", "other");

			_other = other;
			_start = start;
			_count = count;
			_disposeOther = disposeOther;

			_other.Position = _start;
		}

		/// <summary>
		/// When overridden in a derived class, clears all buffers for this stream and causes any buffered data to be written to the underlying device.
		/// </summary>
		/// <exception cref="T:System.IO.IOException">An I/O error occurs.</exception>
		public override void Flush()
		{
			_other.Flush();
		}

		/// <summary>
		/// When overridden in a derived class, sets the position within the current stream.
		/// </summary>
		/// <param name="offset">A byte offset relative to the <paramref name="origin" /> parameter.</param>
		/// <param name="origin">A value of type <see cref="T:System.IO.SeekOrigin" /> indicating the reference point used to obtain the new position.</param>
		/// <returns>The new position within the current stream.</returns>
		/// <exception cref="System.NotImplementedException"></exception>
		/// <exception cref="T:System.IO.IOException">An I/O error occurs.</exception>
		/// <exception cref="T:System.NotSupportedException">The stream does not support seeking, such as if the stream is constructed from a pipe or console output.</exception>
		/// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed.</exception>
		public override long Seek(long offset, SeekOrigin origin)
		{
			switch (origin)
			{
				case SeekOrigin.Begin:
					Position = offset;
					break;

				case SeekOrigin.Current:
					Position += offset;
					break;

				case SeekOrigin.End:
					Position = _count - offset;
					break;
			}

			return Position;
		}

		/// <summary>
		/// When overridden in a derived class, sets the length of the current stream.
		/// </summary>
		/// <param name="value">The desired length of the current stream in bytes.</param>
		/// <exception cref="System.NotSupportedException"></exception>
		/// <exception cref="T:System.IO.IOException"></exception>
		/// <exception cref="T:System.NotSupportedException"></exception>
		/// <exception cref="T:System.ObjectDisposedException">An I/O error occurs.</exception>
		public override void SetLength(long value)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// When overridden in a derived class, reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
		/// </summary>
		/// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between <paramref name="offset" /> and (<paramref name="offset" /> + <paramref name="count" /> - 1) replaced by the bytes read from the current source.</param>
		/// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> at which to begin storing the data read from the current stream.</param>
		/// <param name="count">The maximum number of bytes to be read from the current stream.</param>
		/// <returns>The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.</returns>
		/// <exception cref="System.NotImplementedException"></exception>
		/// <exception cref="T:System.ArgumentException">The sum of <paramref name="offset" /> and <paramref name="count" /> is larger than the buffer length.</exception>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="buffer" /> is null.</exception>
		/// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="offset" /> or <paramref name="count" /> is negative.</exception>
		/// <exception cref="T:System.IO.IOException">An I/O error occurs.</exception>
		/// <exception cref="T:System.NotSupportedException">The stream does not support reading.</exception>
		/// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed.</exception>
		public override int Read(byte[] buffer, int offset, int count)
		{
			if (Position == _count)
				return 0;

			var toCopy = Math.Min((int)(_count - Position), count);

			_other.Read(buffer, offset, toCopy);

			return toCopy;
		}

		/// <summary>
		/// When overridden in a derived class, writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
		/// </summary>
		/// <param name="buffer">An array of bytes. This method copies <paramref name="count" /> bytes from <paramref name="buffer" /> to the current stream.</param>
		/// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> at which to begin copying bytes to the current stream.</param>
		/// <param name="count">The number of bytes to be written to the current stream.</param>
		/// <exception cref="System.NotSupportedException"></exception>
		public override void Write(byte[] buffer, int offset, int count)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// When overridden in a derived class, gets a value indicating whether the current stream supports reading.
		/// </summary>
		/// <value><c>true</c> if this instance can read; otherwise, <c>false</c>.</value>
		public override bool CanRead
		{
			get { return true; }
		}

		/// <summary>
		/// When overridden in a derived class, gets a value indicating whether the current stream supports seeking.
		/// </summary>
		/// <value><c>true</c> if this instance can seek; otherwise, <c>false</c>.</value>
		public override bool CanSeek
		{
			get { return true; }
		}

		/// <summary>
		/// When overridden in a derived class, gets a value indicating whether the current stream supports writing.
		/// </summary>
		/// <value><c>true</c> if this instance can write; otherwise, <c>false</c>.</value>
		public override bool CanWrite
		{
			get { return false; }
		}

		/// <summary>
		/// When overridden in a derived class, gets the length in bytes of the stream.
		/// </summary>
		/// <value>The length.</value>
		/// <exception cref="T:System.NotSupportedException">A class derived from Stream does not support seeking.</exception>
		/// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed.</exception>
		public override long Length
		{
			get { return _count; }
		}

		/// <summary>
		/// When overridden in a derived class, gets or sets the position within the current stream.
		/// </summary>
		/// <value>The position.</value>
		/// <exception cref="System.IO.EndOfStreamException"></exception>
		/// <exception cref="T:System.IO.IOException"></exception>
		/// <exception cref="T:System.NotSupportedException">An I/O error occurs.</exception>
		/// <exception cref="T:System.ObjectDisposedException">The stream does not support seeking.</exception>
		public override long Position
		{
			get
			{
				return _other.Position - _start;
			}
			set
			{
				if (value < 0 || value > _count)
					throw new EndOfStreamException();

				_other.Position = _start + value;
			}
		}

		/// <summary>
		/// Releases the unmanaged resources used by the <see cref="T:System.IO.Stream" /> and optionally releases the managed resources.
		/// </summary>
		/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && _disposeOther)
				_other.Dispose();

			base.Dispose(disposing);
		}
	}
}
