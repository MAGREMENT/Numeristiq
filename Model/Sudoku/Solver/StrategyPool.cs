using System.Collections.Generic;
using Model.Helpers.Settings;
using Model.Sudoku.Solver.Strategies;
using Model.Sudoku.Solver.Strategies.AlternatingInference;
using Model.Sudoku.Solver.Strategies.AlternatingInference.Algorithms;
using Model.Sudoku.Solver.Strategies.AlternatingInference.Types;
using Model.Sudoku.Solver.Strategies.BlossomLoops;
using Model.Sudoku.Solver.Strategies.BlossomLoops.BranchFinder;
using Model.Sudoku.Solver.Strategies.BlossomLoops.LoopFinders;
using Model.Sudoku.Solver.Strategies.BlossomLoops.Types;
using Model.Sudoku.Solver.Strategies.ForcingNets;
using Model.Sudoku.Solver.Strategies.MultiSector;
using Model.Sudoku.Solver.Strategies.MultiSector.Searchers;
using Model.Sudoku.Solver.Strategies.NRCZTChains;
using Model.Sudoku.Solver.Strategies.UniquenessClueCover;
using Model.Sudoku.Solver.Strategies.UniquenessClueCover.PatternCollections.Bands;
using Model.Sudoku.Solver.StrategiesUtility;
using Model.Sudoku.Solver.StrategiesUtility.Graphs;

namespace Model.Sudoku.Solver;

