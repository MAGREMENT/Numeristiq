﻿using Model.Utility;

namespace Model.Core.Explanation;

public class CellPossibilityExplanationElement : ExplanationElement
{
    private readonly CellPossibility _cellPossibility;

    public CellPossibilityExplanationElement(CellPossibility cellPossibility)
    {
        _cellPossibility = cellPossibility;
    }

    public override string ToString()
    {
        return _cellPossibility.ToString();
    }
    
    public override bool ShouldBeBold => true;
    public override ExplanationColor Color => ExplanationColor.Primary;

    public override void Show(IExplanationHighlighter highlighter)
    {
        highlighter.ShowCellPossibility(_cellPossibility, Color);
    }

    public override bool DoesShowSomething => true;
}