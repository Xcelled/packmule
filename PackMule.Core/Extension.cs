using System;

namespace PackMule.Core
{
	public static class Extension
	{
		/// <summary>
		/// Compares the given string to the current instance, as paths.
		/// </summary>
		/// <param name="str1">The STR1.</param>
		/// <param name="str2">The STR2.</param>
		/// <returns><c>true</c> if the two are case-insensitively equal, <c>false</c> otherwise.</returns>
		public static bool ComparePaths(this string str1, string str2)
		{
			return str1.Replace("\\", "/").Equals(str2.Replace("\\", "/"), StringComparison.OrdinalIgnoreCase);
		}

		/// <summary>
		/// Normalizes a path.
		/// </summary>
		/// <param name="str">The string.</param>
		/// <returns>System.String.</returns>
		public static string NormalizePath(this string str)
		{
			return str.ToUpperInvariant().Replace("\\", "/");
		}
	}
}
