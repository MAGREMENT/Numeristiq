using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using DesktopApplication.Presenter;
using DesktopApplication.Presenter.Tectonics;
using DesktopApplication.Presenter.Tectonics.Solve;
using DesktopApplication.View.Controls;
using DesktopApplication.View.Tectonics.Controls;
using DesktopApplication.View.Utility;
using Model.Utility;
using SelectionMode = DesktopApplication.Presenter.Tectonics.Solve.SelectionMode;

namespace DesktopApplication.View.Tectonics.Pages;

public partial class SolvePage : ITectonicSolveView
{
    private readonly TectonicSolvePresenter _presenter;
    private readonly bool _intialized;
    
    public SolvePage()
    {
        InitializeComponent();

        _presenter = PresenterFactory.Instance.Initialize(this);
        _intialized = true;
    }

    public ITectonicDrawer Drawer => (TectonicBoard)EmbeddedDrawer.OptimizableContent!;

    public void SetTectonicString(string s)
    {
        TextBox.SetText(s);
    }

    private void CreateNewTectonic(string s)
    {
        _presenter.SetNewTectonic(s);
    }

    private void Solve(object sender, RoutedEventArgs e)
    {
        _presenter.Solve(false);
    }

    private void Advance(object sender, RoutedEventArgs e)
    {
        _presenter.Solve(true);
    }

    private void Reset(object sender, RoutedEventArgs e)
    {
        _presenter.Reset();
    }

    private void Clear(object sender, RoutedEventArgs e)
    {
        _presenter.Clear();
    }

    private void OnRowCountChange(int number)
    {
        ((DimensionChooser)EmbeddedDrawer.SideControls[0]!).SetDimension(number);
    }

    private void OnColumnCountChange(int number)
    {
        ((DimensionChooser)EmbeddedDrawer.SideControls[1]!).SetDimension(number);
    }

    private void OnRowDimensionChangeAsked(int diff)
    {
        _presenter.SetNewRowCount(diff);
    }

    private void OnColumnDimensionChangeAsked(int diff)
    {
        _presenter.SetNewColumnCount(diff);
    }

    private void OnHideableTextboxShowed()
    {
        _presenter.SetTectonicString();
    }

    private void OnCellSelection(int row, int col)
    {
        _presenter.SelectCell(new Cell(row, col));
    }

    private void OnCellAddedToSelection(int row, int col)
    {
        _presenter.SelectCell(new Cell(row, col));
    }

    private void OnSelectionEnd()
    {
        _presenter.EndCellSelection();
    }

    private void ModeToDefault(object sender, RoutedEventArgs e)
    {
        if(_intialized) _presenter.SetSelectionMode(SelectionMode.Default);
    }
    
    private void ModeToMerge(object sender, RoutedEventArgs e)
    {
        _presenter.SetSelectionMode(SelectionMode.Merge);
    }
    
    private void ModeToEdit(object sender, RoutedEventArgs e)
    {
        _presenter.SetSelectionMode(SelectionMode.Edit);
    }
    
    private void DoBoardInput(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.D1 :
            case Key.NumPad1 :
                _presenter.SetCurrentCell(1);
                break;
            case Key.D2 :
            case Key.NumPad2 :
                _presenter.SetCurrentCell(2);
                break;
            case Key.D3 :
            case Key.NumPad3 :
                _presenter.SetCurrentCell(3);
                break;
            case Key.D4 :
            case Key.NumPad4 :
                _presenter.SetCurrentCell(4);
                break;
            case Key.D5 :
            case Key.NumPad5 :
                _presenter.SetCurrentCell(5);
                break;
            case Key.D6 :
            case Key.NumPad6 :
                _presenter.SetCurrentCell(6);
                break;
            case Key.D7 :
            case Key.NumPad7 :
                _presenter.SetCurrentCell(7);
                break;
            case Key.D8 :
            case Key.NumPad8 :
                _presenter.SetCurrentCell(8);
                break;
            case Key.D9 :
            case Key.NumPad9 :
                _presenter.SetCurrentCell(9);
                break;
            case Key.D0 :
            case Key.NumPad0 :
            case Key.Back :
                _presenter.DeleteCurrentCell();
                break;
        }
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
        return LogPanel;
    }

    protected override ScrollViewer GetStepViewer()
    {
        return LogViewer;
    }

    protected override ISolveWithStepsPresenter GetStepsPresenter()
    {
        return _presenter;
    }

    protected override ISizeOptimizable GetExplanationDrawer()
    {
        var board = new TectonicBoard
        {
            BackgroundBrush = Brushes.Transparent,
            BigLineRange = new DependantThicknessRange
            {
                Minimum = 3,
                Maximum = 5,
                Floor = 50,
                Roof = 100
            },
            SmallLineWidth=1,
            CellSize = 50
        };

        board.SetResourceReference(TectonicBoard.LineBrushProperty, "Text");
        board.SetResourceReference(TectonicBoard.DefaultNumberBrushProperty, "Text");
        board.SetResourceReference(TectonicBoard.ClueNumberBrushProperty, "Primary");
        board.SetResourceReference(TectonicBoard.LinkBrushProperty, "Accent");

        return board;
    }
}