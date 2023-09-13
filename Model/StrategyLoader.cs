using System.IO;
using System.Text.Json;
using Model.Solver;
using Model.Strategies;
using Model.Strategies.AlternatingChains;
using Model.Strategies.AlternatingChains.ChainAlgorithms;
using Model.Strategies.AlternatingChains.ChainTypes;
using Model.Strategies.ForcingNets;
using Model.StrategiesUtil.LinkGraph;

namespace Model;

public class StrategyLoader
{
    private readonly string _path = PathsInfo.PathToData() + @"/strategies.json";

    private readonly IStrategy[] _strategies =
    {
        new NakedSingleStrategy(),
        new HiddenSingleStrategy(),
        new NakedDoublesStrategy(),
        new HiddenDoublesStrategy(),
        new BoxLineReductionStrategy(),
        new PointingPossibilitiesStrategy(),
        new NakedPossibilitiesStrategy(3),
        new HiddenPossibilitiesStrategy(3),
        new NakedPossibilitiesStrategy(4),
        new HiddenPossibilitiesStrategy(4),
        new XWingStrategy(),
        new XYWingStrategy(),
        new XYZWingStrategy(),
        new GridFormationStrategy(3),
        new GridFormationStrategy(4),
        new SimpleColoringStrategy(),
        new BUGStrategy(),
        new FinnedXWingStrategy(),
        new FinnedGridFormationStrategy(3),
        new FinnedGridFormationStrategy(4),
        new FireworksStrategy(),
        new UniqueRectanglesStrategy(),
        new AvoidableRectanglesStrategy(),
        new XYChainStrategy(),
        new ThreeDimensionMedusaStrategy(),
        new WXYZWingStrategy(),
        new AlignedPairExclusionStrategy(4),
        new AlternatingChainGeneralization<ILinkGraphElement>(new GroupedXCycles(),
            new AlternatingChainAlgorithmV2<ILinkGraphElement>(20)),
        new SueDeCoqStrategy(),
        new AlmostLockedSetsStrategy(),
        new AlternatingChainGeneralization<ILinkGraphElement>(new FullAIC(),
            new AlternatingChainAlgorithmV2<ILinkGraphElement>(15)),
        new DigitForcingNetStrategy(),
        new CellForcingNetStrategy(4),
        new UnitForcingNetStrategy(4),
        new NishioForcingNetStrategy(),
        new PatternOverlayStrategy(15),
        new TrialAndMatchStrategy(2)
    };

    private readonly IStrategyHolder _holder;
    public StrategyInfo[] Infos { get; }

    public StrategyLoader(IStrategyHolder holder)
    {
        _holder = holder;
        var buffer = JsonSerializer.Deserialize<StrategyInfo[]>(File.ReadAllText(_path));
        if (buffer is null || buffer.Length != _strategies.Length) Infos = HandleIncorrectJsonFile();
        else Infos = buffer;
    }

    public void Load()
    {
        _holder.SetStrategies(_strategies);
        _holder.SetExcludedStrategies(InitExcludedStrategies());
    }

    private ulong InitExcludedStrategies()
    {
        ulong result = 0;

        for (int i = 0; i < Infos.Length; i++)
        {
            if (!Infos[i].Used) result |= 1ul << i;
        }

        return result;
    }

    private StrategyInfo[] HandleIncorrectJsonFile()
    {
        File.Delete(_path);
        StrategyInfo[] toWrite = new StrategyInfo[_strategies.Length];
        for (int i = 0; i < toWrite.Length; i++)
        {
            toWrite[i] = new StrategyInfo(_strategies[i]);
        }
        
        File.WriteAllText(_path, JsonSerializer.Serialize(toWrite, new JsonSerializerOptions {WriteIndented = true}));

        return toWrite;
    }

}

public class StrategyInfo
{
    public string StrategyName { get; set; }
    public StrategyLevel Difficulty { get; set; }
    public bool Used { get; set; }

    public StrategyInfo()
    {
        StrategyName = "Unknown";
        Difficulty = StrategyLevel.None;
        Used = false;
    }

    public StrategyInfo(IStrategy strategy)
    {
        StrategyName = strategy.Name;
        Difficulty = strategy.Difficulty;
        Used = true;
    }
}