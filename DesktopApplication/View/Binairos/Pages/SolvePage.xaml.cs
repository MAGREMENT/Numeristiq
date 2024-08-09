using System.Windows.Controls;
using DesktopApplication.Presenter;
using DesktopApplication.Presenter.Binairos.Solve;
using DesktopApplication.View.Controls;

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

    protected override ISolveWithStepsPresenter GetStepsPresenter()
    {
        return _presenter;
    }

    protected override ISizeOptimizable GetExplanationDrawer()
    {
        throw new System.NotImplementedException();
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
}