using System.Collections.Generic;
using System.Linq;
using Model.Solver.Helpers.Changes;
using Model.Solver.Helpers.Highlighting;
using Model.Solver.Possibilities;
using Model.Solver.StrategiesUtil;

namespace Model.Solver.Strategies;

public class UniqueRectanglesStrategy : AbstractStrategy
{
    public const string OfficialName = "Unique Rectangles";

    public UniqueRectanglesStrategy() : base(OfficialName, StrategyDifficulty.Hard)
    {
        UniquenessDependency = UniquenessDependency.FullyDependent;
    }
    
    public override void ApplyOnce(IStrategyManager strategyManager)
    {
        Dictionary<BiValue, List<Cell>> map = new();
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (strategyManager.PossibilitiesAt(row, col).Count == 2)
                {
                    var asArray = strategyManager.PossibilitiesAt(row, col).ToArray();
                    BiValue bi = new BiValue(asArray[0], asArray[1]);
                    Cell current = new(row, col);

                    if (map.TryGetValue(bi, out var value))
                    {
                        foreach (var b in value)
                        {
                            Process(strategyManager, bi, current, b);
                        }

                        value.Add(current);
                    }
                    else
                    {
                        map[bi] = new List<Cell> { current };
                    }
                }
            }
        }

        //Hidden type 1
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (strategyManager.PossibilitiesAt(row, col).Count <= 2) continue;
                
                foreach (var bi in strategyManager.PossibilitiesAt(row, col).EachBiValue())
                {
                    if (!map.TryGetValue(bi, out var potentialOpposites)) continue;
                    foreach (var potentialOpposite in potentialOpposites)
                    {
                        if (potentialOpposite.Row == row || potentialOpposite.Col == col ||
                            !AreSpreadOverOnlyTwoBoxes(row, col, potentialOpposite.Row, potentialOpposite.Col,
                                potentialOpposite.Row, col, row, potentialOpposite.Col)) continue;
                        
                        if (strategyManager.PossibilitiesAt(row, potentialOpposite.Col).Peek(bi.One)
                            && strategyManager.PossibilitiesAt(row, potentialOpposite.Col).Peek(bi.Two)
                            && strategyManager.PossibilitiesAt(potentialOpposite.Row, col).Peek(bi.One)
                            && strategyManager.PossibilitiesAt(potentialOpposite.Row, col).Peek(bi.Two))
                        {
                            if (strategyManager.RowPositionsAt(row, bi.One).Count == 2
                                && strategyManager.ColumnPositionsAt(col, bi.One).Count == 2)
                            {
                                strategyManager.ChangeBuffer.AddPossibilityToRemove(bi.Two, row, col);
                                if (strategyManager.ChangeBuffer.Push(this, new UniqueRectanglesReportBuilder(
                                        new Cell(row, col), potentialOpposite,
                                        new Cell(row, potentialOpposite.Col),
                                        new Cell(potentialOpposite.Row, col)))) return;
                            }
                            
                            if (strategyManager.RowPositionsAt(row, bi.Two).Count == 2
                                && strategyManager.ColumnPositionsAt(col, bi.Two).Count == 2)
                            {
                                strategyManager.ChangeBuffer.AddPossibilityToRemove(bi.One, row, col);
                                if (strategyManager.ChangeBuffer.Push(this, new UniqueRectanglesReportBuilder(
                                        new Cell(row, col), potentialOpposite,
                                        new Cell(row, potentialOpposite.Col),
                                        new Cell(potentialOpposite.Row, col)))) return;
                            }
                        }
                    }
                }
            }
        }
    }

    private void Process(IStrategyManager view, BiValue bi, Cell one, Cell two)
    {
        if (one.Row == two.Row)
        {
            ProcessSameRow(view, bi, one, two);
        }
        else if (one.Col == two.Col)
        {
            ProcessSameColumn(view, bi, one, two);
        }
        else
        {
            ProcessDiagonal(view, bi, one, two);
        }
    }

    private void ProcessSameRow(IStrategyManager view, BiValue bi, Cell one, Cell two)
    {
        for (int row = 0; row < 9; row++)
        {
            if(row == one.Row) continue;

            if (view.PossibilitiesAt(row, one.Col).Peek(bi.One) && view.PossibilitiesAt(row, two.Col).Peek(bi.One) &&
                view.PossibilitiesAt(row, one.Col).Peek(bi.Two) && view.PossibilitiesAt(row, two.Col).Peek(bi.Two) &&
                AreSpreadOverOnlyTwoBoxes(one.Row, one.Col, two.Row,
                    two.Col, row, one.Col, row, two.Col))
            {
                var roofOne = view.PossibilitiesAt(row, one.Col).Copy();
                var roofTwo = view.PossibilitiesAt(row, two.Col).Copy();
                roofOne.Remove(bi.One);
                roofOne.Remove(bi.Two);
                roofTwo.Remove(bi.One);
                roofTwo.Remove(bi.Two);
                
                //Type 1
                if (roofOne.Count == 0)
                {
                    view.ChangeBuffer.AddPossibilityToRemove(bi.One, row, two.Col);
                    view.ChangeBuffer.AddPossibilityToRemove(bi.Two, row, two.Col);
                    view.ChangeBuffer.Push(this, new UniqueRectanglesReportBuilder(one, two,
                        new Cell(row, one.Col), new Cell(row, two.Col)));
                    
                    return;
                }

                if (roofTwo.Count == 0)
                {
                    view.ChangeBuffer.AddPossibilityToRemove(bi.One, row, one.Col);
                    view.ChangeBuffer.AddPossibilityToRemove(bi.Two, row, one.Col);
                    view.ChangeBuffer.Push(this, new UniqueRectanglesReportBuilder(one, two,
                        new Cell(row, one.Col), new Cell(row, two.Col)));
                    
                    return;
                }

                //Type 2
                if (roofOne.Count == 1 && roofTwo.Count == 1 && roofOne.Equals(roofTwo))
                {
                    int possibility = roofOne.First();
                    foreach (var coord in 
                             Cells.SharedSeenCells(row, one.Col, row, two.Col))
                    {
                        view.ChangeBuffer.AddPossibilityToRemove(possibility, coord.Row, coord.Col);
                    }
                    view.ChangeBuffer.Push(this, new UniqueRectanglesReportBuilder(one, two,
                        new Cell(row, one.Col), new Cell(row, two.Col)));

                    return;
                }

                //Type 4
                if (one.Col / 3 == two.Col / 3)
                {
                    var ppimn = view.MiniGridPositionsAt(row / 3, one.Col / 3, bi.One);
                    if (ppimn.Count == 2)
                    {
                        view.ChangeBuffer.AddPossibilityToRemove(bi.Two, row, one.Col);
                        view.ChangeBuffer.AddPossibilityToRemove(bi.Two, row, two.Col);
                        view.ChangeBuffer.Push(this, new UniqueRectanglesReportBuilder(one, two,
                            new Cell(row, one.Col), new Cell(row, two.Col)));
                        
                        return;
                    }
                    
                    ppimn = view.MiniGridPositionsAt(row / 3, one.Col / 3, bi.Two);
                    if (ppimn.Count == 2)
                    {
                        view.ChangeBuffer.AddPossibilityToRemove(bi.One, row, one.Col);
                        view.ChangeBuffer.AddPossibilityToRemove(bi.One, row, two.Col);
                        view.ChangeBuffer.Push(this, new UniqueRectanglesReportBuilder(one, two,
                            new Cell(row, one.Col), new Cell(row, two.Col)));
                        
                        return;
                    }
                }
                else
                {
                    var ppir = view.RowPositionsAt(row, bi.One);
                    if (ppir.Count == 2)
                    {
                        view.ChangeBuffer.AddPossibilityToRemove(bi.Two, row, one.Col);
                        view.ChangeBuffer.AddPossibilityToRemove(bi.Two, row, two.Col);
                        view.ChangeBuffer.Push(this, new UniqueRectanglesReportBuilder(one, two,
                            new Cell(row, one.Col), new Cell(row, two.Col)));
                        
                        return;
                    }
                    
                    ppir = view.RowPositionsAt(row, bi.Two);
                    if (ppir.Count == 2)
                    {
                        view.ChangeBuffer.AddPossibilityToRemove(bi.One, row, one.Col);
                        view.ChangeBuffer.AddPossibilityToRemove(bi.One, row, two.Col);
                        view.ChangeBuffer.Push(this, new UniqueRectanglesReportBuilder(one, two,
                            new Cell(row, one.Col), new Cell(row, two.Col)));
                        
                        return;
                    }
                }
                
                //Hidden type 2
                if (view.ColumnPositionsAt(one.Col, bi.One).Count == 2)
                {
                    view.ChangeBuffer.AddPossibilityToRemove(bi.Two, row, two.Col);
                    view.ChangeBuffer.Push(this, new UniqueRectanglesReportBuilder(one, two,
                        new Cell(row, one.Col), new Cell(row, two.Col)));
                    return;
                }
                if (view.ColumnPositionsAt(one.Col, bi.Two).Count == 2)
                {
                    view.ChangeBuffer.AddPossibilityToRemove(bi.One, row, two.Col);
                    view.ChangeBuffer.Push(this, new UniqueRectanglesReportBuilder(one, two,
                        new Cell(row, one.Col), new Cell(row, two.Col)));
                    return;
                }
                if (view.ColumnPositionsAt(two.Col, bi.One).Count == 2)
                {
                    view.ChangeBuffer.AddPossibilityToRemove(bi.Two, row, one.Col);
                    view.ChangeBuffer.Push(this, new UniqueRectanglesReportBuilder(one, two,
                        new Cell(row, one.Col), new Cell(row, two.Col)));
                    return;
                }
                if (view.ColumnPositionsAt(two.Col, bi.Two).Count == 2)
                {
                    view.ChangeBuffer.AddPossibilityToRemove(bi.One, row, one.Col);
                    view.ChangeBuffer.Push(this, new UniqueRectanglesReportBuilder(one, two,
                        new Cell(row, one.Col), new Cell(row, two.Col)));
                    return;
                }

                //Type 3
                IPossibilities mashed = roofOne.Or(roofTwo);
                List<Cell> shared = new List<Cell>(
                    Cells.SharedSeenEmptyCells(view, row, one.Col, row, two.Col));

                foreach (var als in AlmostLockedSetSearcher.InCells(view, shared, 4))
                {
                    if (als.Possibilities.Equals(mashed))
                    {
                        RemovePossibilitiesInAllExcept(view, mashed, shared, als);
                        if (!view.ChangeBuffer.NotEmpty()) continue;

                        view.ChangeBuffer.Push(this, new UniqueRectanglesWithAlsReportBuilder(one, two,
                            new Cell(row, one.Col), new Cell(row, two.Col), als));
                        return;
                    }
                }
            }
        }
    }

    private void ProcessSameColumn(IStrategyManager view, BiValue bi, Cell one, Cell two)
    {
        for (int col = 0; col < 9; col++)
        {
            if(col == one.Col) continue;

            if (view.PossibilitiesAt(one.Row, col).Peek(bi.One) && view.PossibilitiesAt(two.Row, col).Peek(bi.One) &&
                view.PossibilitiesAt(one.Row, col).Peek(bi.Two) && view.PossibilitiesAt(two.Row, col).Peek(bi.Two) &&
                AreSpreadOverOnlyTwoBoxes(one.Row, one.Col, two.Row,
                    two.Col, one.Row, col, two.Row, col))
            {
                var roofOne = view.PossibilitiesAt(one.Row, col).Copy();
                var roofTwo = view.PossibilitiesAt(two.Row, col).Copy();
                roofOne.Remove(bi.One);
                roofOne.Remove(bi.Two);
                roofTwo.Remove(bi.One);
                roofTwo.Remove(bi.Two);
                
                //Type 1
                if (roofOne.Count == 0)
                {
                    view.ChangeBuffer.AddPossibilityToRemove(bi.One, two.Row, col);
                    view.ChangeBuffer.AddPossibilityToRemove(bi.Two, two.Row, col);
                    view.ChangeBuffer.Push(this, new UniqueRectanglesReportBuilder(one, two,
                        new Cell(one.Row, col), new Cell(two.Row, col)));
                    
                    return;
                }

                if (roofTwo.Count == 0)
                {
                    view.ChangeBuffer.AddPossibilityToRemove(bi.One, one.Row, col);
                    view.ChangeBuffer.AddPossibilityToRemove(bi.Two, one.Row, col);
                    view.ChangeBuffer.Push(this, new UniqueRectanglesReportBuilder(one, two,
                        new Cell(one.Row, col), new Cell(two.Row, col)));
                    
                    return;
                }

                //Type 2
                if (roofOne.Count == 1 && roofTwo.Count == 1 && roofOne.Equals(roofTwo))
                {
                    int possibility = roofOne.First();
                    foreach (var coord in 
                             Cells.SharedSeenCells(one.Row, col, two.Row, col))
                    {
                        view.ChangeBuffer.AddPossibilityToRemove(possibility, coord.Row, coord.Col);
                    }
                    view.ChangeBuffer.Push(this, new UniqueRectanglesReportBuilder(one, two,
                        new Cell(one.Row, col), new Cell(two.Row, col)));

                    return;
                }

                //Type 4
                if (one.Row / 3 == two.Row / 3)
                {
                    var ppimn = view.MiniGridPositionsAt(one.Row / 3, col / 3, bi.One);
                    if (ppimn.Count == 2)
                    {
                        view.ChangeBuffer.AddPossibilityToRemove(bi.Two, one.Row, col);
                        view.ChangeBuffer.AddPossibilityToRemove(bi.Two, two.Row, col);
                        view.ChangeBuffer.Push(this, new UniqueRectanglesReportBuilder(one, two,
                            new Cell(one.Row, col), new Cell(two.Row, col)));
                        
                        return;
                    }
                    
                    ppimn = view.MiniGridPositionsAt(one.Row / 3, col / 3, bi.Two);
                    if (ppimn.Count == 2)
                    {
                        view.ChangeBuffer.AddPossibilityToRemove(bi.One, one.Row, col);
                        view.ChangeBuffer.AddPossibilityToRemove(bi.One, two.Row, col);
                        view.ChangeBuffer.Push(this, new UniqueRectanglesReportBuilder(one, two,
                            new Cell(one.Row, col), new Cell(two.Row, col)));
                        
                        return;
                    }
                }
                else
                {
                    var ppic = view.ColumnPositionsAt(col, bi.One);
                    if (ppic.Count == 2)
                    {
                        view.ChangeBuffer.AddPossibilityToRemove(bi.Two, one.Row, col);
                        view.ChangeBuffer.AddPossibilityToRemove(bi.Two, two.Row, col);
                        view.ChangeBuffer.Push(this, new UniqueRectanglesReportBuilder(one, two,
                            new Cell(one.Row, col), new Cell(two.Row, col)));
                        
                        return;
                    }
                    
                    ppic = view.ColumnPositionsAt(col, bi.Two);
                    if (ppic.Count == 2)
                    {
                        view.ChangeBuffer.AddPossibilityToRemove(bi.One, one.Row, col);
                        view.ChangeBuffer.AddPossibilityToRemove(bi.One, two.Row, col);
                        view.ChangeBuffer.Push(this, new UniqueRectanglesReportBuilder(one, two,
                            new Cell(one.Row, col), new Cell(two.Row, col)));
                        
                        return;
                    }
                }
                
                //Hidden type 2
                if (view.RowPositionsAt(one.Row, bi.One).Count == 2)
                {
                    view.ChangeBuffer.AddPossibilityToRemove(bi.Two, two.Row, col);
                    view.ChangeBuffer.Push(this, new UniqueRectanglesReportBuilder(one, two,
                        new Cell(one.Row, col), new Cell(two.Row, col)));
                    
                    return;
                }
                if (view.RowPositionsAt(one.Row, bi.Two).Count == 2)
                {
                    view.ChangeBuffer.AddPossibilityToRemove(bi.One, two.Row, col);
                    view.ChangeBuffer.Push(this, new UniqueRectanglesReportBuilder(one, two,
                        new Cell(one.Row, col), new Cell(two.Row, col)));
                    
                    return;
                }
                if (view.RowPositionsAt(two.Row, bi.One).Count == 2)
                {
                    view.ChangeBuffer.AddPossibilityToRemove(bi.Two, one.Row, col);
                    view.ChangeBuffer.Push(this, new UniqueRectanglesReportBuilder(one, two,
                        new Cell(one.Row, col), new Cell(two.Row, col)));
                    
                    return;
                }
                if (view.RowPositionsAt(two.Row, bi.Two).Count == 2)
                {
                    view.ChangeBuffer.AddPossibilityToRemove(bi.One, one.Row, col);
                    view.ChangeBuffer.Push(this, new UniqueRectanglesReportBuilder(one, two,
                        new Cell(one.Row, col), new Cell(two.Row, col)));
                    
                    return;
                }

                //Type 3
                IPossibilities mashed = roofOne.Or(roofTwo);
                List<Cell> shared = new List<Cell>(
                    Cells.SharedSeenEmptyCells(view, one.Row, col, two.Row, col));

                foreach (var als in AlmostLockedSetSearcher.InCells(view, shared, 4))
                {
                    if (als.Possibilities.Equals(mashed))
                    {
                        RemovePossibilitiesInAllExcept(view, mashed, shared, als);
                        if (!view.ChangeBuffer.NotEmpty()) continue;
                        
                        view.ChangeBuffer.Push(this, new UniqueRectanglesWithAlsReportBuilder(one, two,
                            new Cell(one.Row, col), new Cell(two.Row, col), als));
                        return;
                    }
                        
                }
            }
        }
    }

    private void ProcessDiagonal(IStrategyManager view, BiValue bi, Cell one, Cell two)
    {
        if (view.PossibilitiesAt(one.Row, two.Col).Peek(bi.One) && view.PossibilitiesAt(one.Row, two.Col).Peek(bi.Two) &&
            view.PossibilitiesAt(two.Row, one.Col).Peek(bi.One) && view.PossibilitiesAt(two.Row, one.Col).Peek(bi.Two) &&
            AreSpreadOverOnlyTwoBoxes(one.Row, one.Col, two.Row,
                two.Col, one.Row, two.Col, two.Row, one.Col))
        {
            var roofOne = view.PossibilitiesAt(one.Row, two.Col).Copy();
            var roofTwo = view.PossibilitiesAt(two.Row, one.Col).Copy();
            roofOne.Remove(bi.One);
            roofOne.Remove(bi.Two);
            roofTwo.Remove(bi.One);
            roofTwo.Remove(bi.One);
            
            //Type 2
            if (roofOne.Count == 1 && roofTwo.Count == 1 && roofOne.Equals(roofTwo))
            {
                int possibility = roofOne.First();
                foreach (var coord in 
                         Cells.SharedSeenCells(one.Row, two.Col, two.Row, one.Col))
                {
                    if ((coord.Row == one.Row && coord.Col == one.Col) ||
                        (coord.Row == two.Row && coord.Col == two.Col)) continue;
                    view.ChangeBuffer.AddPossibilityToRemove(possibility, coord.Row, coord.Col);
                }
                view.ChangeBuffer.Push(this, new UniqueRectanglesReportBuilder(one, two,
                    new Cell(one.Row, two.Col), new Cell(two.Row, one.Col)));

                return;
            }
            
            //Type 5
            if (view.RowPositionsAt(one.Row, bi.One).Count == 2 &&
                view.RowPositionsAt(two.Row, bi.One).Count == 2 &&
                view.ColumnPositionsAt(one.Col, bi.One).Count == 2 &&
                view.ColumnPositionsAt(two.Col, bi.One).Count == 2)
            {
                view.ChangeBuffer.AddPossibilityToRemove(bi.Two, one.Row, one.Col);
                view.ChangeBuffer.AddPossibilityToRemove(bi.Two, two.Row, two.Col);
                view.ChangeBuffer.Push(this, new UniqueRectanglesReportBuilder(one, two,
                    new Cell(one.Row, two.Col), new Cell(two.Row, one.Col)));
                
                return;
            }
            
            if (view.RowPositionsAt(one.Row, bi.Two).Count == 2 &&
                view.RowPositionsAt(two.Row, bi.Two).Count == 2 &&
                view.ColumnPositionsAt(one.Col, bi.Two).Count == 2 &&
                view.ColumnPositionsAt(two.Col, bi.Two).Count == 2)
            {
                view.ChangeBuffer.AddPossibilityToRemove(bi.One, one.Row, one.Col);
                view.ChangeBuffer.AddPossibilityToRemove(bi.One, two.Row, two.Col);
                view.ChangeBuffer.Push(this, new UniqueRectanglesReportBuilder(one, two,
                    new Cell(one.Row, two.Col), new Cell(two.Row, one.Col)));
            }
        }
    }

    private bool AreSpreadOverOnlyTwoBoxes(int row1, int col1, int row2, int col2, int row3, int col3, int row4,
        int col4)
    {
        HashSet<int> rows = new();
        HashSet<int> cols = new();

        rows.Add(row1 / 3);
        rows.Add(row2 / 3);
        rows.Add(row3 / 3);
        rows.Add(row4 / 3);

        cols.Add(col1 / 3);
        cols.Add(col2 / 3);
        cols.Add(col3 / 3);
        cols.Add(col4 / 3);

        return (rows.Count == 2 && cols.Count == 1) || (rows.Count == 1 && cols.Count == 2);
    }

    private void RemovePossibilitiesInAllExcept(IStrategyManager view, IPossibilities poss, List<Cell> coords,
        AlmostLockedSet except)
    {
        foreach (var coord in coords)
        {
            if(except.Contains(coord) || !except.ShareAUnit(coord)) continue;
            foreach (var possibility in poss)
            {
                view.ChangeBuffer.AddPossibilityToRemove(possibility, coord.Row, coord.Col);
            }
        }
    }
}

