using System;
using System.IO;

namespace PackMule.Core
{
	/// <summary>
	/// Helper class that automatically deletes a file when this class goes out of scope
	/// </summary>
	class TempFileScope : IDisposable
	{
		public string Filename { get; set; }

		public TempFileScope()
			: this(Path.GetTempFileName())
		{ }

		public TempFileScope(string filename)
		{
			Filename = filename;
		}

		public void Dispose()
		{
			try
			{
				File.Delete(Filename);
			}
			catch { }

			GC.SuppressFinalize(this);
		}

		~TempFileScope()
		{
			Dispose();
		}
	}
}
