using Model.Core;
using Model.Core.Settings;
using Model.Repositories;
using Model.Sudokus.Solver;
using Model.Utility;

namespace Repository;

public static class DAOManager
{
    public static StrategyDAO To(SudokuStrategy strategy)
    {
        Dictionary<string, string> settings = new();

        foreach (var s in strategy.EnumerateSettings())
        {
            settings.Add(s.Name, s.Get().ToString()!);
        }

        return new StrategyDAO(strategy.Name, strategy.Enabled, strategy.Locked, strategy.InstanceHandling, settings);
    }

    public static SudokuStrategy? To(StrategyDAO dao)
    {
        var result = StrategyPool.CreateFrom(dao.Name, dao.Enabled, dao.Locked, dao.InstanceHandling);
        if (result is null) return null;

        if (dao.Settings is null) return result;
        
        foreach (var entry in dao.Settings)
        {
            result.TrySetSetting(entry.Key, new StringSettingValue(entry.Value));
        }

        return result;
    }
    
    public static Theme[] To(IReadOnlyList<ThemeDAO> dtos)
    {
        var result = new Theme[dtos.Count];
        for (int i = 0; i < dtos.Count; i++)
        {
            result[i] = To(dtos[i]);
        }

        return result;
    }
    
    public static List<SudokuStrategy> To(IEnumerable<StrategyDAO> download)
    {
        var result = new List<SudokuStrategy>();
        foreach (var d in download)
        {
            var s = To(d);
            if (s is not null) result.Add(s);
        }

        return result;
    }

    public static List<StrategyDAO> To(IReadOnlyList<SudokuStrategy> list)
    {
        var result = new List<StrategyDAO>(list.Count);
        foreach (var e in list) result.Add(To(e));
        return result;
    }
    
    public static ThemeDAO To(Theme theme)
    {
        return new ThemeDAO(theme.Name, theme.BackgroundDeep.ToHex(), theme.Background1.ToHex(),
            theme.Background2.ToHex(), theme.BackgroundHighlighted.ToHex()
            , theme.Primary.ToHex(), theme.PrimaryHighlighted.ToHex()
            , theme.Secondary.ToHex(), theme.SecondaryHighlighted.ToHex()
            , theme.Accent.ToHex()
            , theme.Text.ToHex()
            , theme.On.ToHex(), theme.Off.ToHex()
            , theme.Disabled.ToHex()
            , theme.DifficultyBasic.ToHex(), theme.DifficultyEasy.ToHex(), theme.DifficultyMedium.ToHex(), theme.DifficultyHard.ToHex()
            , theme.DifficultyExtreme.ToHex(), theme.DifficultyInhuman.ToHex(), theme.DifficultyByTrial.ToHex()
            , theme.StepColorNeutral.ToHex(), theme.StepColorChange1.ToHex(), theme.StepColorChange2.ToHex()
            , theme.StepColorCause1.ToHex(), theme.StepColorCause2.ToHex(), theme.StepColorCause3.ToHex()
            , theme.StepColorCause4.ToHex(), theme.StepColorCause5.ToHex(), theme.StepColorCause6.ToHex()
            , theme.StepColorCause7.ToHex(), theme.StepColorCause8.ToHex(), theme.StepColorCause9.ToHex()
            , theme.StepColorCause10.ToHex(), theme.StepColorOn.ToHex()
            , theme.HighlightColor1.ToHex(), theme.HighlightColor2.ToHex(), theme.HighlightColor3.ToHex(), theme.HighlightColor4.ToHex()
            , theme.HighlightColor5.ToHex(), theme.HighlightColor6.ToHex(), theme.HighlightColor7.ToHex());
    }
    
