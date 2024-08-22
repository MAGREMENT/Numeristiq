using Model.Repositories;
using Model.Utility;

namespace Repository.Files;

public class FileThemeRepository : FileRepository<List<ThemeDto>>, IThemeRepository
{
    private List<ThemeDto>? _buffer;
    
    public FileThemeRepository(string fileName, bool searchParentDirectories, bool createIfNotFound,
        IFileType<List<ThemeDto>> type) : base(fileName, searchParentDirectories, createIfNotFound, type)
    {
    }
    
    public IReadOnlyList<Theme> GetThemes()
    {
        _buffer ??= Download();
        return _buffer is null ? Array.Empty<Theme>() : To(_buffer);
    }

    public int Count()
    {
        _buffer ??= Download();
        return _buffer?.Count ?? 0;
    }

    public void AddTheme(Theme theme)
    {
        _buffer ??= Download() ?? new List<ThemeDto>();

        _buffer.Add(ThemeDto.From(theme));
        Upload(_buffer);
    }

    public void ChangeTheme(int index, Theme newTheme)
    {
        _buffer ??= Download();
        if (_buffer is null || index < 0 || index >= _buffer.Count) return;

        _buffer[index] = ThemeDto.From(newTheme);
        Upload(_buffer);
    }

    public Theme? FindTheme(string name)
    {
        _buffer ??= Download();
        if (_buffer is null) return null;
        foreach (var theme in _buffer)
        {
            if (theme.Name.Equals(name)) return theme.To();
        }

        return null;
    }

    public void ClearThemes()
    {
        _buffer = null;
        Upload(new List<ThemeDto>());
    }

    private static Theme[] To(IReadOnlyList<ThemeDto> dtos)
    {
        var result = new Theme[dtos.Count];
        for (int i = 0; i < dtos.Count; i++)
        {
            result[i] = dtos[i].To();
        }

        return result;
    }
}

public record ThemeDto(string Name,
    int Background1, int Background2, int Background3,
    int Primary1, int Primary2,
    int Secondary1, int Secondary2,
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
    public const int ColorCount = 40;
    
    public static ThemeDto From(Theme theme)
    {
        return new ThemeDto(theme.Name, theme.Background1.ToHex(), theme.Background2.ToHex(), theme.Background3.ToHex()
            , theme.Primary1.ToHex(), theme.Primary2.ToHex()
            , theme.Secondary1.ToHex(), theme.Secondary2.ToHex()
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
    
    public Theme To()
    {
        return new Theme(Name, RGB.FromHex(Background1), RGB.FromHex(Background2), RGB.FromHex(Background3)
            , RGB.FromHex(Primary1), RGB.FromHex(Primary2)
            , RGB.FromHex(Secondary1), RGB.FromHex(Secondary2)
            , RGB.FromHex(Accent)
            , RGB.FromHex(Text)
            , RGB.FromHex(On), RGB.FromHex(Off)
            , RGB.FromHex(Disabled)
            , RGB.FromHex(DifficultyBasic), RGB.FromHex(DifficultyEasy), RGB.FromHex(DifficultyMedium), RGB.FromHex(DifficultyHard)
            , RGB.FromHex(DifficultyExtreme), RGB.FromHex(DifficultyInhuman), RGB.FromHex(DifficultyByTrial)
            , RGB.FromHex(StepColorNeutral), RGB.FromHex(StepColorChange1), RGB.FromHex(StepColorChange2)
            , RGB.FromHex(StepColorCause1), RGB.FromHex(StepColorCause2), RGB.FromHex(StepColorCause3)
            , RGB.FromHex(StepColorCause4), RGB.FromHex(StepColorCause5), RGB.FromHex(StepColorCause6)
            , RGB.FromHex(StepColorCause7), RGB.FromHex(StepColorCause8), RGB.FromHex(StepColorCause9)
            , RGB.FromHex(StepColorCause10), RGB.FromHex(StepColorOn)
            , RGB.FromHex(HighlightColor1), RGB.FromHex(HighlightColor2), RGB.FromHex(HighlightColor3), RGB.FromHex(HighlightColor4)
            , RGB.FromHex(HighlightColor5), RGB.FromHex(HighlightColor6), RGB.FromHex(HighlightColor7));
    }

    public ThemeDto(string name, List<int> colors) : this(name,colors[0],colors[1],
        colors[2],colors[3],colors[4],colors[5],
        colors[6],colors[7],colors[8],colors[9],colors[10],colors[11],
        colors[12],colors[13],colors[14],colors[15],
        colors[16],colors[17],colors[18],colors[18],
        colors[20],colors[21],colors[22],
        colors[23],colors[24],colors[25],colors[26],
        colors[27],colors[28],colors[29],
        colors[30],colors[31],colors[32],
        colors[33],colors[34],colors[35],colors[36],
        colors[37],colors[38], colors[39])
    {
        Name = name;
    }
    
    public IEnumerable<int> AllColors()
    {
        yield return Background1;
        yield return Background2;
        yield return Background3;
        yield return Primary1;
        yield return Primary2;
        yield return Secondary1;
        yield return Secondary2;
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
        yield return HighlightColor7;
        yield return HighlightColor6;
    }
}