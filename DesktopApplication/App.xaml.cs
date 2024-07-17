using System.Windows;
using System.Windows.Media;
using DesktopApplication.Presenter;
using Model;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Explanation;
using Model.Repositories;
using Model.Sudokus.Player;
using Model.Sudokus.Solver;
using Model.Utility;

namespace DesktopApplication;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : IGlobalApplicationView
{
    public new static App Current => (App)Application.Current;

    public ThemeInformation ThemeInformation { get; } = new();
    
    public App()
    {
        InitializeComponent();

        GlobalApplicationPresenter.InitializeInstance(this);
        FrameworkElement.StyleProperty.OverrideMetadata(typeof(Window), new FrameworkPropertyMetadata
        {
            DefaultValue = FindResource(typeof(Window))
        });
    }

    public void SetTheme(Theme t)
    {
        Resources["Background1"] = ThemeInformation.ToBrush(t.Background1);
        Resources["Background2"] = ThemeInformation.ToBrush(t.Background2);
        Resources["Background2Color"] = ThemeInformation.ToColor(t.Background2);
        Resources["Background3"] = ThemeInformation.ToBrush(t.Background3);
        Resources["Primary1"] = ThemeInformation.ToBrush(t.Primary1);
        Resources["Primary1Color"] = ThemeInformation.ToColor(t.Primary1);
        Resources["Primary2"] = ThemeInformation.ToBrush(t.Primary2);
        Resources["Primary2Color"] = ThemeInformation.ToColor(t.Primary2);
        Resources["Secondary1"] = ThemeInformation.ToBrush(t.Secondary1);
        Resources["Secondary1Color"] = ThemeInformation.ToColor(t.Secondary1);
        Resources["Secondary2"] = ThemeInformation.ToBrush(t.Secondary2);
        Resources["Secondary2Color"] = ThemeInformation.ToColor(t.Secondary2);
        Resources["Accent"] = ThemeInformation.ToBrush(t.Accent);
        Resources["Text"] = ThemeInformation.ToBrush(t.Text);
        Resources["ThumbColor"] = ThemeInformation.ToColor(t.Text);
        Resources["On"] = ThemeInformation.ToBrush(t.On);
        Resources["OnColor"] = ThemeInformation.ToColor(t.On);
        Resources["Off"] = ThemeInformation.ToBrush(t.Off);
        Resources["OffColor"] = ThemeInformation.ToColor(t.Off);
        Resources["Disabled"] = ThemeInformation.ToBrush(t.Disabled);
        Resources["DifficultyBasic"] = ThemeInformation.ToBrush(t.DifficultyBasic);
        Resources["DifficultyEasy"] = ThemeInformation.ToBrush(t.DifficultyEasy);
        Resources["DifficultyMedium"] = ThemeInformation.ToBrush(t.DifficultyMedium);
        Resources["DifficultyHard"] = ThemeInformation.ToBrush(t.DifficultyHard);
        Resources["DifficultyExtreme"] = ThemeInformation.ToBrush(t.DifficultyExtreme);
        Resources["DifficultyInhuman"] = ThemeInformation.ToBrush(t.DifficultyInhuman);
        Resources["DifficultyByTrial"] = ThemeInformation.ToBrush(t.DifficultyByTrial);

        ThemeInformation.SetTheme(t);
    }
}

public class ThemeInformation
{
    private Brush Primary1Brush = Brushes.Orange;
    private Brush Secondary1Brush = Brushes.Purple;
    private Brush TextBrush = Brushes.Black;
    
    private Brush ChangeColorationNeutral = Brushes.Silver;
    private Brush ChangeColorationChangeOne = Brushes.RoyalBlue;
    private Brush ChangeColorationChangeTwo = Brushes.CornflowerBlue;
    private Brush ChangeColorationCauseOffOne = Brushes.Red;
    private Brush ChangeColorationCauseOffTwo = Brushes.Coral;
    private Brush ChangeColorationCauseOffThree = Brushes.Orange;
    private Brush ChangeColorationCauseOffFour = Brushes.Yellow;
    private Brush ChangeColorationCauseOffFive = Brushes.Chocolate;
    private Brush ChangeColorationCauseOffSix = Brushes.Firebrick;
    private Brush ChangeColorationCauseOffSeven = Brushes.Brown;
    private Brush ChangeColorationCauseOffEight = Brushes.SaddleBrown;
    private Brush ChangeColorationCauseOffNine = Brushes.DarkRed;
    private Brush ChangeColorationCauseOffTen = Brushes.RosyBrown;
    private Brush ChangeColorationCauseOnOne = Brushes.Green;

    private Brush HighlightColorFirst = Brushes.Red;
    private Brush HighlightColorSecond = Brushes.Green;
    private Brush HighlightColorThird = Brushes.RoyalBlue;
    private Brush HighlightColorFourth = Brushes.Purple;
    private Brush HighlightColorFifth = Brushes.Orange;
    private Brush HighlightColorSixth = Brushes.Yellow;
    private Brush HighlightColorSeventh = Brushes.Cyan;
        
    public event OnThemeChange? ThemeChanged;

