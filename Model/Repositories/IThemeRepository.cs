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
    Theme? FindTheme(string name);
    void ClearThemes();
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
    
    //Tried using reflection but the order wouldn't stay the same so we have this stupid shit instead
    public IEnumerable<(string, RGB)> AllColors()
    {
        yield return (nameof(Background1), Background1);
        yield return (nameof(Background2), Background2);
        yield return (nameof(Background3), Background3);
        yield return (nameof(Primary1), Primary1);
        yield return (nameof(Primary2), Primary2);
        yield return (nameof(Secondary1), Secondary1);
        yield return (nameof(Secondary2), Secondary2);
        yield return (nameof(Accent), Accent);
        yield return (nameof(Text), Text);
        yield return (nameof(On), On);
        yield return (nameof(Off), Off);
        yield return (nameof(Disabled), Disabled);
        yield return (nameof(DifficultyBasic), DifficultyBasic);
        yield return (nameof(DifficultyEasy), DifficultyEasy);
        yield return (nameof(DifficultyMedium), DifficultyMedium);
        yield return (nameof(DifficultyHard), DifficultyHard);
        yield return (nameof(DifficultyExtreme), DifficultyExtreme);
        yield return (nameof(DifficultyInhuman), DifficultyInhuman);
        yield return (nameof(DifficultyByTrial), DifficultyByTrial);
        yield return (nameof(StepColorNeutral), StepColorNeutral);
        yield return (nameof(StepColorChange1), StepColorChange1);
        yield return (nameof(StepColorChange2), StepColorChange2);
        yield return (nameof(StepColorCause1), StepColorCause1);
        yield return (nameof(StepColorCause2), StepColorCause2);
        yield return (nameof(StepColorCause3), StepColorCause3);
        yield return (nameof(StepColorCause4), StepColorCause4);
        yield return (nameof(StepColorCause5), StepColorCause5);
        yield return (nameof(StepColorCause6), StepColorCause6);
        yield return (nameof(StepColorCause7), StepColorCause7);
        yield return (nameof(StepColorCause8), StepColorCause8);
        yield return (nameof(StepColorCause9), StepColorCause9);
        yield return (nameof(StepColorCause10), StepColorCause10);
        yield return (nameof(StepColorOn), StepColorOn);
        yield return (nameof(HighlightColor1), HighlightColor1);
        yield return (nameof(HighlightColor2), HighlightColor2);
        yield return (nameof(HighlightColor3), HighlightColor3);
        yield return (nameof(HighlightColor4), HighlightColor4);
        yield return (nameof(HighlightColor5), HighlightColor5);
        yield return (nameof(HighlightColor7), HighlightColor7);
        yield return (nameof(HighlightColor6), HighlightColor6);
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

    public override bool Equals(object? obj)
    {
        if (obj is not Theme t) return false;

        foreach (var color in t.AllColors())
        {
            if (GetColor(color.Item1) != color.Item2) return false;
        }

        return true;
    }
}