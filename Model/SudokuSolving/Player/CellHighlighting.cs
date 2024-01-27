using System.Collections.Generic;
using Global;
using Global.Enums;

namespace Model.SudokuSolving.Player;

public record CellHighlighting(Cell Cell, HighlightingCollection Highlighting);

public abstract class HighlightingCollection
{
    public abstract int Count { get; }
    public abstract HighlightColor GetFirst();
    public abstract HighlightColor[] GetAll();
    protected abstract bool ProtectedEquals(HighlightingCollection collection);
    protected abstract int ProtectedHashCode();
    public abstract HighlightingCollection Add(HighlightColor color);
    public abstract HighlightingCollection? Remove(HighlightColor color);
    public abstract bool Contains(HighlightColor color);

    public override bool Equals(object? obj)
    {
        return obj is HighlightingCollection ch && ProtectedEquals(ch);
    }

    public override int GetHashCode()
    {
        return ProtectedHashCode();
    }
}

public class MonoHighlighting : HighlightingCollection
{
    private readonly HighlightColor _color;

    public MonoHighlighting(HighlightColor color)
    {
        _color = color;
    }

    public override int Count => 1;
    
    public override HighlightColor GetFirst()
    {
        return _color;
    }

    public override HighlightColor[] GetAll()
    {
        return new[] { _color };
    }

    protected override bool ProtectedEquals(HighlightingCollection collection)
    {
        return collection.Count == 1 && collection.GetFirst() == _color;
    }

    protected override int ProtectedHashCode()
    {
        return (int)_color;
    }

    public override HighlightingCollection Add(HighlightColor color)
    {
        return new MultiHighlighting(_color, color);
    }

    public override HighlightingCollection? Remove(HighlightColor color)
    {
        return _color == color ? null : this;
    }

    public override bool Contains(HighlightColor color)
    {
        return _color == color;
    }
}

public class MultiHighlighting : HighlightingCollection
{
    private readonly List<HighlightColor> _colors = new();

    public MultiHighlighting(params HighlightColor[] colors)
    {
        _colors.AddRange(colors);
    }

    public override int Count => _colors.Count;
    
    public override HighlightColor GetFirst()
    {
        return _colors[0];
    }

    public override HighlightColor[] GetAll()
    {
        return _colors.ToArray();
    }

    protected override bool ProtectedEquals(HighlightingCollection collection)
    {
        if (collection.Count != Count) return false;

        foreach (var c in collection.GetAll())
        {
            if (!_colors.Contains(c)) return false;
        }

        return true;
    }

    protected override int ProtectedHashCode()
    {
        var hash = 0;
        foreach (var c in _colors)
        {
            hash ^= (int)c;
        }
        return hash;
    }

    public override HighlightingCollection Add(HighlightColor color)
    {
        _colors.Add(color);
        return this;
    }

    public override HighlightingCollection Remove(HighlightColor color)
    {
        _colors.Remove(color);
        return _colors.Count == 1 ? new MonoHighlighting(_colors[0]) : this;
    }

    public override bool Contains(HighlightColor color)
    {
        return _colors.Contains(color);
    }
}