using System.Windows;
using System.Windows.Media;
using DesktopApplication.Presenter;
using Model.Core;
using Model.Core.Changes;
using Model.Core.Explanation;
using Model.Repositories;
using Model.Sudokus.Player;
using Model.Utility;

namespace DesktopApplication;

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
        
        ChangeColorationNeutral = ToBrush(theme.StepColorNeutral);
        ChangeColorationChangeOne = ToBrush(theme.StepColorChange1);
        ChangeColorationChangeTwo = ToBrush(theme.StepColorChange2);
        ChangeColorationCauseOffOne = ToBrush(theme.StepColorCause1);
        ChangeColorationCauseOffTwo = ToBrush(theme.StepColorCause2);
        ChangeColorationCauseOffThree = ToBrush(theme.StepColorCause3);
        ChangeColorationCauseOffFour = ToBrush(theme.StepColorCause4);
        ChangeColorationCauseOffFive = ToBrush(theme.StepColorCause5);
        ChangeColorationCauseOffSix = ToBrush(theme.StepColorCause6);
        ChangeColorationCauseOffSeven = ToBrush(theme.StepColorCause7);
        ChangeColorationCauseOffEight = ToBrush(theme.StepColorCause8);
        ChangeColorationCauseOffNine = ToBrush(theme.StepColorCause9);
        ChangeColorationCauseOffTen = ToBrush(theme.StepColorCause10);
        ChangeColorationCauseOnOne = ToBrush(theme.StepColorOn);

        HighlightColorFirst = ToBrush(theme.HighlightColor1);
        HighlightColorSecond = ToBrush(theme.HighlightColor2);
        HighlightColorThird = ToBrush(theme.HighlightColor3);
        HighlightColorFourth = ToBrush(theme.HighlightColor4);
        HighlightColorFifth = ToBrush(theme.HighlightColor5);
        HighlightColorSixth = ToBrush(theme.HighlightColor6);
        HighlightColorSeventh = ToBrush(theme.HighlightColor7);
        
        ThemeChanged?.Invoke();
    }

    public Brush ToBrush(StepColor color)
    {
        return color switch
        {
            StepColor.Change1 => ChangeColorationChangeOne,
            StepColor.Change2 => ChangeColorationChangeTwo,
            StepColor.Neutral => ChangeColorationNeutral,
            StepColor.Cause1 => ChangeColorationCauseOffOne,
            StepColor.Cause2 => ChangeColorationCauseOffTwo,
            StepColor.Cause3 => ChangeColorationCauseOffThree,
            StepColor.Cause4 => ChangeColorationCauseOffFour,
            StepColor.Cause5 => ChangeColorationCauseOffFive,
            StepColor.Cause6 => ChangeColorationCauseOffSix,
            StepColor.Cause7 => ChangeColorationCauseOffSeven,
            StepColor.Cause8 => ChangeColorationCauseOffEight,
            StepColor.Cause9 => ChangeColorationCauseOffNine,
            StepColor.Cause10 => ChangeColorationCauseOffTen,
            StepColor.On => ChangeColorationCauseOnOne,
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

