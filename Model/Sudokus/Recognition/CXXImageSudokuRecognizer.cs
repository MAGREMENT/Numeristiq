using System.Diagnostics;
using Model.Core;
using Model.Utility;

namespace Model.Sudokus.Recognition;

public class CXXImageSudokuRecognizer : IImagePuzzleRecognizer<Sudoku>
{
    private readonly string? _executablePath;
    
    public CXXImageSudokuRecognizer(bool searchParentDirectories)
    {
        //TODO put correct path
        _executablePath = PathFinder.Find("", searchParentDirectories, false);
    }
    
    public Sudoku? Recognize(string path)
    {
        if (_executablePath is null) return null;
        
        var start = new ProcessStartInfo(_executablePath, path)
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