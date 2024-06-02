using System.Collections.Generic;
using System.Linq;
using Model.Core;
using Model.Helpers;
using Model.Helpers.Changes;
using Model.Helpers.Graphs;
using Model.Helpers.Highlighting;
using Model.Sudokus.Solver.Utility;
using Model.Sudokus.Solver.Utility.Graphs;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Sudokus.Solver.Strategies;

public class XYZRingStrategy : SudokuStrategy
{
    public const string OfficialName = "XYZ-Ring";
    
    public XYZRingStrategy() : base(OfficialName, StepDifficulty.Hard, InstanceHandling.UnorderedAll)
    {
    }

    public override void Apply(ISudokuStrategyUser strategyUser)
    {
        strategyUser.PreComputer.Graphs.ConstructComplex(
            SudokuConstructRuleBank.UnitStrongLink, SudokuConstructRuleBank.UnitWeakLink, SudokuConstructRuleBank.PointingPossibilities);
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                var poss = strategyUser.PossibilitiesAt(row, col);
                if (poss.Count == 3 && SearchBaseWing(strategyUser, strategyUser.PreComputer.Graphs.ComplexLinkGraph,
                        new Cell(row, col), poss)) return;
            }
        }
    }

    private bool SearchBaseWing(ISudokuStrategyUser strategyUser, ILinkGraph<ISudokuElement> graph, Cell hinge, ReadOnlyBitSet16 possibilities)
    {
        for (int col = 0; col < 9; col++)
        {
            if (col / 3 == hinge.Column / 3) continue;
            var possC = strategyUser.PossibilitiesAt(hinge.Row, col);
            if (possC.Count != 2 || (possibilities | possC) != possibilities) continue;

            for (int row = 0; row < 9; row++)
            {
                if (row / 3 == hinge.Row / 3) continue;
                var possR = strategyUser.PossibilitiesAt(row, hinge.Column);

                if (possR.Count == 2 && possR != possC && (possR | possC) == possibilities
                    && SearchRing(strategyUser, graph, hinge, new Cell(hinge.Row, col), new Cell(row, hinge.Column),
                        possibilities.AndMulti(possC, possR).FirstPossibility())) return true;
            }
        }
        
        return false;
    }

    private bool SearchRing(ISudokuStrategyUser strategyUser, ILinkGraph<ISudokuElement> graph, Cell hinge, Cell hingeRow, Cell hingeCol, int poss)
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

                if (graph.AreNeighbors(secondNeighbor, cpc) && Process(strategyUser, hinge, hingeRow,
                        hingeCol, poss, neighbor, secondNeighbor)) return true;
            }
        }
        
        return false;
    }

    private bool Process(ISudokuStrategyUser strategyUser, Cell hinge, Cell hingeRow, Cell hingeCol, int poss,
        ISudokuElement rowFriend, ISudokuElement columnFriend)
    {
        var p = strategyUser.PossibilitiesAt(hingeRow).FirstPossibility(poss);
        for (int col = 0; col < 9; col++)
        {
            if (col == hinge.Column || col == hingeRow.Column) continue;

            strategyUser.ChangeBuffer.ProposePossibilityRemoval(p, hinge.Row, col);
        }

        p = strategyUser.PossibilitiesAt(hingeCol).FirstPossibility(poss);
        for (int row = 0; row < 9; row++)
        {
            if(row == hinge.Row || row == hingeCol.Row) continue;

            if (p == poss && columnFriend.Contains(new Cell(row, hinge.Column))) continue;

            strategyUser.ChangeBuffer.ProposePossibilityRemoval(p, row, hinge.Column);
        }

        List<Cell> buffer = new() { hingeRow, hinge };
        buffer.AddRange(rowFriend.EnumerateCell());

        foreach (var cell in SudokuCellUtility.SharedSeenCells(buffer))
        {
            if (cell == hinge || cell == hingeCol ||
                columnFriend.Contains(cell)) continue;

            strategyUser.ChangeBuffer.ProposePossibilityRemoval(poss, cell);
        }

        buffer.Clear();
        buffer.Add(hingeCol);
        buffer.Add(hinge);
        buffer.AddRange(columnFriend.EnumerateCell());
        
        foreach (var cell in SudokuCellUtility.SharedSeenCells(buffer))
        {
            if (cell == hinge || cell == hingeRow ||
                rowFriend.Contains(cell)) continue;

            strategyUser.ChangeBuffer.ProposePossibilityRemoval(poss, cell);
        }
        
        return strategyUser.ChangeBuffer.NotEmpty() && strategyUser.ChangeBuffer.Commit(new XYZRingReportBuilder(hinge,
            hingeRow, hingeCol, rowFriend, columnFriend, poss)) && StopOnFirstPush;
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

public class XYZRingReportBuilder : IChangeReportBuilder<IUpdatableSudokuSolvingState, ISudokuHighlighter>
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

    public ChangeReport<ISudokuHighlighter> BuildReport(IReadOnlyList<SolverProgress> changes, IUpdatableSudokuSolvingState snapshot)
    {
        return new ChangeReport<ISudokuHighlighter>(Description(), lighter =>
        {
            lighter.HighlightCell(_hinge, ChangeColoration.CauseOffTwo);
            lighter.HighlightCell(_hingeRow, ChangeColoration.CauseOffTwo);
            lighter.HighlightCell(_hingeColumn, ChangeColoration.CauseOffTwo);

            foreach (var p in _rowFriend.EnumerateCellPossibility())
            {
                lighter.HighlightPossibility(p, ChangeColoration.CauseOffOne);
            }
            
            foreach (var p in _columnFriend.EnumerateCellPossibility())
            {
                lighter.HighlightPossibility(p, ChangeColoration.CauseOffOne);
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

    public Clue<ISudokuHighlighter> BuildClue(IReadOnlyList<SolverProgress> changes, IUpdatableSudokuSolvingState snapshot)
    {
        return Clue<ISudokuHighlighter>.Default();
    }
}