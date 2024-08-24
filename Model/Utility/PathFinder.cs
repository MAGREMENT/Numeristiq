using System.IO;

namespace Model.Utility;

public static class PathFinder
{
    public static string? Find(string fileName, bool searchParentDirectories, bool createIfNotFound)
    {
        var directory = Directory.GetCurrentDirectory();
        var initialPath = $@"{directory}\{fileName}";
        if (File.Exists(initialPath)) return initialPath;
        
        if (searchParentDirectories)
        {
            var buffer = Directory.GetParent(initialPath);
            while (buffer is not null)
            {
                var p = $@"{buffer.FullName}\{fileName}";
                if (File.Exists(p))
                {
                    return p;
                }

                buffer = Directory.GetParent(buffer.FullName);
            }
        }

        if (createIfNotFound)
        {
            try
            {
                using var stream = File.Create(initialPath);
                return initialPath;
            }
            catch
            {
                //ignored
            }
        }

        return null;
    }
}