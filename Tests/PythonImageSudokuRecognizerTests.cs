using System.Diagnostics;
using Model.Utility;

namespace Tests;

public class PythonImageSudokuRecognizerTests
{
    [Test]
    public void StringOutputTest()
    {
        var exePath =  PathFinder.Find(@"Python\venv\Scripts\python.exe", true, false);
        Assert.That(exePath, Is.Not.Null);
        
        var pyPath = PathFinder.Find(@"Python\main.py", true, false);
        Assert.That(pyPath, Is.Not.Null);

        var imagePath = PathFinder.Find(@"Data\Recognition\SudokuTest.jpg", true, false);
        Assert.That(imagePath, Is.Not.Null);

        var start = new ProcessStartInfo(exePath!, pyPath + " " + imagePath)
        {
            UseShellExecute = false,
            RedirectStandardOutput = true
        };
        using var process = Process.Start(start);
        Assert.That(process, Is.Not.Null);
        
        var output = process!.StandardOutput.ReadToEnd();
        
        Console.WriteLine(output);
        Assert.That(output, Has.Length.EqualTo(81));
    }
}