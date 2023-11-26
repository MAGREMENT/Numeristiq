using System.Collections.Generic;
using Model.Solver.Strategies;
using Model.Solver.Strategies.AlternatingChains;
using Model.Solver.Strategies.AlternatingChains.ChainAlgorithms;
using Model.Solver.Strategies.AlternatingChains.ChainTypes;
using Model.Solver.Strategies.ForcingNets;
using Model.Solver.Strategies.NRCZTChains;
using Model.Solver.Strategies.SetEquivalence;
using Model.Solver.Strategies.SetEquivalence.Searchers;
using Model.Solver.StrategiesUtility;
using Model.Solver.StrategiesUtility.LinkGraph;

namespace Model.Solver.Helpers;

public class StrategyLoader : IStrategyLoader
{
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
        {UnavoidableRectanglesStrategy.OfficialName, new UnavoidableRectanglesStrategy()},
        {XYChainsStrategy.OfficialName, new XYChainsStrategy()},
        {ThreeDimensionMedusaStrategy.OfficialName, new ThreeDimensionMedusaStrategy()},
        {WXYZWingStrategy.OfficialName, new WXYZWingStrategy()},
        {AlignedPairExclusionStrategy.OfficialName, new AlignedPairExclusionStrategy()},
        {SubsetsXCycles.OfficialName, new AlternatingChainGeneralization<ILinkGraphElement>(new SubsetsXCycles(),
            new AlternatingChainAlgorithmV4<ILinkGraphElement>())},
        {SueDeCoqStrategy.OfficialName, new SueDeCoqStrategy()},
        {AlmostLockedSetsStrategy.OfficialName, new AlmostLockedSetsStrategy()},
        {SubsetsAlternatingInferenceChains.OfficialName, new AlternatingChainGeneralization<ILinkGraphElement>(new SubsetsAlternatingInferenceChains(),
            new AlternatingChainAlgorithmV4<ILinkGraphElement>())},
        {DigitForcingNetStrategy.OfficialName, new DigitForcingNetStrategy()},
        {CellForcingNetStrategy.OfficialName, new CellForcingNetStrategy(4)},
        {UnitForcingNetStrategy.OfficialName, new UnitForcingNetStrategy(4)},
        {NishioForcingNetStrategy.OfficialName, new NishioForcingNetStrategy()},
        {PatternOverlayStrategy.OfficialName, new PatternOverlayStrategy(1, 1000)},
        {BruteForceStrategy.OfficialName, new BruteForceStrategy()},
        {SKLoopsStrategy.OfficialName, new SKLoopsStrategy()},
        {GurthTheorem.OfficialName, new GurthTheorem()},
        {SetEquivalenceStrategy.OfficialName, new SetEquivalenceStrategy(new RowsAndColumnsSearcher(2,
            5, 5), new PhistomefelRingLikeSearcher())},
        {DeathBlossomStrategy.OfficialName, new DeathBlossomStrategy()},
        {AlmostHiddenSetsStrategy.OfficialName, new AlmostHiddenSetsStrategy()},
        {AlignedTripleExclusionStrategy.OfficialName, new AlignedTripleExclusionStrategy(5)},
        {BUGLiteStrategy.OfficialName, new BUGLiteStrategy()},
        {RectangleEliminationStrategy.OfficialName, new RectangleEliminationStrategy()},
        {NRCZTChainStrategy.OfficialNameForDefault, new NRCZTChainStrategy()},
        {NRCZTChainStrategy.OfficialNameForTCondition, new NRCZTChainStrategy(new TCondition())},
        {NRCZTChainStrategy.OfficialNameForZCondition, new NRCZTChainStrategy(new ZCondition())},
        {NRCZTChainStrategy.OfficialNameForZAndTCondition, new NRCZTChainStrategy(new TCondition(), new ZCondition())},
        {AlternatingInferenceChains.OfficialName, new AlternatingChainGeneralization<CellPossibility>(new AlternatingInferenceChains(),
            new AlternatingChainAlgorithmV4<CellPossibility>())},
        {XCycles.OfficialName, new AlternatingChainGeneralization<CellPossibility>(new XCycles(),
            new AlternatingChainAlgorithmV4<CellPossibility>())},
        {SkyscraperStrategy.OfficialName, new SkyscraperStrategy()},
        {FishStrategy.OfficialName, new FishStrategy(3, 4)},
        {TwoStringKiteStrategy.OfficialName, new TwoStringKiteStrategy()}
    };

    private readonly List<IStrategy> _strategies = new();
    private ulong _excludedStrategies;
    private ulong _lockedStrategies;
    
    public IReadOnlyList<IStrategy> Strategies => _strategies;
    private readonly IStrategyRepository _repository;

    public event OnListUpdate? ListUpdated;
    private bool _callEvent = true;

    public StrategyLoader(IStrategyRepository repository)
    {
        _repository = repository;
        ListUpdated += UpdateRepository;
    }

    public void Load()
    {
        _callEvent = false;
        
        _strategies.Clear();
        _excludedStrategies = 0;
        _lockedStrategies = 0;

        var download = _repository.DownloadStrategies();
        var count = 0;
        foreach (var dao in download)
        {
            var strategy = StrategyPool[dao.Name];
            strategy.OnCommitBehavior = dao.Behavior;
            _strategies.Add(StrategyPool[dao.Name]);
            if (!dao.Used) ExcludeStrategy(count);
            count++;
        }

        _callEvent = true;
    }
    
    public void ExcludeStrategy(int number)
    {
        if (IsStrategyLocked(number)) return;
        
        _excludedStrategies |= 1ul << number;
        TryCallEvent();
    }
    
    public void UseStrategy(int number)
    {
        if (IsStrategyLocked(number)) return;
        
        _excludedStrategies &= ~(1ul << number);
        TryCallEvent();
    }

    public bool IsStrategyUsed(int number)
    {
        return ((_excludedStrategies >> number) & 1) == 0;
    }

    public bool IsStrategyLocked(int number)
    {
        return ((_lockedStrategies >> number) & 1) > 0;
    }
    
    public void AllowUniqueness(bool yes)
    {
        for (int i = 0; i < Strategies.Count; i++)
        {
            if (Strategies[i].UniquenessDependency != UniquenessDependency.FullyDependent) continue;
            
            if (yes) UnLockStrategy(i);
            else
            {
                ExcludeStrategy(i);
                LockStrategy(i);
            }
        }
    }
    
    public StrategyInformation[] GetStrategyInfo()
    {
        StrategyInformation[] result = new StrategyInformation[Strategies.Count];

        for (int i = 0; i < Strategies.Count; i++)
        {
            result[i] = new StrategyInformation(Strategies[i], IsStrategyUsed(i), IsStrategyLocked(i));
        }

        return result;
    }

    public List<string> FindStrategies(string filter)
    {
        List<string> result = new();
        var lFilter = filter.ToLower();

        foreach (var name in StrategyPool.Keys)
        {
            if (name.ToLower().Contains(lFilter)) result.Add(name);
        }

        return result;
    }
    
    public void AddStrategy(string name)
    {
        _strategies.Add(StrategyPool[name]);
        TryCallEvent();
    }

    public void AddStrategy(string name, int position)
    {
        if (position == _strategies.Count)
        {
            AddStrategy(name);
            return;
        }
        
        _strategies.Insert(position, StrategyPool[name]);
        TryCallEvent();
    }
    
    public void RemoveStrategy(int position)
    {
        _strategies.RemoveAt(position);
        TryCallEvent();
    }
    
    public void InterchangeStrategies(int positionOne, int positionTwo)
    {
        var buffer = _strategies[positionOne].Name;
        _strategies.RemoveAt(positionOne);
        AddStrategy(buffer, positionTwo > positionOne ? positionTwo - 1 : positionTwo);
    }
    
    //Private-----------------------------------------------------------------------------------------------------------
    
    private void LockStrategy(int n)
    {
        _lockedStrategies |= 1ul << n;
    }

    private void UnLockStrategy(int n)
    {
        _lockedStrategies &= ~(1ul << n);
    }

    private void TryCallEvent()
    {
        if (!_callEvent) return;
        
        ListUpdated?.Invoke();
    }

    private void UpdateRepository()
    {
        var list = new List<StrategyDAO>(_strategies.Count);
        for (int i = 0; i < _strategies.Count; i++)
        {
            var s = _strategies[i];
            list.Add(new StrategyDAO(s.Name, IsStrategyUsed(i), s.OnCommitBehavior, new Dictionary<string, string>()));
        }

        _repository.UploadStrategies(list);
    }
}

public class StrategyInformation
{
    public string StrategyName { get; }
    public StrategyDifficulty Difficulty { get; }
    public bool Used { get; }
    public bool Locked { get; }
    
    public IReadOnlyTracker Tracker { get; }

    public StrategyInformation(IStrategy strategy, bool used, bool locked)
    {
        StrategyName = strategy.Name;
        Difficulty = strategy.Difficulty;
        Used = used;
        Locked = locked;
        Tracker = strategy.Tracker;
    }
}

public delegate void OnListUpdate();