using System.Collections.Generic;
using System.Linq;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Utility;
using Model.Utility.BitSets;
using Model.Utility.Collections;

namespace Model.Sudokus.Solver.Strategies;

public class AlmostClaimingPairStrategy : SudokuStrategy //TODO Look into using same algo for triple
{
    public const string OfficialName = "Almost Claiming Pair";
    
    private readonly CreateMultiDictionary<Cell, int> _create;
    
    public AlmostClaimingPairStrategy(CreateMultiDictionary<Cell, int> create) : base(OfficialName, 
        Difficulty.Medium, InstanceHandling.UnorderedAll)
    {
        _create = create;
    }

    public override void Apply(ISudokuSolverData data)
    {
        for (int startRow = 0; startRow < 9; startRow += 3)
        {
            for (int startCol = 0; startCol < 9; startCol += 3)
            {
                for (int u = 0; u < 3; u++)
                {
                    if (Search(data, startRow, startCol, u)) return;
                }
            }
        }
    }

    private bool Search(ISudokuSolverData data, int startRow, int startCol, int u)
    {
        ReadOnlyBitSet16 poss = new();
        var row = startRow + u;
        for (int c = 0; c < 3; c++)
        {
            poss |= data.PossibilitiesAt(row, startCol + c);
        }

        if (poss.Count > 2)
        {
            var opportunities = _create();
            foreach (var p in poss.EnumeratePossibilities())
            {
                var positions = data.MiniGridPositionsAt(startRow / 3, startCol / 3, p).Copy();
                positions.VoidGridRow(u);
                
                if(positions.Count != 1) continue;

                var cell = positions.First();
                foreach (var other in opportunities.EnumerateDefinitions(cell))
                {
                    for (int sc = 0; sc < 9; sc += 3)
                    {
                        if (sc == startCol) continue;

                        for (int c = 0; c < 3; c++)
                        {
                            var maybeBiValue = data.PossibilitiesAt(row, sc + c);
                            if(maybeBiValue.Count != 2 || !maybeBiValue.Contains(p) || !maybeBiValue.Contains(other)) 
                                continue;

                            ProcessRowBiValue(data, p, other, row, startCol, sc + c);
                            ProcessAhsCell(data, p, other, cell);

                            if (data.ChangeBuffer.NeedCommit())
                            {
                                data.ChangeBuffer.Commit(new AlmostClaimingPairBuilder(startRow, startCol,
                                    u, Unit.Row, new Cell(row, sc + c), cell, p, other));
                                if(StopOnFirstCommit) return true;
                            }
                        }
                    }
                }

                opportunities.AddDefinition(cell, p);
            }
            
            opportunities.Clear();
            foreach (var p in poss.EnumeratePossibilities())
            {
                var positions = data.RowPositionsAt(row, p).Copy();
                positions.VoidMiniGrid(startCol / 3);
                
                if(positions.Count != 1) continue;

                var cell = new Cell(row, positions.First());
                foreach (var other in opportunities.EnumerateDefinitions(cell))
                {
                    for (int r = startRow; r < startRow + 3; r++)
                    {
                        if(r == row) continue;

                        for (int c = 0; c < 3; c++)
                        {
                            var maybeBiValue = data.PossibilitiesAt(r, startCol + c);
                            if(maybeBiValue.Count != 2 || !maybeBiValue.Contains(p) || !maybeBiValue.Contains(other)) 
                                continue;

                            ProcessBoxRowBiValue(data, p, other, startRow, startCol, row,
                                new Cell(r, startCol + c));
                            ProcessAhsCell(data, p, other, cell);

                            if (data.ChangeBuffer.NeedCommit())
                            {
                                data.ChangeBuffer.Commit(new AlmostClaimingPairBuilder(startRow, startCol,
                                    u, Unit.Row, new Cell(r, startCol + c), cell,
                                    p, other));
                                if(StopOnFirstCommit) return true;
                            }
                        }
                    }
                }
                
                opportunities.AddDefinition(cell, p);
            }
        }
        
        poss = new ReadOnlyBitSet16();
        var col = startCol + u;
        for (int r = 0; r < 3; r++)
        {
            poss |= data.PossibilitiesAt(startRow + r, col);
        }

        if (poss.Count > 2)
        {
            var opportunities = _create();
            foreach (var p in poss.EnumeratePossibilities())
            {
                var positions = data.MiniGridPositionsAt(startRow / 3, startCol / 3, p).Copy();
                positions.VoidGridColumn(u);
                
                if(positions.Count != 1) continue;

                var cell = positions.First();
                foreach (var other in opportunities.EnumerateDefinitions(cell))
                {
                    for (int sr = 0; sr < 9; sr += 3)
                    {
                        if (sr == startRow) continue;

                        for (int r = 0; r < 3; r++)
                        {
                            var maybeBiValue = data.PossibilitiesAt(sr + r, col);
                            if(maybeBiValue.Count != 2 || !maybeBiValue.Contains(p) || !maybeBiValue.Contains(other)) 
                                continue;

                            ProcessColumnBiValue(data, p, other, col, startRow, sr + r);
                            ProcessAhsCell(data, p, other, cell);

                            if (data.ChangeBuffer.NeedCommit())
                            {
                                data.ChangeBuffer.Commit(new AlmostClaimingPairBuilder(startRow, startCol,
                                    u, Unit.Column, new Cell(sr + r, col), cell,
                                    p, other));
                                if(StopOnFirstCommit) return true;
                            }
                        }
                    }
                }
                
                opportunities.AddDefinition(cell, p);
            }
            
            opportunities.Clear();
            foreach (var p in poss.EnumeratePossibilities())
            {
                var positions = data.ColumnPositionsAt(col, p).Copy();
                positions.VoidMiniGrid(startRow / 3);
                
                if(positions.Count != 1) continue;

                var cell = new Cell(positions.First(), col);
                foreach (var other in opportunities.EnumerateDefinitions(cell))
                {
                    for (int c = startCol; c < startCol + 3; c++)
                    {
                        if(c == col) continue;

                        for (int r = 0; r < 3; r++)
                        {
                            var maybeBiValue = data.PossibilitiesAt(startRow + r, c);
                            if(maybeBiValue.Count != 2 || !maybeBiValue.Contains(p) || !maybeBiValue.Contains(other)) 
                                continue;

                            ProcessBoxColumnBiValue(data, p, other, startRow, startCol, col,
                                new Cell(startRow + r, c));
                            ProcessAhsCell(data, p, other, cell);

                            if (data.ChangeBuffer.NeedCommit())
                            {
                                data.ChangeBuffer.Commit(new AlmostClaimingPairBuilder(startRow, startCol,
                                    u, Unit.Column, new Cell(startRow + r, c), cell,
                                    p, other));
                                if(StopOnFirstCommit) return true;
                            }
                        }
                    }
                }
                
                opportunities.AddDefinition(cell, p);
            }
        }
        
        return false;
    }

