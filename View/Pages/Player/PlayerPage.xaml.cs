using System.Windows;
using Presenter;

namespace View.Pages.Player;

public partial class PlayerPage : HandledPage
{
    private readonly IPageHandler _pageHandler;
    private readonly PresenterFactory _factory;
    
    public PlayerPage(IPageHandler handler, PresenterFactory factory)
    {
        InitializeComponent();
        
        _factory = factory;
        _pageHandler = handler;
        
        Panel.Children.Add(new SudokuGrid(30, 1, 3));
    }

    public override void OnShow()
    {
        
    }

    public override void OnQuit()
    {
        
    }

    private void GoBack(object sender, RoutedEventArgs e)
    {
        _pageHandler.ShowPage(PagesName.First);
    }
}