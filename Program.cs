// See https://aka.ms/new-console-template for more information

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
    static bool PrintIfProblematic(FileSystemInfo info, int depth)
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

    static void Main(string[] args)
    {
        string path;
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
        foreach (var arg in args)
        {
            DirectoryInfo dir = new DirectoryInfo(arg);
            listPath(dir,0);
        }
    }

    static void listPath(DirectoryInfo dir, int depth)
    {
        foreach (DirectoryInfo subDirectory in dir.GetDirectories())
        {
            if (!PrintIfProblematic(subDirectory, depth))
                listPath(subDirectory, depth+1);
        }
        foreach (FileInfo file in dir.GetFiles())
        {
            PrintIfProblematic(file, depth);
        }
    }
}