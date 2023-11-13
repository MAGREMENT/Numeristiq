using System;
using System.IO;

namespace Model.Util;

public class PathsManager
{
    private static PathsManager? _instance;

    public static PathsManager GetInstance()
    {
        _instance ??= new PathsManager();
        return _instance;
    }

    private string _pathToIniFile = "";
    private bool _pathToIniFileSearched;
    
    private PathsManager() {}

    public string GetPathToIniFile()
    {
        if (!_pathToIniFileSearched)
        {
            _pathToIniFile = SearchPathToIniFile();
            _pathToIniFileSearched = true;
        }

        return _pathToIniFile;
    }

    //This is stupid but whatever
    private static string SearchPathToIniFile()
    {
        var current = Directory.GetCurrentDirectory();
        while (!File.Exists(current + @"\strategies.ini"))
        {
            var buffer = Directory.GetParent(current);
            if (buffer is null) throw new Exception();

            current = buffer.FullName;
        }

        return current + @"\strategies.ini";
    }
}