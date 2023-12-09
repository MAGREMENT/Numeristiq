using System.Collections.Generic;
using Model.Solver.Helpers.Changes;
using Model.Solver.StrategiesUtility;
using Model.Solver.StrategiesUtility.CellColoring;
using Model.Solver.StrategiesUtility.CellColoring.ColoringResults;
using Model.Solver.StrategiesUtility.Graphs;

namespace Model.Solver.Strategies.ForcingNets;

public class DigitForcingNetStrategy : AbstractStrategy
{ 
    public const string OfficialName = "Digit Forcing Net";
    private const OnCommitBehavior DefaultBehavior = OnCommitBehavior.Return;
    
    public override OnCommitBehavior DefaultOnCommitBehavior => DefaultBehavior;

    public DigitForcingNetStrategy() : base(OfficialName,  StrategyDifficulty.Extreme, DefaultBehavior)
    {
        
    }
    
    public override void Apply(IStrategyManager strategyManager)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                foreach (var possibility in strategyManager.PossibilitiesAt(row, col))
                {
                    var onColoring = strategyManager.PreComputer.OnColoring(row, col, possibility);
                    var offColoring = strategyManager.PreComputer.OffColoring(row, col, possibility);

                    if(onColoring.Count == 1 || offColoring.Count == 1) continue;

                    if (Process(strategyManager, onColoring, offColoring)) return;
                }
            }
        }
    }

    private bool Process(IStrategyManager view, ColoringDictionary<ILinkGraphElement> onColoring,
        ColoringDictionary<ILinkGraphElement> offColoring)
    {
        foreach (var on in onColoring)
        {
            if (on.Key is not CellPossibility possOn) continue;
            
            if (offColoring.TryGetValue(possOn, out var other))
            {
                switch (other)
                {
                    case Coloring.Off when on.Value == Coloring.Off :
                        view.ChangeBuffer.ProposePossibilityRemoval(possOn.Possibility, possOn.Row, possOn.Column);
                        break;
                    case Coloring.On when on.Value == Coloring.On :
                        view.ChangeBuffer.ProposeSolutionAddition(possOn.Possibility, possOn.Row, possOn.Column);
                        break;
                }

                if (view.ChangeBuffer.NotEmpty() &&view.ChangeBuffer.Commit(this,
                        new DigitForcingNetReportBuilder(onColoring, offColoring, possOn, on.Value, 
                            possOn, other, view.GraphManager.ComplexLinkGraph)) && OnCommitBehavior == OnCommitBehavior.Return) return true;
            }

            if (on.Value != Coloring.On) continue;
            
            foreach (var off in offColoring)
            {
                if (off.Value != Coloring.On || off.Key is not CellPossibility possOff) continue;
                if (possOff.Row == possOn.Row && possOn.Column == possOff.Column)
                {
                    RemoveAll(view, possOn.Row, possOn.Column, possOn.Possibility, possOff.Possibility);
                    if (view.ChangeBuffer.NotEmpty() && view.ChangeBuffer.Commit(this,
                            new DigitForcingNetReportBuilder(onColoring, offColoring, possOn, on.Value,
                                possOff, off.Value, view.GraphManager.ComplexLinkGraph)) && OnCommitBehavior == OnCommitBehavior.Return) return true;
                }
                else if (possOff.Possibility == possOn.Possibility && possOn.ShareAUnit(possOff))
                {
                    foreach (var coord in possOn.SharedSeenCells(possOff))
                    {
                        view.ChangeBuffer.ProposePossibilityRemoval(possOn.Possibility, coord.Row, coord.Column);
                    }
                    
                    if (view.ChangeBuffer.NotEmpty() && view.ChangeBuffer.Commit(this,
                            new DigitForcingNetReportBuilder(onColoring, offColoring, possOn, on.Value,
                                possOff, off.Value, view.GraphManager.ComplexLinkGraph)) && OnCommitBehavior == OnCommitBehavior.Return) return true;
                }
            }
        }

        return false;
    }

    private void RemoveAll(IStrategyManager view, int row, int col, int except1, int except2)
    {
        foreach (var possibility in view.PossibilitiesAt(row, col))
        {
            if (possibility == except1 || possibility == except2) continue;
            view.ChangeBuffer.ProposePossibilityRemoval(possibility, row, col);
        }
    }
}

public class DigitForcingNetReportBuilder : IChangeReportBuilder
{
    private readonly ColoringDictionary<ILinkGraphElement> _on;
    private readonly ColoringDictionary<ILinkGraphElement> _off;
    private readonly CellPossibility _onPos;
    private readonly Coloring _onColoring;
    private readonly CellPossibility _offPos;
    private readonly Coloring _offColoring;
    private readonly LinkGraph<ILinkGraphElement> _graph;

    public DigitForcingNetReportBuilder(ColoringDictionary<ILinkGraphElement> on, 
        ColoringDictionary<ILinkGraphElement> off, CellPossibility onPos, Coloring onColoring,
        CellPossibility offPos, Coloring offColoring, LinkGraph<ILinkGraphElement> graph)
    {
        _on = on;
        _off = off;
        _onPos = onPos;
        _onColoring = onColoring;
        _offPos = offPos;
        _offColoring = offColoring;
        _graph = graph;
    }

    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        var onPaths = ForcingNetsUtility.FindEveryNeededPaths(_on.History!.GetPathToRootWithGuessedLinks(_onPos, _onColoring),
            _on, _graph, snapshot);
        var offPaths = ForcingNetsUtility.FindEveryNeededPaths(_off.History!.GetPathToRootWithGuessedLinks(_offPos, _offColoring),
            _off, _graph, snapshot);

        var first = (CellPossibility)onPaths[0].Elements[0];
        
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), Explanation(onPaths, offPaths, first), lighter =>
        {
            ForcingNetsUtility.HighlightAllPaths(lighter, onPaths, Coloring.On);
            lighter.EncirclePossibility(first);

            IChangeReportBuilder.HighlightChanges(lighter, changes);
        }, lighter =>
        {
            ForcingNetsUtility.HighlightAllPaths(lighter, offPaths, Coloring.Off);
            lighter.EncirclePossibility(first);

            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }

    private string Explanation(List<LinkGraphChain<ILinkGraphElement>> onPaths,
        List<LinkGraphChain<ILinkGraphElement>> offPaths, CellPossibility first)
    {
        return $"If {first} is on : \n{ForcingNetsUtility.AllPathsToString(onPaths)}\n" +
               $"If {first} is off : \n{ForcingNetsUtility.AllPathsToString(offPaths)}";
    }
}