public static class StrategyPool
{
    private static readonly Dictionary<string, GiveStrategy> Pool = new()
    {
        {NakedSingleStrategy.OfficialName, () => new NakedSingleStrategy()},
        {HiddenSingleStrategy.OfficialName, () => new HiddenSingleStrategy()},
        {NakedDoublesStrategy.OfficialName, () => new NakedDoublesStrategy()},
        {HiddenDoublesStrategy.OfficialName, () => new HiddenDoublesStrategy()},
        {ClaimingSetStrategy.OfficialName, () => new ClaimingSetStrategy()},
        {PointingSetStrategy.OfficialName, () => new PointingSetStrategy()},
        {NakedSetStrategy.OfficialNameForType3, () => new NakedSetStrategy(3)},
        {HiddenSetStrategy.OfficialNameForType3, () => new HiddenSetStrategy(3)},
        {NakedSetStrategy.OfficialNameForType4, () => new NakedSetStrategy(4)},
        {HiddenSetStrategy.OfficialNameForType4, () => new HiddenSetStrategy(4)},
        {XWingStrategy.OfficialName, () => new XWingStrategy()},
        {XYWingStrategy.OfficialName, () => new XYWingStrategy()},
        {XYZWingStrategy.OfficialName, () => new XYZWingStrategy()},
        {GridFormationStrategy.OfficialNameForType3, () => new GridFormationStrategy(3)},
        {GridFormationStrategy.OfficialNameForType4, () => new GridFormationStrategy(4)},
        {SimpleColoringStrategy.OfficialName, () => new SimpleColoringStrategy()},
        {BUGStrategy.OfficialName, () => new BUGStrategy(5)},
        {ReverseBUGStrategy.OfficialName, () => new ReverseBUGStrategy()},
        {JuniorExocetStrategy.OfficialName, () => new JuniorExocetStrategy()},
        {FinnedXWingStrategy.OfficialName, () => new FinnedXWingStrategy()},
        {FinnedGridFormationStrategy.OfficialNameForType3, () => new FinnedGridFormationStrategy(3)},
        {FinnedGridFormationStrategy.OfficialNameForType4, () => new FinnedGridFormationStrategy(4)},
        {FireworksStrategy.OfficialName, () => new FireworksStrategy()},
        {UniqueRectanglesStrategy.OfficialName, () => new UniqueRectanglesStrategy(true)},
        {UnavoidableRectanglesStrategy.OfficialName, () => new UnavoidableRectanglesStrategy()},
        {XYChainsStrategy.OfficialName, () => new XYChainsStrategy()},
        {ThreeDimensionMedusaStrategy.OfficialName, () => new ThreeDimensionMedusaStrategy()},
        {WXYZWingStrategy.OfficialName, () => new WXYZWingStrategy()},
        {AlignedPairExclusionStrategy.OfficialName, () => new AlignedPairExclusionStrategy()},
        {SueDeCoqStrategy.OfficialName, () => new SueDeCoqStrategy(2)},
        {AlmostLockedSetsStrategy.OfficialName, () => new AlmostLockedSetsStrategy()},
        {DigitForcingNetStrategy.OfficialName, () => new DigitForcingNetStrategy()},
        {CellForcingNetStrategy.OfficialName, () => new CellForcingNetStrategy(4)},
        {UnitForcingNetStrategy.OfficialName, () => new UnitForcingNetStrategy(4)},
        {NishioForcingNetStrategy.OfficialName, () => new NishioForcingNetStrategy()},
        {PatternOverlayStrategy.OfficialName, () => new PatternOverlayStrategy(1, 1000)},
        {BruteForceStrategy.OfficialName, () => new BruteForceStrategy()},
        {SKLoopsStrategy.OfficialName, () => new SKLoopsStrategy()},
        {GurthTheorem.OfficialName, () => new GurthTheorem()},
        {SetEquivalenceStrategy.OfficialName, () => new SetEquivalenceStrategy(new RowsAndColumnsSearcher(2,
            5, 2), new PhistomefelRingLikeSearcher())},
        {DeathBlossomStrategy.OfficialName, () => new DeathBlossomStrategy()},
        {AlmostHiddenSetsStrategy.OfficialName, () => new AlmostHiddenSetsStrategy()},
        {AlignedTripleExclusionStrategy.OfficialName, () => new AlignedTripleExclusionStrategy(5)},
        {BUGLiteStrategy.OfficialName, () => new BUGLiteStrategy(16)},
        {EmptyRectangleStrategy.OfficialName, () => new EmptyRectangleStrategy()},
        {NRCZTChainStrategy.OfficialNameForDefault, () => new NRCZTChainStrategy()},
        {NRCZTChainStrategy.OfficialNameForTCondition, () => new NRCZTChainStrategy(new TCondition())},
        {NRCZTChainStrategy.OfficialNameForZCondition, () => new NRCZTChainStrategy(new ZCondition())},
        {NRCZTChainStrategy.OfficialNameForZAndTCondition, () => new NRCZTChainStrategy(new TCondition(), new ZCondition())},
        {SkyscraperStrategy.OfficialName, () => new SkyscraperStrategy()},
        {FishStrategy.OfficialName, () => new FishStrategy(3, 4, 3, 2, true)},
        {TwoStringKiteStrategy.OfficialName, () => new TwoStringKiteStrategy()},
        {AlmostLockedSetsChainStrategy.OfficialName, () => new AlmostLockedSetsChainStrategy(false)},
        {AlmostHiddenSetsChainStrategy.OfficialName, () => new AlmostHiddenSetsChainStrategy(false)},
        {XType.OfficialLoopName, () => new AlternatingInferenceGeneralization<CellPossibility>(new XType(),
            new AILoopAlgorithmV3<CellPossibility>())},
        {AIType.OfficialLoopName, () => new AlternatingInferenceGeneralization<CellPossibility>(new AIType(),
            new AILoopAlgorithmV3<CellPossibility>())},
        {SubsetsXType.OfficialLoopName, () => new AlternatingInferenceGeneralization<ISudokuElement>(new SubsetsXType(),
            new AILoopAlgorithmV3<ISudokuElement>())},
        {SubsetsAIType.OfficialLoopName, () => new AlternatingInferenceGeneralization<ISudokuElement>(new SubsetsAIType(),
            new AILoopAlgorithmV3<ISudokuElement>())},
        {XType.OfficialChainName, () => new AlternatingInferenceGeneralization<CellPossibility>(new XType(),
            new AIChainAlgorithmV2<CellPossibility>())},
        {AIType.OfficialChainName, () => new AlternatingInferenceGeneralization<CellPossibility>(new AIType(),
            new AIChainAlgorithmV2<CellPossibility>())},
        {SubsetsXType.OfficialChainName, () => new AlternatingInferenceGeneralization<ISudokuElement>(new SubsetsXType(),
            new AIChainAlgorithmV2<ISudokuElement>())},
        {SubsetsAIType.OfficialChainName, () => new AlternatingInferenceGeneralization<ISudokuElement>(new SubsetsAIType(),
            new AIChainAlgorithmV2<ISudokuElement>())},
        {MultiSectorLockedSetsStrategy.OfficialName, () => new MultiSectorLockedSetsStrategy(new RowsAndColumnsSearcher(
            3, 5, 1))},
        {DistributedDisjointSubsetStrategy.OfficialName, () => new DistributedDisjointSubsetStrategy()},
        {AlmostLockedCandidatesStrategy.OfficialNameForType2, () => new AlmostLockedCandidatesStrategy(2)},
        {AlmostLockedCandidatesStrategy.OfficialNameForType3, () => new AlmostLockedCandidatesStrategy(3)},
        {OddagonStrategy.OfficialName, () => new OddagonStrategy()},
        {OddagonForcingNetStrategy.OfficialName, () => new OddagonForcingNetStrategy(3)},
        {BlossomLoopStrategy.OfficialNameForCell, () => new BlossomLoopStrategy(new BLLoopFinderV2(12), new BLBranchFinderV1(), new CellType())},
        {BlossomLoopStrategy.OfficialNameForUnit, () => new BlossomLoopStrategy(new BLLoopFinderV2(12), new BLBranchFinderV1(), new UnitType())},
        {ThorsHammerStrategy.OfficialName, () => new ThorsHammerStrategy(new TwoByTwoLoopFinder())},
        {ReverseBUGLiteStrategy.OfficialName, () => new ReverseBUGLiteStrategy()},
        {MiniUniquenessStrategy.OfficialName, () => new MiniUniquenessStrategy()},
        {UniquenessClueCoverStrategy.OfficialName, () => new UniquenessClueCoverStrategy(BandCollection.FullCollection())},
        {ExtendedUniqueRectanglesStrategy.OfficialName, () => new ExtendedUniqueRectanglesStrategy()},
        {NonColorablePatternStrategy.OfficialName, () => new NonColorablePatternStrategy(3, 3, 3)}
    };
    