public readonly struct BiValue
{
    public BiValue(int one, int two)
    {
        One = one;
        Two = two;
    }

    public int One { get; }
    public int Two { get; }

    public override int GetHashCode()
    {
        return One ^ Two;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not BiValue bi) return false;
        return (bi.One == One && bi.Two == Two) || (bi.One == Two && bi.Two == One);
    }

    public static bool operator ==(BiValue left, BiValue right)
    {
        return (left.One == right.One && left.Two == right.Two) || (left.One == right.Two && left.Two == right.One);
    }

    public static bool operator !=(BiValue left, BiValue right)
    {
        return !(left == right);
    }
}

public class UniqueRectanglesReportBuilder : IChangeReportBuilder
{
    private readonly Cell _floorOne;
    private readonly Cell _floorTwo;
    private readonly Cell _roofOne;
    private readonly Cell _roofTwo;

    public UniqueRectanglesReportBuilder(Cell floorOne, Cell floorTwo, Cell roofOne, Cell roofTwo)
    {
        _floorOne = floorOne;
        _floorTwo = floorTwo;
        _roofOne = roofOne;
        _roofTwo = roofTwo;
    }
    
    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), "", lighter =>
        {
            lighter.HighlightCell(_floorOne.Row, _floorOne.Col, ChangeColoration.CauseOffOne);
            lighter.HighlightCell(_floorTwo.Row, _floorTwo.Col, ChangeColoration.CauseOffOne);
            lighter.HighlightCell(_roofOne.Row, _roofOne.Col, ChangeColoration.CauseOffTwo);
            lighter.HighlightCell(_roofTwo.Row, _roofTwo.Col, ChangeColoration.CauseOffTwo);
            
            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }
}

