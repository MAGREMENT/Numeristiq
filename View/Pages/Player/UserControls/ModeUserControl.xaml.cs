using System.Windows.Input;
using System.Windows.Media;
using View.Utility;

namespace View.Pages.Player.UserControls;

public partial class ModeUserControl
{
    public event OnSelection? Selected;
    
    public ModeUserControl(string name, double width)
    {
        InitializeComponent();

        Block.Text = name;
        Block.Width = width;
    }

    private void OnSelection(object sender, MouseButtonEventArgs e)
    {
        Selected?.Invoke(this);
    }

    public void ShowSelection()
    {
        Border.BorderBrush = Brushes.MediumPurple;
    }

    public void ShowUnSelection()
    {
        Border.BorderBrush = Brushes.DimGray;
    }

    public void InvokeSelection()
    {
        Selected?.Invoke(this);
    }
}

public delegate void OnSelection(ModeUserControl muc);