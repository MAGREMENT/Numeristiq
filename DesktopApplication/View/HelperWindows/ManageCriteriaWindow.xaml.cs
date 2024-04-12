using System.Windows;
using DesktopApplication.Presenter.Sudoku.Generate;

namespace DesktopApplication.View.HelperWindows;

public partial class ManageCriteriaWindow : IManageCriteriaView
{
    private readonly ManageCriteriaPresenter _presenter;
    
    public ManageCriteriaWindow(ManageCriteriaPresenterBuilder builder)
    {
        InitializeComponent();

        _presenter = builder.Build(this);
        
        TitleBar.RefreshMaximizeRestoreButton(WindowState);
        StateChanged += (_, _) => TitleBar.RefreshMaximizeRestoreButton(WindowState);
    }
    
    private void Minimize()
    {
        WindowState = WindowState.Minimized;
    }

    private void ChangeSize()
    {
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }
}