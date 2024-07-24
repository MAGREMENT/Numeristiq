using System;
using System.Collections.Generic;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Sudokus.Solver.PossibilitySets;
using Model.Sudokus.Solver.Utility;
using Model.Sudokus.Solver.Utility.Graphs;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Sudokus.Solver.Strategies;

public class FireworksStrategy : SudokuStrategy
{
    public const string OfficialName = "Fireworks";
    private const InstanceHandling DefaultInstanceHandling = InstanceHandling.FirstOnly;
    
    public FireworksStrategy() : base(OfficialName, StepDifficulty.Hard, DefaultInstanceHandling)
    {
    }


    public override void Apply(ISudokuSolverData solverData)
    {
        Span<int> rowCandidates = stackalloc int[]{-1, -1, -1, -1, -1, -1, -1, -1, -1};
        Span<int> columnCandidates = stackalloc int[]{-1, -1, -1, -1, -1, -1, -1, -1, -1};
        var dualFireworks = new List<Fireworks>();
        
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                var possibilities = solverData.PossibilitiesAt(row, col);
                if (possibilities.Count == 0) continue;

                foreach (var possibility in possibilities.EnumeratePossibilities())
                {
                    var rowPositions = solverData.RowPositionsAt(row, possibility);
                    var colPositions = solverData.ColumnPositionsAt(col, possibility);

                    var miniRow = row / 3;
                    var miniCol = col / 3;
                    
                    int possibleRow = -1;
                    int possibleCol = -1;

                    foreach (var c in rowPositions)
                    {
                        if (c / 3 == miniCol) continue;
                        
                        if (possibleCol >= 0)
                        {
                            possibleCol = -2;
                            break;
                        }
                            
                        possibleCol = c;
                    }

                    foreach (var r in colPositions)
                    {
                        if (r / 3 == miniRow) continue;
                        
                        if (possibleRow >= 0)
                        {
                            possibleRow = -2;
                            break;
                        }
                            
                        possibleRow = r;
                    }

                    switch (possibleRow, possibleCol)
                    {
                        case (-2, -2) :
                            continue;
                        case (-2, >= 0) :
                            rowCandidates[possibility - 1] = rowPositions.Count == 2 ? possibleCol : -1;
                            break; 
                        case (>= 0, -2) :
                            columnCandidates[possibility - 1] = colPositions.Count == 2 ? possibleRow : -1;
                            break;
                        default :
                            if(possibleCol >= 0) rowCandidates[possibility - 1] = possibleCol;
                            if(possibleRow >= 0) columnCandidates[possibility - 1] = possibleRow;
                            break;
                    }
                }

                var i = 0;
                while (possibilities.HasNextPossibility(ref i))
                {
                    var r1 = rowCandidates[i - 1];
                    var c1 = columnCandidates[i - 1];
                    if (r1 == -1 && c1 == -1) continue;

                    var j = i;
                    while(possibilities.HasNextPossibility(ref j))
                    {
                        var r2 = rowCandidates[j - 1];
                        var c2 = columnCandidates[j - 1];
                        if (r2 == -1 && c2 == -1) continue;

                        if (!TryGetCandidate(r1, r2, true, out var rCandidate)) continue;
                        if (!TryGetCandidate(c1, c2, true, out var cCandidate)) continue;
                        
                        if (rCandidate != -1 && cCandidate != -1)
                        {
                            var poss = new ReadOnlyBitSet16(i, j);
                            dualFireworks.Add(new Fireworks(new Cell(row, col),
                                new Cell(row, rCandidate), new Cell(cCandidate, col), poss));
                        }

                        var k = j;
                        while(possibilities.HasNextPossibility(ref k))
                        {
                            var r3 = rowCandidates[k - 1];
                            var c3 = columnCandidates[k - 1];
                            if (r3 == -1 && c3 == -1) continue;
                                    
                            if (!TryGetCandidate(rowCandidates[k - 1], rCandidate, false, out var rFinal)) 
                                continue;
                            if (!TryGetCandidate(columnCandidates[k - 1], cCandidate, false, out var cFinal))
                                continue;
                                
                            var poss = new ReadOnlyBitSet16(i, j, k);
                                    
                            if(ProcessTripleFireworks(solverData, new Fireworks(new Cell(row, col),
                                   new Cell(row, rFinal), new Cell(cFinal, col), poss)) &&
                               StopOnFirstPush) return;
                        }
                    }
                }

                rowCandidates.Fill(-1);
                columnCandidates.Fill(-1);
            }
        }

        ProcessDualFireworks(solverData, dualFireworks);
    }

    private static bool TryGetCandidate(int one, int two, bool allowNegative, out int result)
    {
        result = -1;
        if (one == -1)
        {
            if (two == -1) return allowNegative;
            
            result = two;
            return true;
        }
        
        if (two == -1 || one == two)
        {
            result = one;
            return true;
        }

        return false;
    }

    private bool ProcessTripleFireworks(ISudokuSolverData user, Fireworks fireworks)
    {
        foreach (var possibility in user.PossibilitiesAt(fireworks.Cross).EnumeratePossibilities())
        {
            if (!fireworks.Possibilities.Contains(possibility)) user.ChangeBuffer.ProposePossibilityRemoval(possibility,
                    fireworks.Cross.Row, fireworks.Cross.Column);
        }
        
        foreach (var possibility in user.PossibilitiesAt(fireworks.RowWing).EnumeratePossibilities())
        {
            if (!fireworks.Possibilities.Contains(possibility)) user.ChangeBuffer.ProposePossibilityRemoval(possibility,
                fireworks.RowWing.Row, fireworks.RowWing.Column);
        }
        
        foreach (var possibility in user.PossibilitiesAt(fireworks.ColumnWing).EnumeratePossibilities())
        {
            if (!fireworks.Possibilities.Contains(possibility)) user.ChangeBuffer.ProposePossibilityRemoval(possibility,
                fireworks.ColumnWing.Row, fireworks.ColumnWing.Column);
        }

        return user.ChangeBuffer.Commit(new FireworksReportBuilder(fireworks));
    }

    private void ProcessDualFireworks(ISudokuSolverData user, List<Fireworks> fireworksList)
    {
        //Quad
        for (int i = 0; i < fireworksList.Count; i++)
        {
            for (int j = i + 1; j < fireworksList.Count; j++)
            {
                var one = fireworksList[i];
                var two = fireworksList[j];
                if (one.Possibilities.ContainsAny(two.Possibilities)) continue;

                if (one.RowWing != two.ColumnWing || one.ColumnWing != two.RowWing) continue;
                
                foreach (var possibility in user.PossibilitiesAt(one.Cross).EnumeratePossibilities())
                {
                    if (!one.Possibilities.Contains(possibility) && !two.Possibilities.Contains(possibility))
                        user.ChangeBuffer.ProposePossibilityRemoval(possibility, one.Cross.Row, one.Cross.Column);
                }
                
                foreach (var possibility in user.PossibilitiesAt(one.RowWing).EnumeratePossibilities())
                {
                    if (!one.Possibilities.Contains(possibility) && !two.Possibilities.Contains(possibility))
                        user.ChangeBuffer.ProposePossibilityRemoval(possibility, one.RowWing.Row, one.RowWing.Column);
                }
                
                foreach (var possibility in user.PossibilitiesAt(two.Cross).EnumeratePossibilities())
                {
                    if (!one.Possibilities.Contains(possibility) && !two.Possibilities.Contains(possibility))
                        user.ChangeBuffer.ProposePossibilityRemoval(possibility, two.Cross.Row, two.Cross.Column);
                }
                
                foreach (var possibility in user.PossibilitiesAt(two.RowWing).EnumeratePossibilities())
                {
                    if (!one.Possibilities.Contains(possibility) && !two.Possibilities.Contains(possibility))
                        user.ChangeBuffer.ProposePossibilityRemoval(possibility, two.RowWing.Row, two.RowWing.Column);
                }

                if (user.ChangeBuffer.Commit( new FireworksReportBuilder(one, two)) &&
                    StopOnFirstPush) return;
            }
        }
        
        //W-Wing
        var allAls = user.PreComputer.AlmostLockedSets();
        List<IPossibilitySet> alsListOne = new();
        List<IPossibilitySet> alsListTwo = new();
        foreach (var df in fireworksList)
        {
            foreach (var als in allAls)
            {
                if (!als.Possibilities.ContainsAll(df.Possibilities)) continue;

                bool one = true;
                bool two = true;

                foreach (var cell in als.EnumerateCells())
                {
                    if (cell == df.Cross || cell == df.RowWing || cell == df.ColumnWing)
                    {
                        one = false;
                        two = false;
                        break;
                    }

                    var possibilities = user.PossibilitiesAt(cell);
                    foreach (var possibility in df.Possibilities.EnumeratePossibilities())
                    {
                        if (!possibilities.Contains(possibility)) continue;

                        if (!SudokuCellUtility.ShareAUnit(cell, df.RowWing)) one = false;
                        if (!SudokuCellUtility.ShareAUnit(cell, df.ColumnWing)) two = false;
                    }

                    if (!one && !two) break;
                }

                if (one) alsListOne.Add(als);
                if (two) alsListTwo.Add(als);
            }

            foreach (var one in alsListOne)
            {
                foreach (var two in alsListTwo)
                {
                    if (one.Equals(two)) continue;

                    List<Cell> total = new(one.EnumerateCells());
                    total.AddRange(two.EnumerateCells());

                    foreach (var sharedSeenCell in SudokuCellUtility.SharedSeenCells(total))
                    {
                        if (sharedSeenCell == df.Cross || sharedSeenCell == df.RowWing ||
                            sharedSeenCell == df.ColumnWing || !SudokuCellUtility.ShareAUnit(sharedSeenCell, df.RowWing) ||
                            !SudokuCellUtility.ShareAUnit(sharedSeenCell, df.ColumnWing)) continue;

                        foreach (var possibility in df.Possibilities.EnumeratePossibilities())
                        {
                            user.ChangeBuffer.ProposePossibilityRemoval(possibility, sharedSeenCell.Row, sharedSeenCell.Column);
                        }
                    }

                    if (user.ChangeBuffer.NotEmpty() && user.ChangeBuffer.Commit(
                            new FireworksWithAlmostLockedSetsReportBuilder(df, one, two))
                                                        && StopOnFirstPush) return;
                }
            }
            
            alsListOne.Clear();
            alsListTwo.Clear();
        }

        //L-Wing
        foreach (var df in fireworksList)
        {
            foreach (var possibility in user.PossibilitiesAt(df.ColumnWing).EnumeratePossibilities())
            {
                if (df.Possibilities.Contains(possibility)) continue;

                var current = new CellPossibility(df.ColumnWing, possibility);
                foreach (var friend in SudokuCellUtility.DefaultStrongLinks(user, current))
                {
                    if (!friend.ShareAUnit(df.RowWing)) continue;

                    user.ChangeBuffer.ProposePossibilityRemoval(possibility, df.RowWing.Row, df.RowWing.Column);
                    if (user.ChangeBuffer.NotEmpty() && user.ChangeBuffer.Commit(
                            new FireworksWithStrongLinkReportBuilder(df, current, friend)) && 
                                StopOnFirstPush) return;
                    break;
                }
            }
            
            foreach (var possibility in user.PossibilitiesAt(df.RowWing).EnumeratePossibilities())
            {
                if (df.Possibilities.Contains(possibility)) continue;

                var current = new CellPossibility(df.RowWing, possibility);
                foreach (var friend in SudokuCellUtility.DefaultStrongLinks(user, current))
                {
                    if (!friend.ShareAUnit(df.ColumnWing)) continue;

                    user.ChangeBuffer.ProposePossibilityRemoval(possibility, df.ColumnWing.Row, df.ColumnWing.Column);
                    if (user.ChangeBuffer.NotEmpty() && user.ChangeBuffer.Commit(
                            new FireworksWithStrongLinkReportBuilder(df, current, friend)) && 
                                StopOnFirstPush) return;
                    break;
                }
            }
        }
        
        //S-Ring
        foreach (var df in fireworksList)
        {
            var opposite = new Cell(df.ColumnWing.Row, df.RowWing.Column);
            if (user.PossibilitiesAt(opposite) == df.Possibilities)
            {
                foreach (var p in user.PossibilitiesAt(df.Cross).EnumeratePossibilities())
                {
                    if (!df.Possibilities.Contains(p)) user.ChangeBuffer.ProposePossibilityRemoval(p, df.Cross);
                }

                if (user.ChangeBuffer.NotEmpty() && user.ChangeBuffer.Commit(
                        new FireworksReportBuilder(df)) && StopOnFirstPush) return;
            }
        }
        
        //Dual ALP
        for (int i = 0; i < fireworksList.Count; i++)
        {
            for (int j = i + 1; j < fireworksList.Count; j++)
            {
                var one = fireworksList[i];
                var two = fireworksList[j];
                
                if(one.Possibilities != two.Possibilities || one.ColumnWing.Row != two.ColumnWing.Row
                   || one.RowWing.Column != two.RowWing.Column) continue;

                var center = new Cell(one.ColumnWing.Row, two.RowWing.Column);
                if (user.PossibilitiesAt(center) != one.Possibilities) continue;

                foreach (var possibility in user.PossibilitiesAt(one.Cross).EnumeratePossibilities())
                {
                    if (one.Possibilities.Contains(possibility)) continue;
                    
                    user.ChangeBuffer.ProposePossibilityRemoval(possibility, one.Cross.Row, one.Cross.Column);
                }
                
                foreach (var possibility in user.PossibilitiesAt(two.Cross).EnumeratePossibilities())
                {
                    if (one.Possibilities.Contains(possibility)) continue;
                    
                    user.ChangeBuffer.ProposePossibilityRemoval(possibility, two.Cross.Row, two.Cross.Column);
                }

                foreach (var possibility in one.Possibilities.EnumeratePossibilities())
                {
                    for (int unit = 0; unit < 9; unit++)
                    {
                        if (unit != center.Column && unit != one.ColumnWing.Column && unit != two.ColumnWing.Column)
                        {
                            user.ChangeBuffer.ProposePossibilityRemoval(possibility, center.Row, unit);
                        }

                        if (unit != center.Row && unit != one.RowWing.Row && unit != two.RowWing.Row)
                        {
                            user.ChangeBuffer.ProposePossibilityRemoval(possibility, unit, center.Column);
                        }
                    }  
                }

                if (user.ChangeBuffer.NotEmpty() && user.ChangeBuffer.Commit(
                        new FireworksWithCellReportBuilder(center, one, two)) &&
                            StopOnFirstPush) return;
            }
        }
    }
}

