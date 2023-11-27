using System.Windows;
using System.Windows.Media;
using View.Utility;

namespace View.Pages.StrategyManager.UserControls;

public partial class SearchItemUserControl
{ 
    public SearchItemUserControl(string s)
    {
        InitializeComponent();

        TextBlock.Text = s;
        
        MouseEnter += (_, _) =>
        {
            Background = Brushes.LightGray;
        };
        MouseLeave += (_, _) =>
        {
            Background = ColorManager.Background1;
        };

        MouseDown += (_, _) =>
        {
            DragDrop.DoDragDrop(this, new StrategyShuffleData(TextBlock.Text, true, -1),
                DragDropEffects.All);
        };
    }
}

public class StrategyShuffleData
{
    public StrategyShuffleData(string name, bool fromSearch, int currentPosition)
    {
        Name = name;
        FromSearch = fromSearch;
        CurrentPosition = currentPosition;
    }

    public string Name { get; }
    public bool FromSearch { get; }
    public int CurrentPosition { get; }
}