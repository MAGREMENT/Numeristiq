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

    public RGB GetColor(string name)
    {
        var p = GetType().GetProperty(name);
        if (p is null) return new RGB();
        
        var v = p.GetValue(this);
        return v is null ? new RGB() : (RGB)v;
    }
    
    public Theme Copy(string name)
    {
        return this with { Name = name };
    }

    public Theme Copy()
    {
        return Copy(Name);
    }
}