public class Fireworks
{
    public Fireworks(Cell cross, Cell rowWing, Cell columnWing, ReadOnlyBitSet16 possibilities)
    {
        Cross = cross;
        RowWing = rowWing;
        ColumnWing = columnWing;
        Possibilities = possibilities;
    }

    public Cell Cross { get; }
    public Cell RowWing { get; }
    public Cell ColumnWing { get; }
    public ReadOnlyBitSet16 Possibilities { get; }
}

public static class FireworksHighlightUtils
{
    public static void Highlight(ISudokuHighlighter lighter, Fireworks firework, ISudokuSolvingState snapshot, ref int startColor)
    {
        foreach (var possibility in firework.Possibilities.EnumeratePossibilities())
        {
            if(snapshot.PossibilitiesAt(firework.ColumnWing).Contains(possibility))
                lighter.HighlightPossibility(possibility, firework.ColumnWing.Row, firework.ColumnWing.Column,
                    (StepColor) startColor);
                
            if(snapshot.PossibilitiesAt(firework.RowWing).Contains(possibility))
                lighter.HighlightPossibility(possibility, firework.RowWing.Row, firework.RowWing.Column,
                    (StepColor) startColor);
                
            lighter.HighlightPossibility(possibility, firework.Cross.Row, firework.Cross.Column,
                (StepColor) startColor);

            startColor++;
        }
    }
}

