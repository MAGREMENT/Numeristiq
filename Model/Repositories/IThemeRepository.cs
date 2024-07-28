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
    void ChangeTheme(int index, Theme newTheme);
}

public class Theme : INamed, ICopyable<Theme>
{
    public string Name { get; }
    
    public RGB Background1 {get; set;}
    public RGB Background2 {get; set;}
    public RGB Background3 {get; set;}
    public RGB Primary1 {get; set;}
    public RGB Primary2 {get; set;}
    public RGB Secondary1 {get; set;}
    public RGB Secondary2 {get; set;}
    public RGB Accent {get; set;}
    public RGB Text {get; set;}
    public RGB On {get; set;}
    public RGB Off {get; set;}
    public RGB Disabled {get; set;}
    public RGB DifficultyBasic {get; set;}
    public RGB DifficultyEasy {get; set;}
    public RGB DifficultyMedium {get; set;}
    public RGB DifficultyHard {get; set;}
    public RGB DifficultyExtreme {get; set;}
    public RGB DifficultyInhuman {get; set;}
    public RGB DifficultyByTrial {get; set;}
    public RGB StepColorNeutral {get; set;}
    public RGB StepColorChange1 {get; set;}
    public RGB StepColorChange2 {get; set;}
    public RGB StepColorCause1 {get; set;}
    public RGB StepColorCause2 {get; set;}
    public RGB StepColorCause3 {get; set;}
    public RGB StepColorCause4 {get; set;}
    public RGB StepColorCause5 {get; set;}
    public RGB StepColorCause6 {get; set;}
    public RGB StepColorCause7 {get; set;}
    public RGB StepColorCause8 {get; set;}
    public RGB StepColorCause9 {get; set;}
    public RGB StepColorCause10 {get; set;}
    public RGB StepColorOn {get; set;}
    public RGB HighlightColor1 {get; set;}
    public RGB HighlightColor2 {get; set;}
    public RGB HighlightColor3 {get; set;}
    public RGB HighlightColor4 {get; set;}
    public RGB HighlightColor5 {get; set;}
    public RGB HighlightColor6 {get; set;}
    public RGB HighlightColor7 {get; set;}

    public Theme(string name,
        RGB background1, RGB background2, RGB background3,
        RGB primary1, RGB primary2,
        RGB secondary1, RGB secondary2, 
        RGB accent,
        RGB text,
        RGB on, RGB off,
        RGB disabled, RGB difficultyBasic, RGB difficultyEasy, RGB difficultyMedium, RGB difficultyHard, RGB difficultyExtreme, RGB difficultyInhuman, RGB difficultyByTrial,
        RGB stepColorNeutral, RGB stepColorChange1, RGB stepColorChange2, RGB stepColorCause1, RGB stepColorCause2, RGB stepColorCause3, RGB stepColorCause4, RGB stepColorCause5, RGB stepColorCause6, RGB stepColorCause7, RGB stepColorCause8, RGB stepColorCause9, RGB stepColorCause10, RGB stepColorOn,
        RGB highlightColor1, RGB highlightColor2, RGB highlightColor3, RGB highlightColor4, RGB highlightColor5, RGB highlightColor6, RGB highlightColor7)
    {
        Name = name;
        Background1 = background1;
        Background2 = background2;
        Background3 = background3;
        Primary1 = primary1;
        Primary2 = primary2;
        Secondary1 = secondary1;
        Secondary2 = secondary2;
        Accent = accent;
        Text = text;
        On = on;
        Off = off;
        Disabled = disabled;
        DifficultyBasic = difficultyBasic;
        DifficultyEasy = difficultyEasy;
        DifficultyMedium = difficultyMedium;
        DifficultyHard = difficultyHard;
        DifficultyExtreme = difficultyExtreme;
        DifficultyInhuman = difficultyInhuman;
        DifficultyByTrial = difficultyByTrial;
        StepColorNeutral = stepColorNeutral;
        StepColorChange1 = stepColorChange1;
        StepColorChange2 = stepColorChange2;
        StepColorCause1 = stepColorCause1;
        StepColorCause2 = stepColorCause2;
        StepColorCause3 = stepColorCause3;
        StepColorCause4 = stepColorCause4;
        StepColorCause5 = stepColorCause5;
        StepColorCause6 = stepColorCause6;
        StepColorCause7 = stepColorCause7;
        StepColorCause8 = stepColorCause8;
        StepColorCause9 = stepColorCause9;
        StepColorCause10 = stepColorCause10;
        StepColorOn = stepColorOn;
        HighlightColor1 = highlightColor1;
        HighlightColor2 = highlightColor2;
        HighlightColor3 = highlightColor3;
        HighlightColor4 = highlightColor4;
        HighlightColor5 = highlightColor5;
        HighlightColor7 = highlightColor7;
        HighlightColor6 = highlightColor6;
    }
    
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

    public void SetColor(string name, RGB value)
    {
        var p = GetType().GetProperty(name);
        if (p is null) return;

        p.SetValue(this, value);
    }
    
    public Theme Copy(string name)
    {
        return new Theme(name, Background1, Background2, Background3
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

    public Theme Copy()
    {
        return Copy(Name);
    }
}