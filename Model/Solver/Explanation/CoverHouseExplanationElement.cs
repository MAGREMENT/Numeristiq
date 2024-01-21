using Model.Solver.StrategiesUtility;

namespace Model.Solver.Explanation;

public class CoverHouseExplanationElement
{
    private readonly CoverHouse _coverHouse;

    public CoverHouse Value => _coverHouse;

    public CoverHouseExplanationElement(CoverHouse coverHouse)
    {
        _coverHouse = coverHouse;
    }

    public override string ToString()
    {
        var s = _coverHouse.Unit switch
        {
            Unit.Row => "row",
            Unit.Column => "column",
            Unit.MiniGrid => "box"
        };

        return s + (_coverHouse.Number + 1);
    }
}