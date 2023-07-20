using System;
using System.Collections.Generic;
using System.Linq;
using Model.Possibilities;
using Model.StrategiesUtil;

namespace Model.Strategies;

public class ThreeDimensionMedusaStrategy : IStrategy {
    public string Name { get; } = "3D medusa";
    
    public StrategyLevel Difficulty { get; } = StrategyLevel.Hard;

    public void ApplyOnce(ISolverView solverView)
    {
        List<ColorableWeb<MedusaCoordinate>> chains = new();
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                foreach (var possibility in solverView.Possibilities[row, col])
                {
                    MedusaCoordinate start = new MedusaCoordinate(row, col, possibility);
                    if (DoesAnyChainContains(chains, start)) continue;
                    
                    ColorableWeb<MedusaCoordinate> web = new();
                    InitChain(solverView, web, start);
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
            SearchByCombination(solverView, chain);
            SearchOffChain(solverView, chain);
        }
        
    }

    private void SearchByCombination(ISolverView solverView, ColorableWeb<MedusaCoordinate> web)
    {
        web.ForEachCombinationOfTwo((one, two) =>
        {
            if (one.Row == two.Row && one.Col == two.Col)
            {
                //Twice in a cell
                if (one.Coloring == two.Coloring)
                {
                    InvalidColoring(solverView, web, one.Coloring, 1);
                    return true;
                }
                //Two colours in a cell
                RemoveAllExcept(solverView, one.Row, one.Col, one.Number, two.Number);
            }
            
            //Twice in a unit
            if (one.Number == two.Number && one.ShareAUnit(two) && one.Coloring == two.Coloring)
            {
                InvalidColoring(solverView, web, one.Coloring, 2);
                return true;
            }

            return false;
        });
    }

    private void SearchOffChain(ISolverView solverView, ColorableWeb<MedusaCoordinate> web)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (solverView.Sudoku[row, col] != 0) continue;
                bool cellTotallyOfChain = true;
                foreach (var possibility in solverView.Possibilities[row, col])
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
                            solverView.RemovePossibility(possibility, row, col, this);
                            break;
                        }
                        if ((twoElsewhere[0] && inCell[1]) ||
                                  (twoElsewhere[1] && inCell[0]))
                        {
                            solverView.RemovePossibility(possibility, row, col, this);
                            break;
                        }
                    }
                }

                if (cellTotallyOfChain)
                {
                    Coordinate here = new Coordinate(row, col);
                    IPossibilities[] cellEmptiedByColor =
                    {
                        solverView.Possibilities[row, col].Copy(),
                        solverView.Possibilities[row, col].Copy()
                    };

                    foreach (var coord in web)
                    {
                        if (coord.ShareAUnit(here))
                        {
                            cellEmptiedByColor[(int)(coord.Coloring - 1)].Remove(coord.Number);

                            if (cellEmptiedByColor[0].Count == 0 || cellEmptiedByColor[1].Count == 0)
                            {
                                InvalidColoring(solverView, web, coord.Coloring, 6);
                                return;
                            }
                        }
                    }
                }
            }
        }
    }

    private void RemoveAllExcept(ISolverView solverView, int row, int col, int exceptOne, int exceptTwo)
    {
        for (int i = 1; i <= 9; i++)
        {
            if (i != exceptOne && i != exceptTwo)
            {
                solverView.RemovePossibility(i, row, col, this);
            }
        }
    }

    private void InvalidColoring(ISolverView solverView, ColorableWeb<MedusaCoordinate> web, Coloring invalid, int type)
    {
        foreach (var coord in web)
        {
            if (coord.Coloring == invalid)
                solverView.RemovePossibility(coord.Number, coord.Row, coord.Col, this);
            else solverView.AddDefinitiveNumber(coord.Number, coord.Row, coord.Col, this);
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
    
    private void InitChain(ISolverView solverView, ColorableWeb<MedusaCoordinate> web, MedusaCoordinate current)
    {
        var ppir = solverView.PossibilityPositionsInRow(current.Row, current.Number);
        if (ppir.Count == 2)
        {
            foreach (var col in ppir)
            {
                if (col != current.Col)
                {
                    MedusaCoordinate next = new MedusaCoordinate(current.Row, col, current.Number);
                    if(web.AddLink(current, next)) InitChain(solverView, web, next);
                    break;
                }
            }
        }
        
        var ppic = solverView.PossibilityPositionsInColumn(current.Col, current.Number);
        if (ppic.Count == 2)
        {
            foreach (var row in ppic)
            {
                if (row != current.Row)
                {
                    MedusaCoordinate next = new MedusaCoordinate(row, current.Col, current.Number);
                    if(web.AddLink(current, next)) InitChain(solverView, web, next);
                    break;
                }
            }
        }
        
        var ppimn = solverView.PossibilityPositionsInMiniGrid(current.Row / 3, current.Col / 3, current.Number);
        if (ppimn.Count == 2)
        {
            foreach (var pos in ppimn)
            {
                if (pos[0] != current.Row && pos[1] != current.Col)
                {
                    MedusaCoordinate next = new MedusaCoordinate(pos[0], pos[1], current.Number);
                    if(web.AddLink(current, next)) InitChain(solverView, web, next);
                    break;
                }
            }
        }

        if (solverView.Possibilities[current.Row, current.Col].Count == 2)
        {
            foreach (var possibility in solverView.Possibilities[current.Row, current.Col])
            {
                if (possibility != current.Number)
                {
                    MedusaCoordinate next = new MedusaCoordinate(current.Row, current.Col, possibility);
                    if(web.AddLink(current, next)) InitChain(solverView, web, next);
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