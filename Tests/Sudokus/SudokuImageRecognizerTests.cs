using Model.Core;
using Model.Sudokus;
using Model.Utility;

namespace Tests.Sudokus;

public class SudokuImageRecognizerTests
{
    private readonly IImagePuzzleRecognizer<Sudoku>[] _recognizers = Array.Empty<IImagePuzzleRecognizer<Sudoku>>();
    
    [Test]
    public void Test()
    {
        var imagePath = PathFinder.Find(@"Data\Recognition\SudokuTest.jpg", true, false);
        Assert.That(imagePath, Is.Not.Null);

        foreach (var r in _recognizers)
        {
            var sudoku = r.Recognize(imagePath!);

            Console.WriteLine(sudoku);
            Assert.That(sudoku, Is.Not.Null);
        }
    }
}