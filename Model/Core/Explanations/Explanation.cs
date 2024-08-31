using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Model.Core.Highlighting;
using Model.Sudokus.Solver.PossibilitySets;
using Model.Sudokus.Solver.Utility;
using Model.Utility;

namespace Model.Core.Explanations;

public class Explanation<T> : IEnumerable<IExplanationElement<T>>
{
    private const int StartLength = 8;

    private IExplanationElement<T>[] _elements = Array.Empty<IExplanationElement<T>>();
    public int Count { get; private set; }
    
    public string FullExplanation()
    {
        StringBuilder builder = new();
        foreach (var e in this) builder.Append(e);
        return builder.ToString();
    }
    
    public Explanation<T> Append(IExplanationElement<T> next)
    {
        GrowIfNeeded();
        _elements[Count++] = next;

        return this;
    }
    
    public IEnumerator<IExplanationElement<T>> GetEnumerator()
    {
        for (int i = 0; i < Count; i++) yield return _elements[i];
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    
    private void GrowIfNeeded()
    {
        if (Count < _elements.Length) return;

        if (_elements.Length == 0)
        {
            _elements = new IExplanationElement<T>[StartLength];
            return;
        }

        var buffer = new IExplanationElement<T>[_elements.Length * 2];
        Array.Copy(_elements, 0, buffer, 0, _elements.Length);
        _elements = buffer;
    }

    public static Explanation<T> Empty { get; } = new();
}

public interface IExplanationElement
{
    public bool ShouldBeBold { get; }
    public ExplanationColor Color { get; }
    public bool DoesShowSomething { get; }
}

public interface IExplanationElement<in T> : IExplanationElement, IHighlightable<T>
{
    
}

public enum ExplanationColor
{
    Text, Primary, Secondary
}

public static class ExplanationExtensions
{
    public static Explanation<ISudokuHighlighter> Append(this Explanation<ISudokuHighlighter> e, string s)
        => e.Append(new StringExplanationElement(s));
    
    public static Explanation<ISudokuHighlighter> Append(this Explanation<ISudokuHighlighter> e, IPossibilitySet set)
        => e.Append(new AlmostLockedSetExplanationElement(set));
    
    public static Explanation<ISudokuHighlighter> Append(this Explanation<ISudokuHighlighter> e, Cell cell)
        => e.Append(new CellExplanationElement(cell));
    
    public static Explanation<ISudokuHighlighter> Append(this Explanation<ISudokuHighlighter> e, CellPossibility cp)
        => e.Append(new CellPossibilityExplanationElement(cp));
    
    public static Explanation<ISudokuHighlighter> Append(this Explanation<ISudokuHighlighter> e, params CellPossibility[] cps)
        => e.Append(new MultiCellPossibilityExplanationElement(cps));
    
    public static Explanation<ISudokuHighlighter> Append(this Explanation<ISudokuHighlighter> e, House h)
        => e.Append(new HouseExplanationElement(h));
}