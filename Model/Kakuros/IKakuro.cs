using System.Collections.Generic;
using Model.Utility;

namespace Model.Kakuros;

public interface IKakuro : IReadOnlyKakuro
{
    bool AddSum(IKakuroSum sum);
    void AddSumUnchecked(IKakuroSum sum);
    
    new int this[int row, int col] { get; set; }
    int this[Cell cell]
    {
        get => this[cell.Row, cell.Column];
        set => this[cell.Row, cell.Column] = value;
    }
    
    bool AddCellTo(IKakuroSum sum);
    bool AddSumTo(Cell cell);
    bool RemoveCell(Cell cell);
    bool ReplaceAmount(IKakuroSum sum, int amount);

    static IKakuro Default(Orientation orientation)
    {
        var result = new ArrayKakuro();
        result.AddSum(orientation == Orientation.Horizontal
            ? new HorizontalKakuroSum(new Cell(0, 0), 1, 1)
            : new VerticalKakuroSum(new Cell(0, 0), 1, 1));

        return result;
    }
}

public interface IReadOnlyKakuro
{
    int RowCount { get; }
    int ColumnCount { get; }
    int GetSolutionCount();
    int GetCellCount();
    IReadOnlyList<IKakuroSum> Sums { get; }
    IEnumerable<Cell> EnumerateCells();
    
    IEnumerable<IKakuroSum> SumsFor(Cell cell);
    IKakuroSum? VerticalSumFor(Cell cell);
    IKakuroSum? HorizontalSumFor(Cell cell);
    IKakuroSum? FindSum(Cell amountCell);
    List<int> GetSolutions(IKakuroSum sum);

    int this[int row, int col] { get; }

    bool IsComplete();

    IKakuro Copy();
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
                                                         && s.Amount == sum.Amount
                                                         && s.GetStartCell() == sum.GetStartCell();
}

public enum Orientation
{
    Vertical, Horizontal
}