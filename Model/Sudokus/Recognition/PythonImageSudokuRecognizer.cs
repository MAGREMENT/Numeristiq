using System.Diagnostics;
using Model.Core;

namespace Model.Sudokus.Recognition;

public class PythonImageSudokuRecognizer : IImagePuzzleRecognizer<Sudoku>
{
    public Sudoku? Recognize(string path)
    {
        var start = new ProcessStartInfo
        {
            FileName = "", //TODO
            Arguments = path,
            UseShellExecute = false,
            RedirectStandardOutput = true
        };
        using var process = Process.Start(start);
        if (process is null) return null;
        
        var reader = process.StandardOutput;
        return SudokuTranslator.TranslateLineFormat(reader.ReadToEnd());
    }
}