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
        Resources["TextColor"] = ThemeInformation.ToColor(t.Text);
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
    private Brush Primary1 = Brushes.Orange;
    private Brush Secondary1 = Brushes.Purple;
    private Brush Text = Brushes.Black;
    
    private Brush StepColorNeutral = Brushes.Silver;
    private Brush StepColorChange1 = Brushes.RoyalBlue;
    private Brush StepColorChange2 = Brushes.CornflowerBlue;
    private Brush StepColorCause1 = Brushes.Red;
    private Brush StepColorCause2 = Brushes.Coral;
    private Brush StepColorCause3 = Brushes.Orange;
    private Brush StepColorCause4 = Brushes.Yellow;
    private Brush StepColorCause5 = Brushes.Chocolate;
    private Brush StepColorCause6 = Brushes.Firebrick;
    private Brush StepColorCause7 = Brushes.Brown;
    private Brush StepColorCause8 = Brushes.SaddleBrown;
    private Brush StepColorCause9 = Brushes.DarkRed;
    private Brush StepColorCause10 = Brushes.RosyBrown;
    private Brush StepColorCauseOn = Brushes.Green;

    private Brush HighlightColor1 = Brushes.Red;
    private Brush HighlightColor2 = Brushes.Green;
    private Brush HighlightColor3 = Brushes.RoyalBlue;
    private Brush HighlightColor4 = Brushes.Purple;
    private Brush HighlightColor5 = Brushes.Orange;
    private Brush HighlightColor6 = Brushes.Yellow;
    private Brush HighlightColor7 = Brushes.Cyan;
        
    public event OnThemeChange? ThemeChanged;

    public void SetTheme(Theme theme)
    {
        Primary1 = ToBrush(theme.Primary1);
        Secondary1 = ToBrush(theme.Secondary1);
        Text = ToBrush(theme.Text);
        
        StepColorNeutral = ToBrush(theme.StepColorNeutral);
        StepColorChange1 = ToBrush(theme.StepColorChange1);
        StepColorChange2 = ToBrush(theme.StepColorChange2);
        StepColorCause1 = ToBrush(theme.StepColorCause1);
        StepColorCause2 = ToBrush(theme.StepColorCause2);
        StepColorCause3 = ToBrush(theme.StepColorCause3);
        StepColorCause4 = ToBrush(theme.StepColorCause4);
        StepColorCause5 = ToBrush(theme.StepColorCause5);
        StepColorCause6 = ToBrush(theme.StepColorCause6);
        StepColorCause7 = ToBrush(theme.StepColorCause7);
        StepColorCause8 = ToBrush(theme.StepColorCause8);
        StepColorCause9 = ToBrush(theme.StepColorCause9);
        StepColorCause10 = ToBrush(theme.StepColorCause10);
        StepColorCauseOn = ToBrush(theme.StepColorOn);

        HighlightColor1 = ToBrush(theme.HighlightColor1);
        HighlightColor2 = ToBrush(theme.HighlightColor2);
        HighlightColor3 = ToBrush(theme.HighlightColor3);
        HighlightColor4 = ToBrush(theme.HighlightColor4);
        HighlightColor5 = ToBrush(theme.HighlightColor5);
        HighlightColor6 = ToBrush(theme.HighlightColor6);
        HighlightColor7 = ToBrush(theme.HighlightColor7);
        
        ThemeChanged?.Invoke();
    }

    public Brush ToBrush(StepColor color)
    {
        return color switch
        {
            StepColor.Change1 => StepColorChange1,
            StepColor.Change2 => StepColorChange2,
            StepColor.Neutral => StepColorNeutral,
            StepColor.Cause1 => StepColorCause1,
            StepColor.Cause2 => StepColorCause2,
            StepColor.Cause3 => StepColorCause3,
            StepColor.Cause4 => StepColorCause4,
            StepColor.Cause5 => StepColorCause5,
            StepColor.Cause6 => StepColorCause6,
            StepColor.Cause7 => StepColorCause7,
            StepColor.Cause8 => StepColorCause8,
            StepColor.Cause9 => StepColorCause9,
            StepColor.Cause10 => StepColorCause10,
            StepColor.On => StepColorCauseOn,
            _ => StepColorNeutral
        };
    }

    public Brush ToBrush(HighlightColor color)
    {
        return color switch
        {
            HighlightColor.First => HighlightColor1,
            HighlightColor.Second => HighlightColor2,
            HighlightColor.Third => HighlightColor3,
            HighlightColor.Fourth => HighlightColor4,
            HighlightColor.Fifth => HighlightColor5,
            HighlightColor.Sixth => HighlightColor6,
            HighlightColor.Seventh => HighlightColor7,
            _ => Brushes.Transparent
        };
    }
    
    public Brush ToBrush(ExplanationColor color)
    {
        return color switch
        {
            ExplanationColor.Primary => Primary1,
            ExplanationColor.Secondary => Secondary1,
            _ => Text
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

