using Cedict;
using System;

namespace Cedict.Example
{
	internal static class Program
	{
		private static int Main(string[] args)
		{
			if (args.Length < 2)
			{
				Console.WriteLine("Usage: Cedict.Example.exe <path> <search-term>");

				return 1;
			}

			var path = args[0];
			var value = args[1];

			var dict = Dict.FromFile(path);

			var options = new SearchOptions(
				targets: Targets.Traditional | Targets.Simplified,
				match: Match.Full,
				limit: 3
			);

			var results = dict.Search(options, value);

			foreach (var entry in results)
			{
				PrintEntry(entry);
			}

			return 0;
		}

		private static void PrintEntry(Entry entry)
		{
			Console.WriteLine($"Traditional: {entry.Traditional}");
			Console.WriteLine($"Simplified: {entry.Simplified}");
			Console.WriteLine($"Pinyin: {entry.Pinyin}");
			Console.Write("English: ");
			Console.WriteLine(String.Join(" / ", entry.English));
			Console.WriteLine();
		}
	}
}
