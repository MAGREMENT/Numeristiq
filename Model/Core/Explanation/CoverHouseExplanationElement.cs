using System;
using Model.Sudokus;
using Model.Sudokus.Solver.Utility;

namespace Model.Core.Explanation;

public class CoverHouseExplanationElement : ExplanationElement
{
    private readonly House _house;

    public CoverHouseExplanationElement(House house)
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
    
    public override bool ShouldBeBold => true;
    public override ExplanationColor Color => ExplanationColor.Secondary;

    public override void Show(IExplanationHighlighter highlighter)
    {
        highlighter.ShowCoverHouse(_house, Color);
    }

    public override bool DoesShowSomething => true;
}