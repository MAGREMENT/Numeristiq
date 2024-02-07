using Model.Sudoku;
using Model.Sudoku.Generator;

namespace Tests;

public class FilledSudokuGeneratorTests
{
    private const int SudokuCount = 5;
    
    [Test]
    public void TestGeneration()
    {
        var suds = new Sudoku[SudokuCount];
        var generator = new BackTrackingFilledSudokuGenerator();

        for (int i = 0; i < SudokuCount; i++)
        {
            suds[i] = generator.Generate();
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