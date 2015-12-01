using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Documents;
using PackMule.Core;
using ManyConsole;

namespace PackMule
{
	internal class Program
	{
		public static void Main(string[] args)
		{
			if (args.Length == 0)
			{
				Console.WriteLine("UI is unimplemented. Try 'help' for usage.");
			}
			else
			{
				ConsoleCommandDispatcher.DispatchCommand(ConsoleCommandDispatcher.FindCommandsInSameAssemblyAs(typeof(Program)),
					args, Console.Out);
			}
		}
	}

	/// <summary>
	/// extract [files] [-p=packfile [-p ...]] [-x=exclude [-e ...]] [-s=package_folder] [-d=dest_folder] [-o] [-e] [-minv] [-maxv]
	/// </summary>
	internal class ExtractCommand : ConsoleCommand
	{
		private readonly HashSet<string> _packfiles = new HashSet<string>();
		private readonly HashSet<string> _excludedPackfiles = new HashSet<string>();

		private string _packageFolder = "./package";
		private string _destFolder = ".";

		private bool _officialOnly = false;
		private bool _useRegex = false;

		private long _minV = 0;
		private long _maxV = uint.MaxValue;

		private static readonly Regex OfficialPattern = new Regex(@"(\d+_full\.pack)|(\d+_to_\d+\.pack)", RegexOptions.IgnoreCase);
		
		public ExtractCommand()
		{
			SkipsCommandSummaryBeforeRunning();

			IsCommand("Extract", "Extracts files from .packs");

			AllowsAnyAdditionalArguments("[file [file ...]]");

			HasOption("p|packfile=", "One or more packfiles to search. If omitted, searches all pack files.",
				a => _packfiles.Add(a));
			HasOption("x|exclude=", "One or more packfiles to exclude.",
				a => _excludedPackfiles.Add(a));

			HasOption("s|packagefolder=", "Folder containing pack files to search. Defaults to 'package'",
				a => _packageFolder = a);
			HasOption("d|destinationfolder=", "Folder that will contain the extracted data folder. Defaults to current directory.",
				a => _destFolder = a);

			HasOption("o", "Include only pack files matching the official naming scheme", a => _officialOnly = true);
			HasOption("e", "If specified, filename matching uses regex syntax.", a => _useRegex = true);

			HasOption("minv=", "Minimum packfile revision number. Defaults to 0", a => _minV = uint.Parse(a));
			HasOption("maxv=", "Maximum packfile revision number. Defaults to " + ((long)uint.MaxValue).ToString("N0"), a => _maxV = uint.Parse(a));
		}

		public override int Run(string[] remainingArguments)
		{
			var toExtract = new List<Regex>();

			if (remainingArguments.Length == 0)
				toExtract.Add(new Regex(""));
			else
			{
				if (_useRegex)
				{
					toExtract.AddRange(remainingArguments.Select(a => new Regex(a.Replace(@"\\", "/"), RegexOptions.IgnoreCase)));
				}
				else
				{
					toExtract.AddRange(remainingArguments.Select(a => new Regex($@"{ Regex.Escape(a).Replace(@"\", "/") }$", RegexOptions.IgnoreCase)));
				}
			}


			var invalidChars = Path.GetInvalidPathChars();
			var packfiles = GetPackFiles();

			Directory.CreateDirectory(_destFolder);
			int nPf = 0, nExtracted = 0;

			var s = Stopwatch.StartNew();

			foreach (var pf in packfiles.Where(a => _minV <= a.Revision && a.Revision <= _maxV).OrderBy(a => a.Revision))
			{
				Console.WriteLine($"Processing {pf.Name}...");

				foreach (var e in pf)
				{
					var fullName = Path.Combine(pf.Root, e.Name);

					var normalizedFn = fullName.Normalize();

					if (toExtract.Any(a => a.IsMatch(normalizedFn)))
					{
						Console.Write($"\tExtracting {fullName} ... ");

						fullName = invalidChars.Aggregate(fullName, (current, c) => current.Replace(c, '?'));

						var outName = Path.Combine(_destFolder, fullName);
						Directory.CreateDirectory(Path.GetDirectoryName(outName));

						using (var f = File.OpenWrite(outName))
							pf.Extract(e).CopyTo(f);
						Console.WriteLine("Done");

						nExtracted++;
					}
				}

				nPf++;
			}

			s.Stop();

			Console.WriteLine($"Extracted {nExtracted.ToString("N0")} files from {nPf.ToString("N0")} packs in {s.Elapsed}");

			foreach (var f in packfiles)
				f.Dispose();

			return 0;
		}

		private List<PackFile> GetPackFiles()
		{
			var excludedNames = new List<Regex>();
			if (_useRegex)
			{
				excludedNames.AddRange(_excludedPackfiles.Select(a => new Regex(a, RegexOptions.IgnoreCase)));
			}
			else
			{
				excludedNames.AddRange(_excludedPackfiles.Select(a => new Regex($@"^{ Regex.Escape(a) }$", RegexOptions.IgnoreCase)));
			}

			var packfileNames = _packfiles.ToList();
			if (packfileNames.Count == 0)
				packfileNames = Directory.GetFiles(_packageFolder, "*.pack").ToList();

			var packFiles = new List<PackFile>();
			foreach (var fn in packfileNames)
			{
				var fullFn = Path.GetFullPath(fn);

				if (!File.Exists(fullFn))
				{
					fullFn = Path.GetFullPath(Path.Combine(_packageFolder, fn));
					if (!File.Exists(fullFn))
						throw new FileNotFoundException("Cannot find specified pack file", fn);
				}

				if (excludedNames.Any(a => a.IsMatch(fullFn)) || (_officialOnly && !OfficialPattern.IsMatch(fullFn)))
					continue;

				packFiles.Add(new PackFile(File.Open(fullFn, FileMode.Open), Path.GetFileName(fn)));
			}

			return packFiles;
		}
	}

	class PackCommand : ConsoleCommand
	{
		public PackCommand()
		{
			SkipsCommandSummaryBeforeRunning();
			IsCommand("Pack", "Packs files into a .pack file");
		}

		public override int Run(string[] remainingArguments)
		{
			throw new NotImplementedException();
		}
	}
}
