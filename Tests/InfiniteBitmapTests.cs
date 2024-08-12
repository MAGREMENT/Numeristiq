using Model.Utility;
using Model.Utility.BitSets;

namespace Tests;

public class InfiniteBitmapTests
{
    [Test]
    public void AddAndRemoveTest()
    {
        const int rCount = 8;
        const int cCount = 10;
        
        var bitmap = new InfiniteBitMap(rCount, cCount);
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
    public void HasNeighborTest()
    {
        var bitmap = new InfiniteBitMap(11, 12);
        
        for (int row = 0; row < bitmap.RowCount; row++)
        {
            for (int col = 0; col < bitmap.ColumnCount; col++)
            {
                bitmap.Add(row, col);
                    
                if (row > 0)
                {
                    if (col > 0) Assert.That(bitmap.HasNeighbor(row - 1, col - 1), Is.True);
                    if (col < bitmap.ColumnCount - 1) Assert.That(bitmap.HasNeighbor(row - 1, col + 1), Is.True);
                    Assert.That(bitmap.HasNeighbor(row - 1, col), Is.True);
                }
        
                if (col > 0) Assert.That(bitmap.HasNeighbor(row, col - 1), Is.True);
                if (col < bitmap.ColumnCount - 1) Assert.That(bitmap.HasNeighbor(row, col + 1), Is.True);
                Assert.That(bitmap.HasNeighbor(row, col), Is.True);
        
                if (row < bitmap.RowCount - 1)
                {
                    if (col > 0) Assert.That(bitmap.HasNeighbor(row + 1, col - 1), Is.True);
                    if (col < bitmap.ColumnCount - 1) Assert.That(bitmap.HasNeighbor(row + 1, col + 1), Is.True);
                    Assert.That(bitmap.HasNeighbor(row + 1, col), Is.True);
                }
                        
                bitmap.Clear();
            }
        }
    }

    [Test]
    public void FillAndEmptyNessTest()
    {
        var bm = new CalibratedInfiniteBitMap(14, 12);
        bm.FillRow(1);
        bm.FillRow(13);
        bm.FillColumn(5);
        bm.FillColumn(7);
        Console.WriteLine(bm);
        
        for (int r = 0; r < bm.RowCount; r++)
        {
            for (int c = 0; c < bm.ColumnCount; c++)
            {
                if (r is 1 or 13 || c is 5 or 7) Assert.That(bm.Contains(r, c), Is.True);
                else Assert.That(bm.Contains(r, c), Is.False);
            }
        }
        
        bm.Clear();
        bm.Add(2, 3);
        
        Assert.That(bm.IsRowEmpty(1), Is.True);
        Assert.That(bm.IsRowEmpty(2), Is.False);
        Assert.That(bm.IsColumnEmpty(3), Is.False);
        Assert.That(bm.IsColumnEmpty(4), Is.True);
    }
}

