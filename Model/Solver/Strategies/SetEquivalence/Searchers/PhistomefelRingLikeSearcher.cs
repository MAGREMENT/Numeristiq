using System.Collections.Generic;
using Model.Solver.Positions;

namespace Model.Solver.Strategies.SetEquivalence.Searchers;

public class PhistomefelRingLikeSearcher : ISetEquivalenceSearcher
{
    public IEnumerable<SetEquivalence> Search(IStrategyManager strategyManager)
    {
        for (int mr1 = 0; mr1 < 2; mr1++)
        {
            for (int mc1 = 0; mc1 < 2; mc1++)
            {
                for (int mr2 = mr1 + 1; mr2 < 3; mr2++)
                {
                    for (int mc2 = mc1 + 1; mc2 < 3; mc2++)
                    {
                        var startRow1 = mr1 * 3;
                        var startRow2 = mr2 * 3;
                        var startCol1 = mc1 * 3;
                        var startCol2 = mc2 * 3;

                        for (int r1 = 0; r1 < 3; r1++)
                        {
                            for (int c1 = 0; c1 < 3; c1++)
                            {
                                for (int r2 = 0; r2 < 3; r2++)
                                {
                                    for (int c2 = 0; c2 < 3; c2++)
                                    {
                                        var row1 = startRow1 + r1;
                                        var row2 = startRow2 + r2;
                                        var col1 = startCol1 + c1;
                                        var col2 = startCol2 + c2;

                                        var gpMinis = new GridPositions();
                                        var gpLines = new GridPositions();

                                        gpMinis.FillMiniGrid(mr1, mc1);
                                        gpMinis.FillMiniGrid(mr1, mc2);
                                        gpMinis.FillMiniGrid(mr2, mc1);
                                        gpMinis.FillMiniGrid(mr2, mc2);

                                        gpLines.FillRow(row1);
                                        gpLines.FillColumn(col1);
                                        gpLines.FillRow(row2);
                                        gpLines.FillColumn(col2);

                                        var newGpMinis = gpMinis.Difference(gpLines);
                                        gpLines = gpLines.Difference(gpMinis);

                                        gpLines.Add(row1, col1);
                                        gpLines.Add(row2, col2);
                                        gpLines.Add(row1, col2);
                                        gpLines.Add(row2, col1);

                                        yield return new SetEquivalence(newGpMinis.ToArray(), 4,
                                            gpLines.ToArray(), 4);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}