public class UniqueRectanglesWithAlsReportBuilder : IChangeReportBuilder
{
    private readonly Cell _floorOne;
    private readonly Cell _floorTwo;
    private readonly Cell _roofOne;
    private readonly Cell _roofTwo;
    private readonly AlmostLockedSet _als;

    public UniqueRectanglesWithAlsReportBuilder(Cell floorOne, Cell floorTwo, Cell roofOne,
        Cell roofTwo, AlmostLockedSet als)
    {
        _floorOne = floorOne;
        _floorTwo = floorTwo;
        _roofOne = roofOne;
        _roofTwo = roofTwo;
        _als = als;
    }
    
    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), "", lighter =>
        {
            lighter.HighlightCell(_floorOne.Row, _floorOne.Col, ChangeColoration.CauseOffOne);
            lighter.HighlightCell(_floorTwo.Row, _floorTwo.Col, ChangeColoration.CauseOffOne);
            lighter.HighlightCell(_roofOne.Row, _roofOne.Col, ChangeColoration.CauseOffTwo);
            lighter.HighlightCell(_roofTwo.Row, _roofTwo.Col, ChangeColoration.CauseOffTwo);

            foreach (var coord in _als.Coordinates)
            {
                lighter.HighlightCell(coord.Row, coord.Col, ChangeColoration.CauseOffThree);
            }
            
            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }
}