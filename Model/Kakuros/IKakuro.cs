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
    bool RemoveCellFrom(IKakuroSum sum);
    bool ReplaceAmount(IKakuroSum sum, int amount);
}

public interface IReadOnlyKakuro
{
    int RowCount { get; }
    int ColumnCount { get; }
    IReadOnlyList<IKakuroSum> Sums { get; }
    IEnumerable<Cell> EnumerateCells();
    
    IEnumerable<IKakuroSum> SumsFor(Cell cell);
    IKakuroSum? VerticalSumFor(Cell cell);
    IKakuroSum? HorizontalSumFor(Cell cell);
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
    Cell GetAmountCell();
    Cell GetStartCell();
    
    Cell this[int index] { get; }

    IKakuroSum WithLength(int length);
    IKakuroSum WithAmount(int amount);
}

public enum Orientation
{
    Vertical, Horizontal
}