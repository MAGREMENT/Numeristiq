using System;
using System.IO;

namespace Model.Util;

public static class PathsInfo
{
    private const string SolutionDirectoryName = "SudokuSolver";

    private static string _pathToSolution = "";
    private static bool _pathToSolutionIsFound = false;

    public static string PathToData()
    {
        return GetPathToSolution() + @"\Model\Data";
    }

    private static string GetPathToSolution()
    {
        if (!_pathToSolutionIsFound)
        {
            _pathToSolution = FindSolutionPath();
            _pathToSolutionIsFound = true;
        }

        return _pathToSolution;
    }

    //This is stupid but whatever
    private static string FindSolutionPath()
    {
        var current = Directory.GetCurrentDirectory();
        var dir = current.Substring(current.LastIndexOf("\\", StringComparison.Ordinal) + 1);
        while (!dir.Equals(SolutionDirectoryName) || !File.Exists(current + @"\Model\Data\strategies.ini"))
        {
            var buffer = Directory.GetParent(current);
            if (buffer is null) throw new Exception();

            current = buffer.FullName;
            dir = current.Substring(current.LastIndexOf("\\", StringComparison.Ordinal) + 1);
        }
        
        return current;
    }
}