using System.Collections.Generic;
using System.Text;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Sudokus.Solver.Utility;
using Model.Sudokus.Solver.Utility.Exocet;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Sudokus.Solver.Strategies;

public class SingleTargetExocetStrategy : SudokuStrategy
{
    public const string OfficialName = "Single Target Exocet";
    
    public SingleTargetExocetStrategy() : base(OfficialName, Difficulty.Extreme, InstanceHandling.UnorderedAll)
    {
    }

    public override void Apply(ISudokuSolverData solverData)
    {
        foreach (var exo in ExocetSearcher.SearchSingleTargets(solverData))
        {
            //Single target specific elimination
            foreach (var cell in SudokuCellUtility.SharedSeenCells(exo.Base1, exo.Base2))
            {
                solverData.ChangeBuffer.ProposePossibilityRemoval(exo.WildCard, cell);
            }
            
            //Elimination 1
            foreach (var p in exo.BaseCandidates.EnumeratePossibilities())
            {
                if (p == exo.WildCard) continue;
                if (exo.ComputeCoverHouses(p).Count == 1)
                {
                    solverData.ChangeBuffer.ProposePossibilityRemoval(p, exo.Target);
                    solverData.ChangeBuffer.ProposePossibilityRemoval(p, exo.Base1);
                    solverData.ChangeBuffer.ProposePossibilityRemoval(p, exo.Base2);
                }
            }

            //Elimination 3
            foreach (var p in solverData.PossibilitiesAt(exo.Target).EnumeratePossibilities())
            {
                if (!exo.BaseCandidates.Contains(p))
                {
                    solverData.ChangeBuffer.ProposePossibilityRemoval(p, exo.Target);
                }
            }

            if (solverData.ChangeBuffer.NeedCommit() && solverData.ChangeBuffer.Commit(
                    new SingleTargetExocetReportBuilder(exo)) && StopOnFirstCommit) return;
        }
    }
}

public class SingleTargetExocetReportBuilder : IChangeReportBuilder<NumericChange, ISudokuSolvingState, ISudokuHighlighter>
{
    private readonly SingleTargetExocet _e;

    public SingleTargetExocetReportBuilder(SingleTargetExocet e)
    {
        _e = e;
    }

    public ChangeReport<ISudokuHighlighter> BuildReport(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        var sCells = _e.AllPossibleSCells();

        List<CellPossibility> sPossibilities = new();
        List<Cell> sSolved = new();
        foreach (var cell in sCells)
        {
            if (_e.BaseCandidates.Contains(snapshot[cell.Row, cell.Column])) sSolved.Add(cell);
            else
            {
                foreach (var possibility in _e.BaseCandidates.EnumeratePossibilities())
                {
                    if(snapshot.PossibilitiesAt(cell).Contains(possibility)) sPossibilities.Add(new CellPossibility(cell, possibility));
                } 
            }
        }

        return new ChangeReport<ISudokuHighlighter>(Description(), lighter =>
        {
            lighter.HighlightCell(_e.Base1, StepColor.Cause1);
            lighter.HighlightCell(_e.Base2, StepColor.Cause1);
            
            lighter.HighlightCell(_e.Target, StepColor.Cause2);
            foreach (var target in _e.EnumerateAbsentTargets())
            {
                lighter.HighlightCell(target, StepColor.On);
            }

            foreach (var cell in sCells)
            {
                lighter.HighlightCell(cell, StepColor.Cause3);
            }

            foreach (var cp in sPossibilities)
            {
                lighter.HighlightPossibility(cp, StepColor.Neutral);
            }

            foreach (var cell in sSolved)
            {
                lighter.HighlightCell(cell, StepColor.Neutral);
            }

            ChangeReportHelper.HighlightChanges(lighter, changes);
        });
    }

    private string Description()
    {
        var baseS = _e.Base1.Row == _e.Base2.Row
            ? $"r{_e.Base1.Row + 1}c{_e.Base1.Column + 1}{_e.Base2.Column + 1}"
            : $"r{_e.Base1.Row + 1}{_e.Base2.Row + 1}c{_e.Base1.Column + 1}";
        var builder = new StringBuilder($"Single Target Exocet in {baseS}, {_e.Target} (Wildcard = {_e.WildCard})\nCover houses :\n");

        foreach (var entry in _e.ComputeAllCoverHouses())
        {
            builder.Append($"{entry.Key} : ");
            if (entry.Value.Count == 0) builder.Append("none\n");
            else
            {
                builder.Append(entry.Value[0].ToString());
                for (int i = 1; i < entry.Value.Count; i++)
                {
                    builder.Append(", " + entry.Value[i]);
                }

                builder.Append('\n');
            }
        }
        
        return builder.ToString();
    }
    
    public Clue<ISudokuHighlighter> BuildClue(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        return Clue<ISudokuHighlighter>.Default();
    }
}