using Model.Repositories;
using Model.Utility;

namespace Repository;

public class JsonThemeRepository : JsonRepository, IThemeRepository
{
    private List<ThemeDto>? _buffer;
    
    public JsonThemeRepository(string filePath, bool searchParentDirectories, bool createIfNotFound)
        : base(filePath, searchParentDirectories, createIfNotFound)
    {
    }

    public IReadOnlyList<Theme> GetThemes()
    {
        _buffer ??= Download<List<ThemeDto>>();
        return _buffer is null ? Array.Empty<Theme>() : To(_buffer);
    }

    public int Count()
    {
        _buffer ??= Download<List<ThemeDto>>();
        return _buffer?.Count ?? 0;
    }

    public void AddTheme(Theme theme)
    {
        _buffer ??= Download<List<ThemeDto>>();
        if (_buffer is null) return;

        _buffer.Add(ThemeDto.From(theme));
        Upload(_buffer);
    }

    public void ChangeTheme(int index, Theme newTheme)
    {
        _buffer ??= Download<List<ThemeDto>>();
        if (_buffer is null || index < 0 || index >= _buffer.Count) return;

        _buffer[index] = ThemeDto.From(newTheme);
        Upload(_buffer);
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
}