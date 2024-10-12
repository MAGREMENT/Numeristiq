using System.IO;
using Model.Utility;

namespace Model.Core.Descriptions;

public abstract class DescriptionParser<T> where T : IDescriptionDisplayer
{
    private readonly string? _path;

    protected DescriptionParser(string directory, bool searchParentDirectories, bool createIfNotFound)
    {
        _path = DirectoryFinder.Find(directory, searchParentDirectories, createIfNotFound);
    }

    public IDescription<T> Get(string name)
    {
        if (_path is null) return DefaultDescription<T>.Instance;

        var buffer = $@"{_path}\{SpacedToCamelCaseConverter.Instance.Convert(name)}";
        var file = buffer + ".xml";
        if (File.Exists(file)) return ParseXml(file);

        file = buffer + ".txt";
        return !File.Exists(file) ? DefaultDescription<T>.Instance : ParseTxt(file);
    }

    protected abstract IDescription<T> ParseTxt(string path);
    protected abstract IDescription<T> ParseXml(string path);
}

public static class DefaultDescription<T> where T : IDescriptionDisplayer
{
    public static IDescription<T> Instance { get; } = new TextDescription<T>("No Description Found");
}