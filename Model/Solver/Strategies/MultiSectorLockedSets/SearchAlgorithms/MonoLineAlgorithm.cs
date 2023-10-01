using System.Collections.Generic;
using Model.Solver.Positions;
using Model.Solver.Possibilities;

namespace Model.Solver.Strategies.MultiSectorLockedSets.SearchAlgorithms;

public class MonoLineAlgorithm : ICoverHouseSearchAlgorithm
{
    public IEnumerable<SearchResult> Search(IStrategyManager strategyManager)
    {
        for (int n = 1; n <= 9; n++)
        {
            for (int o = n + 1; o <= 9; o++)
            {
                for (int p = o + 1; p <= 9; p++)
                {
                    for (int q = p + 1; q <= 9; q++)
                    {
                        var home = IPossibilities.NewEmpty();
                        home.Add(n);
                        home.Add(o);
                        home.Add(p);
                        home.Add(q);
                        var away = home.Invert();

                        LinePositions homeRows = new LinePositions();
                        LinePositions awayCols = new LinePositions();

                        for (int row = 0; row < 9; row++)
                        {
                            for (int col = 0; col < 9; col++)
                            {
                                var solved = strategyManager.Sudoku[row, col];

                                if (home.Peek(solved)) homeRows.Add(row);
                                if (away.Peek(solved)) awayCols.Add(col);
                            }
                        }

                        List<ICoverHouse> homeList = new();
                        List<ICoverHouse> awayList = new();

                        foreach (var row in homeRows)
                        {
                            homeList.Add(MultiSectorLockedSetsStrategy.CoverHouses
                                [MultiSectorLockedSetsStrategy.CoverRowStart + row]);
                        }

                        foreach (var col in awayCols)
                        {
                            awayList.Add(MultiSectorLockedSetsStrategy.CoverHouses
                                [MultiSectorLockedSetsStrategy.CoverColumnStart + col]);
                        }

                        yield return new SearchResult(home, away, homeList, awayList);
                    }
                }
            }
        }
    }
}