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
    int ChangeColorationNeutral, int ChangeColorationChangeOne, int ChangeColorationChangeTwo,
    int ChangeColorationCauseOffOne, int ChangeColorationCauseOffTwo, int ChangeColorationCauseOffThree,
    int ChangeColorationCauseOffFour, int ChangeColorationCauseOffFive, int ChangeColorationCauseOffSix,
    int ChangeColorationCauseOffSeven, int ChangeColorationCauseOffEight, int ChangeColorationCauseOffNine,
    int ChangeColorationCauseOffTen, int ChangeColorationCauseOnOne,
    int HighlightColorFirst, int HighlightColorSecond, int HighlightColorThird, int HighlightColorFourth,
    int HighlightColorFifth, int HighlightColorSixth, int HighlightColorSeventh)
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
            , theme.ChangeColorationNeutral.ToHex(), theme.ChangeColorationChangeOne.ToHex(), theme.ChangeColorationChangeTwo.ToHex()
            , theme.ChangeColorationCauseOffOne.ToHex(), theme.ChangeColorationCauseOffTwo.ToHex(), theme.ChangeColorationCauseOffThree.ToHex()
            , theme.ChangeColorationCauseOffFour.ToHex(), theme.ChangeColorationCauseOffFive.ToHex(), theme.ChangeColorationCauseOffSix.ToHex()
            , theme.ChangeColorationCauseOffSeven.ToHex(), theme.ChangeColorationCauseOffEight.ToHex(), theme.ChangeColorationCauseOffNine.ToHex()
            , theme.ChangeColorationCauseOffTen.ToHex(), theme.ChangeColorationCauseOnOne.ToHex()
            , theme.HighlightColorFirst.ToHex(), theme.HighlightColorSecond.ToHex(), theme.HighlightColorThird.ToHex(), theme.HighlightColorFourth.ToHex()
            , theme.HighlightColorFifth.ToHex(), theme.HighlightColorSixth.ToHex(), theme.HighlightColorSeventh.ToHex());
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
            , RGB.FromHex(ChangeColorationNeutral), RGB.FromHex(ChangeColorationChangeOne), RGB.FromHex(ChangeColorationChangeTwo)
            , RGB.FromHex(ChangeColorationCauseOffOne), RGB.FromHex(ChangeColorationCauseOffTwo), RGB.FromHex(ChangeColorationCauseOffThree)
            , RGB.FromHex(ChangeColorationCauseOffFour), RGB.FromHex(ChangeColorationCauseOffFive), RGB.FromHex(ChangeColorationCauseOffSix)
            , RGB.FromHex(ChangeColorationCauseOffSeven), RGB.FromHex(ChangeColorationCauseOffEight), RGB.FromHex(ChangeColorationCauseOffNine)
            , RGB.FromHex(ChangeColorationCauseOffTen), RGB.FromHex(ChangeColorationCauseOnOne)
            , RGB.FromHex(HighlightColorFirst), RGB.FromHex(HighlightColorSecond), RGB.FromHex(HighlightColorThird), RGB.FromHex(HighlightColorFourth)
            , RGB.FromHex(HighlightColorFifth), RGB.FromHex(HighlightColorSixth), RGB.FromHex(HighlightColorSeventh));
    }
}