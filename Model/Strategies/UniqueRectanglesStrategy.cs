using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Model.Possibilities;
using Model.StrategiesUtil;

namespace Model.Strategies;

public class UniqueRectanglesStrategy : IStrategy
{
    public string Name => "Unique rectangles";
    public StrategyLevel Difficulty => StrategyLevel.Hard;
    public int Score { get; set; }
    public void ApplyOnce(IStrategyManager strategyManager)
    {
        Dictionary<BiValue, List<Coordinate>> map = new();
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (strategyManager.Possibilities[row, col].Count == 2)
                {
                    var asArray = strategyManager.Possibilities[row, col].ToArray();
                    BiValue bi = new BiValue(asArray[0], asArray[1]);
                    Coordinate current = new(row, col);

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
                        map[bi] = new List<Coordinate> { current };
                    }
                }
            }
        }

        //Hidden type 1
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (strategyManager.Possibilities[row, col].Count <= 2) continue;
                
                foreach (var bi in strategyManager.Possibilities[row, col].EachBiValue())
                {
                    if (!map.TryGetValue(bi, out var potentialOpposites)) continue;
                    foreach (var potentialOpposite in potentialOpposites)
                    {
                        if (potentialOpposite.Row == row || potentialOpposite.Col == col ||
                            !AreSpreadOverOnlyTwoBoxes(row, col, potentialOpposite.Row, potentialOpposite.Col,
                                potentialOpposite.Row, col, row, potentialOpposite.Col)) continue;
                        
                        if (strategyManager.Possibilities[row, potentialOpposite.Col].Peek(bi.One)
                            && strategyManager.Possibilities[row, potentialOpposite.Col].Peek(bi.Two)
                            && strategyManager.Possibilities[potentialOpposite.Row, col].Peek(bi.One)
                            && strategyManager.Possibilities[potentialOpposite.Row, col].Peek(bi.Two))
                        {
                            if (strategyManager.PossibilityPositionsInRow(row, bi.One).Count == 2
                                && strategyManager.PossibilityPositionsInColumn(col, bi.One).Count == 2)
                            {
                                strategyManager.RemovePossibility(bi.Two, row, col, this);
                            }
                            
                            if (strategyManager.PossibilityPositionsInRow(row, bi.Two).Count == 2
                                && strategyManager.PossibilityPositionsInColumn(col, bi.Two).Count == 2)
                            {
                                strategyManager.RemovePossibility(bi.One, row, col, this);
                            }
                        }
                    }
                }
            }
        }
    }

    private void Process(IStrategyManager view, BiValue bi, Coordinate one, Coordinate two)
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

    private void ProcessSameRow(IStrategyManager view, BiValue bi, Coordinate one, Coordinate two)
    {
        for (int row = 0; row < 9; row++)
        {
            if(row == one.Row) continue;

            if (view.Possibilities[row, one.Col].Peek(bi.One) && view.Possibilities[row, two.Col].Peek(bi.One) &&
                view.Possibilities[row, one.Col].Peek(bi.Two) && view.Possibilities[row, two.Col].Peek(bi.Two) &&
                AreSpreadOverOnlyTwoBoxes(one.Row, one.Col, two.Row,
                    two.Col, row, one.Col, row, two.Col))
            {
                IPossibilities roofOne = view.Possibilities[row, one.Col].Copy();
                IPossibilities roofTwo = view.Possibilities[row, two.Col].Copy();
                roofOne.Remove(bi.One);
                roofOne.Remove(bi.Two);
                roofTwo.Remove(bi.One);
                roofTwo.Remove(bi.Two);
                
                //Type 1
                if (roofOne.Count == 0)
                {
                    view.RemovePossibility(bi.One, row, two.Col, this);
                    view.RemovePossibility(bi.Two, row, two.Col, this);
                    return;
                }

                if (roofTwo.Count == 0)
                {
                    view.RemovePossibility(bi.One, row, one.Col, this);
                    view.RemovePossibility(bi.Two, row, one.Col, this);
                    return;
                }

                //Type 2
                if (roofOne.Count == 1 && roofTwo.Count == 1 && roofOne.Equals(roofTwo))
                {
                    int possibility = roofOne.GetFirst();
                    foreach (var coord in 
                             Coordinate.SharedSeenCells(row, one.Col, row, two.Col))
                    {
                        view.RemovePossibility(possibility, coord.Row, coord.Col, this);
                    }

                    return;
                }
                
                //Hidden type 2
                if (view.PossibilityPositionsInColumn(one.Col, bi.One).Count == 2)
                {
                    view.RemovePossibility(bi.Two, row, two.Col, this);
                    return;
                }
                if (view.PossibilityPositionsInColumn(one.Col, bi.Two).Count == 2)
                {
                    view.RemovePossibility(bi.One, row, two.Col, this);
                    return;
                }
                if (view.PossibilityPositionsInColumn(two.Col, bi.One).Count == 2)
                {
                    view.RemovePossibility(bi.Two, row, one.Col, this);
                    return;
                }
                if (view.PossibilityPositionsInColumn(two.Col, bi.Two).Count == 2)
                {
                    view.RemovePossibility(bi.One, row, one.Col, this);
                    return;
                }
                
                //Type 4
                if (one.Col / 3 == two.Col / 3)
                {
                    var ppimn = view.PossibilityPositionsInMiniGrid(row / 3, one.Col / 3, bi.One);
                    if (ppimn.Count == 2)
                    {
                        view.RemovePossibility(bi.Two, row, one.Col, this);
                        view.RemovePossibility(bi.Two, row, two.Col, this);
                        return;
                    }
                    
                    ppimn = view.PossibilityPositionsInMiniGrid(row / 3, one.Col / 3, bi.Two);
                    if (ppimn.Count == 2)
                    {
                        view.RemovePossibility(bi.One, row, one.Col, this);
                        view.RemovePossibility(bi.One, row, two.Col, this);
                        return;
                    }
                }
                else
                {
                    var ppir = view.PossibilityPositionsInRow(row, bi.One);
                    if (ppir.Count == 2)
                    {
                        view.RemovePossibility(bi.Two, row, one.Col, this);
                        view.RemovePossibility(bi.Two, row, two.Col, this);
                        return;
                    }
                    
                    ppir = view.PossibilityPositionsInRow(row, bi.Two);
                    if (ppir.Count == 2)
                    {
                        view.RemovePossibility(bi.One, row, one.Col, this);
                        view.RemovePossibility(bi.One, row, two.Col, this);
                        return;
                    }
                }

                //Type 3
                IPossibilities mashed = roofOne.Mash(roofTwo);
                List<Coordinate> shared = new List<Coordinate>(
                    Coordinate.SharedSeenEmptyCells(view, row, one.Col, row, two.Col));

                foreach (var als in AlmostLockedSet.SearchForSingleCellAls(view, shared))
                {
                    if (als.Possibilities.Equals(mashed) && RemovePossibilitiesInAllExcept(view,
                            mashed, shared, als)) return;
                }

                for (int i = 2; i <= 4; i++)
                {
                    foreach (var als in AlmostLockedSet.SearchForMultipleCellsAls(view, shared, i))
                    {
                        if (als.Possibilities.PeekAll(mashed) && RemovePossibilitiesInAllExcept(view,
                                mashed, shared, als)) return;
                    }
                }
            }
        }
    }

    private void ProcessSameColumn(IStrategyManager view, BiValue bi, Coordinate one, Coordinate two)
    {
        for (int col = 0; col < 9; col++)
        {
            if(col == one.Col) continue;

            if (view.Possibilities[one.Row, col].Peek(bi.One) && view.Possibilities[two.Row, col].Peek(bi.One) &&
                view.Possibilities[one.Row, col].Peek(bi.Two) && view.Possibilities[two.Row, col].Peek(bi.Two) &&
                AreSpreadOverOnlyTwoBoxes(one.Row, one.Col, two.Row,
                    two.Col, one.Row, col, two.Row, col))
            {
                IPossibilities roofOne = view.Possibilities[one.Row, col].Copy();
                IPossibilities roofTwo = view.Possibilities[two.Row, col].Copy();
                roofOne.Remove(bi.One);
                roofOne.Remove(bi.Two);
                roofTwo.Remove(bi.One);
                roofTwo.Remove(bi.Two);
                
                //Type 1
                if (roofOne.Count == 0)
                {
                    view.RemovePossibility(bi.One, two.Row, col, this);
                    view.RemovePossibility(bi.Two, two.Row, col, this);
                    return;
                }

                if (roofTwo.Count == 0)
                {
                    view.RemovePossibility(bi.One, one.Row, col, this);
                    view.RemovePossibility(bi.Two, one.Row, col, this);
                    return;
                }

                //Type 2
                if (roofOne.Count == 1 && roofTwo.Count == 1 && roofOne.Equals(roofTwo))
                {
                    int possibility = roofOne.GetFirst();
                    foreach (var coord in 
                             Coordinate.SharedSeenCells(one.Row, col, two.Row, col))
                    {
                        view.RemovePossibility(possibility, coord.Row, coord.Col, this);
                    }

                    return;
                }
                
                //Hidden type 2
                if (view.PossibilityPositionsInRow(one.Row, bi.One).Count == 2)
                {
                    view.RemovePossibility(bi.Two, two.Row, col, this);
                    return;
                }
                if (view.PossibilityPositionsInRow(one.Row, bi.Two).Count == 2)
                {
                    view.RemovePossibility(bi.One, two.Row, col, this);
                    return;
                }
                if (view.PossibilityPositionsInRow(two.Row, bi.One).Count == 2)
                {
                    view.RemovePossibility(bi.Two, one.Row, col, this);
                    return;
                }
                if (view.PossibilityPositionsInRow(two.Row, bi.Two).Count == 2)
                {
                    view.RemovePossibility(bi.One, one.Row, col, this);
                    return;
                }
                
                //Type 4
                if (one.Row / 3 == two.Row / 3)
                {
                    var ppimn = view.PossibilityPositionsInMiniGrid(one.Row / 3, col / 3, bi.One);
                    if (ppimn.Count == 2)
                    {
                        view.RemovePossibility(bi.Two, one.Row, col, this);
                        view.RemovePossibility(bi.Two, two.Row, col, this);
                        return;
                    }
                    
                    ppimn = view.PossibilityPositionsInMiniGrid(one.Row / 3, col / 3, bi.Two);
                    if (ppimn.Count == 2)
                    {
                        view.RemovePossibility(bi.One, one.Row, col, this);
                        view.RemovePossibility(bi.One, two.Row, col, this);
                        return;
                    }
                }
                else
                {
                    var ppic = view.PossibilityPositionsInColumn(col, bi.One);
                    if (ppic.Count == 2)
                    {
                        view.RemovePossibility(bi.Two, one.Row, col, this);
                        view.RemovePossibility(bi.Two, two.Row, col, this);
                        return;
                    }
                    
                    ppic = view.PossibilityPositionsInColumn(col, bi.Two);
                    if (ppic.Count == 2)
                    {
                        view.RemovePossibility(bi.One, one.Row, col, this);
                        view.RemovePossibility(bi.One, two.Row, col, this);
                        return;
                    }
                }

                //Type 3
                IPossibilities mashed = roofOne.Mash(roofTwo);
                List<Coordinate> shared = new List<Coordinate>(
                    Coordinate.SharedSeenEmptyCells(view, one.Row, col, two.Row, col));

                foreach (var als in AlmostLockedSet.SearchForSingleCellAls(view, shared))
                {
                    if (als.Possibilities.Equals(mashed) && RemovePossibilitiesInAllExcept(view,
                            mashed, shared, als)) return;
                }

                for (int i = 2; i <= 4; i++)
                {
                    foreach (var als in AlmostLockedSet.SearchForMultipleCellsAls(view, shared, i))
                    {
                        if (als.Possibilities.PeekAll(mashed) && RemovePossibilitiesInAllExcept(view,
                                mashed, shared, als)) return;
                    }
                }
            }
        }
    }

    private void ProcessDiagonal(IStrategyManager view, BiValue bi, Coordinate one, Coordinate two)
    {
        if (view.Possibilities[one.Row, two.Col].Peek(bi.One) && view.Possibilities[one.Row, two.Col].Peek(bi.Two) &&
            view.Possibilities[two.Row, one.Col].Peek(bi.One) && view.Possibilities[two.Row, one.Col].Peek(bi.Two) &&
            AreSpreadOverOnlyTwoBoxes(one.Row, one.Col, two.Row,
                two.Col, one.Row, two.Col, two.Row, one.Col))
        {
            IPossibilities roofOne = view.Possibilities[one.Row, two.Col].Copy();
            IPossibilities roofTwo = view.Possibilities[two.Row, one.Col].Copy();
            roofOne.Remove(bi.One);
            roofOne.Remove(bi.Two);
            roofTwo.Remove(bi.One);
            roofTwo.Remove(bi.One);
            
            //Type 2
            if (roofOne.Count == 1 && roofTwo.Count == 1 && roofOne.Equals(roofTwo))
            {
                int possibility = roofOne.GetFirst();
                foreach (var coord in 
                         Coordinate.SharedSeenCells(one.Row, two.Col, two.Row, one.Col))
                {
                    if ((coord.Row == one.Row && coord.Col == one.Col) ||
                        (coord.Row == two.Row && coord.Col == two.Col)) continue;
                    view.RemovePossibility(possibility, coord.Row, coord.Col, this);
                }

                return;
            }
            
            //Type 5
            if (view.PossibilityPositionsInRow(one.Row, bi.One).Count == 2 &&
                view.PossibilityPositionsInRow(two.Row, bi.One).Count == 2 &&
                view.PossibilityPositionsInColumn(one.Col, bi.One).Count == 2 &&
                view.PossibilityPositionsInColumn(two.Col, bi.One).Count == 2)
            {
                view.RemovePossibility(bi.Two, one.Row, one.Col, this);
                view.RemovePossibility(bi.Two, two.Row, two.Col, this);
                return;
            }
            
            if (view.PossibilityPositionsInRow(one.Row, bi.Two).Count == 2 &&
                view.PossibilityPositionsInRow(two.Row, bi.Two).Count == 2 &&
                view.PossibilityPositionsInColumn(one.Col, bi.Two).Count == 2 &&
                view.PossibilityPositionsInColumn(two.Col, bi.Two).Count == 2)
            {
                view.RemovePossibility(bi.One, one.Row, one.Col, this);
                view.RemovePossibility(bi.One, two.Row, two.Col, this);
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

    private bool RemovePossibilitiesInAllExcept(IStrategyManager view, IPossibilities poss, List<Coordinate> coords,
        AlmostLockedSet except)
    {
        bool wasProgressMade = false;
        foreach (var coord in coords)
        {
            if(except.Contains(coord) || !except.ShareAUnit(coord)) continue;
            foreach (var possibility in poss)
            {
                if (view.RemovePossibility(possibility, coord.Row, coord.Col, this)) wasProgressMade = true;
            }
        }

        return wasProgressMade;
    }
}

public class BiValue
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
}