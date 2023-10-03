using System.Collections.Generic;
using Model.Solver.Helpers.Changes;
using Model.Solver.Helpers.Highlighting;
using Model.Solver.Positions;
using Model.Solver.StrategiesUtil;
using Model.Solver.StrategiesUtil.LinkGraph;

namespace Model.Solver.Strategies.ForcingNets;

public class UnitForcingNetStrategy : AbstractStrategy
{
    public const string OfficialName = "Unit Forcing Net";
    
    private readonly int _max;

    public UnitForcingNetStrategy(int maxPossibilities) : base(OfficialName, StrategyDifficulty.Extreme)
    {
        _max = maxPossibilities;
    }
    
    public override void ApplyOnce(IStrategyManager strategyManager)
    {
        for (int number = 1; number <= 9; number++)
        {
            for (int row = 0; row < 9; row++)
            {
                var ppir = strategyManager.RowPositionsAt(row, number);
                if (ppir.Count < 2 || ppir.Count > _max) continue;
                
                Dictionary<ILinkGraphElement, Coloring>[] colorings =
                    new Dictionary<ILinkGraphElement, Coloring>[ppir.Count];

                var cursor = 0;
                foreach (var col in ppir)
                {
                    colorings[cursor] = strategyManager.PreComputer.OnColoring(row, col, number);
                    cursor++;
                }
                
                Process(strategyManager, colorings);
                if (strategyManager.ChangeBuffer.NotEmpty()) strategyManager.ChangeBuffer.Push(this,
                    new LineUnitForcingNetReportBuilder(colorings, ppir, row, Unit.Row, number));
            }

            for (int col = 0; col < 9; col++)
            {
                var ppic = strategyManager.ColumnPositionsAt(col, number);
                if (ppic.Count < 2 || ppic.Count > _max) continue;
                
                Dictionary<ILinkGraphElement, Coloring>[] colorings =
                    new Dictionary<ILinkGraphElement, Coloring>[ppic.Count];

                var cursor = 0;
                foreach (var row in ppic)
                {
                    colorings[cursor] = strategyManager.PreComputer.OnColoring(row, col, number);
                    cursor++;
                }
                
                Process(strategyManager, colorings);
                if (strategyManager.ChangeBuffer.NotEmpty()) strategyManager.ChangeBuffer.Push(this,
                    new LineUnitForcingNetReportBuilder(colorings, ppic, col, Unit.Column, number));
            }

            for (int miniRow = 0; miniRow < 3; miniRow++)
            {
                for (int miniCol = 0; miniCol < 3; miniCol++)
                {
                    var ppimn = strategyManager.MiniGridPositionsAt(miniRow, miniCol, number);
                    if (ppimn.Count < 2 || ppimn.Count > _max) continue;
                
                    Dictionary<ILinkGraphElement, Coloring>[] colorings =
                        new Dictionary<ILinkGraphElement, Coloring>[ppimn.Count];

                    var cursor = 0;
                    foreach (var pos in ppimn)
                    {
                        colorings[cursor] = strategyManager.PreComputer.OnColoring(pos.Row, pos.Col, number);
                        cursor++;
                    }
                
                    Process(strategyManager, colorings);
                    if (strategyManager.ChangeBuffer.NotEmpty()) strategyManager.ChangeBuffer.Push(this,
                        new MiniGridUnitForcingNetReportBuilder(colorings, ppimn, number));
                }
            }
        }
    }

    private void Process(IStrategyManager view, Dictionary<ILinkGraphElement, Coloring>[] colorings)
    {
        foreach (var element in colorings[0])
        {
            if (element.Key is not CellPossibility current) continue;

            bool sameInAll = true;
            Coloring col = element.Value;

            for (int i = 1; i < colorings.Length && sameInAll; i++)
            {
                if (!colorings[i].TryGetValue(current, out var c) || c != col)
                {
                    sameInAll = false;
                    break;
                }
            }

            if (sameInAll)
            {
                if (col == Coloring.On) view.ChangeBuffer.AddSolutionToAdd(current.Possibility, current.Row, current.Col);
                else view.ChangeBuffer.AddPossibilityToRemove(current.Possibility, current.Row, current.Col);
            }
        }
    }
}

public class LineUnitForcingNetReportBuilder : IChangeReportBuilder
{
    private readonly Dictionary<ILinkGraphElement, Coloring>[] _colorings;
    private readonly IReadOnlyLinePositions _pos;
    private readonly int _unitNumber;
    private readonly Unit _unit;
    private readonly int _possibility;


    public LineUnitForcingNetReportBuilder(Dictionary<ILinkGraphElement, Coloring>[] colorings, IReadOnlyLinePositions pos,
        int unitNumber, Unit unit, int possibility)
    {
        _colorings = colorings;
        _pos = pos;
        _unitNumber = unitNumber;
        _unit = unit;
        _possibility = possibility;
    }

    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        CellPossibility[] coords = new CellPossibility[_pos.Count];
        var cursor = 0;
        foreach (var other in _pos)
        {
            coords[cursor] = _unit == Unit.Row
                ? new CellPossibility(_unitNumber, other, _possibility)
                : new CellPossibility(other, _unitNumber, _possibility);
            cursor++;
        }
        
        Highlight[] highlights = new Highlight[_colorings.Length + 1];
        highlights[0] = lighter =>
        {
            IChangeReportBuilder.HighlightChanges(lighter, changes);
            foreach (var coord in coords)
            {
                lighter.CirclePossibility(coord);
            }
        };

        for (int i = 0; i < _colorings.Length; i++)
        {
            var filtered = ForcingNetsUtil.FilterPossibilityCoordinates(_colorings[i]);
            var coord = coords[i];
            highlights[i + 1] = lighter =>
            {
                ForcingNetsUtil.HighlightColoring(lighter, filtered);
                lighter.CirclePossibility(coord);
            };
        }
        
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), "", highlights);
    }
}

public class MiniGridUnitForcingNetReportBuilder : IChangeReportBuilder
{
    private readonly Dictionary<ILinkGraphElement, Coloring>[] _colorings;
    private readonly IReadOnlyMiniGridPositions _pos;
    private readonly int _possibility;


    public MiniGridUnitForcingNetReportBuilder(Dictionary<ILinkGraphElement, Coloring>[] colorings, IReadOnlyMiniGridPositions pos,
       int possibility)
    {
        _colorings = colorings;
        _pos = pos;
        _possibility = possibility;
    }

    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        CellPossibility[] coords = new CellPossibility[_pos.Count];
        var cursor = 0;
        foreach (var other in _pos)
        {
            coords[cursor] = new CellPossibility(other, _possibility);
            cursor++;
        }
        
        Highlight[] highlights = new Highlight[_colorings.Length + 1];
        highlights[0] = lighter =>
        {
            IChangeReportBuilder.HighlightChanges(lighter, changes);
            foreach (var coord in coords)
            {
                lighter.CirclePossibility(coord);
            }
        };

        for (int i = 0; i < _colorings.Length; i++)
        {
            var filtered = ForcingNetsUtil.FilterPossibilityCoordinates(_colorings[i]);
            var coord = coords[i];
            highlights[i + 1] = lighter =>
            {
                ForcingNetsUtil.HighlightColoring(lighter, filtered);
                lighter.CirclePossibility(coord);
            };
        }
        
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), "", highlights);
    }
}