public class FireworksReportBuilder : IChangeReportBuilder<NumericChange, ISudokuSolvingState, ISudokuHighlighter>
{
    private readonly Fireworks[] _fireworks;

    public FireworksReportBuilder(params Fireworks[] fireworks)
    {
        _fireworks = fireworks;
    }

    public ChangeReport<ISudokuHighlighter> BuildReport(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        return new ChangeReport<ISudokuHighlighter>( "", lighter =>
        {
            var color = (int)StepColor.Cause1;

            foreach (var firework in _fireworks)
            {
                FireworksHighlightUtils.Highlight(lighter, firework, snapshot, ref color);
            }

            ChangeReportHelper.HighlightChanges(lighter, changes);
        });
    }
    
    public Clue<ISudokuHighlighter> BuildClue(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        return Clue<ISudokuHighlighter>.Default();
    }
}

public class FireworksWithAlmostLockedSetsReportBuilder : IChangeReportBuilder<NumericChange, ISudokuSolvingState, ISudokuHighlighter>
{
    private readonly Fireworks _fireworks;
    private readonly IPossibilitySet[] _als;

    public FireworksWithAlmostLockedSetsReportBuilder(Fireworks fireworks, params IPossibilitySet[] als)
    {
        _fireworks = fireworks;
        _als = als;
    }

