using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Model.DeprecatedStrategies;
using Model.Solver.Strategies;
using Model.Solver.Strategies.AlternatingChains;
using Model.Solver.Strategies.AlternatingChains.ChainAlgorithms;
using Model.Solver.Strategies.AlternatingChains.ChainTypes;
using Model.Solver.Strategies.ForcingNets;
using Model.Solver.Strategies.MultiSectorLockedSets;
using Model.Solver.Strategies.MultiSectorLockedSets.SearchAlgorithms;
using Model.Solver.StrategiesUtil.LinkGraph;

namespace Model.Solver.Helpers;

public class StrategyLoader //TODO finish
{
    private static readonly string Path = PathsInfo.PathToData() + "/strategies.json";

    private static readonly Dictionary<string, IStrategy> StrategyPool = new()
    {
        {NakedSingleStrategy.OfficialName, new NakedSingleStrategy()},
        {HiddenSingleStrategy.OfficialName, new HiddenSingleStrategy()},
        {NakedDoublesStrategy.OfficialName, new NakedDoublesStrategy()},
        {HiddenDoublesStrategy.OfficialName, new HiddenDoublesStrategy()},
        {BoxLineReductionStrategy.OfficialName, new BoxLineReductionStrategy()},
        {PointingPossibilitiesStrategy.OfficialName, new PointingPossibilitiesStrategy()},
        {NakedPossibilitiesStrategy.OfficialNameForType3, new NakedPossibilitiesStrategy(3)},
        {HiddenPossibilitiesStrategy.OfficialNameForType3, new HiddenPossibilitiesStrategy(3)},
        {NakedPossibilitiesStrategy.OfficialNameForType4, new NakedPossibilitiesStrategy(4)},
        {HiddenPossibilitiesStrategy.OfficialNameForType4, new HiddenPossibilitiesStrategy(4)},
        {XWingStrategy.OfficialName, new XWingStrategy()},
        {XYWingStrategy.OfficialName, new XYWingStrategy()},
        {XYZWingStrategy.OfficialName, new XYZWingStrategy()},
        {GridFormationStrategy.OfficialNameForType3, new GridFormationStrategy(3)},
        {GridFormationStrategy.OfficialNameForType4, new GridFormationStrategy(4)},
        {SimpleColoringStrategy.OfficialName, new SimpleColoringStrategy()},
        {BUGStrategy.OfficialName, new BUGStrategy()},
        {ReverseBUGStrategy.OfficialName, new ReverseBUGStrategy()},
        {JuniorExocetStrategy.OfficialName, new JuniorExocetStrategy()},
        {FinnedXWingStrategy.OfficialName, new FinnedXWingStrategy()},
        {FinnedGridFormationStrategy.OfficialNameForType3, new FinnedGridFormationStrategy(3)},
        {FinnedGridFormationStrategy.OfficialNameForType4, new FinnedGridFormationStrategy(4)},
        {FireworksStrategy.OfficialName, new FireworksStrategy()},
        {UniqueRectanglesStrategy.OfficialName, new UniqueRectanglesStrategy()},
        {AvoidableRectanglesStrategy.OfficialName, new AvoidableRectanglesStrategy()},
        {XYChainStrategy.OfficialName, new XYChainStrategy()},
        {ThreeDimensionMedusaStrategy.OfficialName, new ThreeDimensionMedusaStrategy()},
        {WXYZWingStrategy.OfficialName, new WXYZWingStrategy()},
        {AlignedPairExclusionStrategy.OfficialName, new AlignedPairExclusionStrategy(4)},
        {GroupedXCycles.OfficialName, new AlternatingChainGeneralization<ILinkGraphElement>(new GroupedXCycles(),
            new AlternatingChainAlgorithmV2<ILinkGraphElement>(20))},
        {SueDeCoqStrategy.OfficialName, new SueDeCoqStrategy()},
        {AlmostLockedSetsStrategy.OfficialName, new AlmostLockedSetsStrategy()},
        {FullAIC.OfficialName, new AlternatingChainGeneralization<ILinkGraphElement>(new FullAIC(),
            new AlternatingChainAlgorithmV2<ILinkGraphElement>(15))},
        {DigitForcingNetStrategy.OfficialName, new DigitForcingNetStrategy()},
        {CellForcingNetStrategy.OfficialName, new CellForcingNetStrategy(4)},
        {UnitForcingNetStrategy.OfficialName, new UnitForcingNetStrategy(4)},
        {NishioForcingNetStrategy.OfficialName, new NishioForcingNetStrategy()},
        {PatternOverlayStrategy.OfficialName, new PatternOverlayStrategy(15)},
        {BruteForceStrategy.OfficialName, new BruteForceStrategy()},
        {SKLoopsStrategy.OfficialName, new SKLoopsStrategy()},
        {MultiSectorLockedSetsStrategy.OfficialName, new MultiSectorLockedSetsStrategy(
            new MonoLineAlgorithm())},
        {GurthTheorem.OfficialName, new GurthTheorem()}
    };


    public IStrategy[] Strategies { get; private set; } = Array.Empty<IStrategy>();
    public ulong ExcludedStrategies { get; private set; }

    public void Load()
    {
        if (!File.Exists(Path))
        {
            HandleDefault();
            return;
        }
        var buffer = JsonSerializer.Deserialize<StrategyUsage[]>(File.ReadAllText(Path));
        if (buffer is null)
        {
            HandleDefault();
            return;
        }
        
        HandleSpecified(buffer);
    }

    private void HandleSpecified(StrategyUsage[] usage)
    {
        List<IStrategy> strategies = new();
        ulong excluded = 0;

        for (int i = 0; i < usage.Length; i++)
        {
            var current = usage[i];
            if (!StrategyPool.TryGetValue(current.StrategyName, out var strategy)) continue;
            
            strategies.Add(strategy);
            if (!current.Used) excluded |= 1ul << i;
        }

        Strategies = strategies.ToArray();
        ExcludedStrategies = excluded;
    }

    private void HandleDefault()
    {
        
    }

    public static void WritePool()
    {
        List<StrategyUsage> usage = new();
        foreach (var strategy in StrategyPool)
        {
            usage.Add(new StrategyUsage()
            {
                StrategyName = strategy.Key,
                Used = true
            });
        }
        
        File.Delete(Path);
        File.WriteAllText(Path, JsonSerializer.Serialize(usage.ToArray(), new JsonSerializerOptions {WriteIndented = true}));
    }
}

public class StrategyUsage
{
    public string StrategyName { get; init; } = "";
    public bool Used { get; init; }
}