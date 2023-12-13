using System.Linq;
using Global;
using Global.Enums;

namespace Model.Player;

public abstract class CellHighlighting
{
    public Cell Cell { get; }

    protected CellHighlighting(Cell cell)
    {
        Cell = cell;
    }

    public abstract int Count { get; }
    public abstract HighlightColor GetOne();
    public abstract HighlightColor[] GetAll();

    public bool IsSameHighlight(CellHighlighting ch)
    {
        if (ch.Count != Count) return false;

        var allOther = ch.GetAll();
        foreach (var c in GetAll())
        {
            if (!allOther.Contains(c)) return false;
        }

        return true;
    }

    public override bool Equals(object? obj)
    {
        return obj is CellHighlighting ch && ch.Cell == Cell;
    }

    public override int GetHashCode()
    {
        return Cell.GetHashCode();
    }
}

public class MonoCellHighlighting : CellHighlighting
{
    private readonly HighlightColor _color;

    public MonoCellHighlighting(Cell cell, HighlightColor color) : base(cell)
    {
        _color = color;
    }

    public override int Count => 1;
    
    public override HighlightColor GetOne()
    {
        return _color;
    }

    public override HighlightColor[] GetAll()
    {
        return new[] { _color };
    }
}