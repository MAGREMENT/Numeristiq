using System;
using System.Collections.Generic;
using Global;
using Global.Enums;
using Model.Solver.Helpers.Changes;
using Model.Solver.Helpers.Highlighting;
using Model.Solver.Position;
using Model.Solver.Possibility;
using Model.Solver.PossibilityPosition;
using Model.Solver.StrategiesUtil;
using Model.Solver.StrategiesUtil.LinkGraph;

namespace Model.Solver.Strategies;

public class FireworksStrategy : AbstractStrategy
{
    public const string OfficialName = "Fireworks";
    private const OnCommitBehavior DefaultBehavior = OnCommitBehavior.Return;
    
    public FireworksStrategy() : base(OfficialName, StrategyDifficulty.Hard, DefaultBehavior)
    {
    }

    public override OnCommitBehavior DefaultOnCommitBehavior => DefaultBehavior;
    
    
    public override void Apply(IStrategyManager strategyManager)
    {
        GridPositions[] limitations = { new(), new(), new(), new(), new(), new(), new(), new(), new() };
        List<Fireworks> dualFireworks = new List<Fireworks>();
        
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                var possibilities = strategyManager.PossibilitiesAt(row, col);
                if (possibilities.Count == 0) continue;

                foreach (var possibility in possibilities)
                {
                    var rowPositions = strategyManager.RowPositionsAt(row, possibility);
                    var colPositions = strategyManager.ColumnPositionsAt(col, possibility);
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
                            var poss = Possibilities.NewEmpty();
                            poss.Add(i + 1);
                            poss.Add(j + 1);
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
                                    var poss = Possibilities.NewEmpty();
                                    poss.Add(i + 1);
                                    poss.Add(j + 1);
                                    poss.Add(k + 1);
                                    
                                    if(ProcessTripleFireworks(strategyManager, new Fireworks(or2, poss)) &&
                                       OnCommitBehavior == OnCommitBehavior.Return) return;
                                }
                            }
                        }
                    }
                }

                foreach (var gp in limitations)
                {
                    gp.Reset();
                }
            }
        }

        ProcessDualFireworks(strategyManager, dualFireworks);
    }

    private bool ProcessTripleFireworks(IStrategyManager manager, Fireworks fireworks)
    {
        foreach (var possibility in manager.PossibilitiesAt(fireworks.Cross))
        {
            if (!fireworks.Possibilities.Peek(possibility)) manager.ChangeBuffer.ProposePossibilityRemoval(possibility,
                    fireworks.Cross.Row, fireworks.Cross.Col);
        }
        
        foreach (var possibility in manager.PossibilitiesAt(fireworks.RowWing))
        {
            if (!fireworks.Possibilities.Peek(possibility)) manager.ChangeBuffer.ProposePossibilityRemoval(possibility,
                fireworks.RowWing.Row, fireworks.RowWing.Col);
        }
        
        foreach (var possibility in manager.PossibilitiesAt(fireworks.ColumnWing))
        {
            if (!fireworks.Possibilities.Peek(possibility)) manager.ChangeBuffer.ProposePossibilityRemoval(possibility,
                fireworks.ColumnWing.Row, fireworks.ColumnWing.Col);
        }

        return manager.ChangeBuffer.Commit(this, new FireworksReportBuilder(fireworks));
    }

    private void ProcessDualFireworks(IStrategyManager manager, List<Fireworks> fireworksList)
    {
        //Quad
        for (int i = 0; i < fireworksList.Count; i++)
        {
            for (int j = i + 1; j < fireworksList.Count; j++)
            {
                var one = fireworksList[i];
                var two = fireworksList[j];
                if (one.Possibilities.PeekAny(two.Possibilities)) continue;

                if (one.RowWing != two.ColumnWing || one.ColumnWing != two.RowWing) continue;
                
                foreach (var possibility in manager.PossibilitiesAt(one.Cross))
                {
                    if (!one.Possibilities.Peek(possibility) && !two.Possibilities.Peek(possibility))
                        manager.ChangeBuffer.ProposePossibilityRemoval(possibility, one.Cross.Row, one.Cross.Col);
                }
                
                foreach (var possibility in manager.PossibilitiesAt(one.RowWing))
                {
                    if (!one.Possibilities.Peek(possibility) && !two.Possibilities.Peek(possibility))
                        manager.ChangeBuffer.ProposePossibilityRemoval(possibility, one.RowWing.Row, one.RowWing.Col);
                }
                
                foreach (var possibility in manager.PossibilitiesAt(two.Cross))
                {
                    if (!one.Possibilities.Peek(possibility) && !two.Possibilities.Peek(possibility))
                        manager.ChangeBuffer.ProposePossibilityRemoval(possibility, two.Cross.Row, two.Cross.Col);
                }
                
                foreach (var possibility in manager.PossibilitiesAt(two.RowWing))
                {
                    if (!one.Possibilities.Peek(possibility) && !two.Possibilities.Peek(possibility))
                        manager.ChangeBuffer.ProposePossibilityRemoval(possibility, two.RowWing.Row, two.RowWing.Col);
                }

                if (manager.ChangeBuffer.Commit(this, new FireworksReportBuilder(one, two)) &&
                    OnCommitBehavior == OnCommitBehavior.Return) return;
            }
        }
        
        //W-Wing
        var allAls = manager.PreComputer.AlmostLockedSets();
        List<IPossibilitiesPositions> alsListOne = new();
        List<IPossibilitiesPositions> alsListTwo = new();
        foreach (var df in fireworksList)
        {
            foreach (var als in allAls)
            {
                if (!als.Possibilities.PeekAll(df.Possibilities)) continue;

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

                    var possibilities = manager.PossibilitiesAt(cell);
                    foreach (var possibility in df.Possibilities)
                    {
                        if (!possibilities.Peek(possibility)) continue;

                        if (!Cells.ShareAUnit(cell, df.RowWing)) one = false;
                        if (!Cells.ShareAUnit(cell, df.ColumnWing)) two = false;
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

                    foreach (var sharedSeenCell in Cells.SharedSeenCells(total))
                    {
                        if (sharedSeenCell == df.Cross || sharedSeenCell == df.RowWing ||
                            sharedSeenCell == df.ColumnWing || !Cells.ShareAUnit(sharedSeenCell, df.RowWing) ||
                            !Cells.ShareAUnit(sharedSeenCell, df.ColumnWing)) continue;

                        foreach (var possibility in df.Possibilities)
                        {
                            manager.ChangeBuffer.ProposePossibilityRemoval(possibility, sharedSeenCell.Row, sharedSeenCell.Col);
                        }
                    }

                    if (manager.ChangeBuffer.NotEmpty() && manager.ChangeBuffer.Commit(this,
                            new FireworksWithAlmostLockedSetsReportBuilder(df, one, two))
                                                        && OnCommitBehavior == OnCommitBehavior.Return) return;
                }
            }
            
            alsListOne.Clear();
            alsListTwo.Clear();
        }

        //L-Wing
        manager.GraphManager.ConstructSimple(ConstructRule.UnitStrongLink);
        var graph = manager.GraphManager.SimpleLinkGraph;
        foreach (var df in fireworksList)
        {
            foreach (var possibility in manager.PossibilitiesAt(df.ColumnWing))
            {
                if (df.Possibilities.Peek(possibility)) continue;

                var current = new CellPossibility(df.ColumnWing, possibility);
                foreach (var friend in graph.GetLinks(current, LinkStrength.Strong))
                {
                    if (!friend.ShareAUnit(df.RowWing)) continue;

                    manager.ChangeBuffer.ProposePossibilityRemoval(possibility, df.RowWing.Row, df.RowWing.Col);
                    if (manager.ChangeBuffer.NotEmpty() && manager.ChangeBuffer.Commit(this,
                            new FireworksWithStrongLinkReportBuilder(df, current, friend)) && 
                                OnCommitBehavior == OnCommitBehavior.Return) return;
                    break;
                }
            }
            
            foreach (var possibility in manager.PossibilitiesAt(df.RowWing))
            {
                if (df.Possibilities.Peek(possibility)) continue;

                var current = new CellPossibility(df.RowWing, possibility);
                foreach (var friend in graph.GetLinks(current, LinkStrength.Strong))
                {
                    if (!friend.ShareAUnit(df.ColumnWing)) continue;

                    manager.ChangeBuffer.ProposePossibilityRemoval(possibility, df.ColumnWing.Row, df.ColumnWing.Col);
                    if (manager.ChangeBuffer.NotEmpty() && manager.ChangeBuffer.Commit(this,
                            new FireworksWithStrongLinkReportBuilder(df, current, friend)) && 
                                OnCommitBehavior == OnCommitBehavior.Return) return;
                    break;
                }
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
                   || one.RowWing.Col != two.RowWing.Col) continue;

                var center = new Cell(one.ColumnWing.Row, two.RowWing.Col);
                if (!manager.PossibilitiesAt(center).Equals(one.Possibilities)) continue;

                foreach (var possibility in manager.PossibilitiesAt(one.Cross))
                {
                    if (one.Possibilities.Peek(possibility)) continue;
                    
                    manager.ChangeBuffer.ProposePossibilityRemoval(possibility, one.Cross.Row, one.Cross.Col);
                }
                
                foreach (var possibility in manager.PossibilitiesAt(two.Cross))
                {
                    if (one.Possibilities.Peek(possibility)) continue;
                    
                    manager.ChangeBuffer.ProposePossibilityRemoval(possibility, two.Cross.Row, two.Cross.Col);
                }

                foreach (var possibility in one.Possibilities)
                {
                    for (int unit = 0; unit < 9; unit++)
                    {
                        if (unit != center.Col && unit != one.ColumnWing.Col && unit != two.ColumnWing.Col)
                        {
                            manager.ChangeBuffer.ProposePossibilityRemoval(possibility, center.Row, unit);
                        }

                        if (unit != center.Row && unit != one.RowWing.Row && unit != two.RowWing.Row)
                        {
                            manager.ChangeBuffer.ProposePossibilityRemoval(possibility, unit, center.Col);
                        }
                    }  
                }

                if (manager.ChangeBuffer.NotEmpty() && manager.ChangeBuffer.Commit(this,
                        new FireworksWithCellReportBuilder(center, one, two)) &&
                            OnCommitBehavior == OnCommitBehavior.Return) return;
            }
        }
    }
}