    public ChangeReport<ISudokuHighlighter> BuildReport(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        return new ChangeReport<ISudokuHighlighter>( "", lighter =>
        {
            var color = (int)StepColor.Cause1;

            FireworksHighlightUtils.Highlight(lighter, _fireworks, snapshot, ref color);

            foreach (var als in _als)
            {
                foreach (var cell in als.EnumerateCells())
                {
                    lighter.HighlightCell(cell.Row, cell.Column, (StepColor) color);
                }
                
                color++;
            }

            ChangeReportHelper.HighlightChanges(lighter, changes);
        });
    }
    
    public Clue<ISudokuHighlighter> BuildClue(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        return Clue<ISudokuHighlighter>.Default();
    }
}

public class FireworksWithStrongLinkReportBuilder : IChangeReportBuilder<NumericChange, ISudokuSolvingState, ISudokuHighlighter>
{
    private readonly Fireworks _fireworks;
    private readonly CellPossibility[] _cells;

    public FireworksWithStrongLinkReportBuilder(Fireworks fireworks, params CellPossibility[] cells)
    {
        _fireworks = fireworks;
        _cells = cells;
    }

    public ChangeReport<ISudokuHighlighter> BuildReport(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        return new ChangeReport<ISudokuHighlighter>( "", lighter =>
        {
            int color = (int)StepColor.Cause1;
            FireworksHighlightUtils.Highlight(lighter, _fireworks, snapshot, ref color);

            foreach (var cell in _cells)
            {
                lighter.HighlightPossibility(cell, StepColor.On);
            }

            lighter.CreateLink(_cells[0], _cells[1], LinkStrength.Strong);

            ChangeReportHelper.HighlightChanges(lighter, changes);
        });
    }
    
    public Clue<ISudokuHighlighter> BuildClue(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        return Clue<ISudokuHighlighter>.Default();
    }
}

public class FireworksWithCellReportBuilder : IChangeReportBuilder<NumericChange, ISudokuSolvingState, ISudokuHighlighter>
{
    private readonly Fireworks[] _fireworks;
    private readonly Cell _cell;

    public FireworksWithCellReportBuilder(Cell cell, params Fireworks[] fireworks)
    {
        _fireworks = fireworks;
        _cell = cell;
    }

    public ChangeReport<ISudokuHighlighter> BuildReport(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        return new ChangeReport<ISudokuHighlighter>( "", lighter =>
        {
            int color = (int)StepColor.Cause1;
            foreach (var f in _fireworks)
            {
                FireworksHighlightUtils.Highlight(lighter, f, snapshot, ref color);
            }
            
            lighter.HighlightCell(_cell, StepColor.On);

            ChangeReportHelper.HighlightChanges(lighter, changes);
        });
    }
    
    public Clue<ISudokuHighlighter> BuildClue(IReadOnlyList<NumericChange> changes, ISudokuSolvingState snapshot)
    {
        return Clue<ISudokuHighlighter>.Default();
    }
}