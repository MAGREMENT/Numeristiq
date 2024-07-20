using System.Collections.Generic;
using Model.Core.BackTracking;
using Model.Utility;
using Model.Utility.Collections;

namespace Model.Repositories;

public interface IThemeRepository
{
    IReadOnlyList<Theme> GetThemes();
    int Count();
    void AddTheme(Theme theme);
}

public record Theme(string Name,
    RGB Background1, RGB Background2, RGB Background3,
    RGB Primary1, RGB Primary2,
    RGB Secondary1, RGB Secondary2,
    RGB Accent,
    RGB Text,
    RGB On, RGB Off,
    RGB Disabled,
    RGB DifficultyBasic, RGB DifficultyEasy, RGB DifficultyMedium, RGB DifficultyHard,
    RGB DifficultyExtreme, RGB DifficultyInhuman, RGB DifficultyByTrial,
    RGB ChangeColorationNeutral, RGB ChangeColorationChangeOne, RGB ChangeColorationChangeTwo,
    RGB ChangeColorationCauseOffOne, RGB ChangeColorationCauseOffTwo, RGB ChangeColorationCauseOffThree,
    RGB ChangeColorationCauseOffFour, RGB ChangeColorationCauseOffFive, RGB ChangeColorationCauseOffSix,
    RGB ChangeColorationCauseOffSeven, RGB ChangeColorationCauseOffEight, RGB ChangeColorationCauseOffNine,
    RGB ChangeColorationCauseOffTen, RGB ChangeColorationCauseOnOne,
    RGB HighlightColorFirst, RGB HighlightColorSecond, RGB HighlightColorThird, RGB HighlightColorFourth,
    RGB HighlightColorFifth, RGB HighlightColorSixth, RGB HighlightColorSeventh) : INamed, ICopyable<Theme>
{
    public IEnumerable<(string, RGB)> AllColors()
    {
        foreach (var p in GetType().GetProperties())
        {
            var value = p.GetValue(this);
            if(value is not RGB rgb) continue;

            yield return (p.Name, rgb);
        }
    }

    public Theme Copy()
    {
        return new Theme(Name, Background1, Background2, Background3
            , Primary1, Primary2
            , Secondary1, Secondary2
            , Accent
            , Text
            , On, Off
            , Disabled
            , DifficultyBasic, DifficultyEasy, DifficultyMedium, DifficultyHard
            , DifficultyExtreme, DifficultyInhuman, DifficultyByTrial
            , ChangeColorationNeutral, ChangeColorationChangeOne, ChangeColorationChangeTwo
            , ChangeColorationCauseOffOne, ChangeColorationCauseOffTwo, ChangeColorationCauseOffThree
            , ChangeColorationCauseOffFour, ChangeColorationCauseOffFive, ChangeColorationCauseOffSix
            , ChangeColorationCauseOffSeven, ChangeColorationCauseOffEight, ChangeColorationCauseOffNine
            , ChangeColorationCauseOffTen, ChangeColorationCauseOnOne
            , HighlightColorFirst, HighlightColorSecond, HighlightColorThird, HighlightColorFourth
            , HighlightColorFifth, HighlightColorSixth, HighlightColorSeventh);
    }
}