public class Fireworks
{
    public Fireworks(Cell cross, Cell rowWing, Cell columnWing, Possibilities possibilities)
    {
        Cross = cross;
        RowWing = rowWing;
        ColumnWing = columnWing;
        Possibilities = possibilities;
    }

    public Fireworks(GridPositions gp, Possibilities possibilities)
    {
        var asArray = gp.ToArray();
        var first = asArray[0];
        int row = -1;
        int col = -1;

        for (int i = 1; i < 3; i++)
        {
            if (asArray[i].Row == first.Row) row = i;
            if (asArray[i].Col == first.Col) col = i;
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
    public Possibilities Possibilities { get; }
}

public static class FireworksHighlightUtils
{
    public static void Highlight(IHighlightable lighter, Fireworks firework, IPossibilitiesHolder snapshot, ref int startColor)
    {
        foreach (var possibility in firework.Possibilities)
        {
            if(snapshot.PossibilitiesAt(firework.ColumnWing).Peek(possibility))
                lighter.HighlightPossibility(possibility, firework.ColumnWing.Row, firework.ColumnWing.Col,
                    (ChangeColoration) startColor);
                
            if(snapshot.PossibilitiesAt(firework.RowWing).Peek(possibility))
                lighter.HighlightPossibility(possibility, firework.RowWing.Row, firework.RowWing.Col,
                    (ChangeColoration) startColor);
                
            lighter.HighlightPossibility(possibility, firework.Cross.Row, firework.Cross.Col,
                (ChangeColoration) startColor);

            startColor++;
        }
    }
}

public class FireworksReportBuilder : IChangeReportBuilder
{
    private readonly Fireworks[] _fireworks;

    public FireworksReportBuilder(params Fireworks[] fireworks)
    {
        _fireworks = fireworks;
    }

    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), "", lighter =>
        {
            var color = (int)ChangeColoration.CauseOffOne;

            foreach (var firework in _fireworks)
            {
                FireworksHighlightUtils.Highlight(lighter, firework, snapshot, ref color);
            }

            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }
}

public class FireworksWithAlmostLockedSetsReportBuilder : IChangeReportBuilder
{
    private readonly Fireworks _fireworks;
    private readonly IPossibilitiesPositions[] _als;