    private static void ProcessAhsCell(ISudokuSolverData data, int p1, int p2, Cell cell)
    {
        foreach (var toRemove in data.PossibilitiesAt(cell).EnumeratePossibilities())
        {
            if (toRemove != p1 && toRemove != p2) data.ChangeBuffer.ProposePossibilityRemoval(
                toRemove, cell);
        }
    }

    private static void ProcessRowBiValue(ISudokuSolverData data, int p1, int p2, int row, int startCol, int colExcept)
    {
        for (int sc = 0; sc < 9; sc += 3)
        {
            if (sc == startCol) continue;

            for (int c = 0; c < 3; c++)
            {
                var col = sc + c;
                if(col == colExcept) continue;

                data.ChangeBuffer.ProposePossibilityRemoval(p1, row, col);
                data.ChangeBuffer.ProposePossibilityRemoval(p2, row, col);
            }
        }
    }
    
    private static void ProcessColumnBiValue(ISudokuSolverData data, int p1, int p2, int col, int startRow, int rowExcept)
    {
        for (int sr = 0; sr < 9; sr += 3)
        {
            if (sr == startRow) continue;

            for (int r = 0; r < 3; r++)
            {
                var row = sr + r;
                if(row == rowExcept) continue;

                data.ChangeBuffer.ProposePossibilityRemoval(p1, row, col);
                data.ChangeBuffer.ProposePossibilityRemoval(p2, row, col);
            }
        }
    }
    
    private static void ProcessBoxRowBiValue(ISudokuSolverData data, int p1, int p2, int startRow, int startCol,
        int rowExcept, Cell cellExcept)
    {
        for (int r = startRow; r < startRow + 3; r++)
        {
            if(r == rowExcept) continue;

            for (int c = 0; c < 3; c++)
            {
                var col = startCol + c;
                if(cellExcept.Row == r && cellExcept.Column == col) continue;
                
                data.ChangeBuffer.ProposePossibilityRemoval(p1, r, col);
                data.ChangeBuffer.ProposePossibilityRemoval(p2, r, col);
            }
        }
    }
    
