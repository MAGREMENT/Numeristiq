using System.Collections.Generic;
using Model.Solver.Helpers.Changes;
using Model.Solver.Position;
using Model.Solver.StrategiesUtil;

namespace Model.Solver.Strategies;

public class BUGLiteStrategy : AbstractStrategy
{
    public const string OfficialName = "BUG-Lite";
    private const OnCommitBehavior DefaultBehavior = OnCommitBehavior.Return;

    public override OnCommitBehavior DefaultOnCommitBehavior => DefaultBehavior;
    
    public BUGLiteStrategy() : base(OfficialName, StrategyDifficulty.Hard, DefaultBehavior)
    {
        UniquenessDependency = UniquenessDependency.FullyDependent;
    }
    
    public override void Apply(IStrategyManager strategyManager)
    {
        Dictionary<BiValue, GridPositions> biValueMap = new();
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                var poss = strategyManager.PossibilitiesAt(row, col);
                if (poss.Count != 2) continue;

                var i = 0;
                poss.Next(ref i);
                var first = i;
                poss.Next(ref i);
                var second = i;
                var bi = new BiValue(first, second);

                if (!biValueMap.TryGetValue(bi, out var list))
                {
                    list = new GridPositions();
                    biValueMap[bi] = list;
                }

                list.Add(row, col);
            }
        }

        foreach (var entry in biValueMap)
        {
            var soloRow = UniquenessHelper.CheckForSoloRow(entry.Value);
            if (soloRow == -1) continue;

            var soloCol = UniquenessHelper.CheckForSoloColumn(entry.Value);
            if (soloCol == -1) continue;

            var soloMini = UniquenessHelper.CheckForSoloMini(entry.Value);
            if (soloMini == -1) continue;

            var miniRow = soloMini / 3;
            var miniCol = soloCol / 3;

            if (soloRow / 3 == miniRow && soloCol / 3 == miniCol)
            {
                strategyManager.ChangeBuffer.ProposePossibilityRemoval(entry.Key.One, soloRow, soloCol);
                strategyManager.ChangeBuffer.ProposePossibilityRemoval(entry.Key.Two, soloRow, soloCol);
            }
        }
    }
}

public class BUGLiteReportBuilder : IChangeReportBuilder
{
    private readonly GridPositions _gp;
    private readonly int _row;
    private readonly int _col;
    
    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), "", lighter =>
        {
            foreach (var cell in _gp)
            {
                
            }
        });
    }
}