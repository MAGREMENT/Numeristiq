using Presenter.Tectonic;
using View.Tectonic;

namespace View;

public partial class MainTectonicWindow : ISolverView
{
    private readonly ApplicationPresenter _presenter = new();
    
    public MainTectonicWindow()
    {
        InitializeComponent();
        
        Panel.Children.Insert(0, new TectonicGrid(400, 400));
    }
}