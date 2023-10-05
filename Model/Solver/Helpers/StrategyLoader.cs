using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Model.Solver.Strategies;
using Model.Solver.Strategies.AlternatingChains;
using Model.Solver.Strategies.AlternatingChains.ChainAlgorithms;
using Model.Solver.Strategies.AlternatingChains.ChainTypes;
using Model.Solver.Strategies.ForcingNets;
using Model.Solver.Strategies.MultiSectorLockedSets;
using Model.Solver.Strategies.MultiSectorLockedSets.SearchAlgorithms;
using Model.Solver.Strategies.SetEquivalence;
using Model.Solver.Strategies.SetEquivalence.Searchers;
using Model.Solver.StrategiesUtil.LinkGraph;

namespace Model.Solver.Helpers;

public class StrategyLoader
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
        {AlignedPairExclusionStrategy.OfficialName, new AlignedPairExclusionStrategy(5)},
        {GroupedXCycles.OfficialName, new AlternatingChainGeneralization<ILinkGraphElement>(new GroupedXCycles(),
            new AlternatingChainAlgorithmV2<ILinkGraphElement>(20))},
        {SueDeCoqStrategy.OfficialName, new SueDeCoqStrategy()},
        {AlmostLockedSetsStrategy.OfficialName, new AlmostLockedSetsStrategy()},
        {CompleteAlternatingInferenceChains.OfficialName, new AlternatingChainGeneralization<ILinkGraphElement>(new CompleteAlternatingInferenceChains(),
            new AlternatingChainAlgorithmV2<ILinkGraphElement>(15))},
        {DigitForcingNetStrategy.OfficialName, new DigitForcingNetStrategy()},
        {CellForcingNetStrategy.OfficialName, new CellForcingNetStrategy(4)},
        {UnitForcingNetStrategy.OfficialName, new UnitForcingNetStrategy(4)},
        {NishioForcingNetStrategy.OfficialName, new NishioForcingNetStrategy()},
        {PatternOverlayStrategy.OfficialName, new PatternOverlayStrategy(1)},
        {BruteForceStrategy.OfficialName, new BruteForceStrategy()},
        {SKLoopsStrategy.OfficialName, new SKLoopsStrategy()},
        {MultiSectorLockedSetsStrategy.OfficialName, new MultiSectorLockedSetsStrategy(
            new MonoLineAlgorithm())},
        {GurthTheorem.OfficialName, new GurthTheorem()},
        {SetEquivalenceStrategy.OfficialName, new SetEquivalenceStrategy(new PossibilitiesGroupSearcher())}
    };

    private static readonly StrategyUsage[] DefaultUsage =
    {
        new(NakedSingleStrategy.OfficialName, true),
        new(HiddenSingleStrategy.OfficialName, true),
        new(NakedDoublesStrategy.OfficialName, true),
        new(HiddenDoublesStrategy.OfficialName, true),
        new(BoxLineReductionStrategy.OfficialName, true),
        new(PointingPossibilitiesStrategy.OfficialName, true),
        new(NakedPossibilitiesStrategy.OfficialNameForType3, true),
        new(HiddenPossibilitiesStrategy.OfficialNameForType3, true),
        new(NakedPossibilitiesStrategy.OfficialNameForType4, true),
        new(HiddenPossibilitiesStrategy.OfficialNameForType4, true),
        new(GurthTheorem.OfficialName, true),
        new(XWingStrategy.OfficialName, true),
        new(XYWingStrategy.OfficialName, true),
        new(XYZWingStrategy.OfficialName, true),
        new(GridFormationStrategy.OfficialNameForType3, true),
        new(GridFormationStrategy.OfficialNameForType4, true),
        new(SimpleColoringStrategy.OfficialName, true),
        new(BUGStrategy.OfficialName, true),
        new(ReverseBUGStrategy.OfficialName, true),
        new(JuniorExocetStrategy.OfficialName, true),
        new(FinnedXWingStrategy.OfficialName, true),
        new(FinnedGridFormationStrategy.OfficialNameForType3, true),
        new(FinnedGridFormationStrategy.OfficialNameForType4, true),
        new(FireworksStrategy.OfficialName, true),
        new(SKLoopsStrategy.OfficialName, true),
        new(UniqueRectanglesStrategy.OfficialName, true),
        new(AvoidableRectanglesStrategy.OfficialName, true),
        new(XYChainStrategy.OfficialName, true),
        new(ThreeDimensionMedusaStrategy.OfficialName, true),
        new(WXYZWingStrategy.OfficialName, true),
        new(AlignedPairExclusionStrategy.OfficialName, true),
        new(GroupedXCycles.OfficialName, true),
        new(SueDeCoqStrategy.OfficialName, true),
        new(MultiSectorLockedSetsStrategy.OfficialName, true),
        new(AlmostLockedSetsStrategy.OfficialName, true),
        new(CompleteAlternatingInferenceChains.OfficialName, false),
        new(DigitForcingNetStrategy.OfficialName, true),
        new(CellForcingNetStrategy.OfficialName, true),
        new(UnitForcingNetStrategy.OfficialName, true),
        new(NishioForcingNetStrategy.OfficialName, true),
        new(PatternOverlayStrategy.OfficialName, true),
        new(BruteForceStrategy.OfficialName, false)
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
        HandleSpecified(DefaultUsage);
        
        File.Delete(Path);
        File.WriteAllText(Path, JsonSerializer.Serialize(DefaultUsage,
            new JsonSerializerOptions {WriteIndented = true}));
    }
}

public class StrategyUsage
{
    public string StrategyName { get; init; } = "";
    public bool Used { get; init; }

    public StrategyUsage() { }
    
    public StrategyUsage(string strategyName, bool used)
    {
        StrategyName = strategyName;
        Used = used;
    }
}