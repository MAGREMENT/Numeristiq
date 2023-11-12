using System;
using System.Collections.Generic;
using System.IO;
using Model.Solver.Strategies;
using Model.Solver.Strategies.AlternatingChains;
using Model.Solver.Strategies.AlternatingChains.ChainAlgorithms;
using Model.Solver.Strategies.AlternatingChains.ChainTypes;
using Model.Solver.Strategies.ForcingNets;
using Model.Solver.Strategies.SetEquivalence;
using Model.Solver.Strategies.SetEquivalence.Searchers;
using Model.Solver.StrategiesUtil.LinkGraph;
using Model.Util;

namespace Model.Solver.Helpers;

public class StrategyLoader
{
    private static readonly string Path = PathsInfo.PathToData() + @"\strategies.ini";

    private static readonly Dictionary<string, IStrategy> StrategyPool = new()
    {
        {NakedSingleStrategy.OfficialName, new NakedSingleStrategy()},
        {HiddenSingleStrategy.OfficialName, new HiddenSingleStrategy()},
        {NakedDoublesStrategy.OfficialName, new NakedDoublesStrategy()},
        {HiddenDoublesStrategy.OfficialName, new HiddenDoublesStrategy()},
        {BoxLineReductionStrategy.OfficialName, new BoxLineReductionStrategy()},
        {PointingSetStrategy.OfficialName, new PointingSetStrategy()},
        {NakedSetStrategy.OfficialNameForType3, new NakedSetStrategy(3)},
        {HiddenSetStrategy.OfficialNameForType3, new HiddenSetStrategy(3)},
        {NakedSetStrategy.OfficialNameForType4, new NakedSetStrategy(4)},
        {HiddenSetStrategy.OfficialNameForType4, new HiddenSetStrategy(4)},
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
        {AlignedPairExclusionStrategy.OfficialName, new AlignedPairExclusionStrategy()},
        {ComplexXCycles.OfficialName, new AlternatingChainGeneralization<ILinkGraphElement>(new ComplexXCycles(),
            new AlternatingChainAlgorithmV2<ILinkGraphElement>(20))},
        {SueDeCoqStrategy.OfficialName, new SueDeCoqStrategy()},
        {AlmostLockedSetsStrategy.OfficialName, new AlmostLockedSetsStrategy()},
        {ComplexAlternatingInferenceChains.OfficialName, new AlternatingChainGeneralization<ILinkGraphElement>(new ComplexAlternatingInferenceChains(),
            new AlternatingChainAlgorithmV2<ILinkGraphElement>(15))},
        {DigitForcingNetStrategy.OfficialName, new DigitForcingNetStrategy()},
        {CellForcingNetStrategy.OfficialName, new CellForcingNetStrategy(4)},
        {UnitForcingNetStrategy.OfficialName, new UnitForcingNetStrategy(4)},
        {NishioForcingNetStrategy.OfficialName, new NishioForcingNetStrategy()},
        {PatternOverlayStrategy.OfficialName, new PatternOverlayStrategy(1, 1000)},
        {BruteForceStrategy.OfficialName, new BruteForceStrategy()},
        {SKLoopsStrategy.OfficialName, new SKLoopsStrategy()},
        {GurthTheorem.OfficialName, new GurthTheorem()},
        {SetEquivalenceStrategy.OfficialName, new SetEquivalenceStrategy(new RowsAndColumnsSearcher(2,
            5, 5))},
        {DeathBlossomStrategy.OfficialName, new DeathBlossomStrategy()},
        {AlmostHiddenSetsStrategy.OfficialName, new AlmostHiddenSetsStrategy()},
        {AlignedTripleExclusionStrategy.OfficialName, new AlignedTripleExclusionStrategy(5)}
    };

    public IStrategy[] Strategies { get; private set; } = Array.Empty<IStrategy>();
    public ulong ExcludedStrategies { get; private set; }
    public Dictionary<string, OnCommitBehavior> CustomizedOnInstanceFound { get; } = new();

    public void Load()
    {
        if (!File.Exists(Path)) return;

        var result = IniFileReader.Read(Path);
        if (result.TryGetValue("Strategy Order", out var orderSection))
        {
            List<IStrategy> strategies = new();
            ulong excluded = 0;
            int count = 0;

            foreach (var entry in orderSection)
            {
                if (!StrategyPool.TryGetValue(entry.Key, out var strategy)) continue;
            
                strategies.Add(strategy);
                if (entry.Value.Equals("false")) excluded |= 1ul << count;

                count++;
            }
        
            Strategies = strategies.ToArray();
            ExcludedStrategies = excluded;
        }

        if (result.TryGetValue("Customized On Instance Found", out var section))
        {
            foreach (var entry in section)
            {
                if (!StrategyPool.ContainsKey(entry.Key)) continue;
                OnCommitBehavior behavior;

                switch (entry.Value)
                {
                    case "Return" : behavior = OnCommitBehavior.Return;
                        break;
                    case "WaitForAll" : behavior = OnCommitBehavior.WaitForAll;
                        break;
                    case "ChooseBest" : behavior = OnCommitBehavior.ChooseBest;
                        break;
                    default : continue;
                }

                CustomizedOnInstanceFound.Add(entry.Key, behavior);
            }
        }
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