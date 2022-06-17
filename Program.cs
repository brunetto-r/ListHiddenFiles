// See https://aka.ms/new-console-template for more information
using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

/* Future work:
 * detect pathhs where folder name is used twice eg. folder1/folder1
 *  detect forbiden file names eg. deleteMe
 */
namespace ListHiddenFiles
{
	static class MyExtensions
	{
		public static IEnumerable<List<T>> MyGroupBy<T, Key>(this IEnumerable<T> sequence, Func<T, Key> getKey) //where T : IEqualityComparer<T>
		{
			var e = sequence.GetEnumerator();
			e.MoveNext();
			while (e.Current != null)
			{
				var group = new List<T>() { e.Current };
				Key key = getKey(e.Current);
				while (e.MoveNext() && e.Current != null && getKey(e.Current)!.Equals(key))
					group.Add(e.Current);
				yield return group;
			}
		}
	}

	class MyFileInfo
	{
		public long Size { get; set; }
		public string Path { get; set; }

		public MyFileInfo(long size, string path)
		{
			Size = size;
			Path = path;
		}
	}

	class MyFileInfoComparer : IComparer<MyFileInfo>
	{
		public int Compare(MyFileInfo? x, MyFileInfo? y)
		{
			if (x == null || y == null)
			{
				if (x == null && y == null)
					return 0;
				else if (x == null)
					return -1;
				else return 1;
			}
			if (x.Size != y.Size)
				return x.Size.CompareTo(y.Size);
			else
			{
				var file1 = new FileInfo(x.Path);
				var file2 = new FileInfo(y.Path);
				var extension1 = file1.Extension;
				var extension2 = file2.Extension;
				if (extension1 != extension2)
					return extension1.CompareTo(extension2);
				return x.Path.CompareTo(y.Path);
			}
		}
	}

	class MyGrouping<TKey, TElement> : IGrouping<TKey, TElement>
	{
		public TKey Key { get; set; }
		private readonly List<TElement> elements;

		public MyGrouping(TKey key, List<TElement> elements)
		{
			Key = key;
			this.elements = elements;
		}

		public IEnumerator<TElement> GetEnumerator()
		{
			return elements.GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}

	class Program
	{
		//static FileAttributes attrDir = FileAttributes.Directory;
		//static FileAttributes attrArchive = FileAttributes.Archive;

		/// <summary>
		///     Returns if file is problematic
		///     as a side effect prints problematic files and folders to user (with some exceptions user is not informed about $RECYCLE.BIN etc)
		/// </summary>
		/// <param name="info"></param>
		/// <param name="depth"></param>
		/// <returns></returns>
		static bool PrintIfProblematic(IFileOrFolder info, int depth)
		{
			if (info.Name == "$RECYCLE.BIN")
				return true;
			if (info.Name == "System Volume Information")
				return true;
			if (depth == 1 && info.Name.ToLower() == "Desktop.ini") // V hloubce 1 chci odpustit Desktop.ini slozky cisto, pc, atd ... maji ikonu
				return false;
			if (info.Name == ".owncloudsync.log")
				return false;
			if (info.Name == ".owncloudsync.log.1")
				return false;
			if (info.Name == ".sync_journal.db")
				return false;
			if (info.Name == ".sync_journal.db-shm")
				return false;
			if (info.Name == ".sync_journal.db-wal")
				return false;
			if (info.Name[0] == '.' && info.Name.Contains("sync") && info.Name.Contains(".db"))
				return false;
			if (info.Attributes.HasFlag(FileAttributes.ReparsePoint) && info.Name.EndsWith(".iso"))
				return false;
			if (info.FullName.Length > 250)
			{
				Console.WriteLine("Long: " + info.FullName);
				return true;
			}
			if (info.Name[0] == '.')
			{
				Console.WriteLine("Starts with dot: " + info.FullName);
				return true;
			}
			FileAttributes attributes = info.Attributes;
			attributes &= ~FileAttributes.NotContentIndexed;
			attributes &= ~FileAttributes.Archive;
			attributes &= ~FileAttributes.Directory;
			attributes &= ~FileAttributes.ReparsePoint;
			attributes &= ~FileAttributes.Offline;
			attributes &= ~(FileAttributes)(uint)524288; // S:\virtual\sbirky\Desktop.ini pinned and unpinned?
			attributes &= ~(FileAttributes)(uint)4194304; // nevim co to je, ale maj to veci virtualni soubory z cloudu
			attributes &= ~(FileAttributes)(uint)1048576; // uz si nepamatuju, ale mozna pinned nebo unpinned
			if (depth == 0)
				attributes &= ~FileAttributes.System; // V hloubce 0 odpoustim, kdyz je slozka systemova. cisto, pc, atd maji ikonu a jsou oznacene systemove
			if (attributes != 0)
			{
				Console.WriteLine($"{attributes} " + info.FullName);
				if (attributes == FileAttributes.System)
					return false;
				return true;
			}
			return false;
		}