    public static Theme To(ThemeDAO dao)
    {
        return new Theme(dao.Name, RGB.FromHex(dao.BackgroundDeep), RGB.FromHex(dao.Background1),
            RGB.FromHex(dao.Background2), RGB.FromHex(dao.BackgroundHighlighted)
            , RGB.FromHex(dao.Primary), RGB.FromHex(dao.PrimaryHighlighted)
            , RGB.FromHex(dao.Secondary), RGB.FromHex(dao.SecondaryHighlighted)
            , RGB.FromHex(dao.Accent)
            , RGB.FromHex(dao.Text)
            , RGB.FromHex(dao.On), RGB.FromHex(dao.Off)
            , RGB.FromHex(dao.Disabled)
            , RGB.FromHex(dao.DifficultyBasic), RGB.FromHex(dao.DifficultyEasy), RGB.FromHex(dao.DifficultyMedium), RGB.FromHex(dao.DifficultyHard)
            , RGB.FromHex(dao.DifficultyExtreme), RGB.FromHex(dao.DifficultyInhuman), RGB.FromHex(dao.DifficultyByTrial)
            , RGB.FromHex(dao.StepColorNeutral), RGB.FromHex(dao.StepColorChange1), RGB.FromHex(dao.StepColorChange2)
            , RGB.FromHex(dao.StepColorCause1), RGB.FromHex(dao.StepColorCause2), RGB.FromHex(dao.StepColorCause3)
            , RGB.FromHex(dao.StepColorCause4), RGB.FromHex(dao.StepColorCause5), RGB.FromHex(dao.StepColorCause6)
            , RGB.FromHex(dao.StepColorCause7), RGB.FromHex(dao.StepColorCause8), RGB.FromHex(dao.StepColorCause9)
            , RGB.FromHex(dao.StepColorCause10), RGB.FromHex(dao.StepColorOn)
            , RGB.FromHex(dao.HighlightColor1), RGB.FromHex(dao.HighlightColor2), RGB.FromHex(dao.HighlightColor3), RGB.FromHex(dao.HighlightColor4)
            , RGB.FromHex(dao.HighlightColor5), RGB.FromHex(dao.HighlightColor6), RGB.FromHex(dao.HighlightColor7));
    }
}

public record StrategyDAO(string Name, bool Enabled, bool Locked, InstanceHandling InstanceHandling,
    Dictionary<string, string>? Settings);

public record ThemeDAO(string Name,
    int BackgroundDeep, int Background1, int Background2, int BackgroundHighlighted,
    int Primary, int PrimaryHighlighted,
    int Secondary, int SecondaryHighlighted,
    int Accent,
    int Text,
    int On, int Off,
    int Disabled,
    int DifficultyBasic, int DifficultyEasy, int DifficultyMedium, int DifficultyHard,
    int DifficultyExtreme, int DifficultyInhuman, int DifficultyByTrial,
    int StepColorNeutral, int StepColorChange1, int StepColorChange2,
    int StepColorCause1, int StepColorCause2, int StepColorCause3,
    int StepColorCause4, int StepColorCause5, int StepColorCause6,
    int StepColorCause7, int StepColorCause8, int StepColorCause9,
    int StepColorCause10, int StepColorOn,
    int HighlightColor1, int HighlightColor2, int HighlightColor3, int HighlightColor4,
    int HighlightColor5, int HighlightColor6, int HighlightColor7)
{
    public const int ColorCount = 41;
    
    public ThemeDAO(string name, List<int> colors) : this(name,colors[0],colors[1],
        colors[2],colors[3],colors[4],colors[5],
        colors[6],colors[7],colors[8],colors[9],colors[10],colors[11],
        colors[12],colors[13],colors[14],colors[15],
        colors[16],colors[17],colors[18],colors[19],
        colors[20],colors[21],colors[22],
        colors[23],colors[24],colors[25],colors[26],
        colors[27],colors[28],colors[29],
        colors[30],colors[31],colors[32],
        colors[33],colors[34],colors[35],colors[36],
        colors[37],colors[38], colors[39], colors[40])
    {
        Name = name;
    }
    
    public IEnumerable<int> AllColors()
    {
        yield return BackgroundDeep;
        yield return Background1;
        yield return Background2;
        yield return BackgroundHighlighted;
        yield return Primary;
        yield return PrimaryHighlighted;
        yield return Secondary;
        yield return SecondaryHighlighted;
        yield return Accent;
        yield return Text;
        yield return On;
        yield return Off;
        yield return Disabled;
        yield return DifficultyBasic;
        yield return DifficultyEasy;
        yield return DifficultyMedium;
        yield return DifficultyHard;
        yield return DifficultyExtreme;
        yield return DifficultyInhuman;
        yield return DifficultyByTrial;
        yield return StepColorNeutral;
        yield return StepColorChange1;
        yield return StepColorChange2;
        yield return StepColorCause1;
        yield return StepColorCause2;
        yield return StepColorCause3;
        yield return StepColorCause4;
        yield return StepColorCause5;
        yield return StepColorCause6;
        yield return StepColorCause7;
        yield return StepColorCause8;
        yield return StepColorCause9;
        yield return StepColorCause10;
        yield return StepColorOn;
        yield return HighlightColor1;
        yield return HighlightColor2;
        yield return HighlightColor3;
        yield return HighlightColor4;
        yield return HighlightColor5;
        yield return HighlightColor6;
        yield return HighlightColor7;
    }
}