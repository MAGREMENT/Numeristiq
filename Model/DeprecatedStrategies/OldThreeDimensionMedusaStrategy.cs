using System.Collections.Generic;
using Model.Solver;
using Model.Solver.Helpers;
using Model.Solver.Helpers.Changes;
using Model.Solver.Possibilities;
using Model.Solver.StrategiesUtil;
using Model.Solver.StrategiesUtil.LinkGraph;
using Model.StrategiesUtil;

namespace Model.DeprecatedStrategies;

public class OldThreeDimensionMedusaStrategy : IStrategy {
    public string Name => "3D Medusa";
    
    public StrategyDifficulty Difficulty => StrategyDifficulty.Hard;
    public StatisticsTracker Tracker { get; } = new();

    public void ApplyOnce(IStrategyManager strategyManager)
    {
        List<ColorableWeb<CellPossibilityColoring>> chains = new();
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                foreach (var possibility in strategyManager.PossibilitiesAt(row, col))
                {
                    CellPossibilityColoring start = new CellPossibilityColoring(row, col, possibility);
                    if (DoesAnyChainContains(chains, start)) continue;
                    
                    ColorableWeb<CellPossibilityColoring> web = new();
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

    private void SearchByCombination(IStrategyManager strategyManager, ColorableWeb<CellPossibilityColoring> web)
    {
        web.ForEachCombinationOfTwo((one, two) =>
        {
            if (one.CellPossibility.Row == two.CellPossibility.Row && one.CellPossibility.Col == two.CellPossibility.Col)
            {
                //Twice in a cell
                if (one.Coloring == two.Coloring)
                {
                    InvalidColoring(strategyManager.ChangeBuffer, web, one.Coloring);
                    return true;
                }
                //Two colours in a cell
                RemoveAllExcept(strategyManager.ChangeBuffer, one.CellPossibility.Row, one.CellPossibility.Col,
                    one.CellPossibility.Possibility, two.CellPossibility.Possibility);
            }
            
            //Twice in a unit
            if (one.CellPossibility.Possibility == two.CellPossibility.Possibility &&
                one.CellPossibility.ShareAUnit(two.CellPossibility) && one.Coloring == two.Coloring)
            {
                InvalidColoring(strategyManager.ChangeBuffer, web, one.Coloring);
                return true;
            }

            return false;
        });
    }

    private void SearchOffChain(IStrategyManager strategyManager, ColorableWeb<CellPossibilityColoring> web)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (strategyManager.Sudoku[row, col] != 0) continue;
                bool cellTotallyOfChain = true;
                foreach (var possibility in strategyManager.PossibilitiesAt(row, col))
                {
                    CellPossibilityColoring current = new CellPossibilityColoring(row, col, possibility);
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
                        if (coord.CellPossibility.Possibility == possibility && coord.CellPossibility.ShareAUnit(current.CellPossibility)) 
                            twoElsewhere[(int)(coord.Coloring - 1)] = true;

                        //Colour in cell
                        if (coord.CellPossibility.Row == current.CellPossibility.Row && coord.CellPossibility.Col == current.CellPossibility.Col)
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
                    Cell here = new Cell(row, col);
                    IPossibilities[] cellEmptiedByColor =
                    {
                        strategyManager.PossibilitiesAt(row, col).Copy(),
                        strategyManager.PossibilitiesAt(row, col).Copy()
                    };

                    foreach (var coord in web)
                    {
                        if (coord.CellPossibility.ShareAUnit(here))
                        {
                            cellEmptiedByColor[(int)(coord.Coloring - 1)].Remove(coord.CellPossibility.Possibility);

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

    private void InvalidColoring(ChangeBuffer changeBuffer, ColorableWeb<CellPossibilityColoring> web, Coloring invalid)
    {
        foreach (var coord in web)
        {
            if (coord.Coloring == invalid)
                changeBuffer.AddPossibilityToRemove(coord.CellPossibility.Possibility, coord.CellPossibility.Row, coord.CellPossibility.Col);
            else changeBuffer.AddSolutionToAdd(coord.CellPossibility.Possibility, coord.CellPossibility.Row, coord.CellPossibility.Col);
        }
    }

    private bool DoesAnyChainContains(List<ColorableWeb<CellPossibilityColoring>> chains, CellPossibilityColoring coord)
    {
        foreach (var chain in chains)
        {
            if (chain.Contains(coord)) return true;
        }

        return false;
    }
    
    private void InitChain(IStrategyManager strategyManager, ColorableWeb<CellPossibilityColoring> web, CellPossibilityColoring current)
    {
        var ppir = strategyManager.RowPositionsAt(current.CellPossibility.Row, current.CellPossibility.Possibility);
        if (ppir.Count == 2)
        {
            foreach (var col in ppir)
            {
                if (col != current.CellPossibility.Col)
                {
                    CellPossibilityColoring next = new CellPossibilityColoring(current.CellPossibility.Row, col, current.CellPossibility.Possibility);
                    if(web.AddLink(current, next)) InitChain(strategyManager, web, next);
                    break;
                }
            }
        }
        
        var ppic = strategyManager.ColumnPositionsAt(current.CellPossibility.Col, current.CellPossibility.Possibility);
        if (ppic.Count == 2)
        {
            foreach (var row in ppic)
            {
                if (row != current.CellPossibility.Row)
                {
                    CellPossibilityColoring next = new CellPossibilityColoring(row, current.CellPossibility.Col, current.CellPossibility.Possibility);
                    if(web.AddLink(current, next)) InitChain(strategyManager, web, next);
                    break;
                }
            }
        }
        
        var ppimn = strategyManager.MiniGridPositionsAt(current.CellPossibility.Row / 3, current.CellPossibility.Col / 3, current.CellPossibility.Possibility);
        if (ppimn.Count == 2)
        {
            foreach (var pos in ppimn)
            {
                if (pos.Row != current.CellPossibility.Row && pos.Col != current.CellPossibility.Col)
                {
                    CellPossibilityColoring next = new CellPossibilityColoring(pos.Row, pos.Col, current.CellPossibility.Possibility);
                    if(web.AddLink(current, next)) InitChain(strategyManager, web, next);
                    break;
                }
            }
        }

        if (strategyManager.PossibilitiesAt(current.CellPossibility.Row, current.CellPossibility.Col).Count == 2)
        {
            foreach (var possibility in strategyManager.PossibilitiesAt(current.CellPossibility.Row, current.CellPossibility.Col))
            {
                if (possibility != current.CellPossibility.Possibility)
                {
                    CellPossibilityColoring next = new CellPossibilityColoring(current.CellPossibility.Row, current.CellPossibility.Col, possibility);
                    if(web.AddLink(current, next)) InitChain(strategyManager, web, next);
                    break;
                }
            }
        }
    }
}

public class ThreeDimensionMedusaReportBuilder : IChangeReportBuilder
{
    private readonly ColorableWeb<CellPossibilityColoring> _web;

    public ThreeDimensionMedusaReportBuilder(ColorableWeb<CellPossibilityColoring> web)
    {
        _web = web;
    }

    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), "", lighter =>
        {
            IChangeReportBuilder.HighlightChanges(lighter, changes);
            
            foreach (var coord in _web)
            {
                lighter.HighlightPossibility(coord.CellPossibility.Possibility, coord.CellPossibility.Row,
                    coord.CellPossibility.Col, coord.Coloring == Coloring.On ?
                    ChangeColoration.CauseOnOne : ChangeColoration.CauseOffTwo);
                
                foreach (var friend in _web.GetLinkedVertices(coord))
                {
                    if (friend.Coloring == Coloring.Off) continue;
                    lighter.CreateLink(new CellPossibility(friend.CellPossibility.Row, friend.CellPossibility.Col, friend.CellPossibility.Possibility),
                        new CellPossibility(coord.CellPossibility.Row, coord.CellPossibility.Col, coord.CellPossibility.Possibility), LinkStrength.Strong);
                }
            }
        });
    }
}