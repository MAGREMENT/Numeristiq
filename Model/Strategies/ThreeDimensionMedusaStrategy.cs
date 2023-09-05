using System.Collections.Generic;
using Model.Changes;
using Model.Possibilities;
using Model.Solver;
using Model.StrategiesUtil;
using Model.StrategiesUtil.LinkGraph;

namespace Model.Strategies;

public class ThreeDimensionMedusaStrategy : IStrategy {
    public string Name => "3D Medusa";
    
    public StrategyLevel Difficulty => StrategyLevel.Hard;
    public int Score { get; set; }

    public void ApplyOnce(IStrategyManager strategyManager)
    {
        List<ColorableWeb<PossibilityCoordinateColoring>> chains = new();
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                foreach (var possibility in strategyManager.Possibilities[row, col])
                {
                    PossibilityCoordinateColoring start = new PossibilityCoordinateColoring(row, col, possibility);
                    if (DoesAnyChainContains(chains, start)) continue;
                    
                    ColorableWeb<PossibilityCoordinateColoring> web = new();
                    InitChain(strategyManager, web, start);
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
            SearchByCombination(strategyManager, chain);
            SearchOffChain(strategyManager, chain);

            strategyManager.ChangeBuffer.Push(this, new ThreeDimensionMedusaReportBuilder(chain));
        }
        
    }

    private void SearchByCombination(IStrategyManager strategyManager, ColorableWeb<PossibilityCoordinateColoring> web)
    {
        web.ForEachCombinationOfTwo((one, two) =>
        {
            if (one.PossibilityCoordinate.Row == two.PossibilityCoordinate.Row && one.PossibilityCoordinate.Col == two.PossibilityCoordinate.Col)
            {
                //Twice in a cell
                if (one.Coloring == two.Coloring)
                {
                    InvalidColoring(strategyManager.ChangeBuffer, web, one.Coloring);
                    return true;
                }
                //Two colours in a cell
                RemoveAllExcept(strategyManager.ChangeBuffer, one.PossibilityCoordinate.Row, one.PossibilityCoordinate.Col,
                    one.PossibilityCoordinate.Possibility, two.PossibilityCoordinate.Possibility);
            }
            
            //Twice in a unit
            if (one.PossibilityCoordinate.Possibility == two.PossibilityCoordinate.Possibility &&
                one.PossibilityCoordinate.ShareAUnit(two.PossibilityCoordinate) && one.Coloring == two.Coloring)
            {
                InvalidColoring(strategyManager.ChangeBuffer, web, one.Coloring);
                return true;
            }

            return false;
        });
    }

