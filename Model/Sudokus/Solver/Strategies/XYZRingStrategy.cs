using System.Collections.Generic;
using System.Linq;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Graphs;
using Model.Core.Highlighting;
using Model.Sudokus.Solver.Utility;
using Model.Sudokus.Solver.Utility.Graphs;
using Model.Sudokus.Solver.Utility.Graphs.ConstructRules;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Sudokus.Solver.Strategies;

public class XYZRingStrategy : SudokuStrategy
{
    public const string OfficialName = "XYZ-Ring";
    
    public XYZRingStrategy() : base(OfficialName, Difficulty.Hard, InstanceHandling.UnorderedAll)
    {
    }

    public override void Apply(ISudokuSolverData solverData)
    {
        solverData.PreComputer.ComplexGraph.Construct(UnitStrongLinkConstructionRule.Instance,
            UnitWeakLinkConstructionRule.Instance, PointingPossibilitiesConstructionRule.Instance);
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                var poss = solverData.PossibilitiesAt(row, col);
                if (poss.Count == 3 && SearchBaseWing(solverData, solverData.PreComputer.ComplexGraph.Graph,
                        new Cell(row, col), poss)) return;
            }
        }
    }

    private bool SearchBaseWing(ISudokuSolverData solverData, IGraph<ISudokuElement, LinkStrength> graph, Cell hinge, ReadOnlyBitSet16 possibilities)
    {
        for (int col = 0; col < 9; col++)
        {
            if (col / 3 == hinge.Column / 3) continue;
            var possC = solverData.PossibilitiesAt(hinge.Row, col);
            if (possC.Count != 2 || (possibilities | possC) != possibilities) continue;

            for (int row = 0; row < 9; row++)
            {
                if (row / 3 == hinge.Row / 3) continue;
                var possR = solverData.PossibilitiesAt(row, hinge.Column);

                if (possR.Count == 2 && possR != possC && (possR | possC) == possibilities
                    && SearchRing(solverData, graph, hinge, new Cell(hinge.Row, col), new Cell(row, hinge.Column),
                        possibilities.AndMulti(possC, possR).FirstPossibility())) return true;
            }
        }
        
        return false;
    }

    private bool SearchRing(ISudokuSolverData solverData, IGraph<ISudokuElement, LinkStrength> graph, Cell hinge, Cell hingeRow, Cell hingeCol, int poss)
    {
        var cph = new CellPossibility(hinge, poss);
        var cpr = new CellPossibility(hingeRow, poss);
        var cpc = new CellPossibility(hingeCol, poss);
        
        foreach (var neighbor in graph.Neighbors(cpr))
        {
            if (!CheckIntegrity(neighbor, cph, cpc)) continue;

            foreach (var secondNeighbor in graph.Neighbors(neighbor, LinkStrength.Strong))
            {
                if (!CheckIntegrity(secondNeighbor, cph, cpc, cpr)) continue;

                if (graph.AreNeighbors(secondNeighbor, cpc) && Process(solverData, hinge, hingeRow,
                        hingeCol, poss, neighbor, secondNeighbor)) return true;
            }
        }
        
        return false;
    }

    private bool Process(ISudokuSolverData solverData, Cell hinge, Cell hingeRow, Cell hingeCol, int poss,
        ISudokuElement rowFriend, ISudokuElement columnFriend)
    {
        var p = solverData.PossibilitiesAt(hingeRow).FirstPossibility(poss);
        for (int col = 0; col < 9; col++)
        {
            if (col == hinge.Column || col == hingeRow.Column) continue;

            solverData.ChangeBuffer.ProposePossibilityRemoval(p, hinge.Row, col);
        }

        p = solverData.PossibilitiesAt(hingeCol).FirstPossibility(poss);
        for (int row = 0; row < 9; row++)
        {
            if(row == hinge.Row || row == hingeCol.Row) continue;

            if (p == poss && columnFriend.Contains(new Cell(row, hinge.Column))) continue;

            solverData.ChangeBuffer.ProposePossibilityRemoval(p, row, hinge.Column);
        }

        List<Cell> buffer = new() { hingeRow, hinge };
        buffer.AddRange(rowFriend.EnumerateCell());

        foreach (var cell in SudokuUtility.SharedSeenCells(buffer))
        {
            if (cell == hinge || cell == hingeCol ||
                columnFriend.Contains(cell)) continue;

            solverData.ChangeBuffer.ProposePossibilityRemoval(poss, cell);
        }

        buffer.Clear();
        buffer.Add(hingeCol);
        buffer.Add(hinge);
        buffer.AddRange(columnFriend.EnumerateCell());
        
        foreach (var cell in SudokuUtility.SharedSeenCells(buffer))
        {
            if (cell == hinge || cell == hingeRow ||
                rowFriend.Contains(cell)) continue;

            solverData.ChangeBuffer.ProposePossibilityRemoval(poss, cell);
        }
        
        if (!solverData.ChangeBuffer.NeedCommit()) return false;
        solverData.ChangeBuffer.Commit(new XYZRingReportBuilder(hinge,
            hingeRow, hingeCol, rowFriend, columnFriend, poss));
        return StopOnFirstCommit;
    }

    private bool CheckIntegrity(ISudokuElement element, params CellPossibility[] no)
    {
        foreach (var cp in element.EnumerateCellPossibility())
        {
            if (no.Contains(cp)) return false;
        }

        return true;
    }
}