    public void SetTheme(Theme theme)
    {
        Primary1Brush = ToBrush(theme.Primary1);
        Secondary1Brush = ToBrush(theme.Secondary1);
        TextBrush = ToBrush(theme.Text);
        
        ChangeColorationNeutral = ToBrush(theme.ChangeColorationNeutral);
        ChangeColorationChangeOne = ToBrush(theme.ChangeColorationChangeOne);
        ChangeColorationChangeTwo = ToBrush(theme.ChangeColorationChangeTwo);
        ChangeColorationCauseOffOne = ToBrush(theme.ChangeColorationCauseOffOne);
        ChangeColorationCauseOffTwo = ToBrush(theme.ChangeColorationCauseOffTwo);
        ChangeColorationCauseOffThree = ToBrush(theme.ChangeColorationCauseOffThree);
        ChangeColorationCauseOffFour = ToBrush(theme.ChangeColorationCauseOffFour);
        ChangeColorationCauseOffFive = ToBrush(theme.ChangeColorationCauseOffFive);
        ChangeColorationCauseOffSix = ToBrush(theme.ChangeColorationCauseOffSix);
        ChangeColorationCauseOffSeven = ToBrush(theme.ChangeColorationCauseOffSeven);
        ChangeColorationCauseOffEight = ToBrush(theme.ChangeColorationCauseOffEight);
        ChangeColorationCauseOffNine = ToBrush(theme.ChangeColorationCauseOffNine);
        ChangeColorationCauseOffTen = ToBrush(theme.ChangeColorationCauseOffTen);
        ChangeColorationCauseOnOne = ToBrush(theme.ChangeColorationCauseOnOne);

        HighlightColorFirst = ToBrush(theme.HighlightColorFirst);
        HighlightColorSecond = ToBrush(theme.HighlightColorSecond);
        HighlightColorThird = ToBrush(theme.HighlightColorThird);
        HighlightColorFourth = ToBrush(theme.HighlightColorFourth);
        HighlightColorFifth = ToBrush(theme.HighlightColorFifth);
        HighlightColorSixth = ToBrush(theme.HighlightColorSixth);
        HighlightColorSeventh = ToBrush(theme.HighlightColorSeventh);
        
        ThemeChanged?.Invoke();
    }

    public Brush ToBrush(ChangeColoration coloration)
    {
        return coloration switch
        {
            ChangeColoration.ChangeOne => ChangeColorationChangeOne,
            ChangeColoration.ChangeTwo => ChangeColorationChangeTwo,
            ChangeColoration.Neutral => ChangeColorationNeutral,
            ChangeColoration.CauseOffOne => ChangeColorationCauseOffOne,
            ChangeColoration.CauseOffTwo => ChangeColorationCauseOffTwo,
            ChangeColoration.CauseOffThree => ChangeColorationCauseOffThree,
            ChangeColoration.CauseOffFour => ChangeColorationCauseOffFour,
            ChangeColoration.CauseOffFive => ChangeColorationCauseOffFive,
            ChangeColoration.CauseOffSix => ChangeColorationCauseOffSix,
            ChangeColoration.CauseOffSeven => ChangeColorationCauseOffSeven,
            ChangeColoration.CauseOffEight => ChangeColorationCauseOffEight,
            ChangeColoration.CauseOffNine => ChangeColorationCauseOffNine,
            ChangeColoration.CauseOffTen => ChangeColorationCauseOffTen,
            ChangeColoration.CauseOnOne => ChangeColorationCauseOnOne,
            _ => ChangeColorationNeutral
        };
    }

    public Brush ToBrush(HighlightColor color)
    {
        return color switch
        {
            HighlightColor.First => HighlightColorFirst,
            HighlightColor.Second => HighlightColorSecond,
            HighlightColor.Third => HighlightColorThird,
            HighlightColor.Fourth => HighlightColorFourth,
            HighlightColor.Fifth => HighlightColorFifth,
            HighlightColor.Sixth => HighlightColorSixth,
            HighlightColor.Seventh => HighlightColorSeventh,
            _ => Brushes.Transparent
        };
    }
    
    public Brush ToBrush(ExplanationColor color)
    {
        return color switch
        {
            ExplanationColor.Primary => Primary1Brush,
            ExplanationColor.Secondary => Secondary1Brush,
            _ => TextBrush
        };
    }

    public static string ResourceNameFor(ExplanationColor color)
    {
        return color switch
        {
            ExplanationColor.Primary => "Primary1",
            ExplanationColor.Secondary => "Secondary1",
            _ => "Text"
        };
    }

    public static string ResourceNameFor(StepDifficulty difficulty)
    {
        if (difficulty == StepDifficulty.None) return "Disabled";
        return "Difficulty" + difficulty;
    }
    
    public static Brush ToBrush(RGB rgb)
    {
        return new SolidColorBrush(Color.FromRgb(rgb.Red, rgb.Green, rgb.Blue));
    }

    public static Color ToColor(RGB rgb)
    {
        return Color.FromRgb(rgb.Red, rgb.Green, rgb.Blue);
    }
}

public delegate void OnThemeChange();