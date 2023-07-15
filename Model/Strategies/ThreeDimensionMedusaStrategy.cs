using System;
using System.Collections.Generic;
using System.Linq;
using Model.Strategies.ChainingStrategiesUtil;

namespace Model.Strategies;

public class ThreeDimensionMedusaStrategy : IStrategy {
    public void ApplyOnce(ISolver solver)
    {
        List<ColorableWeb<MedusaCoordinate>> chains = new();
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                foreach (var possibility in solver.Possibilities[row, col].All())
                {
                    MedusaCoordinate start = new MedusaCoordinate(row, col, possibility);
                    if (DoesAnyChainContains(chains, start)) continue;
                    
                    ColorableWeb<MedusaCoordinate> web = new();
                    InitChain(solver, web, start);
                    if (web.Count >= 2)
                    {
                        web.StartColoring();
                        chains.Add(web);
                    }
                }
            }
        }

        foreach (var chain in chains)
        {
            SearchByCombination(solver, chain);
            SearchOffChain(solver, chain);
        }
        
    }

    private void SearchByCombination(ISolver solver, ColorableWeb<MedusaCoordinate> web)
    {
        web.ForEachCombinationOfTwo((one, two) =>
        {
            if (one.Row == two.Row && one.Col == two.Col)
            {
                //Twice in a cell
                if (one.Coloring == two.Coloring)
                {
                    InvalidColoring(solver, web, one.Coloring, 1);
                    return true;
                }
                //Two colours in a cell
                RemoveAllExcept(solver, one.Row, one.Col, one.Number, two.Number);
            }
            
            //Twice in a unit
            if (one.Number == two.Number && one.ShareAUnit(two) && one.Coloring == two.Coloring)
            {
                InvalidColoring(solver, web, one.Coloring, 2);
                return true;
            }

            return false;
        });
    }

    private void SearchOffChain(ISolver solver, ColorableWeb<MedusaCoordinate> web)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (solver.Sudoku[row, col] != 0) continue;
                bool cellTotallyOfChain = true;
                foreach (var possibility in solver.Possibilities[row, col].All())
                {
                    MedusaCoordinate current = new MedusaCoordinate(row, col, possibility);
                    if (web.Contains(current))
                    {
                        cellTotallyOfChain = false;
                        continue;
                    }

                    bool[] twoElsewhere = new bool[2];
                    bool[] inCell = new bool[2];
                    foreach (var coord in web)
                    {
                        //Two X & Colours sharing a unit
                        if (coord.Number == possibility && coord.ShareAUnit(current)) 
                            twoElsewhere[(int)(coord.Coloring - 1)] = true;

                        //Colour in cell
                        if (coord.Row == current.Row && coord.Col == current.Col)
                            inCell[(int)(coord.Coloring - 1)] = true;
                            
                        
                        //Note : the inCell[0] && inCell[1] should be taken care of by the SearchByCombination function
                        if (twoElsewhere[0] && twoElsewhere[1])
                        {
                            solver.RemovePossibility(possibility, row, col,
                                new MedusaLog(coord.Number, row, col, false, 4));
                            break;
                        }
                        if ((twoElsewhere[0] && inCell[1]) ||
                                  (twoElsewhere[1] && inCell[0]))
                        {
                            solver.RemovePossibility(possibility, row, col,
                                new MedusaLog(coord.Number, row, col, false, 5));
                            break;
                        }
                    }
                }

                if (cellTotallyOfChain)
                {
                    Coordinate here = new Coordinate(row, col);
                    IPossibilities[] cellEmptiedByColor =
                    {
                        solver.Possibilities[row, col].Copy(),
                        solver.Possibilities[row, col].Copy()
                    };

                    foreach (var coord in web)
                    {
                        if (coord.ShareAUnit(here))
                        {
                            cellEmptiedByColor[(int)(coord.Coloring - 1)].Remove(coord.Number);

                            if (cellEmptiedByColor[0].Count == 0 || cellEmptiedByColor[1].Count == 0)
                            {
                                InvalidColoring(solver, web, coord.Coloring, 6);
                                return;
                            }
                        }
                    }
                }
            }
        }
    }

    private void RemoveAllExcept(ISolver solver, int row, int col, int exceptOne, int exceptTwo)
    {
        for (int i = 1; i <= 9; i++)
        {
            if (i != exceptOne && i != exceptTwo)
            {
                solver.RemovePossibility(i, row, col, new MedusaLog(i, row, col, false, 3));
            }
        }
    }

    private void InvalidColoring(ISolver solver, ColorableWeb<MedusaCoordinate> web, Coloring invalid, int type)
    {
        foreach (var coord in web)
        {
            if (coord.Coloring == invalid)
                solver.RemovePossibility(coord.Number, coord.Row, coord.Col,
                    new MedusaLog(coord.Number, coord.Row, coord.Col, false, type));
            else solver.AddDefinitiveNumber(coord.Number, coord.Row, coord.Col,
                new MedusaLog(coord.Number, coord.Row, coord.Col, true, type));
        }
    }

    private bool DoesAnyChainContains(List<ColorableWeb<MedusaCoordinate>> chains, MedusaCoordinate coord)
    {
        foreach (var chain in chains)
        {
            if (chain.Contains(coord)) return true;
        }

        return false;
    }
    
    private void InitChain(ISolver solver, ColorableWeb<MedusaCoordinate> web, MedusaCoordinate current)
    {
        var ppir = solver.PossibilityPositionsInRow(current.Row, current.Number);
        if (ppir.Count == 2)
        {
            foreach (var col in ppir.All())
            {
                if (col != current.Col)
                {
                    MedusaCoordinate next = new MedusaCoordinate(current.Row, col, current.Number);
                    if(web.AddLink(current, next)) InitChain(solver, web, next);
                    break;
                }
            }
        }
        
        var ppic = solver.PossibilityPositionsInColumn(current.Col, current.Number);
        if (ppic.Count == 2)
        {
            foreach (var row in ppic.All())
            {
                if (row != current.Row)
                {
                    MedusaCoordinate next = new MedusaCoordinate(row, current.Col, current.Number);
                    if(web.AddLink(current, next)) InitChain(solver, web, next);
                    break;
                }
            }
        }
        
        var ppimn = solver.PossibilityPositionsInMiniGrid(current.Row / 3, current.Col / 3, current.Number);
        if (ppimn.Count == 2)
        {
            foreach (var pos in ppimn)
            {
                if (pos[0] != current.Row && pos[1] != current.Col)
                {
                    MedusaCoordinate next = new MedusaCoordinate(pos[0], pos[1], current.Number);
                    if(web.AddLink(current, next)) InitChain(solver, web, next);
                    break;
                }
            }
        }

        if (solver.Possibilities[current.Row, current.Col].Count == 2)
        {
            foreach (var possibility in solver.Possibilities[current.Row, current.Col].All())
            {
                if (possibility != current.Number)
                {
                    MedusaCoordinate next = new MedusaCoordinate(current.Row, current.Col, possibility);
                    if(web.AddLink(current, next)) InitChain(solver, web, next);
                    break;
                }
            }
        }
    }
}

public class MedusaCoordinate : ColoringCoordinate
{
    public int Number { get; }
    
    public MedusaCoordinate(int row, int col, int number) : base(row, col)
    {
        Number = number;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Row, Col, Number);
    }

    public override bool Equals(object? obj)
    {
        if (obj is not MedusaCoordinate coord) return false;
        return coord.Row == Row && coord.Col == Col && coord.Number == Number;
    }

    public override string ToString()
    {
        return $"[{Row + 1}, {Col + 1} => {Number}]";
    }
}

public class MedusaLog : ISolverLog
{
    public string AsString { get; }
    public StrategyLevel Level { get; } = StrategyLevel.Hard;

    public MedusaLog(int number, int row, int col, bool definitiveNumber, int type)
    {
        string action = definitiveNumber ? "added as definitive" : "removed from possibilities";
        AsString = $"[{row + 1}, {col + 1}] {number} {action} because of 3D medusa type {type}";
    }
}