using System;
using System.Collections.Generic;
using Model.Helpers;
using Model.Helpers.Changes;
using Model.Helpers.Highlighting;
using Model.Sudokus.Solver.Position;
using Model.Sudokus.Solver.PossibilityPosition;
using Model.Sudokus.Solver.Utility;
using Model.Sudokus.Solver.Utility.Graphs;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Sudokus.Solver.Strategies;

public class FireworksStrategy : SudokuStrategy
{
    public const string OfficialName = "Fireworks";
    private const InstanceHandling DefaultInstanceHandling = InstanceHandling.FirstOnly;
    
    public FireworksStrategy() : base(OfficialName, StrategyDifficulty.Hard, DefaultInstanceHandling)
    {
    }


    public override void Apply(ISudokuStrategyUser strategyUser)
    {
        GridPositions[] limitations = { new(), new(), new(), new(), new(), new(), new(), new(), new() };
        List<Fireworks> dualFireworks = new List<Fireworks>();
        
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                var possibilities = strategyUser.PossibilitiesAt(row, col);
                if (possibilities.Count == 0) continue;

                foreach (var possibility in possibilities.EnumeratePossibilities())
                {
                    var rowPositions = strategyUser.RowPositionsAt(row, possibility);
                    var colPositions = strategyUser.ColumnPositionsAt(col, possibility);
                    var currentLimitations = limitations[possibility - 1];

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
                        case (-2, _) :
                            if (rowPositions.Count != 2) continue;

                            possibleCol = rowPositions.First(col);
                            if (possibleCol / 3 == col / 3) continue;

                            currentLimitations.Add(row, col);
                            currentLimitations.Add(row, possibleCol);

                            break; 
                        case (_, -2) :
                            if (colPositions.Count != 2) continue;

                            possibleRow = colPositions.First(row);
                            if (possibleRow / 3 == row / 3) continue;

                            currentLimitations.Add(row, col);
                            currentLimitations.Add(possibleRow, col);

                            break;
                        default :
                            currentLimitations.Add(row, col);
                            if(possibleCol >= 0) currentLimitations.Add(row, possibleCol);
                            if(possibleRow >= 0) currentLimitations.Add(possibleRow, col);
                            break;
                    }
                }

                for (int i = 0; i < limitations.Length; i++)
                {
                    if (limitations[i].Count == 0) continue;
                    
                    for (int j = i + 1; j < limitations.Length; j++)
                    {
                        if (limitations[j].Count == 0) continue;

                        var or = limitations[i].Or(limitations[j]);
                        if (or.RowCount(row) > 2 || or.ColumnCount(col) > 2) continue;
                        
                        if (or.Count == 3)
                        {
                            var poss = new ReadOnlyBitSet16(i + 1, j + 1);
                            dualFireworks.Add(new Fireworks(or, poss));
                        }

                        if (or.Count <= 3)
                        {
                            for (int k = j + 1; k < limitations.Length; k++)
                            {
                                if (limitations[k].Count == 0) continue;
                                    
                                var or2 = or.Or(limitations[k]);
                                if (or2.RowCount(row) > 2 || or2.ColumnCount(col) > 2) continue;
                                
                                if (or2.Count == 3)
                                {
                                    var poss = new ReadOnlyBitSet16(i + 1, j + 1, k + 1);
                                    
                                    if(ProcessTripleFireworks(strategyUser, new Fireworks(or2, poss)) &&
                                       StopOnFirstPush) return;
                                }
                            }
                        }
                    }
                }

