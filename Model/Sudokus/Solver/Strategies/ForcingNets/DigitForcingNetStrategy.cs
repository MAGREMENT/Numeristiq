using System.Collections.Generic;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Sudokus.Solver.Utility;
using Model.Sudokus.Solver.Utility.CellColoring;
using Model.Sudokus.Solver.Utility.CellColoring.ColoringResults;
using Model.Sudokus.Solver.Utility.Graphs;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Sudokus.Solver.Strategies.ForcingNets;

public class DigitForcingNetStrategy : SudokuStrategy
{ 
    public const string OfficialName = "Digit Forcing Net";
    private const InstanceHandling DefaultInstanceHandling = InstanceHandling.FirstOnly;

    public DigitForcingNetStrategy() : base(OfficialName,  Difficulty.Inhuman, DefaultInstanceHandling)
    {
        
    }
    
    public override void Apply(ISudokuSolverData solverData)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                foreach (var possibility in solverData.PossibilitiesAt(row, col).EnumeratePossibilities())
                {
                    var onColoring = solverData.PreComputer.OnColoring(row, col, possibility);
                    var offColoring = solverData.PreComputer.OffColoring(row, col, possibility);

                    if(onColoring.Count == 1 || offColoring.Count == 1) continue;

                    if (Process(solverData, onColoring, offColoring)) return;
                }
            }
        }
    }

    private bool Process(ISudokuSolverData solverData, ColoringDictionary<ISudokuElement> onColoring,
        ColoringDictionary<ISudokuElement> offColoring)
    {
        foreach (var on in onColoring)
        {
            if (on.Key is not CellPossibility possOn) continue;
            
            if (offColoring.TryGetValue(possOn, out var other))
            {
                switch (other)
                {
                    case Coloring.Off when on.Value == Coloring.Off :
                        solverData.ChangeBuffer.ProposePossibilityRemoval(possOn.Possibility, possOn.Row, possOn.Column);
                        break;
                    case Coloring.On when on.Value == Coloring.On :
                        solverData.ChangeBuffer.ProposeSolutionAddition(possOn.Possibility, possOn.Row, possOn.Column);
                        break;
                }

                if (solverData.ChangeBuffer.NeedCommit())
                {
                    solverData.ChangeBuffer.Commit(new DigitForcingNetReportBuilder(onColoring, offColoring, possOn, on.Value, 
                        possOn, other, solverData.PreComputer.Graphs.ComplexLinkGraph));
                    if (StopOnFirstCommit) return true;
                }
            }

            if (on.Value != Coloring.On) continue;
            
            foreach (var off in offColoring)
            {
                if (off.Value != Coloring.On || off.Key is not CellPossibility possOff) continue;
                if (possOff.Row == possOn.Row && possOn.Column == possOff.Column)
                {
                    RemoveAll(solverData, possOn.Row, possOn.Column, possOn.Possibility, possOff.Possibility);
                    if (solverData.ChangeBuffer.NeedCommit())
                    {
                        solverData.ChangeBuffer.Commit(new DigitForcingNetReportBuilder(onColoring, offColoring, possOn, on.Value,
                            possOff, off.Value, solverData.PreComputer.Graphs.ComplexLinkGraph));
                        if (StopOnFirstCommit) return true;
                    }
                }
                else if (possOff.Possibility == possOn.Possibility && possOn.ShareAUnit(possOff))
                {
                    foreach (var coord in possOn.SharedSeenCells(possOff))
                    {
                        solverData.ChangeBuffer.ProposePossibilityRemoval(possOn.Possibility, coord.Row, coord.Column);
                    }
                    
                    if (solverData.ChangeBuffer.NeedCommit())
                    {
                        solverData.ChangeBuffer.Commit(new DigitForcingNetReportBuilder(onColoring, offColoring, possOn, on.Value,
                            possOff, off.Value, solverData.PreComputer.Graphs.ComplexLinkGraph));
                        if (StopOnFirstCommit) return true;
                    }
                }
            }
        }

        return false;
    }

    private void RemoveAll(ISudokuSolverData view, int row, int col, int except1, int except2)
    {
        foreach (var possibility in view.PossibilitiesAt(row, col).EnumeratePossibilities())
        {
            if (possibility == except1 || possibility == except2) continue;
            view.ChangeBuffer.ProposePossibilityRemoval(possibility, row, col);
        }
    }
}

public class DigitForcingNetReportBuilder : IChangeReportBuilder<NumericChange, ISudokuSolvingState, ISudokuHighlighter>
{
    private readonly ColoringDictionary<ISudokuElement> _on;
    private readonly ColoringDictionary<ISudokuElement> _off;
    private readonly CellPossibility _onPos;
    private readonly Coloring _onColoring;
    private readonly CellPossibility _offPos;
    private readonly Coloring _offColoring;
    private readonly ILinkGraph<ISudokuElement> _graph;

    public DigitForcingNetReportBuilder(ColoringDictionary<ISudokuElement> on, 
        ColoringDictionary<ISudokuElement> off, CellPossibility onPos, Coloring onColoring,
        CellPossibility offPos, Coloring offColoring, ILinkGraph<ISudokuElement> graph)
    {
        _on = on;
        _off = off;
        _onPos = onPos;
        _onColoring = onColoring;
        _offPos = offPos;
        _offColoring = offColoring;
        _graph = graph;
    }

    public ChangeReport<ISudokuHighlighter> BuildReport(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        var onPaths = ForcingNetsUtility.FindEveryNeededPaths(_on.History!.GetPathToRootWithGuessedLinks(_onPos, _onColoring),
            _on, _graph, snapshot);
        var offPaths = ForcingNetsUtility.FindEveryNeededPaths(_off.History!.GetPathToRootWithGuessedLinks(_offPos, _offColoring),
            _off, _graph, snapshot);

        var first = (CellPossibility)onPaths[0].Elements[0];
        
        return new ChangeReport<ISudokuHighlighter>( Explanation(onPaths, offPaths, first), lighter =>
        {
            ForcingNetsUtility.HighlightAllPaths(lighter, onPaths, Coloring.On);
            lighter.EncirclePossibility(first);

            ChangeReportHelper.HighlightChanges(lighter, changes);
        }, lighter =>
        {
            ForcingNetsUtility.HighlightAllPaths(lighter, offPaths, Coloring.Off);
            lighter.EncirclePossibility(first);

            ChangeReportHelper.HighlightChanges(lighter, changes);
        });
    }

    private string Explanation(List<LinkGraphChain<ISudokuElement>> onPaths,
        List<LinkGraphChain<ISudokuElement>> offPaths, CellPossibility first)
    {
        return $"If {first} is on : \n{ForcingNetsUtility.AllPathsToString(onPaths)}\n" +
               $"If {first} is off : \n{ForcingNetsUtility.AllPathsToString(offPaths)}";
    }
    
    public Clue<ISudokuHighlighter> BuildClue(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        return Clue<ISudokuHighlighter>.Default();
    }
}