    public static List<string> FindStrategies(string filter)
    {
        List<string> result = new();
        var lFilter = filter.ToLower();

        foreach (var name in Pool.Keys)
        {
            if (name.ToLower().Contains(lFilter)) result.Add(name);
        }

        return result;
    }

    public static SudokuStrategy? CreateFrom(string name, bool enabled, bool locked, OnCommitBehavior behavior,
        Dictionary<string, SettingValue> settings)
    {
        if (!Pool.TryGetValue(name, out var giver)) return null;

        var result = giver();
        result.Enabled = enabled;
        result.Locked = locked;
        result.OnCommitBehavior = behavior;

        foreach (var entry in settings)
        {
            result.TrySetSetting(entry.Key, entry.Value);
        }

        return result;
    }
    
    public static SudokuStrategy? CreateFrom(string name, bool enabled, bool locked, OnCommitBehavior behavior)
    {
        if (!Pool.TryGetValue(name, out var giver)) return null;

        var result = giver();
        result.Enabled = enabled;
        result.Locked = locked;
        result.OnCommitBehavior = behavior;

        return result;
    }

    
    public static SudokuStrategy? CreateFrom(string name)
    {
        if (!Pool.TryGetValue(name, out var giver)) return null;

        return giver();
    }
}

public delegate SudokuStrategy GiveStrategy();