    private static void ProcessBoxColumnBiValue(ISudokuSolverData data, int p1, int p2, int startRow, int startCol,
        int colExcept, Cell cellExcept)
    {
        for (int c = startCol; c < startCol + 3; c++)
        {
            if(c == colExcept) continue;

            for (int r = 0; r < 3; r++)
            {
                var row = startRow + r;
                if(cellExcept.Row == row && cellExcept.Column == c) continue;
                
                data.ChangeBuffer.ProposePossibilityRemoval(p1, row, c);
                data.ChangeBuffer.ProposePossibilityRemoval(p2, row, c);
            }
        }
    }
}

public delegate IMultiDictionary<TIn, TOut> CreateMultiDictionary<in TIn, TOut>();

public interface IMultiDictionary<in TIn, TOut>
{
    void AddDefinition(TIn word, TOut definition);
    IEnumerable<TOut> EnumerateDefinitions(TIn word);
    void Clear();
}

public class ListDictionary<TIn, TOut> : Dictionary<TIn, List<TOut>>, IMultiDictionary<TIn, TOut> where TIn : notnull
{
    public void AddDefinition(TIn word, TOut definition)
    {
        if (!TryGetValue(word, out var list))
        {
            list = new List<TOut>();
            this[word] = list;
        }

        list.Add(definition);
    }

    public IEnumerable<TOut> EnumerateDefinitions(TIn word)
    {
        return TryGetValue(word, out var list) ? list : Enumerable.Empty<TOut>();
    }
}

public class CandidateListMultiDictionary : List<CellPossibility>, IMultiDictionary<Cell, int>
{
    public void AddDefinition(Cell word, int definition)
    {
        Add(new CellPossibility(word, definition));
    }

    public IEnumerable<int> EnumerateDefinitions(Cell word)
    {
        foreach (var cp in this)
        {
            if (cp.Row == word.Row && cp.Column == word.Column) yield return cp.Possibility;
        }
    }
}

public class AlmostClaimingPairBuilder : IChangeReportBuilder<NumericChange, ISudokuSolvingState, ISudokuHighlighter>
{
    private readonly int _startRow;
    private readonly int _startCol;
    private readonly int _u;
    private readonly Unit _unit;
    private readonly Cell _alsCell;
    private readonly Cell _ahsCell;
    private readonly int _p1;
    private readonly int _p2;

    public AlmostClaimingPairBuilder(int startRow, int startCol, int u, Unit unit, Cell alsCell, Cell ahsCell, int p1, int p2)
    {
        _startRow = startRow;
        _startCol = startCol;
        _u = u;
        _unit = unit;
        _alsCell = alsCell;
        _ahsCell = ahsCell;
        _p1 = p1;
        _p2 = p2;
    }

    public ChangeReport<ISudokuHighlighter> BuildReport(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        var cells = new Cell[3];
        for (int o = 0; o < 3; o++)
        {
            cells[o] = _unit == Unit.Row
                ? new Cell(_startRow + _u, _startCol + o)
                : new Cell(_startRow + o, _startCol + _u);
        }
        
        return new ChangeReport<ISudokuHighlighter>($"Almost Claiming Pair in {cells.ToStringSequence(", ")}" +
                                                    $" with {_ahsCell} and {_alsCell}", lighter =>
        {
            foreach (var cell in cells)
            {
                if(snapshot.PossibilitiesAt(cell).Contains(_p1)) 
                    lighter.HighlightPossibility(cell, _p1, StepColor.Cause1);
                if(snapshot.PossibilitiesAt(cell).Contains(_p2)) 
                    lighter.HighlightPossibility(cell, _p2, StepColor.Cause1);
            }
            
            lighter.HighlightPossibility(_alsCell, _p1, StepColor.Cause2);
            lighter.HighlightPossibility(_alsCell, _p2, StepColor.Cause2);
            
            lighter.HighlightPossibility(_ahsCell, _p1, StepColor.Cause3);
            lighter.HighlightPossibility(_ahsCell, _p2, StepColor.Cause3);

            ChangeReportHelper.HighlightChanges(lighter, changes);
        });
    }

    public Clue<ISudokuHighlighter> BuildClue(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        return Clue<ISudokuHighlighter>.Default();
    }
}