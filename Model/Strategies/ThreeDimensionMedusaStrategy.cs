using System;
using System.Collections.Generic;
using Model.Strategies.ChainingStrategiesUtil;

namespace Model.Strategies;

public class ThreeDimensionMedusaStrategy : IStrategy { //TODO : fix ->   3   8 5s4s1  38 6 4s4s4  83   2  2   3  9   72  1s4s6 1 77  1s4s4 9   5
    public void ApplyOnce(ISolver solver)
    {
        List<ColorableChain<MedusaCoordinate>> chains = new();
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                foreach (var possibility in solver.Possibilities[row, col].All())
                {
                    MedusaCoordinate start = new MedusaCoordinate(row, col, possibility);
                    if (DoesAnyChainContains(chains, start)) continue;
                    
                    ColorableChain<MedusaCoordinate> chain = new();
                    InitChain(solver, chain, start);
                    if (chain.Count > 0)
                    {
                        chain.StartColoring();
                        chains.Add(chain);
                    }
                }
            }
        }

        foreach (var chain in chains)
        {
            SearchByCombination(solver, chain);
            SearchEachCell(solver, chain);
        }
        
    }

    private void SearchByCombination(ISolver solver, ColorableChain<MedusaCoordinate> chain)
    {
        chain.ForEachCombinationOfTwo((one, two) =>
        {
            //Twice in a cell
            if (one.Row == two.Row && one.Col == two.Col && one.Coloring == two.Coloring)
            {
                InvalidColoring(solver, chain, one.Coloring);
            }
            
            //Twice in a unit
            if (one.Number == two.Number && one.ShareAUnit(two))
            {
                InvalidColoring(solver, chain, one.Coloring);
            }
        });
    }

    private void SearchEachCell(ISolver solver, ColorableChain<MedusaCoordinate> chain)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if(solver.Sudoku[row, col] != 0) continue;
                Coordinate current = new Coordinate(row, col);

                int[] twoInACell = new int[2];
                Dictionary<int, bool[]> twoElsewhere = new();
                IPossibilities[] cellEmptiedByColor = new IPossibilities[]
                {
                    solver.Possibilities[row, col].Copy(),
                    solver.Possibilities[row, col].Copy()
                };
                foreach (var coord in chain)
                {
                    //Two colour in a cell
                    if (coord.Row == current.Row && coord.Col == current.Col)
                    {
                        twoInACell[(int)(coord.Coloring - 1)] = coord.Number;
                        if (twoInACell[0] != 0 && twoInACell[1] != 0)
                        {
                            RemoveAllExcept(solver, row, col, twoInACell[0], twoInACell[1]);
                        }
                    }
                    
                    if (coord.ShareAUnit(current))
                    {
                        int arrayCoord = (int)(coord.Coloring - 1);
                        
                        //Two X sharing unit
                        twoElsewhere.TryAdd(coord.Number, new bool[2]);
                        var array = twoElsewhere[coord.Number];
                        array[arrayCoord] = true;
                        if (array[0] && array[1])
                        {
                            solver.RemovePossibility(coord.Number, row, col,
                                new MedusaLog(coord.Number, row, col, false));
                        }
                        
                        //Cell emptied by color
                        cellEmptiedByColor[arrayCoord].Remove(coord.Number);
                        if (cellEmptiedByColor[arrayCoord].Count == 0)
                        {
                            InvalidColoring(solver, chain, coord.Coloring);
                        }
                    }

                    //Two sharing Unit + Cell
                    foreach (var possibility in solver.Possibilities[row, col].All())
                    {
                        bool[]? buffer;
                        if ((twoInACell[0] != 0 && twoElsewhere.TryGetValue(possibility, out buffer) && buffer[1]) ||
                            (twoInACell[1] != 0 && twoElsewhere.TryGetValue(possibility, out buffer) && buffer[0]))
                        {
                            solver.RemovePossibility(possibility, row, col,
                                new MedusaLog(possibility, row, col, false));
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
                solver.RemovePossibility(i, row, col, new MedusaLog(i, row, col, false));
            }
        }
    }

    private void InvalidColoring(ISolver solver, ColorableChain<MedusaCoordinate> chain, Coloring invalid)
    {
        foreach (var coord in chain)
        {
            if (coord.Coloring == invalid)
                solver.RemovePossibility(coord.Number, coord.Row, coord.Col,
                    new MedusaLog(coord.Number, coord.Row, coord.Col, false));
            else solver.AddDefinitiveNumber(coord.Number, coord.Row, coord.Col,
                new MedusaLog(coord.Number, coord.Row, coord.Col, true));
        }
    }

    private bool DoesAnyChainContains(List<ColorableChain<MedusaCoordinate>> chains, MedusaCoordinate coord)
    {
        foreach (var chain in chains)
        {
            if (chain.Contains(coord)) return true;
        }

        return false;
    }
    
    private void InitChain(ISolver solver, ColorableChain<MedusaCoordinate> chain, MedusaCoordinate current)
    {
        var ppir = solver.PossibilityPositionsInRow(current.Row, current.Number);
        if (ppir.Count == 2)
        {
            foreach (var col in ppir.All())
            {
                if (col != current.Col)
                {
                    MedusaCoordinate next = new MedusaCoordinate(current.Row, col, current.Number);
                    if(chain.AddLink(current, next)) InitChain(solver, chain, next);
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
                    if(chain.AddLink(current, next)) InitChain(solver, chain, next);
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
                    if(chain.AddLink(current, next)) InitChain(solver, chain, next);
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
                    MedusaCoordinate next = new MedusaCoordinate(current.Row, current.Col, current.Number);
                    if(chain.AddLink(current, next)) InitChain(solver, chain, next);
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

    public MedusaLog(int number, int row, int col, bool definitiveNumber)
    {
        string action = definitiveNumber ? "added as definitive" : "removed from possibilities";
        AsString = $"[{row + 1}, {col + 1}] {number} {action} because of 3D medusa";
    }
}