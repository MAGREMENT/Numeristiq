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
    RGB StepColorNeutral, RGB StepColorChange1, RGB StepColorChange2,
    RGB StepColorCause1, RGB StepColorCause2, RGB StepColorCause3,
    RGB StepColorCause4, RGB StepColorCause5, RGB StepColorCause6,
    RGB StepColorCause7, RGB StepColorCause8, RGB StepColorCause9,
    RGB StepColorCause10, RGB StepColorOn,
    RGB HighlightColor1, RGB HighlightColor2, RGB HighlightColor3, RGB HighlightColor4,
    RGB HighlightColor5, RGB HighlightColor6, RGB HighlightColor7) : INamed, ICopyable<Theme>
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
            , StepColorNeutral, StepColorChange1, StepColorChange2
            , StepColorCause1, StepColorCause2, StepColorCause3
            , StepColorCause4, StepColorCause5, StepColorCause6
            , StepColorCause7, StepColorCause8, StepColorCause9
            , StepColorCause10, StepColorOn
            , HighlightColor1, HighlightColor2, HighlightColor3, HighlightColor4
            , HighlightColor5, HighlightColor6, HighlightColor7);
    }
}