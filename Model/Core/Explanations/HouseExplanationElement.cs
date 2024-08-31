using System;
using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Sudokus;
using Model.Sudokus.Solver.Utility;

namespace Model.Core.Explanations;

public class HouseExplanationElement : IExplanationElement<ISudokuHighlighter>
{
    private readonly House _house;

    public HouseExplanationElement(House house)
    {
        _house = house;
    }

    public override string ToString()
    {
        var s = _house.Unit switch
        {
            Unit.Row => "row ",
            Unit.Column => "column ",
            Unit.Box => "box ",
            _ => throw new ArgumentOutOfRangeException()
        };

        return s + (_house.Number + 1);
    }
    
    public bool ShouldBeBold => true;
    public ExplanationColor Color => ExplanationColor.Secondary;

    public void Highlight(ISudokuHighlighter highlighter)
    {
        highlighter.EncircleHouse(_house, StepColor.Cause1);
    }

    public bool DoesShowSomething => true;
}