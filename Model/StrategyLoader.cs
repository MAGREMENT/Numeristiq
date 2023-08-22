using System.IO;
using System.Text.Json;
using Model.Strategies;
using Model.Strategies.AlternatingChains;
using Model.Strategies.AlternatingChains.ChainAlgorithms;
using Model.Strategies.AlternatingChains.ChainTypes;
using Model.Strategies.ForcingNets;
using Model.StrategiesUtil;

namespace Model;

public class StrategyLoader //TODO improve this and do relative paths
{
    private const string Path = "C:\\Users\\Zach\\Desktop\\Perso\\SudokuSolver\\Model\\Data\\strategies.json";

    private readonly IStrategy[] _strategies =
    {
        new NakedSingleStrategy(),
        new HiddenSingleStrategy(),
        new NakedPossibilitiesStrategy(2),
        new HiddenPossibilitiesStrategy(2),
        new NakedPossibilitiesStrategy(3),
        new HiddenPossibilitiesStrategy(3),
        new NakedPossibilitiesStrategy(4),
        new HiddenPossibilitiesStrategy(4),
        new BoxLineReductionStrategy(),
        new PointingPossibilitiesStrategy(),
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
        //new PatternOverlayStrategy()
        new TrialAndMatchStrategy(2)
    };

    private readonly IStrategyHolder _holder;
    public StrategyInfo[] Infos { get; }

    public StrategyLoader(IStrategyHolder holder)
    {
        _holder = holder;
        var buffer = JsonSerializer.Deserialize<StrategyInfo[]>(File.ReadAllText(Path));
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
        File.Delete(Path);
        StrategyInfo[] toWrite = new StrategyInfo[_strategies.Length];
        for (int i = 0; i < toWrite.Length; i++)
        {
            toWrite[i] = new StrategyInfo(_strategies[i]);
        }
        
        File.WriteAllText(Path, JsonSerializer.Serialize(toWrite, new JsonSerializerOptions {WriteIndented = true}));

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
        StrategyName = "unknown";
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