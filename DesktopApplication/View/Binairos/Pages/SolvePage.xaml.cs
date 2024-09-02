using System.Windows;
using System.Windows.Controls;
using DesktopApplication.Presenter;
using DesktopApplication.Presenter.Binairos.Solve;
using DesktopApplication.View.Binairos.Controls;
using DesktopApplication.View.Controls;
using DesktopApplication.View.HelperWindows;

namespace DesktopApplication.View.Binairos.Pages;

public partial class SolvePage : IBinairoSolveView
{
    private readonly BinairoSolvePresenter _presenter;
    
    public SolvePage()
    {
        InitializeComponent();

        _presenter = PresenterFactory.Instance.Initialize(this);
    }

    public override string Header => "Solve";
    public override void OnShow()
    {
        
    }

    public override void OnClose()
    {
        
    }

    public override object TitleBarContent()
    {
        var settings = new SettingsButton();
        settings.Clicked += () =>
        {
            var window = new SettingWindow(_presenter.SettingsPresenter);
            window.Show();
        };

        return settings;
    }

    protected override StackPanel GetStepPanel()
    {
        return StepPanel;
    }

    protected override ScrollViewer GetStepViewer()
    {
        return StepViewer;
    }

    protected override ISolveWithStepsPresenter GetStepsPresenter()
    {
        return _presenter;
    }

    protected override ISizeOptimizable GetExplanationDrawer()
    {
        var board = new BinairoBoard
        {
            BigLineWidth = 3
        };
        board.SetResourceReference(DrawingBoard.LineBrushProperty, "Text");
        board.SetResourceReference(DrawingBoard.DefaultNumberBrushProperty, "Text");
        board.SetResourceReference(DrawingBoard.ClueNumberBrushProperty, "Primary");
        board.SetResourceReference(BinairoBoard.CircleFirstColorProperty, "Text");
        board.SetResourceReference(BinairoBoard.CircleSecondColorProperty, "Secondary");
        board.SetResourceReference(DrawingBoard.LinkBrushProperty, "Accent");

        return board;
    }

    public void SetBinairoAsString(string s)
    {
        HideableTextBox.SetText(s);
    }

    public IBinairoDrawer Drawer => (IBinairoDrawer)Embedded.OptimizableContent!;

    private void OnShowed()
    {
        _presenter.ShowBinairoAsString();
    }

    private void OnTextChanged(string s)
    {
        _presenter.SetNewBinairo(s);
    }

    private void OnSolve(object sender, RoutedEventArgs e)
    {
        _presenter.Solve(false);
    }

    private void OnAdvance(object sender, RoutedEventArgs e)
    {
        _presenter.Solve(true);
    }
}