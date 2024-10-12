using System.Collections.Generic;
using Model.Core;
using Model.Sudokus.Solver.Strategies;
using Model.Sudokus.Solver.Strategies.AlternatingInference;
using Model.Sudokus.Solver.Strategies.AlternatingInference.Algorithms;
using Model.Sudokus.Solver.Strategies.AlternatingInference.Types;
using Model.Sudokus.Solver.Strategies.BlossomLoops;
using Model.Sudokus.Solver.Strategies.BlossomLoops.BranchFinder;
using Model.Sudokus.Solver.Strategies.BlossomLoops.LoopFinders;
using Model.Sudokus.Solver.Strategies.BlossomLoops.Types;
using Model.Sudokus.Solver.Strategies.ForcingNets;
using Model.Sudokus.Solver.Strategies.MultiSector;
using Model.Sudokus.Solver.Strategies.MultiSector.Searchers;
using Model.Sudokus.Solver.Strategies.NRCZTChains;
using Model.Sudokus.Solver.Strategies.Symmetry;
using Model.Sudokus.Solver.Strategies.UniquenessClueCover;
using Model.Sudokus.Solver.Strategies.UniquenessClueCover.PatternCollections.Bands;
using Model.Utility;

namespace Model.Sudokus.Solver;

public static class SudokuStrategyPool
{
    #region Pool

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
        {FireworksStrategy.OfficialName, () => new FireworksStrategy(5)},
        {UniqueRectanglesStrategy.OfficialName, () => new UniqueRectanglesStrategy(true,5)},
        {UnavoidableRectanglesStrategy.OfficialName, () => new UnavoidableRectanglesStrategy(5)},
        {XYChainsStrategy.OfficialName, () => new XYChainsStrategy()},
        {ThreeDimensionMedusaStrategy.OfficialName, () => new ThreeDimensionMedusaStrategy()},
        {WXYZWingStrategy.OfficialName, () => new WXYZWingStrategy()},
        {AlignedPairExclusionStrategy.OfficialName, () => new AlignedPairExclusionStrategy(5)},
        {SueDeCoqStrategy.OfficialName, () => new SueDeCoqStrategy(2)},
        {AlmostLockedSetsStrategy.OfficialName, () => new AlmostLockedSetsStrategy(5)},
        {DigitForcingNetStrategy.OfficialName, () => new DigitForcingNetStrategy()},
        {CellForcingNetStrategy.OfficialName, () => new CellForcingNetStrategy(4)},
        {UnitForcingNetStrategy.OfficialName, () => new UnitForcingNetStrategy(4)},
        {NishioForcingNetStrategy.OfficialName, () => new NishioForcingNetStrategy()},
        {PatternOverlayStrategy.OfficialName, () => new PatternOverlayStrategy(1, 1000)},
        {BruteForceStrategy.OfficialName, () => new BruteForceStrategy()},
        {SKLoopsStrategy.OfficialName, () => new SKLoopsStrategy()},
        {GurthTheorem.OfficialName, () => new GurthTheorem(SudokuSymmetry.All())},
        {AntiGurthTheorem.OfficialName, () => new AntiGurthTheorem(SudokuSymmetry.All())},
        {SetEquivalenceStrategy.OfficialName, () => new SetEquivalenceStrategy(new RowsAndColumnsSearcher(2,
            5, 2), new PhistomefelRingLikeSearcher())},
        {DeathBlossomStrategy.OfficialName, () => new DeathBlossomStrategy(5)},
        {AlmostHiddenSetsStrategy.OfficialName, () => new AlmostHiddenSetsStrategy(5)},
        {AlignedTripleExclusionStrategy.OfficialName, () => new AlignedTripleExclusionStrategy(5, 5, 5)},
        {BUGLiteStrategy.OfficialName, () => new BUGLiteStrategy(16)},
        {EmptyRectangleStrategy.OfficialName, () => new EmptyRectangleStrategy()},
        {NRCZTChainStrategy.OfficialNameForDefault, () => new NRCZTChainStrategy()},
        {NRCZTChainStrategy.OfficialNameForTCondition, () => new NRCZTChainStrategy(new TCondition())},
        {NRCZTChainStrategy.OfficialNameForZCondition, () => new NRCZTChainStrategy(new ZCondition())},
        {NRCZTChainStrategy.OfficialNameForZAndTCondition, () => new NRCZTChainStrategy(new TCondition(), new ZCondition())},
        {SkyscraperStrategy.OfficialName, () => new SkyscraperStrategy()},
        {FishStrategy.OfficialName, () => new FishStrategy(3, 4, 3, 2, true)},
        {TwoStringKiteStrategy.OfficialName, () => new TwoStringKiteStrategy()},
        {AlmostLockedSetsChainStrategy.OfficialName, () => new AlmostLockedSetsChainStrategy(true, 5)},
        {AlmostHiddenSetsChainStrategy.OfficialName, () => new AlmostHiddenSetsChainStrategy(true, 5)},
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
        {DistributedDisjointSubsetStrategy.OfficialName, () => new DistributedDisjointSubsetStrategy(8)},
        {AlmostClaimingSetStrategy.OfficialNameForType2, () => new AlmostClaimingPairStrategy(() => new CandidateListMultiDictionary())},
        {AlmostClaimingSetStrategy.OfficialNameForType3, () => new AlmostClaimingSetStrategy(3)},
        {OddagonStrategy.OfficialName, () => new OddagonStrategy(7, 3)},
        {OddagonForcingNetStrategy.OfficialName, () => new OddagonForcingNetStrategy(7, 3)},
        {BlossomLoopStrategy.OfficialNameForCell, () => new BlossomLoopStrategy(new BLLoopFinderV1(12), new BLBranchFinderV1(), new CellType())},
        {BlossomLoopStrategy.OfficialNameForUnit, () => new BlossomLoopStrategy(new BLLoopFinderV1(12), new BLBranchFinderV1(), new UnitType())},
        {ThorsHammerStrategy.OfficialName, () => new ThorsHammerStrategy(new TwoByTwoLoopFinder())},
        {ReverseBUGLiteStrategy.OfficialName, () => new ReverseBUGLiteStrategy()},
        {BandUniquenessStrategy.OfficialName, () => new BandUniquenessStrategy()},
        {UniquenessClueCoverStrategy.OfficialName, () => new UniquenessClueCoverStrategy(BandCollection.FullCollection())},
        {ExtendedUniqueRectanglesStrategy.OfficialName, () => new ExtendedUniqueRectanglesStrategy()},
        {NonColorablePatternStrategy.OfficialName, () => new NonColorablePatternStrategy(3, 3, 3)},
        {XYZRingStrategy.OfficialName, () => new XYZRingStrategy()},
        {SeniorExocetStrategy.OfficialName, () => new SeniorExocetStrategy()},
        {SingleTargetExocetStrategy.OfficialName, () => new SingleTargetExocetStrategy()}
    };

    #endregion
    
    public static IEnumerable<string> EnumerateStrategies(string filter)
    {
        var lFilter = filter.ToLower();

        foreach (var name in Pool.Keys)
        {
            if (name.ToLower().Contains(lFilter)) yield return name;
        }
    }

    public static IEnumerable<string> EnumerateStrategies() => Pool.Keys;
    
    public static SudokuStrategy? CreateFrom(string name, bool enabled, bool locked, InstanceHandling handling)
    {
        if (!Pool.TryGetValue(name, out var giver)) return null;

        var result = giver();
        result.Enabled = enabled;
        result.Locked = locked;
        result.InstanceHandling = handling;

        return result;
    }

    public static SudokuStrategy? CreateFrom(string name)
    {
        return !Pool.TryGetValue(name, out var giver) ? null : giver();
    }
}

public delegate SudokuStrategy GiveStrategy();