    private void SearchOffChain(IStrategyManager strategyManager, ColorableWeb<PossibilityCoordinateColoring> web)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (strategyManager.Sudoku[row, col] != 0) continue;
                bool cellTotallyOfChain = true;
                foreach (var possibility in strategyManager.Possibilities[row, col])
                {
                    PossibilityCoordinateColoring current = new PossibilityCoordinateColoring(row, col, possibility);
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
                        if (coord.PossibilityCoordinate.Possibility == possibility && coord.PossibilityCoordinate.ShareAUnit(current.PossibilityCoordinate)) 
                            twoElsewhere[(int)(coord.Coloring - 1)] = true;

                        //Colour in cell
                        if (coord.PossibilityCoordinate.Row == current.PossibilityCoordinate.Row && coord.PossibilityCoordinate.Col == current.PossibilityCoordinate.Col)
                            inCell[(int)(coord.Coloring - 1)] = true;
                            
                        
                        //Note : the inCell[0] && inCell[1] should be taken care of by the SearchByCombination function
                        if (twoElsewhere[0] && twoElsewhere[1])
                        {
                            strategyManager.ChangeBuffer.AddPossibilityToRemove(possibility, row, col);
                            break;
                        }
                        if ((twoElsewhere[0] && inCell[1]) ||
                                  (twoElsewhere[1] && inCell[0]))
                        {
                            strategyManager.ChangeBuffer.AddPossibilityToRemove(possibility, row, col);
                            break;
                        }
                    }
                }

                if (cellTotallyOfChain)
                {
                    Coordinate here = new Coordinate(row, col);
                    IPossibilities[] cellEmptiedByColor =
                    {
                        strategyManager.Possibilities[row, col].Copy(),
                        strategyManager.Possibilities[row, col].Copy()
                    };

                    foreach (var coord in web)
                    {
                        if (coord.PossibilityCoordinate.ShareAUnit(here))
                        {
                            cellEmptiedByColor[(int)(coord.Coloring - 1)].Remove(coord.PossibilityCoordinate.Possibility);

                            if (cellEmptiedByColor[0].Count == 0 || cellEmptiedByColor[1].Count == 0)
                            {
                                InvalidColoring(strategyManager.ChangeBuffer, web, coord.Coloring);
                                return;
                            }
                        }
                    }
                }
            }
        }
    }

    private void RemoveAllExcept(ChangeBuffer changeBuffer, int row, int col, int exceptOne, int exceptTwo)
    {
        for (int i = 1; i <= 9; i++)
        {
            if (i != exceptOne && i != exceptTwo)
            {
                changeBuffer.AddPossibilityToRemove(i, row, col);
            }
        }
    }

    private void InvalidColoring(ChangeBuffer changeBuffer, ColorableWeb<PossibilityCoordinateColoring> web, Coloring invalid)
    {
        foreach (var coord in web)
        {
            if (coord.Coloring == invalid)
                changeBuffer.AddPossibilityToRemove(coord.PossibilityCoordinate.Possibility, coord.PossibilityCoordinate.Row, coord.PossibilityCoordinate.Col);
            else changeBuffer.AddDefinitiveToAdd(coord.PossibilityCoordinate.Possibility, coord.PossibilityCoordinate.Row, coord.PossibilityCoordinate.Col);
        }
    }

    private bool DoesAnyChainContains(List<ColorableWeb<PossibilityCoordinateColoring>> chains, PossibilityCoordinateColoring coord)
    {
        foreach (var chain in chains)
        {
            if (chain.Contains(coord)) return true;
        }

        return false;
    }
    
    private void InitChain(IStrategyManager strategyManager, ColorableWeb<PossibilityCoordinateColoring> web, PossibilityCoordinateColoring current)
    {
        var ppir = strategyManager.RowPositions(current.PossibilityCoordinate.Row, current.PossibilityCoordinate.Possibility);
        if (ppir.Count == 2)
        {
            foreach (var col in ppir)
            {
                if (col != current.PossibilityCoordinate.Col)
                {
                    PossibilityCoordinateColoring next = new PossibilityCoordinateColoring(current.PossibilityCoordinate.Row, col, current.PossibilityCoordinate.Possibility);
                    if(web.AddLink(current, next)) InitChain(strategyManager, web, next);
                    break;
                }
            }
        }
        
        var ppic = strategyManager.ColumnPositions(current.PossibilityCoordinate.Col, current.PossibilityCoordinate.Possibility);
        if (ppic.Count == 2)
        {
            foreach (var row in ppic)
            {
                if (row != current.PossibilityCoordinate.Row)
                {
                    PossibilityCoordinateColoring next = new PossibilityCoordinateColoring(row, current.PossibilityCoordinate.Col, current.PossibilityCoordinate.Possibility);
                    if(web.AddLink(current, next)) InitChain(strategyManager, web, next);
                    break;
                }
            }
        }
        
        var ppimn = strategyManager.MiniGridPositions(current.PossibilityCoordinate.Row / 3, current.PossibilityCoordinate.Col / 3, current.PossibilityCoordinate.Possibility);
        if (ppimn.Count == 2)
        {
            foreach (var pos in ppimn)
            {
                if (pos[0] != current.PossibilityCoordinate.Row && pos[1] != current.PossibilityCoordinate.Col)
                {
                    PossibilityCoordinateColoring next = new PossibilityCoordinateColoring(pos[0], pos[1], current.PossibilityCoordinate.Possibility);
                    if(web.AddLink(current, next)) InitChain(strategyManager, web, next);
                    break;
                }
            }
        }

        if (strategyManager.Possibilities[current.PossibilityCoordinate.Row, current.PossibilityCoordinate.Col].Count == 2)
        {
            foreach (var possibility in strategyManager.Possibilities[current.PossibilityCoordinate.Row, current.PossibilityCoordinate.Col])
            {
                if (possibility != current.PossibilityCoordinate.Possibility)
                {
                    PossibilityCoordinateColoring next = new PossibilityCoordinateColoring(current.PossibilityCoordinate.Row, current.PossibilityCoordinate.Col, possibility);
                    if(web.AddLink(current, next)) InitChain(strategyManager, web, next);
                    break;
                }
            }
        }
    }
}

public class ThreeDimensionMedusaReportBuilder : IChangeReportBuilder
{
    private readonly ColorableWeb<PossibilityCoordinateColoring> _web;

    public ThreeDimensionMedusaReportBuilder(ColorableWeb<PossibilityCoordinateColoring> web)
    {
        _web = web;
    }

    public ChangeReport Build(List<SolverChange> changes, IChangeManager manager)
    {
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), "", lighter =>
        {
            IChangeReportBuilder.HighlightChanges(lighter, changes);
            
            foreach (var coord in _web)
            {
                lighter.HighlightPossibility(coord.PossibilityCoordinate.Possibility, coord.PossibilityCoordinate.Row,
                    coord.PossibilityCoordinate.Col, coord.Coloring == Coloring.On ?
                    ChangeColoration.CauseOnOne : ChangeColoration.CauseOffTwo);
                
                foreach (var friend in _web.GetLinkedVertices(coord))
                {
                    if (friend.Coloring == Coloring.Off) continue;
                    lighter.CreateLink(new PossibilityCoordinate(friend.PossibilityCoordinate.Row, friend.PossibilityCoordinate.Col, friend.PossibilityCoordinate.Possibility),
                        new PossibilityCoordinate(coord.PossibilityCoordinate.Row, coord.PossibilityCoordinate.Col, coord.PossibilityCoordinate.Possibility), LinkStrength.Strong);
                }
            }
        });
    }
}