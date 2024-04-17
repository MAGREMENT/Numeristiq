using Model.Sudokus;
using Model.Sudokus.Generator;

namespace Tests;

public class FilledSudokuGeneratorTests
{
    private const int SudokuCount = 5;

    private readonly IFilledSudokuGenerator _generator = new BackTrackingFilledSudokuGenerator();
    
    [Test]
    public void GenerationTest()
    {
        var suds = new Sudoku[SudokuCount];

        for (int i = 0; i < SudokuCount; i++)
        {
            suds[i] = _generator.Generate();
            Console.WriteLine(suds[i]);
        }
        
        Assert.Multiple(() =>
        {
            for (int i = 0; i < SudokuCount - 1; i++)
            {
                Assert.That(suds[i].IsComplete, Is.True);
                
                for (int j = i + 1; j < SudokuCount; j++)
                {
                    Assert.That(suds[j], Is.Not.EqualTo(suds[i]));
                }
            }
        });
    }
}