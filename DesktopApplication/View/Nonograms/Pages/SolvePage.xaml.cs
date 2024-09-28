using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DesktopApplication.Presenter;
using DesktopApplication.Presenter.Nonograms.Solve;
using DesktopApplication.View.Controls;
using DesktopApplication.View.Nonograms.Controls;

namespace DesktopApplication.View.Nonograms.Pages;

public partial class SolvePage : INonogramSolveView
{
    private readonly NonogramSolvePresenter _presenter;

    public INonogramDrawer Drawer => (INonogramDrawer)EmbeddedDrawer.OptimizableContent!;

    public SolvePage()
    {
        InitializeComponent();

        _presenter = PresenterFactory.Instance.Initialize(this);
    }
    
    public void ShowNonogramAsString(string s)
    {
        TextBox.SetText(s);
    }

    private void Solve(object sender, RoutedEventArgs e)
    {
        _presenter.Solve(false);
    }

    private void Advance(object sender, RoutedEventArgs e)
    {
        _presenter.Solve(true);
    }

    private void CreateNewNonogram(string s)
    {
        _presenter.SetNewNonogram(s);
    }

    private void OnHideableTextboxShowed()
    {
        _presenter.ShowNonogramAsString();
    }

    public override string Header => "Solve";

    public override void OnShow()
    {
        
    }

    public override void OnClose()
    {
        
    }

    public override object? TitleBarContent()
    {
        return null;
    }

    protected override StackPanel GetStepPanel()
    {
        return StepPanel;
    }

    protected override ScrollViewer GetStepViewer()
    {
        return StepViewer;
    }

    protected override IStepManagingPresenter GetStepsPresenter()
    {
        return _presenter;
    }

    protected override ISizeOptimizable GetExplanationDrawer()
    {
        var board = new NonogramBoard
        {
            CellSize = 50,
            BackgroundBrush = Brushes.Transparent,
            BigLineWidth = 3
        };

        board.SetResourceReference(DrawingBoard.LineBrushProperty, "Text");
        board.SetResourceReference(NonogramBoard.FillingBrushProperty, "Primary");
        board.SetResourceReference(NonogramBoard.UnavailableBrushProperty, "Secondary");

        return board;
    }

    private void OnRowDimensionChangeAsked(int diff)
    {
        _presenter.SetSizeTo(diff, 0);
    }

    private void OnColumnDimensionChangeAsked(int diff)
    {
        _presenter.SetSizeTo(0, diff);
    }

    private void UpdateRowCount(int number)
    {
        ((DimensionChooser)EmbeddedDrawer.SideControls[0]!).SetDimension(number);
    }
    
    private void UpdateColumnCount(int number)
    {
        ((DimensionChooser)EmbeddedDrawer.SideControls[1]!).SetDimension(number);
    }
}