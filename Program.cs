// See https://aka.ms/new-console-template for more information

class Program
{
    //static FileAttributes attrDir = FileAttributes.Directory;
    //static FileAttributes attrArchive = FileAttributes.Archive;
    static bool PrintIfProblematic(FileSystemInfo info, int depth)
    {
        if (depth == 1 && info.Name == "Desktop.ini") // V hloubce 1 chci odpustit Desktop.ini slozky cisto, pc, atd ... maji ikonu
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
        if (info.Attributes.HasFlag(FileAttributes.ReparsePoint) && info.Name.EndsWith(".iso"))
            return false;
        if (info.FullName.Length > 250)
        {
            Console.WriteLine("Long: " + info.FullName);
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
            args = new string[] { Directory.GetCurrentDirectory() };
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