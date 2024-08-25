using DesktopApplication.Presenter;
using DesktopApplication.Presenter.YourPuzzles;

namespace DesktopApplication.View.YourPuzzles.Pages;

public partial class CreatePage : IYourPuzzleView
{
    private readonly YourPuzzlePresenter _presenter;
    
    public CreatePage()
    {
        InitializeComponent();

        _presenter = PresenterFactory.Instance.Initialize(this);
    }

    public IYourPuzzleDrawer Drawer => (IYourPuzzleDrawer)Embedded.OptimizableContent!;

    public override string Header => "Create";
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

    private void OnRowDimensionChangeAsked(int diff)
    {
        _presenter.SetRowCount(diff);
    }

    private void OnColumnDimensionChangeAsked(int diff)
    {
        _presenter.SetColumnCount(diff);
    }
}