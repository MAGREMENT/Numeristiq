using System;
using System.Collections.Generic;
using Global;
using Model.Solver.StrategiesUtility;

namespace Model.Solver.Strategies;

public class ComplexBUGLiteStrategy : AbstractStrategy
{
    public const string OfficialName = "BUG-Lite";
    private const OnCommitBehavior DefaultBehavior = OnCommitBehavior.Return;

    public override OnCommitBehavior DefaultOnCommitBehavior => DefaultBehavior;
    
    public ComplexBUGLiteStrategy() : base(OfficialName, StrategyDifficulty.Extreme, DefaultBehavior) { }
    
    public override void Apply(IStrategyManager strategyManager)
    {
        var biValueMap = GetBiValueMap(strategyManager);

        foreach (var entry in biValueMap)
        {
            var list = entry.Value;
            if (list.Count < 2) continue;

            for (int i = 0; i < list.Count - 1; i++)
            {
                for (int j = i + 1; j < list.Count; j++)
                {
                    var one = list[i];
                    var two = list[j];

                    var sharedUnits = new SharedUnits(one);
                    sharedUnits.Share(two);
                    if (sharedUnits.Count != 2) continue;

                    if (Search(strategyManager, new BiValueBlock(entry.Key, sharedUnits, one, two))) return;
                }
            }
        }
    }

    private bool Search(IStrategyManager strategyManager, BiValueBlock start)
    {
        return false;
    }

    private Dictionary<BiValue, List<Cell>> GetBiValueMap(IStrategyManager strategyManager)
    {
        Dictionary<BiValue, List<Cell>> biValueMap = new();
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
                    list = new List<Cell>();
                    biValueMap[bi] = list;
                }

                list.Add(new Cell(row, col));
            }
        }

        return biValueMap;
    }
}

public class BiValueBlock
{
    public BiValueBlock(BiValue biValue, SharedUnits sharedUnits, params Cell[] cells)
    {
        if (cells.Length != 2) throw new ArgumentException("Has to be 2 cells");
        
        BiValue = biValue;
        SharedUnits = sharedUnits;
        Cells = cells;
    }

    public BiValue BiValue { get; }
    public SharedUnits SharedUnits { get; }
    public Cell[] Cells { get; }
}