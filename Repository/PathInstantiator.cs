namespace Repository;

public class PathInstantiator
{
    private readonly bool _searchParentDirectories;
    private readonly bool _createIfNotFound;

    public PathInstantiator(bool searchParentDirectories, bool createIfNotFound)
    {
        _searchParentDirectories = searchParentDirectories;
        _createIfNotFound = createIfNotFound;
    }

    public string Instantiate(string fileName)
    {
        var directory = Directory.GetCurrentDirectory();
        var path = $@"{directory}\{fileName}";
        bool exists = File.Exists(path);

        if (_searchParentDirectories && !exists)
        {
            var buffer = Directory.GetParent(path);
            while (buffer is not null)
            {
                var p = $@"{buffer.FullName}\{fileName}";
                if (File.Exists(p))
                {
                    path = p;
                    exists = true;
                    break;
                }

                buffer = Directory.GetParent(buffer.FullName);
            }
        }

        if (_createIfNotFound && !exists)
        {
            try
            {
                using var stream = File.Create(path);
            }
            catch (Exception)
            {
                //Ignored
            }
        }

        return path;
    }
}