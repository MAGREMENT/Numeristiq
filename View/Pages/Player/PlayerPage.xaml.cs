namespace View.Pages.Player;

public partial class PlayerPage : HandledPage
{
    public PlayerPage()
    {
        InitializeComponent();
        
        Panel.Children.Add(new SudokuGrid(20, 1, 3));
    }

    public override void OnShow()
    {
        
    }

    public override void OnQuit()
    {
        
    }
}