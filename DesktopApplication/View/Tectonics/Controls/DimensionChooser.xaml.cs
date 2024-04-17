using System.Windows.Input;

namespace DesktopApplication.View.Tectonics.Controls;

public partial class DimensionChooser
{
    public event OnDimensionChangeAsked? DimensionChangeAsked;
    
    public string TitleText
    {
        set => Title.Text = value;
    }
    
    public DimensionChooser()
    {
        InitializeComponent();
    }

    public void SetDimension(int n)
    {
        Number.Text = n.ToString();
    }

    private void AddToDimension(object sender, MouseButtonEventArgs e)
    {
        DimensionChangeAsked?.Invoke(1);
    }

    private void RemoveFromDimension(object sender, MouseButtonEventArgs e)
    {
        DimensionChangeAsked?.Invoke(-1);
    }
}

public delegate void OnDimensionChangeAsked(int diff);