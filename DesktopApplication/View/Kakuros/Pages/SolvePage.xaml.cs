using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using DesktopApplication.Presenter;
using DesktopApplication.Presenter.Kakuros.Solve;
using DesktopApplication.View.Controls;
using DesktopApplication.View.Kakuros.Controls;
using Model.Utility;

namespace DesktopApplication.View.Kakuros.Pages;

public partial class SolvePage : IKakuroSolveView
{
    private readonly KakuroSolvePresenter _presenter;

    public IKakuroSolverDrawer Drawer => (KakuroBoard)ContentControl.OptimizableContent!;

    public SolvePage()
    {
        InitializeComponent();

        _presenter = PresenterFactory.Instance.Initialize(this);
        DefaultMode.IsChecked = true;
    }
    
    public void SetKakuroAsString(string s)
    {
        KakuroAsString.SetText(s);
    }

    private void OnNewKakuro(string s)
    {
        _presenter.SetNewKakuro(s);
    }

    private void Solve(object sender, RoutedEventArgs e)
    {
        _presenter.Solve(false);
    }

    private void Advance(object sender, RoutedEventArgs e)
    {
        _presenter.Solve(true);
    }

    private void OnCellSelection(Cell cell, bool isAmountCell)
    {
        if (isAmountCell) _presenter.SelectSum(cell.Row, cell.Column);
        else _presenter.SelectCell(cell.Row, cell.Column);
    }

    private void DoBoardInput(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.NumPad1 : _presenter.AddDigitToAmount(1);
                break;
            case Key.NumPad2 : _presenter.AddDigitToAmount(2);
                break;
            case Key.NumPad3 : _presenter.AddDigitToAmount(3);
                break;
            case Key.NumPad4 : _presenter.AddDigitToAmount(4);
                break;
            case Key.NumPad5 : _presenter.AddDigitToAmount(5);
                break;
            case Key.NumPad6 : _presenter.AddDigitToAmount(6);
                break;
            case Key.NumPad7 : _presenter.AddDigitToAmount(7);
                break;
            case Key.NumPad8 : _presenter.AddDigitToAmount(8);
                break;
            case Key.NumPad9 : _presenter.AddDigitToAmount(9);
                break;
            case Key.Back : _presenter.RemoveDigitFromAmount();
                break;
            case Key.Enter : _presenter.EnterAmount();
                break;
        }
    }

    private void OnLostFocus(object sender, RoutedEventArgs e)
    {
        _presenter.EnterAmount();
    }

    private void OnKakuroAsStringShowed()
    {
        _presenter.OnKakuroAsStringBoxShowed();
    }

    private void ModeToDefault(object sender, RoutedEventArgs e)
    {
        _presenter.SetEditMode(EditMode.Default);
    }

    private void ModeToEdit(object sender, RoutedEventArgs e)
    {
        _presenter.SetEditMode(EditMode.Edit);
    }

    private void CreateDefault(object sender, MouseButtonEventArgs e)
    {
        _presenter.AddDefault();
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

    protected override StackPanel GetStepPanel() => StepPanel;

    protected override ScrollViewer GetStepViewer() => StepViewer;

    protected override ISolveWithStepsPresenter GetStepsPresenter() => _presenter;

    protected override ISizeOptimizable GetExplanationDrawer()
    {
        var board = new KakuroBoard
        {
            BackgroundBrush = Brushes.Transparent,
            BigLineWidth = 3,
            AmountHeightFactor = 0.5,
            AmountWidthFactor = 0.5,
            CellSize = 20
        };

        board.SetResourceReference(DrawingBoard.DefaultNumberBrushProperty, "Text");
        board.SetResourceReference(KakuroBoard.AmountLineBrushProperty, "Accent");

        return board;
    }
}