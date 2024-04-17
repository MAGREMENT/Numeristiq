using System;
using Model.Sudokus.Solver.StrategiesUtility;

namespace Model.Sudokus.Solver.Explanation;

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
            Unit.MiniGrid => "box ",
            _ => throw new ArgumentOutOfRangeException()
        };

        return s + (_house.Number + 1);
    }
    
    public override bool ShouldBeBold => true;
    public override ExplanationColor Color => ExplanationColor.Secondary;

    public override void Show(IExplanationHighlighter highlighter)
    {
        highlighter.ShowCoverHouse(_house);
    }

    public override bool DoesShowSomething => true;
}