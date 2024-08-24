using System.Collections.Generic;
using Model.Core.BackTracking;
using Model.Utility;

namespace Model.Kakuros;

public interface IKakuro : IReadOnlyKakuro
{
    /// <summary>
    /// Adds the sum if it doesn't interfere with any already present sum.
    /// The only exception is when the sum to be added correspond exactly to an already
    /// existing sum, in which case its amount will be replaced
    /// </summary>
    /// <param name="sum"></param>
    /// <returns></returns>
    bool AddSum(IKakuroSum sum);
    /// <summary>
    /// Forcibly adds the sum, deleting existing sums in the path
    /// </summary>
    /// <param name="sum"></param>
    void ForceSum(IKakuroSum sum);
    bool RemoveSum(IKakuroSum sum);
    
    new int this[int row, int col] { get; set; }
    int this[Cell cell]
    {
        get => this[cell.Row, cell.Column];
        set => this[cell.Row, cell.Column] = value;
    }
    
    bool AddCellTo(IKakuroSum sum);
    bool RemoveCell(Cell cell);
    bool ReplaceAmount(IKakuroSum sum, int amount);

    static IKakuro Default(Orientation orientation)
    {
        var result = new SumListKakuro();
        result.AddSum(orientation == Orientation.Horizontal
            ? new HorizontalKakuroSum(new Cell(0, 0), 1, 1)
            : new VerticalKakuroSum(new Cell(0, 0), 1, 1));

        return result;
    }
}

public interface IReadOnlyKakuro : ICopyable<IKakuro>
{
    int RowCount { get; }
    int ColumnCount { get; }
    int GetSolutionCount();
    int GetCellCount();
    IEnumerable<IKakuroSum> Sums { get; }
    IEnumerable<Cell> EnumerateCells();
    
    IEnumerable<IKakuroSum> SumsFor(Cell cell);
    IKakuroSum VerticalSumFor(Cell cell);
    IKakuroSum HorizontalSumFor(Cell cell);
    IKakuroSum? FindSum(Cell amountCell);
    List<int> GetSolutions(IKakuroSum sum);

    int this[int row, int col] { get; }

    bool IsComplete();
    bool IsCorrect();
}

public interface IKakuroSum : IEnumerable<Cell>
{
    Orientation Orientation { get; }
    int Amount { get; }
    int Length { get; }

    int GetFarthestRow();
    int GetFarthestColumn();
    Cell GetFarthestCell(int additionalLength);
    Cell GetAmountCell();
    Cell GetStartCell();
    bool Contains(Cell cell);
    (Cell, Cell) GetPerpendicularNeighbors(int length);
    
    Cell this[int index] { get; }

    static bool AreSame(IKakuroSum sum, IKakuroSum s) => s.Length == sum.Length 
                                                         && s.Orientation == sum.Orientation 
                                                         && s.GetStartCell() == sum.GetStartCell();
}