public class XYZRingReportBuilder : IChangeReportBuilder<NumericChange, ISudokuSolvingState, ISudokuHighlighter>
{
    private readonly Cell _hinge;
    private readonly Cell _hingeRow;
    private readonly Cell _hingeColumn;
    private readonly ISudokuElement _rowFriend;
    private readonly ISudokuElement _columnFriend;
    private readonly int _poss;

    public XYZRingReportBuilder(Cell hinge, Cell hingeRow, Cell hingeColumn, ISudokuElement rowFriend, ISudokuElement columnFriend, int poss)
    {
        _hinge = hinge;
        _hingeRow = hingeRow;
        _hingeColumn = hingeColumn;
        _rowFriend = rowFriend;
        _columnFriend = columnFriend;
        _poss = poss;
    }

    public ChangeReport<ISudokuHighlighter> BuildReport(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        return new ChangeReport<ISudokuHighlighter>(Description(), lighter =>
        {
            lighter.HighlightCell(_hinge, StepColor.Cause2);
            lighter.HighlightCell(_hingeRow, StepColor.Cause2);
            lighter.HighlightCell(_hingeColumn, StepColor.Cause2);

            foreach (var p in _rowFriend.EnumerateCellPossibility())
            {
                lighter.HighlightPossibility(p, StepColor.Cause1);
            }
            
            foreach (var p in _columnFriend.EnumerateCellPossibility())
            {
                lighter.HighlightPossibility(p, StepColor.Cause1);
            }
            
            lighter.CreateLink(new CellPossibility(_hingeRow, _poss), _rowFriend, LinkStrength.Weak);
            lighter.CreateLink(new CellPossibility(_hingeColumn, _poss), _columnFriend, LinkStrength.Weak);
            lighter.CreateLink(_rowFriend, _columnFriend, LinkStrength.Strong);

            ChangeReportHelper.HighlightChanges(lighter, changes);
        });
    }

    private string Description() =>
            $"XYZ-Ring in {_hinge}, {_hingeRow} & {_hingeColumn} + {new CellPossibility(_hingeRow, _poss)}" +
            $" - {_rowFriend} = {_columnFriend} - {new CellPossibility(_hingeColumn, _poss)}";

    public Clue<ISudokuHighlighter> BuildClue(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        return Clue<ISudokuHighlighter>.Default();
    }
}