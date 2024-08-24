using System.Diagnostics;
using Model.Core;
using Model.Utility;

namespace Model.Sudokus.Recognition;

public class PythonImageSudokuRecognizer : IImagePuzzleRecognizer<Sudoku>
{
    private readonly string? _executablePath;
    private readonly string? _scriptPath;
    
    public PythonImageSudokuRecognizer(bool searchParentDirectories)
    {
        _executablePath = PathFinder.Find(@"Python\venv\Scripts\python.exe", searchParentDirectories, false);
        _scriptPath = PathFinder.Find(@"Python\main.py", searchParentDirectories, false);
    }
    
    public Sudoku? Recognize(string path)
    {
        if (_executablePath is null) return null;
        
        var start = new ProcessStartInfo(_executablePath, _scriptPath + " " + path)
        {
            UseShellExecute = false,
            RedirectStandardOutput = true
        };
        using var process = Process.Start(start);
        if (process is null) return null;
        
        var output = process.StandardOutput.ReadToEnd();
        if (output.Length == 0 || output[0] == '_') return null;
        
        return SudokuTranslator.TranslateLineFormat(output);
    }
}