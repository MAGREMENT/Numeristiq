using Model.Tectonics;
using Model.Utility;
using Model.Utility.BitSets;
using Tests.Utility;

namespace Tests;

public class InfiniteBitmapTests
{
    [Test]
    public void AddAndRemoveTest()
    {
        var rCount = 8;
        var cCount = 10;
        
        var bitmap = new InfiniteBitmap(rCount, cCount);
        Cell[] toAdd = { new(0, 2), new(4, 4), new(6, 3), new(7, 9) };

        foreach (var cell in toAdd)
        {
            bitmap.Add(cell.Row, cell.Column);
        }
        Console.WriteLine(bitmap.ToString());
        
        Assert.Multiple(() =>
        {
            for (int r = 0; r < rCount; r++)
            {
                for (int c = 0; c < cCount; c++)
                {
                    Assert.That(bitmap.Contains(r, c), toAdd.Contains(new Cell(r, c)) ? Is.True : Is.False);
                }
            }
        });
        
        Cell[] toRemove = { new(1, 1), new(6, 3), new(7, 9) };
        
        foreach (var cell in toRemove)
        {
            bitmap.Remove(cell.Row, cell.Column);
        }
        Console.WriteLine(bitmap.ToString());
        
        Assert.Multiple(() =>
        {
            for (int r = 0; r < rCount; r++)
            {
                for (int c = 0; c < cCount; c++)
                {
                    var cell = new Cell(r, c);
                    var shouldBeContained = !toRemove.Contains(cell) && toAdd.Contains(cell);
                    Assert.That(bitmap.Contains(r, c), shouldBeContained ? Is.True : Is.False);
                }
            }
        });
    }

    [Test]
    public void Add3x3Test()
    {
        ImplementationSpeedComparator.Compare<Add3x3ToBitmap>((impl) =>
        {
            var bitmap = new TectonicBitmap(11, 12);

            for (int row = 0; row < bitmap.RowCount; row++)
            {
                for (int col = 0; col < bitmap.ColumnCount; col++)
                {
                    impl(bitmap, row, col);
                    
                    if (row > 0)
                    {
                        if (col > 0) Assert.That(bitmap.Contains(row - 1, col - 1), Is.True);
                        if (col < bitmap.ColumnCount - 1) Assert.That(bitmap.Contains(row - 1, col + 1), Is.True);
                        Assert.That(bitmap.Contains(row - 1, col), Is.True);
                    }
        
                    if (col > 0) Assert.That(bitmap.Contains(row, col - 1), Is.True);
                    if (col < bitmap.ColumnCount - 1) Assert.That(bitmap.Contains(row, col + 1), Is.True);
                    Assert.That(bitmap.Contains(row, col), Is.True);
        
                    if (row < bitmap.RowCount - 1)
                    {
                        if (col > 0) Assert.That(bitmap.Contains(row + 1, col - 1), Is.True);
                        if (col < bitmap.ColumnCount - 1) Assert.That(bitmap.Contains(row + 1, col + 1), Is.True);
                        Assert.That(bitmap.Contains(row + 1, col), Is.True);
                    }
                        
                    bitmap.Clear();
                }
            }
        }, 1, Add3x3ToBitmapNaively, Add3x3ToBitmapWithMethod);
    }

    private static void Add3x3ToBitmapWithMethod(TectonicBitmap bitmap, int row, int col)
    {
        bitmap.Add3x3(row, col);
    }

    private static void Add3x3ToBitmapNaively(TectonicBitmap bitmap, int row, int col)
    {
        if (row > 0)
        {
            if (col > 0) bitmap.Add(row - 1, col - 1);
            if (col < bitmap.ColumnCount - 1) bitmap.Add(row - 1, col + 1);
            bitmap.Add(row - 1, col);
        }
        
        if (col > 0) bitmap.Add(row, col - 1);
        if (col < bitmap.ColumnCount - 1) bitmap.Add(row, col + 1);
        bitmap.Add(row, col);
        
        if (row < bitmap.RowCount - 1)
        {
            if (col > 0) bitmap.Add(row + 1, col - 1);
            if (col < bitmap.ColumnCount - 1) bitmap.Add(row + 1, col + 1);
            bitmap.Add(row + 1, col);
        }
    }
}

public delegate void Add3x3ToBitmap(TectonicBitmap bitmap, int row, int col);