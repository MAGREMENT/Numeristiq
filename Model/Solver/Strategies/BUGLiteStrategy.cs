using System.Collections.Generic;
using System.Linq;
using Global.Enums;
using Model.Solver.Helpers.Changes;
using Model.Solver.Position;
using Model.Solver.Possibility;
using Model.Solver.StrategiesUtility;

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
            if (entry.Value.Count < 3) continue;
            
            var soloRow = UniquenessHelper.SearchExceptionInUnit(Unit.Row, 2, entry.Value);
            if (soloRow == -1) continue;

            var soloCol = UniquenessHelper.SearchExceptionInUnit(Unit.Column, 2, entry.Value);
            if (soloCol == -1) continue;

            var soloMini = UniquenessHelper.SearchExceptionInUnit(Unit.MiniGrid, 2, entry.Value);
            if (soloMini == -1) continue;

            var miniRow = soloMini / 3;
            var miniCol = soloCol / 3;

            if (soloRow / 3 == miniRow && soloCol / 3 == miniCol)
            {
                strategyManager.ChangeBuffer.ProposePossibilityRemoval(entry.Key.One, soloRow, soloCol);
                strategyManager.ChangeBuffer.ProposePossibilityRemoval(entry.Key.Two, soloRow, soloCol);
            }

            if (strategyManager.ChangeBuffer.NotEmpty() && strategyManager.ChangeBuffer.Commit(this,
                    new BUGLiteReportBuilder(entry.Value, soloRow, soloCol)))
            {
                if(OnCommitBehavior == OnCommitBehavior.Return) return;
            } 
        }

        var asArray = biValueMap.Keys.ToArray();

        for (int i = 0; i < asArray.Length - 2; i++)
        {
            for (int j = i + 1; j < asArray.Length - 1; j++)
            {
                for (int k = j + 1; j < asArray.Length; j++)
                {
                    var one = asArray[i];
                    var two = asArray[j];
                    var three = asArray[k];

                    var possibilities = Possibilities.NewEmpty();
                    possibilities.Add(one.One);
                    possibilities.Add(one.Two);
                    possibilities.Add(two.One);
                    possibilities.Add(two.Two);
                    possibilities.Add(three.One);
                    possibilities.Add(three.Two);
                    
                    if(possibilities.Count != 3) continue;

                    var or = biValueMap[one].Or(biValueMap[two].Or(biValueMap[three]));
                    if (or.Count < 5) continue;

                    var soloRow = UniquenessHelper.SearchExceptionInUnit(Unit.Row, 
                        UniquenessHelper.ComputeExpectedCount(Unit.Row, biValueMap[one], biValueMap[two], biValueMap[three]), or);
                    if (soloRow == -1) continue;

                    var soloCol = UniquenessHelper.SearchExceptionInUnit(Unit.Column, 
                        UniquenessHelper.ComputeExpectedCount(Unit.Column, biValueMap[one], biValueMap[two], biValueMap[three]), or);
                    if (soloCol == -1) continue;

                    var soloMini = UniquenessHelper.SearchExceptionInUnit(Unit.MiniGrid, 
                        UniquenessHelper.ComputeExpectedCount(Unit.MiniGrid, biValueMap[one], biValueMap[two], biValueMap[three]), or);
                    if (soloMini == -1) continue;

                    var miniRow = soloMini / 3;
                    var miniCol = soloCol / 3;

                    if (soloRow / 3 == miniRow && soloCol / 3 == miniCol)
                    {
                        foreach (var p in possibilities)
                        {
                            strategyManager.ChangeBuffer.ProposePossibilityRemoval(p, soloRow, soloCol);
                        }
                    }

                    if (strategyManager.ChangeBuffer.NotEmpty() && strategyManager.ChangeBuffer.Commit(this,
                            new BUGLiteReportBuilder(or, soloRow, soloCol)))
                    {
                        if(OnCommitBehavior == OnCommitBehavior.Return) return;
                    } 
                }
            }
        }
    }
}

public class BUGLiteReportBuilder : IChangeReportBuilder
{
    private readonly GridPositions _gp;
    private readonly int _row;
    private readonly int _col;

    public BUGLiteReportBuilder(GridPositions gp, int row, int col)
    {
        _gp = gp;
        _row = row;
        _col = col;
    }

    public ChangeReport Build(List<SolverChange> changes, IPossibilitiesHolder snapshot)
    {
        return new ChangeReport(IChangeReportBuilder.ChangesToString(changes), "", lighter =>
        {
            foreach (var cell in _gp)
            {
                lighter.HighlightCell(cell, ChangeColoration.CauseOffOne);
            }
            
            lighter.HighlightCell(_row, _col, ChangeColoration.CauseOffTwo);

            IChangeReportBuilder.HighlightChanges(lighter, changes);
        });
    }
}