using Tests.Utility;

namespace Tests;

public class ArrayGameTests //Might be useful for some optimizations
{
    [Test]
    public void Test()
    {
        (IArrayGame, string)[] impl =
        [
            (new ArrayArrayGame(), "Array"),
            (new BinaryArrayGame(), "Binary")
        ];
        
        ImplementationSpeedComparator.Compare(impl, 1000,game =>
            {
                int cursor = 0;
                for (int row = 0; row < IArrayGame.RowCount; row++)
                {
                    for (int col = 0; col < IArrayGame.ColumnCount; col++)
                    {
                        game[row, col] = cursor;
                        cursor = (cursor + 1) % 9;
                    }
                }
                
                cursor = 0;
                for (int row = 0; row < IArrayGame.RowCount; row++)
                {
                    for (int col = 0; col < IArrayGame.ColumnCount; col++)
                    {
                        Assert.That(game[row, col], Is.EqualTo(cursor));
                        cursor = (cursor + 1) % 9;
                    }
                }
            });
    }
}

public interface IArrayGame
{
    public const int RowCount = 4;
    public const int ColumnCount = 4;
    
    int this[int row, int col] { get; set; }
}

public class ArrayArrayGame : IArrayGame
{
    private readonly int[,] _cells = new int[4,4];

    public int this[int row, int col]
    {
        get => _cells[row, col];
        set => _cells[row, col] = value;
    }
}

public class BinaryArrayGame : IArrayGame
{
    private ulong _bits;

    public int this[int row, int col]
    {
        get => (int)((_bits >> ((col + row * IArrayGame.ColumnCount) * 4)) & 0b1111);
        set => _bits |= (ulong)value << ((col + row * IArrayGame.ColumnCount) * 4);
    }
}