using System.Collections.Generic;
using Model.Core;
using Model.Core.Descriptions;
using Model.Sudokus.Solver.Descriptions;
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
using Model.Sudokus.Solver.Utility.Graphs;
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

    #region Descriptions

    private static readonly IDescription<SudokuDescriptionDisplayer> NoDescription = 
        new TextDescription<SudokuDescriptionDisplayer>("No Description Found");

    private static readonly IDescription<SudokuDescriptionDisplayer> NakedSingleDescription = new TextDescription<SudokuDescriptionDisplayer>(
        "One of the basic strategies for solving Sudoku's. When a cell has only one possibility, then it must be the solution for that cell");

    private static readonly IDescription<SudokuDescriptionDisplayer> HiddenSingleDescription = new TextDescription<SudokuDescriptionDisplayer>(
        "One the basic strategies for solving Sudoku's. When a unit (row, column or box) has only one cell holding a possibility, " +
        "then that possibility must be the solution for that cell");

    private static readonly IDescription<SudokuDescriptionDisplayer> JuniorExocetDescription = new TextDescription<SudokuDescriptionDisplayer>("This is a very complex strategy. For a Junior Exocet to exist, there are multiple requirements :\n" +
        "1) There exist 2 base cells and 2 target cells in a band (e.g. 3 lines in 3 boxes). The base cells" +
        "need to be in the same mini-line, meaning a line restricted to a box, and have a total of" +
        "candidates between 2 and 4, called base candidates. Each target cell is in a different line and in a different box" +
        "than the base cells and the other target. These targets must each hold at least one base candidate" +
        "2) TODO");

    private static readonly IDescription<SudokuDescriptionDisplayer> UniquenessClueCoverDescription = new TextDescription<SudokuDescriptionDisplayer>("This is a strategy relying on uniqueness." +
        " It consists of a catalogue of patterns that examine every clue and any solution in a designated area of the Sudoku. These patterns" +
        " are generated beforehand by computers, making this strategy a bit controversial.");

    private static readonly IDescription<SudokuDescriptionDisplayer> BUGDescription = new TextDescription<SudokuDescriptionDisplayer>("A Bi-value Universal Grave (BUG) is a pattern in" +
        "which every candidates is found either 0 or 2 times in each unit (row, column and box). A Sudoku presenting this pattern" +
        "has more than one solution. In the case of Sudoku with a unique solution, at least one possibility preventing the" +
        "BUG must be true");

    private static readonly IDescription<SudokuDescriptionDisplayer> APEDescription = new DescriptionCollection<SudokuDescriptionDisplayer>().Add(
        "An aligned pair exclusion is the removal of candidate(s) using the following logic : " +
                                "Any two cells cannot have solutions that are candidates contained in an almost locked set they both see.")
        .Add(new SudokuStepDescription("Let's take these two boxes as an example where r45c1 is the target. There are 3 ways to make 5 fit into r4c1 :\n\n" +
                                       "    5 4 -> Impossible because of r56c3\n" +
                                       "    5 8 -> Impossible because of r12c1\n" +
                                       "    5 9 -> Impossible because of r12c1\n\n" +
                                       "We can therefore safely remove 5 from r4c1",
            "t0o803p00h60p02805tgoot0p00560p0280321o00503p009410hp0sgogs098p8032105l00511s0og21gg0903k00321090541h00hp0p0h8052141h88103h00hp0030h21h005p04109p841p01o03hg05p021",
            0, 5, 3, 5, 
            "bbaddaaabbaedaaaebaefaaaebaffaaafbaadaaafbabdaaadafddaaa", TextDisposition.Left));

    private static readonly IDescription<SudokuDescriptionDisplayer> BandUniquenessDescription = new TextDescription<SudokuDescriptionDisplayer>(
        "If a group of all instances of N different digits in a band (aka 3 rows or 3 columns in the same 3 boxes) is spread" +
        " over N+1 or less mini-rows/-columns, then the group will contain at least one unavoidable set.");

    private static readonly IDescription<SudokuDescriptionDisplayer> ClaimingSetDescription = new TextDescription<SudokuDescriptionDisplayer>("When all possibilities in a" +
        " row/column are restrained to a single box, the solution, wherever it is is, will always remove the possibilities" +
        " from the remaining cells of the box.");

    private static readonly Dictionary<string, IDescription<SudokuDescriptionDisplayer>> Descriptions = new()
    {
        {NakedSingleStrategy.OfficialName, NakedSingleDescription},
        {HiddenSingleStrategy.OfficialName, HiddenSingleDescription},
        {JuniorExocetStrategy.OfficialName, JuniorExocetDescription},
        {UniquenessClueCoverStrategy.OfficialName, UniquenessClueCoverDescription},
        {BUGStrategy.OfficialName, BUGDescription},
        {AlignedPairExclusionStrategy.OfficialName, APEDescription},
        {BandUniquenessStrategy.OfficialName, BandUniquenessDescription},
        {ClaimingSetStrategy.OfficialName, ClaimingSetDescription}
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

    public static IDescription<SudokuDescriptionDisplayer> GetDescription(string name) 
        => Descriptions.TryGetValue(name, out var d) ? d : NoDescription;
}

public delegate SudokuStrategy GiveStrategy();