		/// <summary>
		/// Turn a string into a CSV cell output
		/// </summary>
		/// <param name="str">String to output</param>
		/// <returns>The CSV cell formatted string</returns>
		public static string StringToCSVCell(string str)
		{
			bool mustQuote = (str.Contains(',') || str.Contains('\"') || str.Contains('\r') || str.Contains('\n'));
			if (mustQuote)
			{
				var sb = new StringBuilder();
				sb.Append('\"');
				foreach (char nextChar in str)
				{
					sb.Append(nextChar);
					if (nextChar == '"')
						sb.Append('\"');
				}
				sb.Append('\"');
				return sb.ToString();
			}

			return str;
		}

		static string ToLine(MyFileInfo myInfo)
		{
			var info = new System.IO.FileInfo(myInfo.Path);
			return string.Join(",",
				myInfo.Size,
				StringToCSVCell(info.Extension),
				StringToCSVCell(info.Name),
				StringToCSVCell(info.FullName)
			);
		}

		static void Main(string[] args)
		{
			if (args.Length == 0)
			{
				Console.WriteLine("Usage: ListHiddenFiles folder [more folders]");
				Console.WriteLine("Recursively searches the specified folders for file and folders with nonstandard permissions");
				Console.WriteLine("It prints all problems found.");
				Console.WriteLine("This app detects:");
				Console.WriteLine(" - hidden files and folders (specfied by attribute in windows)");
				Console.WriteLine(" - read only files and folders");
				Console.WriteLine(" - other attributes");
				Console.WriteLine(" - files with name starting with .");
				Console.WriteLine(" - long paths");
			}
			var fileInfos = new List<MyFileInfo>();
			foreach (var arg in args)
			{
				var dir = new DirectoryInfo(arg);
				RecursePath(new RealFolder(dir), 0, fileInfos);
			}
			fileInfos.Sort(new MyFileInfoComparer());
			var filtered = fileInfos
				.MyGroupBy(item => item.Size)
				.Where(group => group.Count > 1)
				.Select(group =>
					group.GroupBy(GetHash).Select(group => { return InteractiveDeletition(group); })
					.Where(g => g.ToList().Count > 1)
				)
				.SelectMany(x => x) // toto zapomene rozdeleni dle velikosti a mame skupinky dle stejneho hashe (a velikosti)
				.SelectMany(x => x); // toto zapomene na skupinky dle hashu a mame seznam souboru
			File.WriteAllLines("sameFiles.csv", filtered.Select(ToLine));
		}

		private static IGrouping<int, MyFileInfo> InteractiveDeletition(IGrouping<int, MyFileInfo> group)
		{
			var notDeletedElements = new List<MyFileInfo>();
			foreach (var folder in group.GroupBy(file => new FileInfo(file.Path).DirectoryName))
			{
				var folderList = new List<MyFileInfo>(folder.OrderBy(info => info.Path.Length));
				if (folderList.Count > 1)
				{
					Console.WriteLine("Which of these do you want to keep?");
					for (int i = 0; i < folderList.Count; i++)
					{
						Console.WriteLine($"{i + 1}. {folderList[i].Path}");
					}
					if (int.TryParse(Console.ReadLine(), out int index) && index >= 1 && index <= folderList.Count)
					{
						index--;
						for (int i = 0; i < folderList.Count; i++)
						{
							if (i == index)
							{
								notDeletedElements.Add(folderList[i]);
							}
							else
							{
								File.Delete(folderList[i].Path);
							}
						}
					}
					else
						Console.WriteLine("Wrong input");
				}
				else
					notDeletedElements.Add(folderList[0]);
			}
			return new MyGrouping<int, MyFileInfo>(group.Key, notDeletedElements);
		}

		static int GetHash(MyFileInfo info)
		{
			int hash = 0;
			unchecked
			{
				foreach (byte byte1 in File.ReadAllBytes(info.Path))
				{
					hash = hash * 257 + byte1;
				}
			}
			return hash;
		}

		static void RecursePath(IFolder dir, int depth, List<MyFileInfo> fileInfos)
		{
			var directories = dir.GetDirectories();
			foreach (var subDirectory in directories)
			{
				if (!PrintIfProblematic(subDirectory, depth))
					RecursePath(subDirectory, depth + 1, fileInfos);
			}
			var files = dir.GetFiles();
			foreach (var file in files)
			{
				fileInfos.Add(new(file.Size, file.FullName));
				PrintIfProblematic(file, depth);
			}
		}
	}
}