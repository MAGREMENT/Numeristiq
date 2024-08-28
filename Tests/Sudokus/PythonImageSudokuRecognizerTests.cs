using Model.Sudokus.Recognition;
using Model.Utility;

namespace Tests.Sudokus;

public class PythonImageSudokuRecognizerTests
{
    [Test]
    public void OutputTest()
    {
        var imagePath = PathFinder.Find(@"Data\Recognition\SudokuTest.jpg", true, false);
        Assert.That(imagePath, Is.Not.Null);

        var recognizer = new PythonImageSudokuRecognizer(true);
        var sudoku = recognizer.Recognize(imagePath!);

        Console.WriteLine(sudoku);
        Assert.That(sudoku, Is.Not.Null);
    }
}