                foreach (var gp in limitations)
                {
                    gp.Void();
                }
            }
        }

        ProcessDualFireworks(strategyUser, dualFireworks);
    }

    private bool ProcessTripleFireworks(ISudokuStrategyUser user, Fireworks fireworks)
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

        return user.ChangeBuffer.Commit( new FireworksReportBuilder(fireworks));
    }

    private void ProcessDualFireworks(ISudokuStrategyUser user, List<Fireworks> fireworksList)
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
        List<IPossibilitiesPositions> alsListOne = new();
        List<IPossibilitiesPositions> alsListTwo = new();
        foreach (var df in fireworksList)
        {
            foreach (var als in allAls)
            {
                if (!als.Possibilities.ContainsAll(df.Possibilities)) continue;

                bool one = true;
                bool two = true;

                foreach (var cell in als.EachCell())
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

                    List<Cell> total = new(one.EachCell());
                    total.AddRange(two.EachCell());

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
            if (user.PossibilitiesAt(opposite).Equals(df.Possibilities))
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
                
                if(!one.Possibilities.Equals(two.Possibilities) || one.ColumnWing.Row != two.ColumnWing.Row
                   || one.RowWing.Column != two.RowWing.Column) continue;

                var center = new Cell(one.ColumnWing.Row, two.RowWing.Column);
                if (!user.PossibilitiesAt(center).Equals(one.Possibilities)) continue;

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

    public Fireworks(GridPositions gp, ReadOnlyBitSet16 possibilities)
    {
        var asArray = gp.ToArray();
        var first = asArray[0];
        int row = -1;
        int col = -1;

        for (int i = 1; i < 3; i++)
        {
            if (asArray[i].Row == first.Row) row = i;
            if (asArray[i].Column == first.Column) col = i;
        }

        switch (row, col)
        {
            case (> 0, > 0) :
                Cross = first;
                RowWing = asArray[row];
                ColumnWing = asArray[col];
                break;
            case (> 0, -1) :
                Cross = asArray[row];
                RowWing = first;
                ColumnWing = asArray[row == 1 ? 2 : 1];
                break;
            case (-1, > 0) :
                Cross = asArray[col];
                ColumnWing = first;
                RowWing = asArray[col == 1 ? 2 : 1];
                break;
            default:
                throw new ArgumentException("Not a firework");
        }
        
        Possibilities = possibilities;
    }

    public Cell Cross { get; }
    public Cell RowWing { get; }
    public Cell ColumnWing { get; }
    public ReadOnlyBitSet16 Possibilities { get; }
}

public static class FireworksHighlightUtils
{
    public static void Highlight(ISudokuHighlighter lighter, Fireworks firework, IUpdatableSudokuSolvingState snapshot, ref int startColor)
    {
        foreach (var possibility in firework.Possibilities.EnumeratePossibilities())
        {
            if(snapshot.PossibilitiesAt(firework.ColumnWing).Contains(possibility))
                lighter.HighlightPossibility(possibility, firework.ColumnWing.Row, firework.ColumnWing.Column,
                    (ChangeColoration) startColor);
                
            if(snapshot.PossibilitiesAt(firework.RowWing).Contains(possibility))
                lighter.HighlightPossibility(possibility, firework.RowWing.Row, firework.RowWing.Column,
                    (ChangeColoration) startColor);
                
            lighter.HighlightPossibility(possibility, firework.Cross.Row, firework.Cross.Column,
                (ChangeColoration) startColor);

            startColor++;
        }
    }
}

public class FireworksReportBuilder : IChangeReportBuilder<IUpdatableSudokuSolvingState, ISudokuHighlighter>
{
    private readonly Fireworks[] _fireworks;

    public FireworksReportBuilder(params Fireworks[] fireworks)
    {
        _fireworks = fireworks;
    }

    public ChangeReport<ISudokuHighlighter> BuildReport(IReadOnlyList<SolverProgress> changes, IUpdatableSudokuSolvingState snapshot)
    {
        return new ChangeReport<ISudokuHighlighter>( "", lighter =>
        {
            var color = (int)ChangeColoration.CauseOffOne;

            foreach (var firework in _fireworks)
            {
                FireworksHighlightUtils.Highlight(lighter, firework, snapshot, ref color);
            }

            ChangeReportHelper.HighlightChanges(lighter, changes);
        });
    }
    
    public Clue<ISudokuHighlighter> BuildClue(IReadOnlyList<SolverProgress> changes, IUpdatableSudokuSolvingState snapshot)
    {
        return Clue<ISudokuHighlighter>.Default();
    }
}

public class FireworksWithAlmostLockedSetsReportBuilder : IChangeReportBuilder<IUpdatableSudokuSolvingState, ISudokuHighlighter>
{
    private readonly Fireworks _fireworks;
    private readonly IPossibilitiesPositions[] _als;

    public FireworksWithAlmostLockedSetsReportBuilder(Fireworks fireworks, params IPossibilitiesPositions[] als)
    {
        _fireworks = fireworks;
        _als = als;
    }

    public ChangeReport<ISudokuHighlighter> BuildReport(IReadOnlyList<SolverProgress> changes, IUpdatableSudokuSolvingState snapshot)
    {
        return new ChangeReport<ISudokuHighlighter>( "", lighter =>
        {
            var color = (int)ChangeColoration.CauseOffOne;

            FireworksHighlightUtils.Highlight(lighter, _fireworks, snapshot, ref color);

            foreach (var als in _als)
            {
                foreach (var cell in als.EachCell())
                {
                    lighter.HighlightCell(cell.Row, cell.Column, (ChangeColoration) color);
                }
                
                color++;
            }

            ChangeReportHelper.HighlightChanges(lighter, changes);
        });
    }
    
    public Clue<ISudokuHighlighter> BuildClue(IReadOnlyList<SolverProgress> changes, IUpdatableSudokuSolvingState snapshot)
    {
        return Clue<ISudokuHighlighter>.Default();
    }
}

public class FireworksWithStrongLinkReportBuilder : IChangeReportBuilder<IUpdatableSudokuSolvingState, ISudokuHighlighter>
{
    private readonly Fireworks _fireworks;
    private readonly CellPossibility[] _cells;

    public FireworksWithStrongLinkReportBuilder(Fireworks fireworks, params CellPossibility[] cells)
    {
        _fireworks = fireworks;
        _cells = cells;
    }

    public ChangeReport<ISudokuHighlighter> BuildReport(IReadOnlyList<SolverProgress> changes, IUpdatableSudokuSolvingState snapshot)
    {
        return new ChangeReport<ISudokuHighlighter>( "", lighter =>
        {
            int color = (int)ChangeColoration.CauseOffOne;
            FireworksHighlightUtils.Highlight(lighter, _fireworks, snapshot, ref color);

            foreach (var cell in _cells)
            {
                lighter.HighlightPossibility(cell, ChangeColoration.CauseOnOne);
            }

            lighter.CreateLink(_cells[0], _cells[1], LinkStrength.Strong);

            ChangeReportHelper.HighlightChanges(lighter, changes);
        });
    }
    
    public Clue<ISudokuHighlighter> BuildClue(IReadOnlyList<SolverProgress> changes, IUpdatableSudokuSolvingState snapshot)
    {
        return Clue<ISudokuHighlighter>.Default();
    }
}

public class FireworksWithCellReportBuilder : IChangeReportBuilder<IUpdatableSudokuSolvingState, ISudokuHighlighter>
{
    private readonly Fireworks[] _fireworks;
    private readonly Cell _cell;

    public FireworksWithCellReportBuilder(Cell cell, params Fireworks[] fireworks)
    {
        _fireworks = fireworks;
        _cell = cell;
    }

    public ChangeReport<ISudokuHighlighter> BuildReport(IReadOnlyList<SolverProgress> changes, IUpdatableSudokuSolvingState snapshot)
    {
        return new ChangeReport<ISudokuHighlighter>( "", lighter =>
        {
            int color = (int)ChangeColoration.CauseOffOne;
            foreach (var f in _fireworks)
            {
                FireworksHighlightUtils.Highlight(lighter, f, snapshot, ref color);
            }
            
            lighter.HighlightCell(_cell, ChangeColoration.CauseOnOne);

            ChangeReportHelper.HighlightChanges(lighter, changes);
        });
    }
    
    public Clue<ISudokuHighlighter> BuildClue(IReadOnlyList<SolverProgress> changes, IUpdatableSudokuSolvingState snapshot)
    {
        return Clue<ISudokuHighlighter>.Default();
    }
}