    public FireworksWithAlmostLockedSetsReportBuilder(Fireworks fireworks, params IPossibilitiesPositions[] als)
    {
        _fireworks = fireworks;
        _als = als;
    }

    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), "", lighter =>
        {
            var color = (int)ChangeColoration.CauseOffOne;

            FireworksHighlightUtils.Highlight(lighter, _fireworks, snapshot, ref color);

            foreach (var als in _als)
            {
                foreach (var cell in als.EachCell())
                {
                    lighter.HighlightCell(cell.Row, cell.Col, (ChangeColoration) color);
                }
                
                color++;
            }

            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }
}

public class FireworksWithStrongLinkReportBuilder : IChangeReportBuilder
{
    private readonly Fireworks _fireworks;
    private readonly CellPossibility[] _cells;

    public FireworksWithStrongLinkReportBuilder(Fireworks fireworks, params CellPossibility[] cells)
    {
        _fireworks = fireworks;
        _cells = cells;
    }

    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), "", lighter =>
        {
            int color = (int)ChangeColoration.CauseOffOne;
            FireworksHighlightUtils.Highlight(lighter, _fireworks, snapshot, ref color);

            foreach (var cell in _cells)
            {
                lighter.HighlightPossibility(cell, ChangeColoration.CauseOnOne);
            }

            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }
}

public class FireworksWithCellReportBuilder : IChangeReportBuilder
{
    private readonly Fireworks[] _fireworks;
    private readonly Cell _cell;

    public FireworksWithCellReportBuilder(Cell cell, params Fireworks[] fireworks)
    {
        _fireworks = fireworks;
        _cell = cell;
    }

    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), "", lighter =>
        {
            int color = (int)ChangeColoration.CauseOffOne;
            foreach (var f in _fireworks)
            {
                FireworksHighlightUtils.Highlight(lighter, f, snapshot, ref color);
            }
            
            lighter.HighlightCell(_cell, ChangeColoration.CauseOnOne);

            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }
}